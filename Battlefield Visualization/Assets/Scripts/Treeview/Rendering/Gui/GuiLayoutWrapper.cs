namespace Assets.Treeview.Rendering.Gui
{
    using UnityEngine;

    public class GuiLayoutWrapper : IGuiLayout
    {
        public void Label(string text, GUIStyle style)
        {
            GUILayout.Label(text, style);
        }

        public Vector2 BeginScrollView(Vector2 scrollPosition)
        {
            return GUILayout.BeginScrollView(scrollPosition);
        }

        public void EndScrollView()
        {
            GUILayout.EndScrollView();
        }

        public void BeginHorizontal()
        {
            GUILayout.BeginHorizontal();
        }

        public void EndHorizontal()
        {
            GUILayout.EndHorizontal();
        }

        public void Space(float pixels)
        {
            GUILayout.Space(pixels);
        }

        public void BeginArea(Rect area)
        {
            GUILayout.BeginArea(area);
        }

        public void EndArea()
        {
            GUILayout.EndArea();
        }

        public Rect GetLastRect()
        {
            return GUILayoutUtility.GetLastRect();
        }
    }
}