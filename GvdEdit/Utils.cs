using System;
using System.Drawing;

namespace GvdEdit
{
    static class Utils
    {
        public static TimeSpan RoundUp(this TimeSpan time, int seconds)
        {
            long ticks = time.Ticks + (seconds * TimeSpan.TicksPerSecond);
            return new TimeSpan(ticks - (ticks % TimeSpan.TicksPerSecond));
        }

        public static bool ContainsX(this RectangleF rectangle, float x)
        {
            return x >= rectangle.Left && x <= rectangle.Right;
        }

        public static bool ContainsY(this RectangleF rectangle, float y)
        {
            return y >= rectangle.Top && y <= rectangle.Bottom;
        }
    }
}
