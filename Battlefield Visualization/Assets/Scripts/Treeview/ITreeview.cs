namespace Assets.Treeview
{
    using System;

    public interface ITreeview<TSource>
    {
        event EventHandler SelectedItemChanged;

        event EventHandler ActiveItemChanged;

        Treeview_DataModel ItemsSource { get; set; }

        ITreeviewSourceDecoder<TSource> TreeviewSourceDecoder { get; set; }

        TSource ActiveItem { get; set; }

        TSource SelectedItem { get; set; }
    }
}
