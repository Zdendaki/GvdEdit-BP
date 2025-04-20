using GvdEdit.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace GvdEdit.Windows
{
    /// <summary>
    /// Interakční logika pro CisJrImport.xaml
    /// </summary>
    public partial class CisJrImportWindow : Window
    {
        List<Train> trains = [];

        public CisJrImportWindow(Window owner)
        {
            InitializeComponent();

            Owner = owner;
        }

        private async void LoadTrains_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog ofd = new()
            {
                Title = "Vyberte složku s vlaky",
                Multiselect = false
            };

            bool? result = null;
            await Dispatcher.InvokeAsync(() => result = ofd.ShowDialog(this));

            if (result != true)
                return;

            await Task.Run(() => LoadTimetables(ofd.FolderName));
        }

        private async Task LoadTimetables(string filename)
        {
            try
            {
                await LoadProgress.Dispatcher.BeginInvoke(() => LoadProgress.IsIndeterminate = true);

                DirectoryInfo di = new(filename);
                IEnumerable<FileInfo> files = di.EnumerateFiles("*.xml", SearchOption.AllDirectories);

                int count = files.Count();
                if (count == 0)
                {
                    Dispatcher.Invoke(() => MessageBox.Show(this, "Nebyl nalezen žádný soubor s vlaky", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error));
                    return;
                }

                await LoadProgress.Dispatcher.BeginInvoke(() =>
                {
                    LoadProgress.IsIndeterminate = false;
                    LoadProgress.Maximum = count;
                    LoadProgress.Value = 0;
                });

                foreach (FileInfo file in files)
                {
                    LoadTrain(file.FullName);
                    await LoadProgress.Dispatcher.BeginInvoke(() => LoadProgress.Value++);
                }

                Dispatcher.Invoke(() =>
                {
                    foreach (Train train in trains)
                    {
                        App.Data.TrainsVM.Add(new(train));
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show(this, ex.Message, "Data se nepodařilo načíst", MessageBoxButton.OK, MessageBoxImage.Error));
            }
            finally
            {
                await LoadProgress.Dispatcher.BeginInvoke(() =>
                {
                    LoadProgress.Value = 0;
                    LoadProgress.IsIndeterminate = false;
                });
            }
        }

        private void LoadTrain(string fileName)
        {
            using FileStream fs = new(fileName, FileMode.Open, FileAccess.Read);
            using XmlReader reader = XmlReader.Create(fs);

            XmlDocument document = new();
            document.Load(reader);

            XmlElement? root = document.DocumentElement;

            if (root is null || root.Name != "CZPTTCISMessage")
                return;

            foreach (XmlNode child in root.ChildNodes)
            {
                if (child.Name == "CZPTTInformation")
                    ParseInformation(child);
            }
        }

        private void ParseInformation(XmlNode node)
        {
            Train train = new();
            bool numberSet = false;
            bool first = true;
            bool lastNull = true;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "CZPTTLocation")
                { 
                    Stop? stop = ParseLocation(child, out int number);
                    if (stop is not null)
                    {
                        if (first)
                            stop.Starts = true;

                        train.Stops.Add(stop);

                        if (!numberSet)
                        {
                            train.Number = number;
                            train.Category = stop.Category;
                            numberSet = true;
                        }

                        lastNull = false;
                    }
                    else
                        lastNull = true;

                    first = false;
                }
            }

            if (train.Stops.Count >= 2)
            {
                if (!lastNull)
                    train.Stops.Last().Ends = true;

                trains.Add(train);
            }
        }

        // Stanice vlaku
        private Stop? ParseLocation(XmlNode node, out int number)
        {
            string? sr70 = null;
            TimeSpan? arrival, departure;
            arrival = departure = null;
            TrainCategory category = TrainCategory.Invalid;
            number = -1;

            bool zdd = false;
            bool shortStay = false;
            bool onlyIn = false;
            bool onlyOut = false;
            bool telD3 = false;

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "Location")
                    getLocation(child);
                else if (child.Name == "TimingAtLocation")
                    getTiming(child);
                else if (child.Name == "TrafficType")
                    getCategory(child.InnerText);
                else if (child.Name == "OperationalTrainNumber")
                {
                    if (!int.TryParse(child.InnerText.Trim(), out number))
                        number = -1;
                }
                else if (child.Name == "TrainActivity")
                    parseActivity(child);
            }

            void getLocation(XmlNode node)
            {
                foreach (XmlNode child in node)
                {
                    if (child.Name == "LocationPrimaryCode")
                        sr70 = child.InnerText.Trim();
                }
            }

            void getTiming(XmlNode node)
            {
                foreach (XmlNode child in node)
                {
                    if (child.Name == "Timing" && child.Attributes is not null)
                    {
                        TrainTimeType type = TrainTimeType.None;
                        TimeSpan time = TimeSpan.Zero;
                        foreach (XmlAttribute attr in child.Attributes)
                        {
                            if (attr.Name == "TimingQualifierCode")
                            {
                                string code = attr.InnerText.ToUpper().Trim();
                                if (code == "ALD")
                                    type = TrainTimeType.Departure;
                                else if (code == "ALA")
                                    type = TrainTimeType.Arrival;
                                else
                                    continue;

                                break;
                            }
                        }

                        foreach (XmlNode timingChild in child.ChildNodes)
                        {
                            if (timingChild.Name == "Time")
                            {
                                string timeStr = timingChild.InnerText.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
                                if (TimeSpan.TryParseExact(timeStr, "c", null, TimeSpanStyles.None, out time))
                                    break;
                            }
                        }

                        if (time == TimeSpan.Zero)
                            continue;

                        if (type == TrainTimeType.Arrival)
                            arrival = time;
                        else if (type == TrainTimeType.Departure)
                            departure = time;
                    }
                }
            }

            void getCategory(string type)
            {
                switch (type.Trim().ToUpperInvariant())
                {
                    case "11":
                        category = TrainCategory.Os;
                        break;
                    case "C1":
                        category = TrainCategory.Ex;
                        break;
                    case "C2":
                        category = TrainCategory.R;
                        break;
                    case "C3":
                        category = TrainCategory.Sp;
                        break;
                    case "C4":
                        category = TrainCategory.Sv;
                        break;
                    case "C5":
                        category = TrainCategory.Nex;
                        break;
                    case "C6":
                        category = TrainCategory.Pn;
                        break;
                    case "C7":
                        category = TrainCategory.Mn;
                        break;
                    case "C8":
                        category = TrainCategory.Lv;
                        break;
                    case "C9":
                        category = TrainCategory.Vlec;
                        break;
                    case "CA":
                        category = TrainCategory.Sluz;
                        break;
                    case "CB":
                        category = TrainCategory.Pom;
                        break;
                }
            }

            void parseActivity(XmlNode node)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.Name == "TrainActivityType")
                    {
                        switch (child.InnerText.Trim().ToUpperInvariant())
                        {
                            case "0002":
                                zdd = true;
                                break;
                            case "CZ02":
                                shortStay = true;
                                break;
                            case "CZ08":
                                telD3 = true;
                                break;
                        }
                    }
                }
            }

            if ((!arrival.HasValue && !departure.HasValue) || category == TrainCategory.Invalid || number < 0)
                return null;

            if (App.Data.Stations.FirstOrDefault(x => x.SR70 == sr70) is not Station station)
                return null;

            if (!arrival.HasValue)
                arrival = departure!.Value;
            if (!departure.HasValue)
                departure = arrival!.Value;

            return new Stop
            {
                Arrival = arrival.Value,
                Category = category,
                Departure = departure.Value,
                OnlyIn = onlyIn,
                OnlyOut = onlyOut,
                ShortStop = shortStay,
                Station = station.ID,
                TelD3 = telD3,
                ZDD = zdd
            };
        }
    }

    internal enum TrainTimeType
    {
        None,
        Arrival,
        Departure
    }
}
