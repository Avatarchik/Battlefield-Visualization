namespace Assets.Treeview
{
    public class RenderDisplayContext
    {
        public RenderDisplayContext(bool isSelected, bool isExpanded, bool isActive)
        {
            IsSelected = isSelected;
            IsExpanded = isExpanded;
            IsActive = isActive;
        }

        public bool IsSelected { get; private set; }

        public bool IsExpanded { get; private set; }

        public bool IsActive { get; set; }
    }
}
