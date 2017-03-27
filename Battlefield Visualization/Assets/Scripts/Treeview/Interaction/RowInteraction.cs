namespace Assets.Treeview.Interaction
{
    using System;

    using UnityEngine;

    public class RowInteraction<TSource> : IRowInteraction
    {
        private readonly IRowClickableLocations<TSource> clickableLocations;

        private readonly IRowExpanderClicker<TSource> rowExpanderClicker;

        private readonly IRowContentClicker<TSource> rowContentClicker;

        private readonly IRowContentActivator<TSource> rowContentActivator;

        public RowInteraction(
            IRowClickableLocations<TSource> clickableLocations,
            IRowExpanderClicker<TSource> rowExpanderClicker,
            IRowContentClicker<TSource> rowContentClicker,
            IRowContentActivator<TSource> rowContentActivator)
        {
            if (clickableLocations == null)
            {
                throw new ArgumentNullException("clickableLocations");
            }

            if (rowExpanderClicker == null)
            {
                throw new ArgumentNullException("rowExpanderClicker");
            }

            if (rowContentClicker == null)
            {
                throw new ArgumentNullException("rowContentClicker");
            }

            if (rowContentActivator == null)
            {
                throw new ArgumentNullException("rowContentActivator");
            }

            this.clickableLocations = clickableLocations;
            this.rowExpanderClicker = rowExpanderClicker;
            this.rowContentClicker = rowContentClicker;
            this.rowContentActivator = rowContentActivator;
        }

        public void PerformClickInteraction(Vector2 location)
        {
            foreach (var registered in this.clickableLocations.GetClickableLocations())
            {
                if (registered.Location.Contains(location))
                {
                    switch (registered.RowDataType)
                    {
                        case RowDataType.Expander:
                            rowExpanderClicker.Click(registered);
                            return;

                        case RowDataType.Content:
                        case RowDataType.Icon:
                        default:
                            rowContentClicker.Click(registered);
                            return;
                    }
                }
            }
        }

        public void PerformMouseoverInteraction(Vector2 location)
        {
            foreach (var registered in this.clickableLocations.GetClickableLocations())
            {
                if (registered.Location.Contains(location))
                {
                    switch (registered.RowDataType)
                    {
                        case RowDataType.Content:                            
                        case RowDataType.Icon:
                            rowContentActivator.Activate(registered);
                            return;
                    }

                    return;
                }
            }
        }        
    }
}