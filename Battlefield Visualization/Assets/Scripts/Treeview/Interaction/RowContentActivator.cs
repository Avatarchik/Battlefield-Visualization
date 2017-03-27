namespace Assets.Treeview.Interaction
{
    public class RowContentActivator<TSource> : IRowContentActivator<TSource>
    {
        public void Activate(RowData<TSource> row)
        {
            if (row == null)
            {
                return;
            }

            if (row.Data == null)
            {
                return;
            }
            
            row.Data.Treeview.ActiveItem = row.Data.Value;
        }
    }
}