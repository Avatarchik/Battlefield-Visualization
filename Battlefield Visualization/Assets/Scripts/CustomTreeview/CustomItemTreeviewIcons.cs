namespace Assets.CustomTreeview
{
    using System;

    using Assets.CustomTreeview;
    using Assets.Treeview;
    
    using UnityEngine;

    public class CustomItemTreeviewIcons : TextureAssetTreeviewIcons<Treeview_DataModel>
    {
        public override Texture2D ItemIcon(Treeview_DataModel item, RenderDisplayContext context)
        {
            if (item.EntityID < 0)
                return null;
            if (item.IsUnit)
                return this.LoadImageFromFile(string.Format("Images/DamageUnits/{0}", GetTexureID(item.Health)));

            return this.LoadImageFromFile(string.Format("Images/DamageMembers/{0}", item.Health));
        }

        private Texture2D LoadImageFromFile(string path)
        {
            try
            {
                return (Texture2D) Resources.Load(path, typeof(Texture2D));
            }
            catch (Exception)
            {                
                return null;
            }
        }

        public static int GetTexureID(int health)
        {
            if (health < 100 / 6)
                return 3;
            if (health < 100 / 2)
                return 2;
            if (health < 100 * 5 / 6)
                return 1;

            return 0;
        }
    }
}