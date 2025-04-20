using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Najde n bodů ležících na úsečce mezi p1 a p2 (nezahrnuje p1 a p2).
        /// Těchto n bodů rozděluje úsečku spolu s krajními body p1 a p2
        /// na (n + 1) stejně dlouhých segmentů.
        /// </summary>
        /// <param name="p1">Počáteční bod úsečky.</param>
        /// <param name="p2">Koncový bod úsečky.</param>
        /// <param name="n">Počet bodů k nalezení mezi p1 a p2 (musí být >= 0).</param>
        /// <returns>
        /// Seznam n bodů mezi p1 a p2. Pokud n=0, vrací prázdný seznam.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Pokud je n menší než 0.</exception>
        public static List<PointF> GetPointsOnLine(PointF p1, PointF p2, int n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "Počet bodů (n) nesmí být záporný.");
            }

            var points = new List<PointF>(n);

            if (n == 0)
            {
                return points;
            }

            // Vzorec pro t_i = (2*i - 1) / (2*n), kde i jde od 1 do n.
            // Vypočítáme body pomocí lineární interpolace: P = P1 + t * (P2 - P1)
            for (int i = 1; i <= n; i++) // i jde od 1 do n
            {
                // Vypočteme interpolační faktor t pro i-tý bod
                // Používáme float literály pro přesnost
                float t = (2.0f * i - 1.0f) / (2.0f * n);

                // Vypočítáme souřadnice nového bodu (pomocí float)
                float x = p1.X + t * (p2.X - p1.X);
                float y = p1.Y + t * (p2.Y - p1.Y);

                points.Add(new PointF(x, y));
            }

            return points;
        }
    }
}
