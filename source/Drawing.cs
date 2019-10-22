using UnityEngine;

namespace Eflatun.WebView
{
    public static class Drawing
    {
        public static int HalfScreenWidth => Screen.width / 2;
        public static int HalfScreenHeight => Screen.height / 2;

        public static Rect ScreenRect => new Rect(Vector2.zero, new Vector2(Screen.width, Screen.height));

        public static (float topMargin, float bottomMargin, float leftMargin, float rightMargin)
            CalculateMarginsFromScreen(this RectTransform uiElement)
        {
            var p = uiElement.GetRectInScreenCoordinates();
            var s = ScreenRect;
            
            var bottomMargin = p.y - s.y;
            var leftMargin = p.x - s.x;
            var topMargin = s.y + s.height - (p.y + p.height);
            var rightMargin = s.x + s.width - (p.x + p.width);

            return (topMargin, bottomMargin, leftMargin, rightMargin);
        }

        public static Rect GetRectInScreenCoordinates(this RectTransform uiElement)
        {
            var worldCorners = new Vector3[4];
            uiElement.GetWorldCorners(worldCorners);
            var result = new Rect(
                worldCorners[0].x,
                worldCorners[0].y,
                worldCorners[2].x - worldCorners[0].x,
                worldCorners[2].y - worldCorners[0].y);
            return result;
        }
    }
}
