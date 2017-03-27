namespace Assets.Treeview.Structure
{
    public interface ICachingObserver<TSource>
        where TSource : class
    {
        ITreeViewItemWrapper<TSource> Root(ITreeview<TSource> treeview, ITreeviewSourceDecoder<TSource> sourceDecoder);
    }
}