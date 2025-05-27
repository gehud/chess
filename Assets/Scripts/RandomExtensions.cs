using Unity.Mathematics;

namespace Chess
{
    public static class RandomExtensions
    {
        public static ulong NextULong(this Random random)
        {
            var value = random.NextUInt2();
            return value.x << sizeof(uint) | value.y;
        }
    }
}
