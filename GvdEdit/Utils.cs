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

        /// <summary>
        /// Zaokrouhlí TimeSpan nahoru na nejbližší půlminutu (30 sekund).
        /// </summary>
        /// <param name="ts">Vstupní TimeSpan.</param>
        /// <returns>TimeSpan zaokrouhlený nahoru na nejbližší půlminutu.</returns>
        /// <remarks>Například: 25s -> 30s, 31s -> 1m, 45s -> 1m, 1m10s -> 1m30s.</remarks>
        public static TimeSpan RoundUpToHalfMinute(this TimeSpan ts)
        {
            // Interval pro zaokrouhlení v sekundách (půl minuty)
            const double intervalSeconds = 30.0;

            // Získání celkového počtu sekund ze vstupního TimeSpan
            double totalSeconds = ts.TotalSeconds;

            // Pokud je vstupní čas přesně nula, vrátíme nulu
            if (totalSeconds == 0)
            {
                return TimeSpan.Zero;
            }

            // Vypočítáme počet 30sekundových intervalů.
            // Použijeme Math.Ceiling pro zaokrouhlení nahoru (kladným směrem).
            // Např. 25s / 30 = 0.83 => Ceiling = 1
            // Např. 31s / 30 = 1.03 => Ceiling = 2
            // Např. 30s / 30 = 1.00 => Ceiling = 1
            double intervals = Math.Ceiling(totalSeconds / intervalSeconds);

            // Vynásobíme počet intervalů délkou intervalu (30s), abychom dostali
            // celkový počet sekund v zaokrouhleném čase.
            double roundedTotalSeconds = intervals * intervalSeconds;

            // Vrátíme nový TimeSpan vytvořený z vypočítaných sekund.
            return TimeSpan.FromSeconds(roundedTotalSeconds);
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
