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

        public static T Instantiate<T>(T original, Transform parent, bool instantiateInWorldSpace)
            where T : Object
        {
            return null;
        }
    }

    public class Component : Object
    {
        public Component()
        {
        }

        public Transform transform
        {
            get { return null; }
        }

        public GameObject gameObject
        {
            get { return null; }
        }

        public T GetComponent<T>() where T : Component
        {
            return null;
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

        public Transform parent
        {
            get { return null; }
        }

        public int GetSiblingIndex()
        {
            return 0;
        }

        public void SetSiblingIndex(int index)
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

    public struct Color
    {
        public Color(float red, float green, float blue, float alpha)
        {
        }
    }

    public class Material : Object
    {
        public Material(Material source)
        {
        }

        public void SetBuffer(string name, ComputeBuffer value)
        {
        }

        public void SetColor(string name, Color value)
        {
        }
    }

    public sealed class ComputeBuffer
    {
        public ComputeBuffer(int count, int stride)
        {
        }

        public void SetData(Array data)
        {
        }

        public void Release()
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
