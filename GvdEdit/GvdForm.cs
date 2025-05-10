using GvdEdit.Models;
using SvgNet;
using SvgNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static GvdEdit.DrawingData;

namespace GvdEdit
{
    public partial class GvdForm : Form
    {
        private const int CP_NOCLOSE_BUTTON = 0x200;

        private const int STATION_OFFSET = 40;
        private const int LINE_OFFSET = 20;
        private const int OFFSET_X = 500;
        private const int OFFSET_Y = 220;

        internal bool AllowClose = false;

        private Stopwatch _stopwatch = new();
        private Point? _lastDragPoint = null;

        private int HourFrom => (int)HourStart.Value;

        private int HourTo => (int)HourEnd.Value;

        private int MinuteScale => (int)ScaleX.Value;

        private int StationScale => (int)ScaleY.Value;

        private int NumberFrequency => (int)TrainNumberFrequency.Value;

        public GvdForm()
        {
            InitializeComponent();

            DoubleBuffered = true;
        }

        internal Size ComputeSize()
        {
            if (App.Data.Stations.Count == 0)
                return Size.Empty;

            int totalMinutes = (HourTo - HourFrom) * 60;
            int name2X = OFFSET_X + (totalMinutes + 10) * MinuteScale;

            int width = name2X + 540;
            float height = OFFSET_Y;
            float lastkm = -1;

            foreach (Station station in App.Data.Stations)
            {
                if (lastkm < 0)
                    lastkm = station.Position2;

                height += STATION_OFFSET + Math.Abs(station.Position - lastkm) * StationScale;
                lastkm = station.Position2;
            }

            height += 200;

            return new(width, (int)height);
        }

        public void UpdateSize()
        {
            if (Canvas.InvokeRequired)
                Canvas.Invoke(UpdateSizeInternal);
            else
                UpdateSizeInternal();
        }

        private void UpdateSizeInternal()
        {
            Canvas.Size = ComputeSize();
            ExportButton.Enabled = Canvas.Size != Size.Empty;
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
            _stopwatch.Restart();

            Draw(new GdiGraphics(e.Graphics));

            _stopwatch.Stop();
            e.Graphics.DrawString(_stopwatch.ElapsedMilliseconds.ToString(), new Font(Verdana, 10, FontStyle.Bold), Brushes.Red, 0, 0);
        }

        private void Draw(IGraphics g, bool export = false)
        {
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.Clear(Color.White);

            if (App.Data.Stations.Count == 0)
                return;

            DrawHeader(g);
            DrawStations(g);
            DrawGrid(g, export);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            DrawTrains(g, export);
        }

        private void DrawHeader(IGraphics g)
        {
            g.DrawString("Nákresný jízdní řád", new Font(Verdana, 24, FontStyle.Bold), SZ_BLUE, 200, 20);
            g.DrawString(App.Data.TimetableName, new Font(Verdana, 20, FontStyle.Bold), SZ_ORGE, 200, 80);
            g.DrawString(App.Data.Route, new Font(Verdana, 48, FontStyle.Bold), SZ_ORGE, 760, 20);
            g.DrawString($"{HourFrom}-{HourTo}", new Font(Verdana, 30, FontStyle.Regular), Brushes.Black, 1100, 36);
            g.DrawString($"{App.Data.Stations.First().Name} - {App.Data.Stations.Last().Name}", new Font(Verdana, 24, FontStyle.Bold), Brushes.Black, 1300, 20);
            g.DrawString(App.Data.Variant, new Font(Verdana, 20, FontStyle.Bold), SZ_BLUE, 1300, 80);
        }

        private void DrawStations(IGraphics g)
        {
            int totalMinutes = (HourTo - HourFrom) * 60;
            int name2X = OFFSET_X + (totalMinutes + 10) * MinuteScale;

            float y, lastY;
            y = lastY = OFFSET_Y;

            float lastkm = -1;

            g.DrawLine(BLACK2, 400, y + LINE_OFFSET, 440, y + LINE_OFFSET);

            Font km = new Font(Verdana, 12);

            RouteInterlocking interlocking = RouteInterlocking.None;

            int id = 0;
            bool lastHidden = false;
            foreach (Station station in App.Data.Stations)
            {
                if (lastkm < 0)
                    lastkm = station.Position2;

                int fontSize = station.StationType == StationType.ZST ? 16 : 12;

                int offset = lastHidden ? STATION_OFFSET / 3 : STATION_OFFSET;
                y += offset + Math.Abs(station.Position - lastkm) * StationScale;

                lastHidden = station.Hidden;
                station.DrawY = y + LINE_OFFSET;

                if (!station.Hidden)
                    station.DrawID = id++;

                Pen? pen = interlocking switch
                {
                    RouteInterlocking.Telephone => BLACK3Da,
                    RouteInterlocking.SemiAutomatic => BLACK3,
                    RouteInterlocking.Automatic => BLACK3DaDo,
                    _ => null,
                };
                if (pen is not null)
                    g.DrawLine(pen, 420, lastY, 420, y + LINE_OFFSET);

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
                lastY = y + LINE_OFFSET;

                if (station.StationType == StationType.ZST)
                    g.DrawLine(BLACK2, 400, y + LINE_OFFSET, 440, y + LINE_OFFSET);
            }

            g.DrawLine(BLACK2, name2X + 360, OFFSET_Y + LINE_OFFSET, name2X + 360, y + LINE_OFFSET + LINE_OFFSET + STATION_OFFSET);
            g.DrawLine(BLACK2, 400, OFFSET_Y + LINE_OFFSET, 400, y + LINE_OFFSET + STATION_OFFSET);
            g.DrawLine(BLACK2, 400, y + LINE_OFFSET + STATION_OFFSET, 440, y + LINE_OFFSET + STATION_OFFSET);
            g.DrawLine(BLACK2, 440, OFFSET_Y + LINE_OFFSET, 440, y + LINE_OFFSET + STATION_OFFSET);
        }

        private void DrawGrid(IGraphics g, bool export)
        {
            int totalMinutes = (HourTo - HourFrom) * 60;
            int width = (totalMinutes + 10) * MinuteScale;

            g.DrawLine(ORANGE4, OFFSET_X, OFFSET_Y + LINE_OFFSET, OFFSET_X + width, OFFSET_Y + LINE_OFFSET);

            float lastY = OFFSET_Y;
            float startX = OFFSET_X + MinuteScale * 5;

            foreach (Station station in App.Data.Stations)
            {
                if (station.Hidden)
                    continue;

                bool isZST = station.StationType == StationType.ZST;

                g.DrawLine(isZST ? ORANGE2 : ORANGE1, OFFSET_X, station.DrawY, OFFSET_X + width, station.DrawY);
                lastY = station.DrawY;

                for (int x = 0; x < totalMinutes; x++)
                {
                    int deltaY = isZST ? 5 : 3;
                    float dx = startX + x * MinuteScale;

                    if (!export && !g.VisibleClipBounds.ContainsX(dx))
                        continue;

                    if (x % 10 == 0)
                        continue;

                    if (x % 5 == 0)
                        deltaY *= 2;

                    g.DrawLine(ORANGE1, dx, station.DrawY - deltaY, dx, station.DrawY + deltaY);
                }
            }

            for (int x = 10; x < totalMinutes; x += 10)
            {
                float dx = startX + x * MinuteScale;
                if (!export && !g.VisibleClipBounds.ContainsX(dx))
                    continue;

                Pen pen;
                if (x % 60 == 0)
                    pen = ORANGE2;
                else if (x % 30 == 0)
                    pen = new(SZ_ORGE, 1) { DashStyle = DashStyle.Custom, DashPattern = [20, 6] };
                else
                    pen = ORANGE1;

                g.DrawLine(pen, dx, OFFSET_Y + LINE_OFFSET, dx, lastY + STATION_OFFSET);
            }

            for (int h = 0; h <= (HourTo - HourFrom); h++)
            {
                Font font = new Font(Verdana, 24);
                string str = (HourFrom + h).ToString();
                SizeF size = g.MeasureString(str, font);
                float x = startX + (h * 60) * MinuteScale - size.Width / 2f;
                g.DrawString(str, font, Brushes.Black, x, OFFSET_Y - size.Height);
                g.DrawString(str, font, Brushes.Black, x, lastY + LINE_OFFSET + size.Height);
            }

            g.DrawLine(ORANGE4, OFFSET_X, lastY + STATION_OFFSET, OFFSET_X + width, lastY + STATION_OFFSET);

            g.DrawLine(ORANGE4, OFFSET_X, OFFSET_Y + LINE_OFFSET, OFFSET_X, lastY + STATION_OFFSET);
            g.DrawLine(ORANGE4, OFFSET_X + width, OFFSET_Y + LINE_OFFSET, OFFSET_X + width, lastY + STATION_OFFSET);
            g.DrawLine(ORANGE4, OFFSET_X + MinuteScale * 5, OFFSET_Y + LINE_OFFSET, OFFSET_X + MinuteScale * 5, lastY + STATION_OFFSET);
            g.DrawLine(ORANGE4, OFFSET_X + width - MinuteScale * 5, OFFSET_Y + LINE_OFFSET, OFFSET_X + width - MinuteScale * 5, lastY + STATION_OFFSET);
        }

        private void DrawTrains(IGraphics g, bool export)
        {
            Font font = new Font(Verdana, 10, FontStyle.Bold);
            int totalMinutes = (HourTo - HourFrom) * 60;
            int width = (totalMinutes + 10) * MinuteScale;
            PointF? savedP1 = null;

            foreach (Train train in App.Data.GetDrawableTrains())
            {
                if (train.Stops.Count < 2)
                    continue;

                List<PointF> points = new((train.Stops.Count - 1) * 2);
                List<PointF> svPoints = [];
                List<TrainNumberData> trainNumbers = [];

                Pen pen = BLACK1;
                for (int i = 0; i < train.Stops.Count - 1; i++)
                {
                    Stop st1 = train.Stops[i];
                    Stop st2 = train.Stops[i + 1];

                    Station s1 = getStation(st1);
                    Station s2 = getStation(st2);

                    PointF p1 = GetPoint(s1, st1.Departure, out bool ip1);
                    PointF p2 = GetPoint(s2, st2.Arrival, out bool ip2);

                    if (ip1 && ip2)
                        continue;

                    if (ip1)
                        p1 = CalculatePointX(p1, p2, OFFSET_X);
                    if (ip2)
                        p2 = CalculatePointX(p1, p2, OFFSET_X + width);

                    pen = getPen(train, st1);

                    bool directionDown = p2.Y > p1.Y;

                    if (i == 0 && !ip1)
                        pointDecor(p1, st1, directionDown, 0);

                    if (!ip2)
                        pointDecor(p2, st2, directionDown, p2.X - p1.X);

                    points.Add(p1);
                    points.Add(p2);

                    if (st1.Category == TrainCategory.Sv)
                        svPoints.AddRange(GetSvPoints(p1, p2));

                    if ((s1.DrawID % NumberFrequency == 0 || (train.Stops.Count <= NumberFrequency && !train.Drawn)) && !ip1 && !ip2 && !s1.Hidden)
                    {
                        if (s2.Hidden)
                            savedP1 = p1;
                        else
                            trainNumbers.Add(new TrainNumberData(st1, p1, p2, pen.Brush));
                    }
                    else if (savedP1.HasValue && !s2.Hidden)
                    {
                        trainNumbers.Add(new TrainNumberData(st1, savedP1.Value, p2, pen.Brush));
                        savedP1 = null;
                    }

                    if ((st2.Departure - st2.Arrival) > TimeSpan.FromMinutes(1) || st1.Category != st2.Category)
                    {
                        if (train.Highlight)
                            g.DrawLines(HIGHLIGHT, points.ToArray());

                        g.DrawLines(pen, points.ToArray());
                        points.Clear();
                    }

                    if (st1.Category != st2.Category)
                        train.Drawn = false;
                }

                if (points.Count > 0)
                {
                    if (train.Highlight)
                        g.DrawLines(HIGHLIGHT, points.ToArray());

                    g.DrawLines(pen, points.ToArray());
                }

                DrawSv(g, svPoints);

                foreach (TrainNumberData num in trainNumbers)
                {
                    DrawTrainNumber(g, num.Stop, train, num.P1, num.P2, num.Brush, train.Highlight);
                }

                Pen getPen(Train train, Stop stop)
                {
                    bool adHoc = train.AdHocPath;

                    if (train.ID == App.SelectedTrain && !export)
                        return adHoc ? GREEN4Da : GREEN4;
                    else if (stop.Category < TrainCategory.Os || stop.Category == TrainCategory.Pom)
                        return adHoc ? BLACK4Da : BLACK4;
                    else if (stop.Category == TrainCategory.Sv)
                        return adHoc ? BLACK1Da : BLACK1;
                    else if (stop.Category < TrainCategory.Nex)
                        return adHoc ? BLACK2Da : BLACK2;
                    else if (stop.Category == TrainCategory.Nex)
                        return adHoc ? BLUE4Da : BLUE4;
                    else if (stop.Category < TrainCategory.Lv)
                        return adHoc ? BLUE2Da : BLUE2;

                    return adHoc ? BLACK1Da : BLACK1;
                }

                void pointDecor(in PointF point, Stop stop, bool directionDown, float delta)
                {
                    if (stop.Starts || stop.Ends)
                        g.FillEllipse(pen.Brush, point.X - 5, point.Y - 5, 10, 10);

                    string decor;
                    if (stop.ShortStop)
                        decor = "▲";
                    else if (stop.OnlyIn)
                        decor = "◗";
                    else if (stop.OnlyOut)
                        decor = "◖";
                    else if (stop.ZDD)
                        decor = "+";
                    else if (stop.TelD3)
                        decor = "☎";
                    else
                        return;

                    SizeF sts = g.MeasureString(decor, font);
                    float offsetY = directionDown ? sts.Height : 0;
                    g.DrawString(decor, font, pen.Brush, point.X - sts.Width - delta / 5, point.Y - offsetY);
                }
            }

            Station getStation(Stop stop) => App.Data.Stations.First(x => x.ID == stop.Station);
        }

        private PointF CalculatePointX(PointF p1, PointF p2, float x)
        {
            float slope = MathF.Abs(p2.Y - p1.Y) / MathF.Abs(p2.X - p1.X);
            float relX;

            if (p1.Y > p2.Y)
                relX = MathF.Max(p1.X, p2.X) - x;
            else
                relX = x - MathF.Min(p1.X, p2.X);

            return new PointF(x, slope * relX + MathF.Min(p1.Y, p2.Y));
        }

        private void DrawTrainNumber(IGraphics g, Stop stop, Train train, PointF p1, PointF p2, Brush color, bool highlight)
        {
            const float DEG = 180f / MathF.PI;

            float angle = MathF.Atan2(p2.Y - p1.Y, p2.X - p1.X) * DEG;
            PointF middle = new((p1.X + p2.X) / 2f, (p1.Y + p2.Y) / 2f);
            float size = (stop.Category < TrainCategory.Os || stop.Category == TrainCategory.Nex) ? 14 : 12;
            string text = train.Number.ToString();

            train.Drawn = true;

            DrawRotatedText(g, middle.X, middle.Y, angle, text, new(Verdana, size), color, highlight);
        }

        private List<PointF> GetSvPoints(PointF p1, PointF p2)
        {
            int distance = (int)MathF.Sqrt(MathF.Pow(p2.X - p1.X, 2) + MathF.Pow(p2.Y - p1.Y, 2));
            int count = distance / 25;
            return Utils.GetPointsOnLine(p1, p2, count);
        }

        private void DrawSv(IGraphics g, List<PointF> points)
        {
            foreach (PointF point in points)
            {
                RectangleF rect = new(point.X - 5, point.Y - 5, 10, 10);
                g.FillEllipse(Brushes.White, rect);
                g.DrawEllipse(BLACK1, rect);
            }
        }

        private static void DrawRotatedText(IGraphics g, float x, float y, float angle, string text, Font font, Brush brush, bool highlight)
        {
            g.TranslateTransform(x, y);
            g.RotateTransform(angle);
            g.TranslateTransform(-x, -y);
            SizeF size = g.MeasureString(text, font);
            RectangleF rect = new RectangleF(x - size.Width / 2f, y - size.Height, size.Width, size.Height);
            if (highlight)
                g.FillRectangle(HIGHLIGHT.Brush, rect);
            g.DrawString(text, font, brush, rect, new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            g.ResetTransform();
        }

        private PointF GetPoint(Station station, TimeSpan time, out bool incomplete)
        {
            if (time.TotalMinutes < HourFrom * 60 - 5 || time.TotalMinutes > HourTo * 60 + 5)
                incomplete = true;
            else
                incomplete = false;

            float minutes = (float)time.TotalMinutes - HourFrom * 60 + 5;
            float x = OFFSET_X + minutes * MinuteScale;
            float y = station.DrawY;

            return new(x, y);
        }

        private void Canvas_Click(object sender, EventArgs e)
        {
            Canvas.Invalidate();
        }

        public void Redraw()
        {
            if (Canvas.InvokeRequired)
                Canvas.Invoke(Canvas.Invalidate);
            else
                Canvas.Invalidate();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            Size size = ComputeSize();

            string fileName = App.Data.Stations.First().Name + " - " + App.Data.Stations.Last().Name;
            using SaveFileDialog dialog = new()
            {
                Filter = "Soubory PNG (*.png)|*.png|Soubory SVG (*.svg)|*.svg|Soubory EMF (*.emf)|*.emf|Všechny soubory|*.*",
                FileName = fileName,
                Title = "Exportovat"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            string path = dialog.FileName;
            string pathTrim = path.ToLowerInvariant().TrimEnd();

            if (pathTrim.EndsWith(".svg"))
            {
                SvgGraphics graphics = new();
                Draw(graphics, true);

                try
                {
                    File.WriteAllText(path, graphics.WriteSVGString(size));
                    MessageBox.Show(this, $"Obrázek exportován do {path}", "Exportováno", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Chyba při exportu obrázku: {ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else if (pathTrim.EndsWith("*.emf"))
            {
                try
                {
                    using (Metafile metafile = new(path, nint.Zero, new(0, 0, size.Width, size.Height), MetafileFrameUnit.Pixel, EmfType.EmfPlusOnly))
                    {
                        using Graphics g = Graphics.FromImage(metafile);
                        Draw(new GdiGraphics(g), true);
                    }

                    MessageBox.Show(this, $"Obrázek exportován do {path}", "Exportováno", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Chyba při exportu obrázku: {ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                using Bitmap bitmap = new(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage(bitmap))
                    Draw(new GdiGraphics(g), true);

                try
                {
                    bitmap.Save(path, ImageFormat.Png);
                    MessageBox.Show(this, $"Obrázek exportován do {path}", "Exportováno", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Chyba při exportu obrázku: {ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            HourStart.Maximum = HourEnd.Value - 1;
            HourEnd.Minimum = HourStart.Value + 1;

            UpdateSize();
            Redraw();
        }

        private void MoveLeft_Click(object sender, EventArgs e)
        {
            if (HourStart.Value == 0)
                return;

            HourStart.Value--;
            HourEnd.Value--;
        }

        private void MoveRight_Click(object sender, EventArgs e)
        {
            if (HourEnd.Value == 24)
                return;

            HourStart.Value++;
            HourEnd.Value++;
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            Redraw();
        }

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _lastDragPoint = Cursor.Position;
                Canvas.Cursor = Cursors.SizeAll;
            }
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _lastDragPoint = null;
                Canvas.Cursor = Cursors.Default;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                if (_lastDragPoint.HasValue)
                {
                    _lastDragPoint = null;
                    Canvas.Cursor = Cursors.Default;
                }
                return;
            }

            if (!_lastDragPoint.HasValue)
            {
                _lastDragPoint = Cursor.Position;
                Canvas.Cursor = Cursors.SizeAll;
                return;
            }

            Point currentMousePos = Cursor.Position;

            int deltaX = _lastDragPoint.Value.X - currentMousePos.X;
            int deltaY = _lastDragPoint.Value.Y - currentMousePos.Y;

            _lastDragPoint = currentMousePos;

            if (deltaX == 0 && deltaY == 0)
            {
                return;
            }

            Point currentScroll = ScrollPanel.AutoScrollPosition;

            int targetScrollX = -currentScroll.X + deltaX;
            int targetScrollY = -currentScroll.Y + deltaY;
            ScrollPanel.AutoScrollPosition = new Point(targetScrollX, targetScrollY);
        }
    }

    internal static class DrawingData
    {
        public static readonly FontFamily Verdana = new FontFamily("Verdana");

        public static readonly SolidBrush SZ_BLUE = new SolidBrush(Color.FromArgb(0, 43, 89));

        public static readonly SolidBrush SZ_ORGE = new SolidBrush(Color.FromArgb(255, 82, 0));

        public static readonly SolidBrush SZ_GREN = new SolidBrush(Color.FromArgb(52, 164, 154));

        public static readonly Pen BLACK1 = new(Brushes.Black, 1);

        public static readonly Pen BLACK1Da = new(Brushes.Black, 1) { DashStyle = DashStyle.Dash };

        public static readonly Pen BLACK2 = new(Brushes.Black, 2);

        public static readonly Pen BLACK2Da = new(Brushes.Black, 2) { DashStyle = DashStyle.Dash };

        public static readonly Pen BLACK3 = new(Brushes.Black, 3);

        public static readonly Pen BLACK3Da = new(Brushes.Black, 3) { DashStyle = DashStyle.Dash };

        public static readonly Pen BLACK4 = new(Brushes.Black, 4);

        public static readonly Pen BLACK4Da = new(Brushes.Black, 4) { DashStyle = DashStyle.Dash };

        public static readonly Pen BLACK3DaDo = new(Brushes.Black, 3) { DashStyle = DashStyle.DashDot };

        public static readonly Pen ORANGE1 = new(SZ_ORGE, 1);

        public static readonly Pen ORANGE1Da = new(SZ_ORGE, 1) { DashStyle = DashStyle.Custom, DashPattern = [20, 5] };

        public static readonly Pen ORANGE2 = new(SZ_ORGE, 2);

        public static readonly Pen ORANGE4 = new(SZ_ORGE, 4);

        public static readonly Pen GREEN4 = new(SZ_GREN, 4);

        public static readonly Pen GREEN4Da = new(SZ_GREN, 4) { DashStyle = DashStyle.Dash };

        public static readonly Pen BLUE1 = new(Brushes.Blue, 1);

        public static readonly Pen BLUE1Da = new(Brushes.Blue, 1) { DashStyle = DashStyle.Dash };

        public static readonly Pen BLUE2 = new(Brushes.Blue, 2);

        public static readonly Pen BLUE2Da = new(Brushes.Blue, 2) { DashStyle = DashStyle.Dash };

        public static readonly Pen BLUE3 = new(Brushes.Blue, 3);

        public static readonly Pen BLUE3Da = new(Brushes.Blue, 3) { DashStyle = DashStyle.Dash };

        public static readonly Pen BLUE4 = new(Brushes.Blue, 4);

        public static readonly Pen BLUE4Da = new(Brushes.Blue, 4) { DashStyle = DashStyle.Dash };

        public static readonly Pen HIGHLIGHT = new(new SolidBrush(Color.FromArgb(127, 255, 255, 0)), 20);
    }
}

readonly struct TrainNumberData
{
    public readonly Stop Stop;
    public readonly PointF P1;
    public readonly PointF P2;
    public readonly Brush Brush;

    public TrainNumberData(Stop stop, PointF p1, PointF p2, Brush brush)
    {
        Stop = stop;
        P1 = p1;
        P2 = p2;
        Brush = brush;
    }
}