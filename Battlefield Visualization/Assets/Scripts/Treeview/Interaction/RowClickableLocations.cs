namespace Assets.Treeview.Interaction
{
    using System.Collections.Generic;

    using Assets.Treeview.Structure;

    using UnityEngine;

    public class RowClickableLocations<TSource> : IRowClickableLocations<TSource> where TSource : class
    {
        private readonly IList<RowData<TSource>> locations;

        public RowClickableLocations()
        {
            this.locations = new List<RowData<TSource>>();
        }

        public void RegisterRowContent(Rect content, ITreeViewItemWrapper<TSource> data)
        {
            this.locations.Add(new RowData<TSource> { Data = data, Location = content, RowDataType = RowDataType.Content });
        }

        public void RegisterRowExpander(Rect expander, ITreeViewItemWrapper<TSource> data)
        {
            this.locations.Add(new RowData<TSource> { Data = data, Location = expander, RowDataType = RowDataType.Expander });
        }

        public void RegisterRowIcon(Rect icon, ITreeViewItemWrapper<TSource> data)
        {
            this.locations.Add(new RowData<TSource> { Data = data, Location = icon, RowDataType = RowDataType.Icon });
        }

        public void Reset()
        {
            this.locations.Clear();
        }

        public IList<RowData<TSource>> GetClickableLocations()
        {
            return this.locations;
        }
    }
}