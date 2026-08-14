namespace UnityEngine.EventSystems
{
    public abstract class UIBehaviour : UnityEngine.MonoBehaviour
    {
        protected UIBehaviour()
        {
        }
    }
}

namespace UnityEngine.UI
{
    public abstract class Graphic : UnityEngine.EventSystems.UIBehaviour
    {
        protected Graphic()
        {
        }

        public virtual bool raycastTarget
        {
            set { }
        }
    }

    public abstract class MaskableGraphic : Graphic
    {
        protected MaskableGraphic()
        {
        }
    }

    public class Image : MaskableGraphic
    {
        protected Image()
        {
        }

        public UnityEngine.Sprite sprite
        {
            set { }
        }
    }
}
