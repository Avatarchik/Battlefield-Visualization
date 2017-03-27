namespace Assets.CustomTreeview
{
    using UnityEngine;

    public static class ContentRenderingColours
    {
        //public static Color NormalText = new Color(Conversion * 255, Conversion * 182, Conversion * 110);
        public static Color NormalText = new Color(Conversion * 255, Conversion * 255, Conversion * 255);
        public static Color SelectedText = new Color(Conversion * 179, Conversion * 56, Conversion * 45);

        private const float Conversion = 1f / 255f;
    }
}
