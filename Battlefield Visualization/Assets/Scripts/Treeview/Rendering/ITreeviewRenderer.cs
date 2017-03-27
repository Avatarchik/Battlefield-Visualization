namespace Assets.Treeview.Rendering
{
    using Assets.Treeview;

    using UnityEngine;

    public interface ITreeviewRenderer<TSource>
    {
        Vector2 Render(ITreeview<TSource> treeview, EventType eventType);
    }
}
