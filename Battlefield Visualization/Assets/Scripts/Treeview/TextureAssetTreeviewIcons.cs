namespace Assets.Treeview
{
    using System;

    using UnityEngine;

    public class TextureAssetTreeviewIcons<TSource> : ITreeviewIcons<TSource>
    {
        private readonly Texture2D _defaultIcon;

        public TextureAssetTreeviewIcons()
        {
            // Note: For texture assets which are not a Power of 2 size (16x16, 32x32 etc) need to be imported
            // with the "Non Power of 2" set to "None"
            BlankIcon = LoadImageFromFile(@"Images/Blank");

            BarDownIcon = LoadImageFromFile(@"Images/Bar");

            BarLIcon = LoadImageFromFile(@"Images/LBar");

            BarTIcon = LoadImageFromFile(@"Images/TBar");

            LMinusIcon = LoadImageFromFile(@"Images/LMinus");

            TMinusIcon = LoadImageFromFile(@"Images/TMinus");

            LPlusIcon = LoadImageFromFile(@"Images/LPlus");

            TPlusIcon = LoadImageFromFile(@"Images/TPlus");

            _defaultIcon = LoadImageFromFile(@"Images/Question");
        }

        public Texture2D BlankIcon { get; private set; }

        public Texture2D BarDownIcon { get; private set; }

        public Texture2D BarLIcon { get; private set; }

        public Texture2D BarTIcon { get; private set; }

        public Texture2D LMinusIcon { get; private set; }

        public Texture2D TMinusIcon { get; private set; }

        public Texture2D LPlusIcon { get; private set; }

        public Texture2D TPlusIcon { get; private set; }

        public virtual Texture2D ItemIcon(TSource item, RenderDisplayContext context)
        {
            return _defaultIcon;
        }
        
        private Texture2D LoadImageFromFile(string path)
        {
            try
            {
                return (Texture2D)Resources.Load(path, typeof(Texture2D));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}