using GvdEdit.Models;
using GvdEdit.ViewModels;
using GvdEdit.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GvdEdit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GvdForm _gvdForm;

        public MainWindow()
        {
            App.MainWindow = this;
            InitializeComponent();

            _gvdForm = new();
            Loaded += MainWindow_Loaded;

            DataContext = App.Data;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _gvdForm.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _gvdForm.AllowClose = true;
            _gvdForm.Close();

            base.OnClosing(e);
        }

        internal void StationsChanged()
        {
            PointFrom.ItemsSource = PointTo.ItemsSource = App.Data.Stations;
            _gvdForm.UpdateSize();
        }

        internal void RedrawGVD() => _gvdForm.Redraw();

        private void EditStations_Click(object sender, RoutedEventArgs e)
        {
            StationsWindow sw = new() { Owner = this };
            sw.ShowDialog();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Filter = "Soubory GVD (*.gvd)|*.gvd|Všechny soubory|*.*",
                Title = "Otevřít soubor GVD"
            };

            if (ofd.ShowDialog() != true)
                return;

            string fileName = ofd.FileName;
            try
            {
                string json = File.ReadAllText(fileName);

                if (Newtonsoft.Json.JsonConvert.DeserializeObject<GvdData>(json) is not GvdData data)
                    return;

                App.Data = data;
                App.FileName = fileName;
                Save.IsEnabled = true;
                DataContext = App.Data;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (App.FileName is null)
                return;

            SaveFile(App.FileName);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new()
            {
                Filter = "Soubory GVD (*.gvd)|*.gvd|Všechny soubory|*.*",
                Title = "Uložit soubor GVD"
            };

            if (sfd.ShowDialog() != true)
                return;

            SaveFile(sfd.FileName);
        }

        private void SaveFile(string fileName)
        {
            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(App.Data);
                File.WriteAllText(fileName, json);

                App.FileName = fileName;
                Save.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewTrain_Click(object sender, RoutedEventArgs e)
        {
            TrainVM train = new();
            App.Data.TrainsVM.Add(train);
            Trains.SelectedItem = train;
        }

        private void CreateTrain_Click(object sender, RoutedEventArgs e)
        {
            if (Trains.SelectedItem is not TrainVM train)
                return;

            train.CreateStops();
        }

        private void Trains_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TrainGroupBox.IsEnabled = Trains.SelectedIndex >= 0;
            RedrawGVD();
        }

        private void RailCalcImport_Click(object sender, RoutedEventArgs e)
        {
            if (Trains.SelectedItem is not TrainVM train)
                return;

            bool loadTime = LoadStartTime.IsChecked != true;

            if (!loadTime && !StartTime.Value.HasValue)
            {
                MessageBox.Show(this, "Nastavte čas odjezdu vlaku.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            OpenFileDialog ofd = new()
            {
                Filter = "Soubory CSV (*.csv)|*.csv|Všechny soubory|*.*",
                Title = "Načíst soubor z RailCalc"
            };

            if (ofd.ShowDialog() != true)
                return;

            string fileName = ofd.FileName;
            train.TrainStops.Clear();

            try
            {
                IEnumerable<string> lines = File.ReadAllLines(fileName).Skip(1);
                TimeSpan offset = loadTime ? TimeSpan.Zero : StartTime.Value!.Value.TimeOfDay;

                int i = 0;
                foreach (string line in lines)
                {
                    bool first = i == 0;
                    bool last = lines.Count() - 1 == i;

                    i++;
                    string[] parts = line.Split(';');
                    if (parts.Length < 4)
                        continue;
                    if (parts[0].Length != 1)
                        continue;

                    TimeSpan arrival, departure;
                    if (!TimeSpan.TryParseExact(parts[2], "h\\:mm\\:ss", null, out arrival) && !first)
                        continue;

                    if (!TimeSpan.TryParseExact(parts[3], "h\\:mm\\:ss", null, out departure) && !last)
                        continue;

                    if (first)
                    {
                        arrival = departure;

                        if (!loadTime)
                            offset -= departure;
                    }
                    if (last)
                        departure = arrival;

                    bool stop = parts[0].Equals("Z", StringComparison.OrdinalIgnoreCase);
                    string name = parts[1];
                    bool shortStop = false;

                    if (App.Data.Stations.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) is not Station station)
                        continue;

                    if (stop && (departure - arrival) < TimeSpan.FromSeconds(30))
                        shortStop = true;

                    train.TrainStops.Add(new StopVM
                    {
                        Station = station,
                        Arrival = arrival + offset,
                        Departure = departure + offset,
                        ShortStop = shortStop,
                        Starts = first,
                        Ends = last,
                    });
                }

                int lastID = -1;
                bool invalid = false;
                foreach (var stop in train.TrainStops)
                {
                    if (lastID < 0)
                    {
                        lastID = App.Data.Stations.IndexOf(stop.Station);
                        continue;
                    }

                    int id = App.Data.Stations.IndexOf(stop.Station);

                    if (id < 0 || Math.Abs(id - lastID) > 1)
                    {
                        invalid = true;
                        break;
                    }

                    lastID = id;
                }

                if (invalid)
                {
                    train.TrainStops.Clear();
                    MessageBox.Show(this, "Načtený vlak není platný. Zkontrolujte, zda jsou zastávky v pořádku.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}