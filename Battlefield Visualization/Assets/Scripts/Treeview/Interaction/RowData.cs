namespace Assets.Treeview.Interaction
{
    using Assets.Treeview.Structure;

    using UnityEngine;

    public class RowData<TSource>
    {
        public Rect Location { get; set; }

        public ITreeViewItemWrapper<TSource> Data { get; set; }

        public RowDataType RowDataType { get; set; }
    }
}