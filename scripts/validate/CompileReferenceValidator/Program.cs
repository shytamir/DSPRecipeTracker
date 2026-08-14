using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;

if (args.Length != 3)
{
    Console.Error.WriteLine("Usage: CompileReferenceValidator <production.dll> <shim.dll> <surface-inventory.json>");
    return 2;
}

var inventory = JsonSerializer.Deserialize<Inventory>(File.ReadAllText(args[2]), new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
}) ?? throw new InvalidDataException("Surface inventory is empty.");

var failures = new List<string>();
var assembly = inventory.Assemblies.SingleOrDefault(item => item.Name == Path.GetFileNameWithoutExtension(args[1]));
if (assembly is null)
{
    failures.Add("Shim assembly is absent from the surface inventory: " + Path.GetFileNameWithoutExtension(args[1]));
}
else
{
    ValidateShim(args[1], assembly, failures);
    ValidateProduction(args[0], inventory, failures);
}

if (failures.Count != 0)
{
    foreach (var failure in failures)
    {
        Console.Error.WriteLine("FAIL: " + failure);
    }

    return 1;
}

Console.WriteLine("Compile-reference surface validation passed.");
return 0;

static void ValidateShim(string path, AssemblyInventory expectedAssembly, List<string> failures)
{
    using var stream = File.OpenRead(path);
    using var pe = new PEReader(stream);
    var reader = pe.GetMetadataReader();
    var actualAssemblyName = reader.GetString(reader.GetAssemblyDefinition().Name);
    if (actualAssemblyName != expectedAssembly.Name)
    {
        failures.Add($"Shim assembly identity is {actualAssemblyName}, expected {expectedAssembly.Name}.");
    }

    var isReferenceAssembly = reader.GetAssemblyDefinition().GetCustomAttributes()
        .Select(handle => CustomAttributeTypeName(reader, reader.GetCustomAttribute(handle)))
        .Any(name => name == "System.Runtime.CompilerServices.ReferenceAssemblyAttribute");
    if (!isReferenceAssembly)
    {
        failures.Add("Shim validation input is not a compiler-produced reference assembly.");
    }

    var expectedTypes = expectedAssembly.Types.Select(item => item.Name).ToHashSet(StringComparer.Ordinal);
    var actualTypes = new HashSet<string>(StringComparer.Ordinal);
    var expectedMembers = expectedAssembly.Types
        .SelectMany(type => type.Members.Select(member => MemberKey(type.Name, member.Kind, member.Name, member.Signature)))
        .ToHashSet(StringComparer.Ordinal);
    var actualMembers = new HashSet<string>(StringComparer.Ordinal);
    var provider = new TypeNameProvider();

    foreach (var handle in reader.TypeDefinitions)
    {
        var definition = reader.GetTypeDefinition(handle);
        var attributes = definition.Attributes;
        if ((attributes & TypeAttributes.VisibilityMask) is not TypeAttributes.Public and not TypeAttributes.NestedPublic)
        {
            continue;
        }

        var typeName = FullTypeDefinitionName(reader, definition);
        actualTypes.Add(typeName);

        foreach (var methodHandle in definition.GetMethods())
        {
            var method = reader.GetMethodDefinition(methodHandle);
            if ((method.Attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public)
            {
                continue;
            }

            var signature = method.DecodeSignature(provider, genericContext: null);
            var signatureText = signature.ReturnType + "(" + string.Join(",", signature.ParameterTypes) + ")";
            var methodName = reader.GetString(method.Name);
            actualMembers.Add(MemberKey(typeName, "method", methodName, signatureText));
        }
    }

    foreach (var missing in expectedTypes.Except(actualTypes))
    {
        failures.Add("Inventory type missing from shim: " + missing);
    }

    foreach (var extra in actualTypes.Except(expectedTypes))
    {
        failures.Add("Shim type missing from inventory: " + extra);
    }

    foreach (var missing in expectedMembers.Except(actualMembers))
    {
        failures.Add("Inventory member missing from shim: " + missing);
    }

    foreach (var extra in actualMembers.Except(expectedMembers))
    {
        failures.Add("Shim member missing from inventory: " + extra);
    }
}

static void ValidateProduction(string path, Inventory inventory, List<string> failures)
{
    using var stream = File.OpenRead(path);
    using var pe = new PEReader(stream);
    var reader = pe.GetMetadataReader();
    var inventoryAssemblies = inventory.Assemblies.ToDictionary(item => item.Name, StringComparer.Ordinal);
    var provider = new TypeNameProvider();

    foreach (var handle in reader.TypeReferences)
    {
        var type = reader.GetTypeReference(handle);
        var assemblyName = ResolveAssemblyName(reader, type.ResolutionScope);
        if (assemblyName is null || !inventoryAssemblies.TryGetValue(assemblyName, out var expectedAssembly))
        {
            continue;
        }

        var typeName = FullTypeReferenceName(reader, type);
        if (!expectedAssembly.Types.Any(item => item.Name == typeName))
        {
            failures.Add($"Production consumes unlisted external type: {assemblyName}:{typeName}.");
        }
    }

    foreach (var handle in reader.MemberReferences)
    {
        var member = reader.GetMemberReference(handle);
        if (member.Parent.Kind != HandleKind.TypeReference)
        {
            continue;
        }

        var type = reader.GetTypeReference((TypeReferenceHandle)member.Parent);
        var assemblyName = ResolveAssemblyName(reader, type.ResolutionScope);
        if (assemblyName is null || !inventoryAssemblies.TryGetValue(assemblyName, out var expectedAssembly))
        {
            continue;
        }

        var typeName = FullTypeReferenceName(reader, type);
        var memberName = reader.GetString(member.Name);
        var signature = member.DecodeMethodSignature(provider, genericContext: null);
        var signatureText = signature.ReturnType + "(" + string.Join(",", signature.ParameterTypes) + ")";
        var key = MemberKey(typeName, "method", memberName, signatureText);
        var expected = expectedAssembly.Types
            .Where(item => item.Name == typeName)
            .SelectMany(item => item.Members)
            .Any(item => MemberKey(typeName, item.Kind, item.Name, item.Signature) == key);
        if (!expected)
        {
            failures.Add($"Production consumes unlisted external member: {assemblyName}:{key}.");
        }
    }
}

static string? ResolveAssemblyName(MetadataReader reader, EntityHandle scope)
{
    return scope.Kind switch
    {
        HandleKind.AssemblyReference => reader.GetString(reader.GetAssemblyReference((AssemblyReferenceHandle)scope).Name),
        HandleKind.TypeReference => ResolveAssemblyName(reader, reader.GetTypeReference((TypeReferenceHandle)scope).ResolutionScope),
        _ => null
    };
}

static string? CustomAttributeTypeName(MetadataReader reader, CustomAttribute attribute)
{
    if (attribute.Constructor.Kind == HandleKind.MemberReference)
    {
        var constructor = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
        if (constructor.Parent.Kind == HandleKind.TypeReference)
        {
            return FullTypeReferenceName(reader, reader.GetTypeReference((TypeReferenceHandle)constructor.Parent));
        }
    }

    return null;
}

static string FullTypeDefinitionName(MetadataReader reader, TypeDefinition definition)
{
    var name = reader.GetString(definition.Name);
    var ns = reader.GetString(definition.Namespace);
    return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
}

static string FullTypeReferenceName(MetadataReader reader, TypeReference reference)
{
    var name = reader.GetString(reference.Name);
    var ns = reader.GetString(reference.Namespace);
    return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
}

static string MemberKey(string type, string kind, string name, string signature) => $"{type}|{kind}|{name}|{signature}";

sealed class TypeNameProvider : ISignatureTypeProvider<string, object?>
{
    public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[" + new string(',', shape.Rank - 1) + "]";
    public string GetByReferenceType(string elementType) => elementType + "&";
    public string GetFunctionPointerType(MethodSignature<string> signature) => "methodptr";
    public string GetGenericInstantiation(string genericType, System.Collections.Immutable.ImmutableArray<string> typeArguments) => genericType + "<" + string.Join(",", typeArguments) + ">";
    public string GetGenericMethodParameter(object? genericContext, int index) => "!!" + index;
    public string GetGenericTypeParameter(object? genericContext, int index) => "!" + index;
    public string GetModifiedType(string modifierType, string unmodifiedType, bool isRequired) => unmodifiedType;
    public string GetPinnedType(string elementType) => elementType;
    public string GetPointerType(string elementType) => elementType + "*";
    public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode switch
    {
        PrimitiveTypeCode.Void => "System.Void",
        PrimitiveTypeCode.Boolean => "System.Boolean",
        PrimitiveTypeCode.Byte => "System.Byte",
        PrimitiveTypeCode.SByte => "System.SByte",
        PrimitiveTypeCode.Int16 => "System.Int16",
        PrimitiveTypeCode.UInt16 => "System.UInt16",
        PrimitiveTypeCode.Int32 => "System.Int32",
        PrimitiveTypeCode.UInt32 => "System.UInt32",
        PrimitiveTypeCode.Int64 => "System.Int64",
        PrimitiveTypeCode.UInt64 => "System.UInt64",
        PrimitiveTypeCode.Single => "System.Single",
        PrimitiveTypeCode.Double => "System.Double",
        PrimitiveTypeCode.Char => "System.Char",
        PrimitiveTypeCode.String => "System.String",
        PrimitiveTypeCode.Object => "System.Object",
        PrimitiveTypeCode.IntPtr => "System.IntPtr",
        PrimitiveTypeCode.UIntPtr => "System.UIntPtr",
        _ => typeCode.ToString()
    };
    public string GetSZArrayType(string elementType) => elementType + "[]";
    public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
    {
        var definition = reader.GetTypeDefinition(handle);
        return JoinName(reader.GetString(definition.Namespace), reader.GetString(definition.Name));
    }
    public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
    {
        var reference = reader.GetTypeReference(handle);
        return JoinName(reader.GetString(reference.Namespace), reader.GetString(reference.Name));
    }
    public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) => reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);

    private static string JoinName(string ns, string name) => string.IsNullOrEmpty(ns) ? name : ns + "." + name;
}

sealed class Inventory
{
    public int SchemaVersion { get; set; }
    public List<AssemblyInventory> Assemblies { get; set; } = new();
}

sealed class AssemblyInventory
{
    public string Name { get; set; } = "";
    public string ShimProject { get; set; } = "";
    public List<TypeInventory> Types { get; set; } = new();
}

sealed class TypeInventory
{
    public string Name { get; set; } = "";
    public string Usage { get; set; } = "";
    public List<MemberInventory> Members { get; set; } = new();
}

sealed class MemberInventory
{
    public string Kind { get; set; } = "";
    public string Name { get; set; } = "";
    public string Signature { get; set; } = "";
}
