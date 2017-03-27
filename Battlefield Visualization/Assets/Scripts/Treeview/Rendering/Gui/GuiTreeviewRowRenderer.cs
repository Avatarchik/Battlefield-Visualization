namespace Assets.Treeview.Rendering.Gui
{
    using System;

    using Assets.Treeview.Interaction;
    using Assets.Treeview.Structure;

    using UnityEngine;

    public class GuiTreeviewRowRenderer<TSource> : ITreeviewRowRenderer<TSource>
    {
        private readonly IGuiLayout guiLayout;
        private readonly ITreeviewIconRenderer<TSource> iconRenderer;
        private readonly IRowClickableLocations<TSource> rowClickableLocations;
        private readonly ITreeviewHierarchyLinesRenderer<TSource> hierarchyLinesRenderer;

        public GuiTreeviewRowRenderer(
            IGuiLayout guiLayout, 
            ITreeviewIconRenderer<TSource> iconRenderer, 
            IRowClickableLocations<TSource> rowClickableLocations,
            ITreeviewHierarchyLinesRenderer<TSource> hierarchyLinesRenderer)
        {
            if (guiLayout == null)
            {
                throw new ArgumentNullException("guiLayout");
            }

            if (iconRenderer == null)
            {
                throw new ArgumentNullException("iconRenderer");
            }

            if (rowClickableLocations == null)
            {
                throw new ArgumentNullException("rowClickableLocations");
            }

            if (hierarchyLinesRenderer == null)
            {
                throw new ArgumentNullException("hierarchyLinesRenderer");
            }

            this.guiLayout = guiLayout;
            this.iconRenderer = iconRenderer;
            this.rowClickableLocations = rowClickableLocations;
            this.hierarchyLinesRenderer = hierarchyLinesRenderer;
        }

        public void RenderRow(ITreeViewItemWrapper<TSource> item, ITreeviewSourceDecoder<TSource> treeviewSourceDecoder, EventType currentEventType)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (treeviewSourceDecoder == null)
            {
                throw new ArgumentNullException("treeviewSourceDecoder");
            }

            var gridRect = new Rect(0, 0, 0, 0);

            guiLayout.BeginHorizontal();

            guiLayout.Space(1);

            // In order to use the GUI.DrawTexture which takes absolute position on the page
            // we have to get the relative location of the space we just drew.
            if (currentEventType == EventType.repaint)
            {
                gridRect = guiLayout.GetLastRect();
            }

            var structureX = RenderTreeviewIcons(item, gridRect, currentEventType);

            var iconTopLeft = new Vector2(gridRect.x + structureX, gridRect.y);

            guiLayout.Space(gridRect.x + structureX);

            var itemIconDimensions = iconRenderer.RenderItemIcon(item, iconTopLeft);

            if (itemIconDimensions.x > 0)
            {
                rowClickableLocations.RegisterRowIcon(new Rect(iconTopLeft.x, iconTopLeft.y, itemIconDimensions.x, itemIconDimensions.y), item); 
            }

            guiLayout.Space(itemIconDimensions.x);

            var renderContext = new RenderDisplayContext(item.IsSelected, item.IsExpanded, item.IsActive);

            treeviewSourceDecoder.RenderDisplay(item.Value, renderContext);

            if (currentEventType == EventType.repaint)
            {
                gridRect = guiLayout.GetLastRect();
                rowClickableLocations.RegisterRowContent(gridRect, item);
            }

            guiLayout.EndHorizontal();
        }

        private float RenderTreeviewIcons(ITreeViewItemWrapper<TSource> item, Rect iconSpace, EventType currentEventType)
        {
            var gridx = 0f;
            Vector2 icon;

            if (item.Parent == null)
            {
                var iconType = item.IsExpanded ? IconTypes.LMinus : IconTypes.LPlus;

                // Root item 
                icon = iconRenderer.RenderIcon(iconType, new Vector2(0, iconSpace.y));

                if (currentEventType == EventType.Repaint)
                {
                    // Co-ordinates only accurate during repaint.
                    this.rowClickableLocations.RegisterRowExpander(new Rect(0, iconSpace.y, icon.x, icon.y), item);
                }

                gridx = icon.x;
            }
            else
            {
                // All other items
                gridx = hierarchyLinesRenderer.Render(item, new Vector2(0, iconSpace.y));

                // Item
                if (item.HasChildren)
                {
                    var iconType = item.IsExpanded
                                       ? item.IsLastSibling ? IconTypes.LMinus : IconTypes.TMinus
                                       : item.IsLastSibling ? IconTypes.LPlus : IconTypes.TPlus;

                    icon = iconRenderer.RenderIcon(iconType, new Vector2(gridx, iconSpace.y));

                    if (currentEventType == EventType.Repaint)
                    {
                        // Co-ordinates only accurate during repaint.
                        this.rowClickableLocations.RegisterRowExpander(new Rect(gridx, iconSpace.y, icon.x, icon.y), item);
                    }

                    gridx += icon.x;
                }
                else
                {
                    var iconType = item.IsLastSibling ? IconTypes.LBar : IconTypes.TBar;

                    icon = iconRenderer.RenderIcon(iconType, new Vector2(gridx, iconSpace.y));
                    gridx += icon.x;
                }
            }

            return gridx;
        }        
    }
}