namespace Chess
{
    public static class FigureExtensions
    {
        public static bool IsSliding(this Figure figure)
        {
            return figure switch
            {
                Figure.Queen or Figure.Rook or Figure.Bishop => true,
                _ => false,
            };
        }
    }
}
