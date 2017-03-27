namespace Assets.Treeview
{
    using UnityEngine;

    public interface ITreeviewIcons<TSource>
    {
        Texture2D BlankIcon { get; }

        Texture2D BarDownIcon { get; }

        Texture2D BarLIcon { get; }

        Texture2D BarTIcon { get; }

        Texture2D LMinusIcon { get; }

        Texture2D TMinusIcon { get; }

        Texture2D LPlusIcon { get; }

        Texture2D TPlusIcon { get; }

        Texture2D ItemIcon(TSource item, RenderDisplayContext context);
    }
}
