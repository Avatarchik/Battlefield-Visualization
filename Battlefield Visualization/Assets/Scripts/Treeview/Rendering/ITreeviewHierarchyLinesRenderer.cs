namespace Assets.Treeview.Rendering
{
    using Assets.Treeview.Structure;

    using UnityEngine;

    public interface ITreeviewHierarchyLinesRenderer<TSource>
    {
        float Render(ITreeViewItemWrapper<TSource> item, Vector2 lineTopLeft);
    }
}
