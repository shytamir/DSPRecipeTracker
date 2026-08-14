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
    public UIGameMenu gameMenu;
}

public class UIRoot : ManualBehaviour
{
    private UIRoot()
    {
    }

    public static UIRoot instance { get { return null; } }
    public UIGame uiGame;
}

public class UIGameMenu : ManualBehaviour
{
    private UIGameMenu()
    {
    }

    public UnityEngine.UI.Button button3;
    public UnityEngine.UI.Button buttonS;
}

public class UIButton : UnityEngine.MonoBehaviour
{
    private UIButton()
    {
    }

    public enum ItemTipType
    {
        None,
        Item,
        Recipe,
        Other,
        IgnoreIncPoint
    }

    public struct TipSettings
    {
        public int itemId;
        public int itemCount;
        public int itemInc;
        public UnityEngine.Sprite tipSprite;
        public string tipTitle;
        public string tipText;
        public ItemTipType type;
    }

    public TipSettings tips;
    public string tipTitleFormatString;
    public string tipTextFormatString;
}

public class Localizer : UnityEngine.MonoBehaviour
{
    private Localizer()
    {
    }
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
