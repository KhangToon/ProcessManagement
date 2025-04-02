using System.Drawing;

namespace ProcessManagement.Commons
{
    public static class ColorBuilder
    {
        public static Color[] CategoricalTwelveColors = new Color[]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Yellow,
            Color.Cyan,
            Color.Magenta,
            Color.Orange,
            Color.Purple,
            Color.Brown,
            Color.Gray,
            Color.Pink,
            Color.Teal
        };

        public static Color ToColor(this Color color)
        {
            // Implement conversion logic if needed
            return color;
        }

        public static string ToRgbString(this Color color)
        {
            return $"rgb({color.R}, {color.G}, {color.B})";
        }
    }
}
