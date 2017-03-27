namespace Assets.Treeview.Structure
{
    using System;

    using Assets.Treeview;

    public class CachingObserver<TSource> : ICachingObserver<TSource>
        where TSource : class
    {
        private readonly IUnityLog _log;
        private TreeViewItemWrapper<TSource> _cachedRoot;
        private object _decodedSource;

        public CachingObserver(IUnityLog log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this._log = log;
        }

        public ITreeViewItemWrapper<TSource> Root(ITreeview<TSource> treeview, ITreeviewSourceDecoder<TSource> sourceDecoder)
        {
            if (treeview == null)
            {
                throw new ArgumentNullException("treeview");
            }

            if (sourceDecoder == null)
            {
                throw new ArgumentNullException("sourceDecoder");
            }

            var itemSourceCopy = treeview.ItemsSource;
            if (itemSourceCopy == null)
            {
                return null;
            }

            if (itemSourceCopy != this._decodedSource)
            {
                // Itemsource on treeview has changed since last cached.
                this._cachedRoot = null;
            }

            if (this._cachedRoot != null)
            {
                return this._cachedRoot;
            }

            var stronglyTypedSource = itemSourceCopy as TSource;
            
            if (stronglyTypedSource == null)
            {
                this._log.LogError("Source is of type {0}, expected type {1}", treeview.ItemsSource.GetType().Name, typeof(TSource).Name);
                return null;
            }

            this._cachedRoot = new TreeViewItemWrapper<TSource>(stronglyTypedSource, sourceDecoder, null, true, treeview);
            this._decodedSource = itemSourceCopy;
            
            return this._cachedRoot;
        }
    }
}