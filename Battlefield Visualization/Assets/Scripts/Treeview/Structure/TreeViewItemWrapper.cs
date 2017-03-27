namespace Assets.Treeview.Structure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Assets.Treeview;

    public class TreeViewItemWrapper<TSource> : ITreeViewItemWrapper<TSource> where TSource : class
    {
        private readonly TSource _dataItem;
        private readonly ITreeviewSourceDecoder<TSource> _decoder;
        private readonly ITreeview<TSource> treeview;
        private readonly ITreeViewItemWrapper<TSource> _parent;
        private bool _isLastSibling;

        private List<ITreeViewItemWrapper<TSource>> _childCache;

        public TreeViewItemWrapper(
            TSource dataItem,
            ITreeviewSourceDecoder<TSource> decoder,
            ITreeViewItemWrapper<TSource> parent,
            bool isLastSibling,
            ITreeview<TSource> treeview)
        {
            if (dataItem == null)
            {
                throw new ArgumentNullException("dataItem");
            }

            if (decoder == null)
            {
                throw new ArgumentNullException("decoder");
            }

            if (treeview == null)
            {
                throw new ArgumentNullException("treeview");
            }

            this._dataItem = dataItem;
            this._decoder = decoder;
            this._parent = parent;
            this._isLastSibling = isLastSibling;
            this.treeview = treeview;
        }

        public bool IsExpanded { get; set; }

        public bool IsSelected
        {
            get
            {
                return this.Value.Equals(this.treeview.SelectedItem);
            }
        }

        public bool IsActive
        {
            get
            {
                return this.Value.Equals(this.treeview.ActiveItem);
            }
        }

        public bool HasChildren
        {
            get { return this.Children.Any(); }
        }

        public bool IsLastSibling
        {
            get
            {
                if (this.Parent == null)
                {
                    return true;
                }

                return this._isLastSibling;
            }
        }

        public bool IsVisible
        {
            get
            {
                if (this.Parent == null)
                {
                    return true;
                }

                return this.Parent.IsVisible && this.Parent.IsExpanded;
            }
        }

        public ITreeViewItemWrapper<TSource> Parent
        {
            get
            {
                return this._parent;
            }
        }

        public ITreeview<TSource> Treeview
        {
            get
            {
                return this.treeview;
            }
        }

        public IEnumerable<ITreeViewItemWrapper<TSource>> Children
        {
            get
            {
                var children = this._decoder.Children(this._dataItem);

                if (children == null)
                {
                    yield break;
                }

                if (this._childCache == null)
                {
                    this._childCache = new List<ITreeViewItemWrapper<TSource>>();

                    var childEnumerator = children.GetEnumerator();
                    var anotherChild = childEnumerator.MoveNext();

                    while (anotherChild)
                    {
                        var child = (TSource)childEnumerator.Current;

                        anotherChild = childEnumerator.MoveNext();

                        var wrappedChild = new TreeViewItemWrapper<TSource>(child, this._decoder, this, !anotherChild, this.treeview);
                        this._childCache.Add(wrappedChild);
                    }
                }

                foreach (var cachedChild in this._childCache)
                {
                    yield return cachedChild;
                }
            }
        }

        public TSource Value
        {
            get
            {
                return this._dataItem;
            }
        }
    }
}