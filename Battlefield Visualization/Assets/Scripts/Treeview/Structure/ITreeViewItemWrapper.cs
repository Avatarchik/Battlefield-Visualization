namespace Assets.Treeview.Structure
{
    using System.Collections.Generic;

    using Assets.Treeview;

    public interface ITreeViewItemWrapper<TSource>
    {
        bool IsExpanded { get; set; }

        bool IsSelected { get; }
        
        bool HasChildren { get; }

        bool IsLastSibling { get; }

        bool IsVisible { get; }

        bool IsActive { get; }

        ITreeViewItemWrapper<TSource> Parent { get; }

        ITreeview<TSource> Treeview { get; }
        
        IEnumerable<ITreeViewItemWrapper<TSource>> Children { get; }
        
        TSource Value { get; }
    }
}