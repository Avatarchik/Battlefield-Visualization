namespace Assets.Treeview.Rendering.Gui
{
    using UnityEngine;

    public interface IGuiLayout
    {
        void Label(string text, GUIStyle style);

        Vector2 BeginScrollView(Vector2 scrollPosition);

        void EndScrollView();

        void BeginHorizontal();

        void EndHorizontal();

        void Space(float pixels);

        void BeginArea(Rect area);

        void EndArea();

        Rect GetLastRect();
    }
}
