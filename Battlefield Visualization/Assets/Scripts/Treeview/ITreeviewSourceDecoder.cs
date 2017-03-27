namespace Assets.Treeview
{
    using System;
    using System.Collections;

    public interface ITreeviewSourceDecoder<TSource>
    {
        Func<TSource, IEnumerable> Children { get; }

        Action<TSource, RenderDisplayContext> RenderDisplay { get; }
    }
}