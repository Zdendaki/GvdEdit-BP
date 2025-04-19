using GvdEdit.Models;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using static GvdEdit.DrawingData;

namespace GvdEdit
{
    public partial class GvdForm : Form
    {
        private const int CP_NOCLOSE_BUTTON = 0x200;

        internal bool AllowClose = false;

        private Stopwatch _stopwatch = new();

        private int HourFrom = 0;
        private int HourTo = 12;
        private int MinuteScale = 8;
        private int StationScale = 10;

        public GvdForm()
        {
            InitializeComponent();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        private void GvdForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !AllowClose;
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            _stopwatch.Restart();

            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.Clear(Color.White);

            if (App.Data.Stations.Count > 0)
            {
                DrawHeader(g);
                DrawStations(g);
                DrawGrid(g);
            }

            _stopwatch.Stop();
            g.DrawString(_stopwatch.ElapsedMilliseconds.ToString(), new Font(Verdana, 10), Brushes.Red, 0, 0);
        }

        private void DrawHeader(Graphics g)
        {
            g.DrawString("Nákresný jízdní řád", new Font(Verdana, 24, FontStyle.Bold), SZ_BLUE, 200, 20);
            g.DrawString("Jízdní řád 2025", new Font(Verdana, 20, FontStyle.Bold), SZ_ORGE, 200, 80);
            g.DrawString("302A", new Font(Verdana, 48, FontStyle.Bold), SZ_ORGE, 760, 20);
            g.DrawString($"{HourFrom}-{HourTo}", new Font(Verdana, 30, FontStyle.Regular), Brushes.Black, 1100, 36);
            g.DrawString($"{App.Data.Stations.First().Name} - {App.Data.Stations.Last().Name}", new Font(Verdana, 24, FontStyle.Bold), Brushes.Black, 1300, 20);
            g.DrawString("VARIANTA ZÁKLADNÍ", new Font(Verdana, 20, FontStyle.Bold), SZ_BLUE, 1300, 80);
        }

        private void DrawStations(Graphics g)
        {
            int totalMinutes = (HourTo - HourFrom) * 60;
            int name2X = 500 + (totalMinutes + 10) * MinuteScale;

            float y, lastY;
            y = lastY = 200f;

            float lastkm = -1;

            g.DrawLine(BLACK2, 400, y + 20, 440, y + 20);

            Font km = new Font(Verdana, 12);

            RouteInterlocking interlocking = RouteInterlocking.None;

            foreach (Station station in App.Data.Stations)
            {
                if (lastkm < 0)
                    lastkm = station.Position2;

                int fontSize = station.StationType == StationType.ZST ? 16 : 12;

                y += 60 + Math.Abs(station.Position - lastkm) * StationScale;
                
                station.DrawY = y + 20;

                Pen? pen = interlocking switch
                {
                    RouteInterlocking.Telephone => BLACK3Da,
                    RouteInterlocking.SemiAutomatic => BLACK3,
                    RouteInterlocking.Automatic => BLACK3DaDo,
                    _ => null,
                };
                if (pen is not null)
                    g.DrawLine(pen, 420, lastY, 420, y + 20);

                if (!station.Hidden)
                {
                    FontStyle style = station.StationType == StationType.Other || station.StationType == StationType.AHr ? FontStyle.Italic : FontStyle.Regular;
                    Font font = new Font(Verdana, fontSize, style);

                    if (station.StationBuilding == StationBuilding.Left)
                        g.DrawString("\u25AE", font, Brushes.Black, 20, y);

                    g.DrawString(station.GetPrettyName(true), font, Brushes.Black, 40, y);
                    g.DrawString(station.GetPrettyName(), font, Brushes.Black, name2X + 10, y);

                    string kmString;
                    if (station.Position == station.Position2)
                        kmString = station.Position.ToString("0.0");
                    else
                        kmString = $"{station.Position:0.0}={station.Position2:0.0}";
                    g.DrawString(kmString, km, Brushes.Black, name2X + 380, y);
                }

                lastkm = station.Position2;
                interlocking = station.RouteInterlocking;
                lastY = y + 20;

                if (station.StationType == StationType.ZST)
                    g.DrawLine(BLACK2, 400, y + 20, 440, y + 20);
            }

            g.DrawLine(BLACK2, name2X + 360, 220, name2X + 360, y + 80);
            g.DrawLine(BLACK2, 400, 220, 400, y + 80);
            g.DrawLine(BLACK2, 400, y + 80, 440, y + 80);
            g.DrawLine(BLACK2, 440, 220, 440, y + 80);
        }

        private void DrawGrid(Graphics g)
        {
            int totalMinutes = (HourTo - HourFrom) * 60;
            int width = (totalMinutes + 10) * MinuteScale;

            g.DrawLine(ORANGE4, 500, 220, 500 + width, 220);

            float lastY = 200;
            float startX = 500 + MinuteScale * 5;
            
            foreach (Station station in App.Data.Stations)
            {
                if (station.Hidden)
                    continue;

                bool isZST = station.StationType == StationType.ZST;
                
                g.DrawLine(isZST ? ORANGE2 : ORANGE1, 500, station.DrawY, 500 + width, station.DrawY);
                lastY = station.DrawY;

                for (int x = 0; x < totalMinutes; x++)
                {
                    int deltaY = isZST ? 5 : 3;
                    float dx = startX + x * MinuteScale;

                    if (x % 10 == 0)
                        continue;

                    if (x % 5 == 0)
                        deltaY *= 2;

                    g.DrawLine(ORANGE1, dx, station.DrawY - deltaY, dx, station.DrawY + deltaY);
                }
            }

            for (int x = 10; x < totalMinutes; x += 10)
            {
                Pen pen;
                if (x % 60 == 0)
                    pen = ORANGE2;
                else if (x % 30 == 0)
                    pen = new(SZ_ORGE, 1) { DashStyle = DashStyle.Custom, DashPattern = [20, 6] };
                else
                    pen = ORANGE1;

                float dx = startX + x * MinuteScale;
                g.DrawLine(pen, dx, 220, dx, lastY + 60);
            }

            for (int h = HourFrom; h <= HourTo; h++)
            {
                Font font = new Font(Verdana, 24);
                string str = h.ToString();
                SizeF size = g.MeasureString(str, font);
                float x = startX + (h * 60) * MinuteScale - size.Width / 2f;
                g.DrawString(str, font, Brushes.Black, x, 220 - size.Height * 1.5f);
                g.DrawString(str, font, Brushes.Black, x, lastY + 60 + size.Height * 0.5f);
            }

            g.DrawLine(ORANGE4, 500, lastY + 60, 500 + width, lastY + 60);

            g.DrawLine(ORANGE4, 500, 220, 500, lastY + 60);
            g.DrawLine(ORANGE4, 500 + width, 220, 500 + width, lastY + 60);
            g.DrawLine(ORANGE4, 500 + MinuteScale * 5, 220, 500 + MinuteScale * 5, lastY + 60);
            g.DrawLine(ORANGE4, 500 + width - MinuteScale * 5, 220, 500 + width - MinuteScale * 5, lastY + 60);
        }

        private void Canvas_Click(object sender, System.EventArgs e)
        {
            Canvas.Invalidate();
        }
    }

    internal static class DrawingData
    {
        public static readonly FontFamily Verdana = new FontFamily("Verdana");

        public static readonly SolidBrush SZ_BLUE = new SolidBrush(Color.FromArgb(0, 43, 89));

        public static readonly SolidBrush SZ_ORGE = new SolidBrush(Color.FromArgb(255, 82, 0));

        public static readonly Pen BLACK1 = new(Brushes.Black, 1);

        public static readonly Pen BLACK2 = new(Brushes.Black, 2);

        public static readonly Pen BLACK3 = new(Brushes.Black, 3);

        public static readonly Pen BLACK4 = new(Brushes.Black, 4);

        public static readonly Pen BLACK3Da = new(Brushes.Black, 3) { DashStyle = DashStyle.Dash };

        public static readonly Pen BLACK3DaDo = new(Brushes.Black, 3) { DashStyle = DashStyle.DashDot };

        public static readonly Pen ORANGE1 = new(SZ_ORGE, 1);

        public static readonly Pen ORANGE1Da = new(SZ_ORGE, 1) { DashStyle = DashStyle.Custom, DashPattern = [20, 5] };

        public static readonly Pen ORANGE2 = new(SZ_ORGE, 2);

        public static readonly Pen ORANGE4 = new(SZ_ORGE, 4);
    }
}
