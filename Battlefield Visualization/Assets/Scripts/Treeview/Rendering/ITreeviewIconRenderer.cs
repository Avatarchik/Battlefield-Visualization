namespace Assets.Treeview.Rendering
{
    using Assets.Treeview.Structure;

    using UnityEngine;

    public interface ITreeviewIconRenderer<TSource>
    {
        Vector2 RenderIcon(IconTypes type, Vector2 topleft);

        Vector2 RenderItemIcon(ITreeViewItemWrapper<TSource> item, Vector2 topLeft);
    }
}
