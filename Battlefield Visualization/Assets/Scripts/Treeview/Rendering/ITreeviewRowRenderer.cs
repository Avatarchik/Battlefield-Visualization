namespace Assets.Treeview.Rendering
{
    using Assets.Treeview.Structure;

    using UnityEngine;

    public interface ITreeviewRowRenderer<TSource>
    {
        void RenderRow(
            ITreeViewItemWrapper<TSource> item,
            ITreeviewSourceDecoder<TSource> treeviewSourceDecoder,
            EventType currentEventType);
    }
}
