namespace Assets.Treeview.Interaction
{
    using System.Collections.Generic;

    using Assets.Treeview.Structure;

    using UnityEngine;

    public interface IRowClickableLocations<TSource>
    {
        void RegisterRowContent(Rect content, ITreeViewItemWrapper<TSource> data);

        void RegisterRowExpander(Rect expander, ITreeViewItemWrapper<TSource> data);

        void RegisterRowIcon(Rect icon, ITreeViewItemWrapper<TSource> data);

        void Reset();

        IList<RowData<TSource>> GetClickableLocations();
    }
}