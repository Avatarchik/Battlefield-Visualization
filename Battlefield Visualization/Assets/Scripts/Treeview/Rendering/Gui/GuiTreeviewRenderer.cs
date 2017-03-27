namespace Assets.Treeview.Rendering.Gui
{
    using System;

    using Assets.Treeview;
    using Assets.Treeview.Interaction;
    using Assets.Treeview.Structure;

    using UnityEngine;

    public class GuiTreeviewRenderer<TSource> : ITreeviewRenderer<TSource> where TSource : class
    {
        private readonly IGuiLayout guiLayout;
        private readonly IRowClickableLocations<TSource> rowClickableLocations;
        private readonly ICachingObserver<TSource> itemCache;
        private readonly ITreeviewRowRenderer<TSource> treeviewRowRenderer;

        private Vector2 _scrollPosition;

        public GuiTreeviewRenderer(
            IGuiLayout guiLayout,
            IRowClickableLocations<TSource> rowClickableLocations,
            ICachingObserver<TSource> itemCache,
            ITreeviewRowRenderer<TSource> treeviewRowRenderer)
        {
            if (guiLayout == null)
            {
                throw new ArgumentNullException("guiLayout");
            }

            if (rowClickableLocations == null)
            {
                throw new ArgumentNullException("rowClickableLocations");
            }

            if (itemCache == null)
            {
                throw new ArgumentNullException("itemCache");
            }

            if (treeviewRowRenderer == null)
            {
                throw new ArgumentNullException("treeviewRowRenderer");
            }

            this.guiLayout = guiLayout;
            this.rowClickableLocations = rowClickableLocations;
            this.itemCache = itemCache;
            this.treeviewRowRenderer = treeviewRowRenderer;

            this.itemCache = itemCache;
        }

        public Vector2 Render(ITreeview<TSource> treeview, EventType eventType)
        {
            if (treeview == null)
            {
                throw new ArgumentNullException("treeview");
            }

            if (treeview.ItemsSource == null)
            {
                // Nothing to render
                return _scrollPosition;
            }

            if (treeview.TreeviewSourceDecoder == null)
            {
                // Cannot render as cannot decode.
                return _scrollPosition;
            }

            if (eventType == EventType.Repaint)
            {
                // Only reset before the click locations are due to be refreshed.
                rowClickableLocations.Reset();
            }

            var localDecoder = treeview.TreeviewSourceDecoder;

            _scrollPosition = guiLayout.BeginScrollView(_scrollPosition);

            var root = itemCache.Root(treeview, localDecoder);

            // render root
            treeviewRowRenderer.RenderRow(root, localDecoder, eventType);

            if (root.IsExpanded)
            {
                RenderChildren(root, localDecoder, eventType);
            }

            guiLayout.EndScrollView();

            return _scrollPosition;
        }

        private void RenderChildren(ITreeViewItemWrapper<TSource> parent, ITreeviewSourceDecoder<TSource> treeviewSourceDecoder, EventType currentEventType)
        {
            foreach (var child in parent.Children)
            {
                treeviewRowRenderer.RenderRow(child, treeviewSourceDecoder, currentEventType);

                if (child.IsExpanded)
                {
                    RenderChildren(child, treeviewSourceDecoder, currentEventType);
                }
            }
        }        
    }
}