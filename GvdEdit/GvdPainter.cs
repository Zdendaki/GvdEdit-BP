using GvdEdit.Models;
using SkiaSharp;
using System;
using System.Diagnostics;

namespace GvdEdit
{
    internal class GvdPainter : IDisposable
    {
        private static readonly SKColor SZ_BLUE = new SKColor(0, 43, 89);
        private static readonly SKColor SZ_ORGE = new SKColor(255, 82, 0);

        private readonly SKTypeface _verdana = SKTypeface.FromFamilyName("Verdana");
        private readonly SKTypeface _verdanaBold = SKTypeface.FromFamilyName("Verdana", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        private readonly SKTypeface _verdanaItalic = SKTypeface.FromFamilyName("Verdana", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic);
        private readonly SKPaint _textPaint;
        private readonly SKFont _text;

        Stopwatch sw = new();

        public GvdPainter()
        {
            _textPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 0,
                Style = SKPaintStyle.Fill
            };

            _text = new SKFont
            {
                Typeface = _verdana,
                Subpixel = true,
                Edging = SKFontEdging.Antialias,
                Hinting = SKFontHinting.Full
            };
        }

        public void DrawGvd(SKCanvas canvas)
        {
            canvas.Clear(SKColors.White);
            sw.Reset();
            sw.Start();

            //canvas.RotateDegrees(45);
            DrawText(canvas, 24, "Nákresný jízdní řád", 10, 10, SZ_BLUE, _verdanaBold);
            DrawText(canvas, 24, "Jízdní řád 2025", 10, 38, SZ_ORGE, _verdanaBold);
            DrawText(canvas, 48, "302A", 320, 10, SZ_ORGE, _verdanaBold);
            //canvas.Restore();

            DrawGrid(canvas);

            DrawText(canvas, 12, sw.ElapsedMilliseconds.ToString(), 0, 0, SKColors.Red, _verdana);

            sw.Stop();
        }

        private void DrawGrid(SKCanvas canvas)
        {
            float y = 80f;
            float lastkm = -1;

            foreach (Station station in App.Data.Stations)
            {
                if (lastkm < 0)
                    lastkm = station.Position2;

                int fontSize = station.StationType == StationType.ZST ? 20 : 16;

                y += fontSize + Math.Abs(station.Position - lastkm) * 20;

                if (!station.Hidden)
                {
                    SKTypeface typeface = station.StationType == StationType.Other || station.StationType == StationType.AHr ? _verdanaItalic : _verdana;
                    DrawText(canvas, fontSize, station.GetPrettyName(), 20, y, SKColors.Black, typeface);
                }

                lastkm = station.Position2;
            }
        }

        private void DrawText(SKCanvas canvas, float size, string text, float x, float y, SKColor color, SKTypeface typeface)
        {
            _text.Typeface = typeface;
            _text.Size = size;
            _textPaint.Color = color;

            canvas.DrawText(text, x, y + size, _text, _textPaint);
        }

        public void Dispose()
        {
            _textPaint.Dispose();
        }
    }
}
