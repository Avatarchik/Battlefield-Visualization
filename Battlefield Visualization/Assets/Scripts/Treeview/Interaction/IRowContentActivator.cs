namespace Assets.Treeview.Interaction
{
    public interface IRowContentActivator<TSource>
    {
        void Activate(RowData<TSource> row);
    }
}