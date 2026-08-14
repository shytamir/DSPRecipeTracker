using System;

namespace DSPRecipeTracker
{
    internal readonly struct DragDelta
    {
        public DragDelta(float horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
        }

        public float Horizontal { get; }

        public float Vertical { get; }
    }

    internal readonly struct ParentBounds
    {
        public ParentBounds(float left, float top, float width, float height)
        {
            if (width < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        public float Left { get; }

        public float Top { get; }

        public float Width { get; }

        public float Height { get; }
    }

    internal readonly struct PanelRectangle
    {
        public PanelRectangle(float left, float top, float width, float height)
        {
            if (width < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        public float Left { get; }

        public float Top { get; }

        public float Width { get; }

        public float Height { get; }

        public PanelRectangle Move(DragDelta delta)
        {
            return new PanelRectangle(Left + delta.Horizontal, Top + delta.Vertical, Width, Height);
        }
    }

    internal static class PanelGeometry
    {
        public const float FixedWidth = 360f;
        public const float FixedHeight = 252f;

        public static PanelRectangle Create(float left, float top)
        {
            return new PanelRectangle(left, top, FixedWidth, FixedHeight);
        }

        public static PanelRectangle MoveAndClamp(
            PanelRectangle panel,
            DragDelta delta,
            ParentBounds parent)
        {
            return Clamp(panel.Move(delta), parent);
        }

        public static PanelRectangle Clamp(PanelRectangle panel, ParentBounds parent)
        {
            var left = ClampAxis(panel.Left, panel.Width, parent.Left, parent.Width);
            var top = ClampAxis(panel.Top, panel.Height, parent.Top, parent.Height);
            return new PanelRectangle(left, top, panel.Width, panel.Height);
        }

        private static float ClampAxis(float position, float panelSize, float parentStart, float parentSize)
        {
            if (panelSize >= parentSize)
            {
                return parentStart;
            }

            return Math.Max(parentStart, Math.Min(position, parentStart + parentSize - panelSize));
        }
    }
}
