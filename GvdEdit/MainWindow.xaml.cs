using GvdEdit.Models;
using GvdEdit.ViewModels;
using GvdEdit.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GvdEdit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GvdForm _gvdForm;
        private ICollectionView _trainsView;

        public MainWindow()
        {
            App.MainWindow = this;
            InitializeComponent();

            _gvdForm = new();
            Loaded += MainWindow_Loaded;

            DataContext = App.Data;
            _trainsView = LoadCollectionView();
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
                App.Data.LoadTrains();
                App.FileName = fileName;
                Save.IsEnabled = true;
                DataContext = App.Data;
                _trainsView = LoadCollectionView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ICollectionView LoadCollectionView()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(App.Data.TrainsVM);
            view.SortDescriptions.Add(new("Number", ListSortDirection.Ascending));
            return view;
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
                App.Data.PrepareTrains();
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(App.Data);
                File.WriteAllText(fileName, json);

                App.FileName = fileName;
                Save.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                App.Data.ClearTrains();
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

            train.CreateStops(IgnoreHidden.IsChecked == true);
        }

        private void DeleteTrain_Click(object sender, RoutedEventArgs e)
        {
            if (Trains.SelectedItem is not TrainVM train)
                return;

            App.Data.TrainsVM.Remove(train);
            RedrawGVD();
        }

        private void CloneTrain_Click(object sender, RoutedEventArgs e)
        {
            if (Trains.SelectedItem is not TrainVM train)
                return;

            TrainVM newTrain = new(train.CloneTrain());
            App.Data.TrainsVM.Add(newTrain);
            Trains.SelectedItem = newTrain;
        }

        private void ClearTrains_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Opravdu chcete odstranit VŠECHNY vlaky?", "Vymazat vlaky", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
                return;

            App.Data.TrainsVM.Clear();
            RedrawGVD();
        }

        private void Trains_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TrainGroupBox.IsEnabled = Trains.SelectedIndex >= 0;
            RedrawGVD();
        }

        private void RailCalcImport_Click(object sender, RoutedEventArgs e)
        {
            if (Trains.SelectedItem is not TrainVM train || Category.SelectedItem is not TrainCategoryItem category)
                return;

            bool loadExternTime = LoadStartTime.IsChecked == true;

            OpenFileDialog ofd = new()
            {
                Filter = "Soubory CSV (*.csv)|*.csv|Všechny soubory|*.*",
                Title = "Načíst soubor z RailCalc"
            };

            if (ofd.ShowDialog() != true)
                return;

            string fileName = ofd.FileName;
            train.Initializing = true;
            train.TrainStops.Clear();

            try
            {
                IEnumerable<string> lines = File.ReadAllLines(fileName).Skip(1);

                TimeSpan offset = loadExternTime ? TimeSpan.Zero : train.DepartureTime;

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
                    }
                    if (last)
                    {
                        departure = arrival;
                    }

                    arrival = arrival.RoundUpToHalfMinute();
                    departure = departure.RoundUpToHalfMinute();

                    if (first)
                    {
                        if (loadExternTime)
                            train.DepartureTime = departure;
                        else
                            offset -= departure;
                    }
                    
                    arrival += offset;
                    departure += offset;

                    bool stop = parts[0].Equals("Z", StringComparison.OrdinalIgnoreCase);
                    string name = parts[1];
                    bool shortStop = false;

                    if (App.Data.Stations.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) is not Station station)
                        continue;

                    if (stop && (departure - arrival) < TimeSpan.FromSeconds(30))
                        shortStop = true;
                    float waitTime = 0;

                    waitTime = (float)(departure - arrival).TotalMinutes;

                    train.AddStop(new StopVM
                    {
                        Station = station,
                        Arrival = arrival,
                        Departure = departure,
                        ShortStop = shortStop,
                        Starts = first,
                        Ends = last,
                        WaitTime = waitTime,
                        Category = category.Category
                    });
                }

                for (int j = 0; j < train.TrainStops.Count - 1; j++)
                {
                    var stop = train.TrainStops[j];
                    var next = train.TrainStops[j + 1];

                    stop.TravelTime = (float)(next.Arrival - stop.Departure).TotalMinutes;
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
                        for (int j = lastID + 1; j < id; j++)
                        {
                            if (!App.Data.Stations[j].Hidden)
                            {
                                invalid = true;
                                break;
                            }
                        }
                        if (invalid)
                        {
                            train.TrainStops.Clear();
                            MessageBox.Show(this, $"Načtený vlak není platný. Zkontrolujte, zda jsou zastávky v pořádku.\nVlak nenavazuje mezi body {App.Data.Stations[lastID].GetPrettyName(false)} a {stop.Station.GetPrettyName(false)}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    lastID = id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                train.Initializing = false;
                train.RecountStops();
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new(this);
            sw.ShowDialog();
        }

        private void CIS_JR_Click(object sender, RoutedEventArgs e)
        {
            if (OwnedWindows.OfType<CisJrImportWindow>().Any())
                return;

            CisJrImportWindow cisJrImportWindow = new(this);
            cisJrImportWindow.Show();
        }

        private void TrainFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TrainFilter.Text))
            {
                _trainsView.Filter = null;
                return;
            }

            string filterText = TrainFilter.Text.Trim();
            _trainsView.Filter = filter;

            bool filter(object obj)
            {
                if (obj is not TrainVM train)
                    return false;

                return train.Number.ToString().Contains(filterText, StringComparison.OrdinalIgnoreCase);
            }
        }

        private void TrainNumber_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Task.Run(() =>
            {
                Trains.Dispatcher.Invoke(_trainsView.Refresh);
            });
        }
    }
}