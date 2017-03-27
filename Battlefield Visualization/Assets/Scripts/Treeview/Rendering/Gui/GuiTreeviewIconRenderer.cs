namespace Assets.Treeview.Rendering.Gui
{
    using System;

    using Assets.Treeview;
    using Assets.Treeview.Structure;

    using UnityEngine;

    public class GuiTreeviewIconRenderer<TSource> : ITreeviewIconRenderer<TSource>
    {
        private readonly IGui _gui;
        private readonly ITreeviewIcons<TSource> _treeviewIcons;

        public GuiTreeviewIconRenderer(
            IGui gui,
            ITreeviewIcons<TSource> treeviewIcons)
        {
            if (gui == null)
            {
                throw new ArgumentNullException("gui");
            }

            if (treeviewIcons == null)
            {
                throw new ArgumentNullException("treeviewIcons");
            }
            
            this._gui = gui;
            this._treeviewIcons = treeviewIcons;
        }

        public Vector2 RenderIcon(IconTypes type, Vector2 topleft)
        {
            var iconRect = this.InternalRenderIcon(type, topleft, default(ITreeViewItemWrapper<TSource>));
            return new Vector2(iconRect.width, iconRect.height);
        }

        public Vector2 RenderItemIcon(ITreeViewItemWrapper<TSource> item, Vector2 topLeft)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            var iconRect = this.InternalRenderIcon(IconTypes.Item, topLeft, item);
            return new Vector2(iconRect.width, iconRect.height);
        }

        private Rect InternalRenderIcon(IconTypes type, Vector2 topleft, ITreeViewItemWrapper<TSource> item)
        {
            Texture2D icon;

            switch (type)
            {
                case IconTypes.Bar:
                    icon = this._treeviewIcons.BarDownIcon;
                    break;
                case IconTypes.LBar:
                    icon = this._treeviewIcons.BarLIcon;
                    break;
                case IconTypes.LMinus:
                    icon = this._treeviewIcons.LMinusIcon;
                    break;
                case IconTypes.LPlus:
                    icon = this._treeviewIcons.LPlusIcon;
                    break;
                case IconTypes.TMinus:
                    icon = this._treeviewIcons.TMinusIcon;
                    break;
                case IconTypes.TPlus:
                    icon = this._treeviewIcons.TPlusIcon;
                    break;
                case IconTypes.Space:
                    icon = this._treeviewIcons.BlankIcon;
                    break;
                case IconTypes.TBar:
                    icon = this._treeviewIcons.BarTIcon;
                    break;
                case IconTypes.Item:
                    if (item == null)
                    {
                        // in situations where RenderIcon is used to render the Itemicon.
                        return new Rect(topleft.x, topleft.y, 0, 0);
                    }

                    icon = this._treeviewIcons.ItemIcon(item.Value, new RenderDisplayContext(item.IsSelected, item.IsExpanded, item.IsActive));

                    // For blank icons, null is passed.
                    if (icon == null)
                    {
                        return new Rect(topleft.x, topleft.y, 0, 0);
                    }

                    break;
                default:
                    throw new Exception(string.Format("Unknown icon type: {0}", type));
            }
            
            if (icon == null)
            {
                throw new NullReferenceException(string.Format("Icon {0} is null.", type));
            }

            var iconRect = new Rect(topleft.x, topleft.y, icon.width, icon.height);

            this._gui.DrawTexture(iconRect, icon);

            return iconRect;
        }
    }
}