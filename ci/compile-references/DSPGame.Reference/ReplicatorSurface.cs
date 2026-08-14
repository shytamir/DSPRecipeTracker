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
}

public class UIReplicatorWindow
{
    private UIReplicatorWindow()
    {
    }

    public UnityEngine.UI.Image recipeBg;
    public UnityEngine.UI.RawImage recipeIcons;
}
