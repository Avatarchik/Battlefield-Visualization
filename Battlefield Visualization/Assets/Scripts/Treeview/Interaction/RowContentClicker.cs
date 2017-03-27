namespace Assets.Treeview.Interaction
{
    public class RowContentClicker<TSource> : IRowContentClicker<TSource>
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
            
            row.Data.Treeview.SelectedItem = row.Data.Value;
        }
    }
}