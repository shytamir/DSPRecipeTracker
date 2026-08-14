using System.IO.Compression;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

if (args.Length != 8)
{
    Console.Error.WriteLine("Usage: PackageValidator <zip> <source-dll> <semantic-version> <assembly-version> <diagnostic-label> <plugin-guid> <display-name> <report.json>");
    return 2;
}

var zipPath = Path.GetFullPath(args[0]);
var sourceDllPath = Path.GetFullPath(args[1]);
var semanticVersion = args[2];
var assemblyVersion = args[3];
var diagnosticLabel = args[4];
var pluginGuid = args[5];
var displayName = args[6];
var reportPath = Path.GetFullPath(args[7]);
var failures = new List<string>();
var expectedEntries = new HashSet<string>(StringComparer.Ordinal)
{
    "manifest.json",
    "README.md",
    "icon.png",
    "BepInEx/plugins/DSPRecipeTracker/DSPRecipeTracker.dll"
};

if (!File.Exists(zipPath) || new FileInfo(zipPath).Length == 0)
{
    failures.Add("Package ZIP is missing or empty.");
}
else
{
    using var archive = ZipFile.OpenRead(zipPath);
    var entryNames = archive.Entries.Select(entry => entry.FullName).ToList();
    var duplicate = entryNames.GroupBy(name => name, StringComparer.OrdinalIgnoreCase).FirstOrDefault(group => group.Count() > 1);
    if (duplicate is not null)
    {
        failures.Add("Archive contains a duplicate portable path: " + duplicate.Key);
    }

    foreach (var name in entryNames)
    {
        if (name.Contains('\\') || name.StartsWith('/') || name.Contains(':') || name.Split('/').Any(part => part is "." or ".." or ""))
        {
            failures.Add("Archive path is not portable: " + name);
        }
    }

    foreach (var missing in expectedEntries.Except(entryNames, StringComparer.Ordinal))
    {
        failures.Add("Archive entry is missing: " + missing);
    }

    foreach (var extra in entryNames.Except(expectedEntries, StringComparer.Ordinal))
    {
        failures.Add("Archive entry is not allowed: " + extra);
    }

    ValidateManifest(ReadEntry(archive, "manifest.json", failures), semanticVersion, failures);
    ValidateReadme(ReadEntry(archive, "README.md", failures), failures);
    ValidatePng(ReadEntry(archive, "icon.png", failures), failures);
    ValidatePlugin(
        ReadEntry(archive, "BepInEx/plugins/DSPRecipeTracker/DSPRecipeTracker.dll", failures),
        sourceDllPath,
        semanticVersion,
        assemblyVersion,
        diagnosticLabel,
        pluginGuid,
        displayName,
        failures);
}

Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
var report = new
{
    schemaVersion = 1,
    package = Path.GetFileName(zipPath),
    semanticVersion,
    packageSha256 = File.Exists(zipPath) ? HashFile(zipPath) : null,
    sourceDllSha256 = File.Exists(sourceDllPath) ? HashFile(sourceDllPath) : null,
    passed = failures.Count == 0,
    failures
};
File.WriteAllText(reportPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));

if (failures.Count != 0)
{
    foreach (var failure in failures)
    {
        Console.Error.WriteLine("FAIL: " + failure);
    }

    return 1;
}

Console.WriteLine("Thunderstore package static validation passed.");
return 0;

static byte[]? ReadEntry(ZipArchive archive, string name, List<string> failures)
{
    var entry = archive.GetEntry(name);
    if (entry is null)
    {
        return null;
    }

    using var stream = entry.Open();
    using var buffer = new MemoryStream();
    stream.CopyTo(buffer);
    var bytes = buffer.ToArray();
    if (bytes.Length == 0)
    {
        failures.Add("Archive entry is empty: " + name);
    }
    return bytes;
}

static void ValidateManifest(byte[]? bytes, string expectedVersion, List<string> failures)
{
    if (bytes is null)
    {
        return;
    }

    try
    {
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;
        CheckJsonString(root, "name", "DSPRecipeTracker", failures);
        CheckJsonString(root, "version_number", expectedVersion, failures);
        CheckJsonString(root, "website_url", "https://github.com/shytamir/DSPRecipeTracker", failures);
        var description = root.TryGetProperty("description", out var descriptionElement) ? descriptionElement.GetString() : null;
        if (string.IsNullOrWhiteSpace(description) || !description.Contains("not release-ready", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Manifest description must state that the artifact is not release-ready.");
        }
        if (!root.TryGetProperty("dependencies", out var dependencies) || dependencies.ValueKind != JsonValueKind.Array ||
            dependencies.GetArrayLength() != 1 || dependencies[0].GetString() != "xiaoye97-BepInEx-5.4.17")
        {
            failures.Add("Manifest dependencies must contain only xiaoye97-BepInEx-5.4.17.");
        }
        var allowed = new HashSet<string>(new[] { "name", "version_number", "website_url", "description", "dependencies" }, StringComparer.Ordinal);
        foreach (var property in root.EnumerateObject())
        {
            if (!allowed.Contains(property.Name))
            {
                failures.Add("Manifest property is not allowed: " + property.Name);
            }
        }
    }
    catch (Exception exception)
    {
        failures.Add("Manifest is not valid JSON: " + exception.Message);
    }
}

static void CheckJsonString(JsonElement root, string property, string expected, List<string> failures)
{
    if (!root.TryGetProperty(property, out var element) || element.ValueKind != JsonValueKind.String || element.GetString() != expected)
    {
        failures.Add($"Manifest {property} must be {expected}.");
    }
}

static void ValidateReadme(byte[]? bytes, List<string> failures)
{
    if (bytes is null)
    {
        return;
    }
    var text = Encoding.UTF8.GetString(bytes);
    if (!text.Contains("not been installed or validated in-game", StringComparison.OrdinalIgnoreCase))
    {
        failures.Add("Package README must state that installed and in-game validation have not been performed.");
    }
    if (!text.Contains("not release-ready", StringComparison.OrdinalIgnoreCase))
    {
        failures.Add("Package README must state that the artifact is not release-ready.");
    }
}

static void ValidatePng(byte[]? bytes, List<string> failures)
{
    if (bytes is null)
    {
        return;
    }
    ReadOnlySpan<byte> signature = stackalloc byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
    if (bytes.Length < 24 || !bytes.AsSpan(0, 8).SequenceEqual(signature))
    {
        failures.Add("icon.png is not a PNG file.");
        return;
    }
    var width = ReadBigEndianInt32(bytes.AsSpan(16, 4));
    var height = ReadBigEndianInt32(bytes.AsSpan(20, 4));
    if (width != 256 || height != 256)
    {
        failures.Add($"icon.png dimensions are {width}x{height}; expected 256x256.");
    }
}

static int ReadBigEndianInt32(ReadOnlySpan<byte> bytes) =>
    (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];

static void ValidatePlugin(byte[]? bytes, string sourcePath, string semanticVersion, string assemblyVersion,
    string diagnosticLabel, string pluginGuid, string displayName, List<string> failures)
{
    if (bytes is null || bytes.Length == 0)
    {
        return;
    }
    if (!File.Exists(sourcePath))
    {
        failures.Add("Source DLL used for hash comparison is missing.");
        return;
    }
    var sourceHash = SHA256.HashData(File.ReadAllBytes(sourcePath));
    if (!sourceHash.AsSpan().SequenceEqual(SHA256.HashData(bytes)))
    {
        failures.Add("Packaged DLL hash differs from the real Release output.");
    }

    try
    {
        using var stream = new MemoryStream(bytes, writable: false);
        using var pe = new PEReader(stream);
        if (!pe.HasMetadata)
        {
            failures.Add("Packaged DLL is not a managed assembly.");
            return;
        }
        var reader = pe.GetMetadataReader();
        if (!reader.IsAssembly)
        {
            failures.Add("Packaged DLL metadata does not define an assembly.");
            return;
        }
        var assembly = reader.GetAssemblyDefinition();
        if (reader.GetString(assembly.Name) != "DSPRecipeTracker")
        {
            failures.Add("Packaged DLL assembly name is not DSPRecipeTracker.");
        }
        if (assembly.Version.ToString() != assemblyVersion)
        {
            failures.Add($"Packaged DLL assembly version is {assembly.Version}; expected {assemblyVersion}.");
        }

        string? fileVersion = null;
        string? informationalVersion = null;
        string[]? pluginIdentity = null;
        foreach (var handle in assembly.GetCustomAttributes())
        {
            var attribute = reader.GetCustomAttribute(handle);
            var typeName = AttributeTypeName(reader, attribute);
            if (typeName == "System.Reflection.AssemblyFileVersionAttribute")
            {
                fileVersion = ReadFixedStrings(reader, attribute, 1)?[0];
            }
            else if (typeName == "System.Reflection.AssemblyInformationalVersionAttribute")
            {
                informationalVersion = ReadFixedStrings(reader, attribute, 1)?[0];
            }
        }
        foreach (var typeHandle in reader.TypeDefinitions)
        {
            var type = reader.GetTypeDefinition(typeHandle);
            foreach (var handle in type.GetCustomAttributes())
            {
                var attribute = reader.GetCustomAttribute(handle);
                if (AttributeTypeName(reader, attribute) != "BepInEx.BepInPlugin")
                {
                    continue;
                }
                if (pluginIdentity is not null)
                {
                    failures.Add("Packaged DLL contains more than one BepInPlugin identity.");
                }
                pluginIdentity = ReadFixedStrings(reader, attribute, 3);
            }
        }
        if (fileVersion != assemblyVersion)
        {
            failures.Add($"Packaged DLL file version is {fileVersion ?? "<missing>"}; expected {assemblyVersion}.");
        }
        if (informationalVersion != diagnosticLabel)
        {
            failures.Add($"Packaged DLL diagnostic version is {informationalVersion ?? "<missing>"}; expected {diagnosticLabel}.");
        }
        if (pluginIdentity is null || pluginIdentity[0] != pluginGuid || pluginIdentity[1] != displayName || pluginIdentity[2] != semanticVersion)
        {
            failures.Add("Packaged DLL BepInPlugin GUID, display name, or semantic version does not match the package contract.");
        }
    }
    catch (BadImageFormatException)
    {
        failures.Add("Packaged DLL is not a managed PE assembly.");
    }
    catch (Exception exception)
    {
        failures.Add("Packaged DLL metadata could not be inspected: " + exception.Message);
    }
}

static string? AttributeTypeName(MetadataReader reader, CustomAttribute attribute)
{
    EntityHandle parent;
    if (attribute.Constructor.Kind == HandleKind.MemberReference)
    {
        parent = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent;
    }
    else if (attribute.Constructor.Kind == HandleKind.MethodDefinition)
    {
        var method = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor);
        parent = method.GetDeclaringType();
    }
    else
    {
        return null;
    }
    if (parent.Kind == HandleKind.TypeReference)
    {
        var type = reader.GetTypeReference((TypeReferenceHandle)parent);
        var ns = reader.GetString(type.Namespace);
        var name = reader.GetString(type.Name);
        return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
    }
    if (parent.Kind == HandleKind.TypeDefinition)
    {
        var type = reader.GetTypeDefinition((TypeDefinitionHandle)parent);
        var ns = reader.GetString(type.Namespace);
        var name = reader.GetString(type.Name);
        return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
    }
    return null;
}

static string[]? ReadFixedStrings(MetadataReader reader, CustomAttribute attribute, int count)
{
    try
    {
        var blob = reader.GetBlobReader(attribute.Value);
        if (blob.ReadUInt16() != 1)
        {
            return null;
        }
        var values = new string[count];
        for (var index = 0; index < count; index++)
        {
            values[index] = blob.ReadSerializedString() ?? "";
        }
        return values;
    }
    catch
    {
        return null;
    }
}

static string HashFile(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
