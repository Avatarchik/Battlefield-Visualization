namespace Assets.Treeview.Interaction
{
    public class RowExpanderClicker<TSource> : IRowExpanderClicker<TSource>
    {
        public void Click(RowData<TSource> row)
        {
            if (row == null)
            {
                return;
            }

            if (row.Data == null)
            {
                return;
            }
            
            row.Data.IsExpanded = !row.Data.IsExpanded;
        }
    }
}