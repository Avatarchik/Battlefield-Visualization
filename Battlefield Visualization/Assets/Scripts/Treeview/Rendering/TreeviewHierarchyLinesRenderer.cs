namespace Assets.Treeview.Rendering
{
    using System;

    using Assets.Treeview.Structure;

    using UnityEngine;

    public class TreeviewHierarchyLinesRenderer<TSource> : ITreeviewHierarchyLinesRenderer<TSource>
    {
        private readonly ITreeviewIconRenderer<TSource> _iconRenderer;

        public TreeviewHierarchyLinesRenderer(ITreeviewIconRenderer<TSource> iconRenderer)
        {
            if (iconRenderer == null)
            {
                throw new ArgumentNullException("iconRenderer");
            }

            this._iconRenderer = iconRenderer;
        }

        public float Render(ITreeViewItemWrapper<TSource> item, Vector2 lineTopLeft)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (item.Parent == null)
            {
                // Item is the root item, there is no hierarchy to render for this node.
                return 0;
            }

            return this.RenderLevel(item.Parent, lineTopLeft);
        }

        private float RenderLevel(ITreeViewItemWrapper<TSource> item, Vector2 lineTopLeft)
        {
            var xStart = lineTopLeft.x;

            if (item.Parent != null)
            {
                xStart = this.RenderLevel(item.Parent, lineTopLeft);

                var iconType = item.IsLastSibling ? IconTypes.Space : IconTypes.Bar;

                var icon1 = this._iconRenderer.RenderIcon(iconType, new Vector2(xStart, lineTopLeft.y));

                xStart += icon1.x;
            }
            else
            {
                var icon2 = this._iconRenderer.RenderIcon(IconTypes.Space, new Vector2(xStart, lineTopLeft.y));

                xStart += icon2.x;
            }

            return xStart;
        }
    }
}