namespace Assets.Treeview.Rendering.Gui
{
    using UnityEngine;

    public class GuiWrapper : IGui
    {
        public void DrawTexture(Rect position, Texture2D texture)
        {
            GUI.DrawTexture(position, texture);
        }
    }
}
