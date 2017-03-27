namespace Assets.Treeview.Interaction
{
    using UnityEngine;

    public interface IRowInteraction
    {
        void PerformClickInteraction(Vector2 location);

        void PerformMouseoverInteraction(Vector2 location);
    }
}
