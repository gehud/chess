namespace Chess
{
    public static class ColorExtensions
    {
        public static Color Opposite(this Color color)
        {
            return color switch
            {
                Color.Black => Color.White,
                Color.White => Color.Black,
                _ => default
            };
        }
    }
}
