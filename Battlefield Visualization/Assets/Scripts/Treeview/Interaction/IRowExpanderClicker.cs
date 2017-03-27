namespace Assets.Treeview.Interaction
{
    public interface IRowExpanderClicker<TSource>
    {
        void Click(RowData<TSource> row);
    }
}