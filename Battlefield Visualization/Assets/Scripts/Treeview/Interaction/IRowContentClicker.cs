namespace Assets.Treeview.Interaction
{
    public interface IRowContentClicker<TSource>
    {
        void Click(RowData<TSource> row);
    }
}