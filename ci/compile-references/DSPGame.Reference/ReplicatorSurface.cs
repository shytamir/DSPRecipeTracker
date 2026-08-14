public class Proto
{
    protected Proto()
    {
    }

    public int ID;
}

public class RecipeProto : Proto
{
    private RecipeProto()
    {
    }

    public UnityEngine.Sprite iconSprite
    {
        get { return null; }
    }
}

public class ProtoTable : UnityEngine.ScriptableObject
{
    protected ProtoTable()
    {
    }
}

public class ProtoSet<T> : ProtoTable where T : Proto
{
    protected ProtoSet()
    {
    }

    public T Select(int id)
    {
        return null;
    }
}

public class RecipeProtoSet : ProtoSet<RecipeProto>
{
    private RecipeProtoSet()
    {
    }
}

public static class LDB
{
    public static RecipeProtoSet recipes
    {
        get { return null; }
    }
}

public class ManualBehaviour : UnityEngine.MonoBehaviour
{
    protected ManualBehaviour()
    {
    }

    public bool active { get; private set; }
}

public class UIGame : ManualBehaviour
{
    private UIGame()
    {
    }

    public UITechTree techTree;
    public UIDysonEditor dysonEditor;
    public UIInventoryWindow inventoryWindow;
    public UIReplicatorWindow replicator;
    public UIStatisticsWindow statWindow;
    public UIDashboard dashboard;
}

public class UITechTree : ManualBehaviour
{
    private UITechTree() { }
}

public class UIDysonEditor : ManualBehaviour
{
    private UIDysonEditor() { }
}

public class UIInventoryWindow : ManualBehaviour
{
    private UIInventoryWindow() { }
}

public class UIStatisticsWindow : ManualBehaviour
{
    private UIStatisticsWindow() { }
}

public class UIDashboard : ManualBehaviour
{
    private UIDashboard() { }
}

public class UIReplicatorWindow : ManualBehaviour
{
    private UIReplicatorWindow()
    {
    }

    public UnityEngine.UI.Image recipeBg;
    public UnityEngine.UI.RawImage recipeIcons;
}
