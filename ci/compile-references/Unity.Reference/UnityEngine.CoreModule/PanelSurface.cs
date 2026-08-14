using System;

namespace UnityEngine.Events
{
    public delegate void UnityAction<in T0>(T0 arg0);

    public class UnityEvent<T0>
    {
        public UnityEvent()
        {
        }

        public void AddListener(UnityAction<T0> call)
        {
        }

        public void RemoveListener(UnityAction<T0> call)
        {
        }
    }
}

namespace UnityEngine
{
    public class Object
    {
        public Object()
        {
        }

        public static void Destroy(Object value)
        {
        }
    }

    public class Component : Object
    {
        public Component()
        {
        }
    }

    public class Behaviour : Component
    {
        public Behaviour()
        {
        }
    }

    public class MonoBehaviour : Behaviour
    {
        public MonoBehaviour()
        {
        }
    }

    public class Transform : Component
    {
        protected Transform()
        {
        }

        public void SetParent(Transform parent, bool worldPositionStays)
        {
        }
    }

    public sealed class RectTransform : Transform
    {
        public RectTransform()
        {
        }

        public Vector2 anchorMin
        {
            set { }
        }

        public Vector2 anchorMax
        {
            set { }
        }

        public Vector2 anchoredPosition
        {
            set { }
        }

        public Vector2 sizeDelta
        {
            set { }
        }

        public Vector2 pivot
        {
            set { }
        }
    }

    public sealed class GameObject : Object
    {
        public GameObject(string name)
        {
        }

        public Component AddComponent(Type componentType)
        {
            return null;
        }

        public void SetActive(bool value)
        {
        }
    }

    public struct Vector2
    {
        public Vector2(float x, float y)
        {
        }
    }

    public sealed class Sprite : Object
    {
        private Sprite()
        {
        }
    }
}
