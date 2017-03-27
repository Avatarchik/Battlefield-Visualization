namespace Assets.Treeview
{ 
    using System;


    public class Treeview<TSource> : ITreeview<TSource> where TSource : class
    {
        private TSource activeItem;
        private TSource selectedItem;

        public event EventHandler SelectedItemChanged;

        public event EventHandler ActiveItemChanged;

        public Treeview_DataModel ItemsSource { get; set; }

        public ITreeviewSourceDecoder<TSource> TreeviewSourceDecoder { get; set; }

        public TSource ActiveItem
        {
            get
            {
                return this.activeItem;
            }

            set
            {
                if (value == this.activeItem)
                {
                    return;
                }

                this.activeItem = value;

                if (ActiveItemChanged != null)
                {
                    var eventHandler = ActiveItemChanged;
                    eventHandler(this, new EventArgs());
                }
            }
        }

        public TSource SelectedItem
        {
            get
            {
                return this.selectedItem;
            }

            set
            {
                if (value == this.selectedItem)
                {
                    return;
                }
                
                this.selectedItem = value;

                if (SelectedItemChanged != null)
                {
                    var eventHandler = SelectedItemChanged;
                    eventHandler(this, new EventArgs());
                }
            }
        }
    }
}