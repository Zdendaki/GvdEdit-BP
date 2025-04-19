using GvdEdit.Models;
using System.Windows;
using System.Windows.Controls;

namespace GvdEdit.Windows
{
    /// <summary>
    /// Interakční logika pro StationsWindow.xaml
    /// </summary>
    public partial class StationsWindow : Window
    {
        public StationsWindow()
        {
            InitializeComponent();

            DataContext = App.Data;
        }

        private void AddStation_Click(object sender, RoutedEventArgs e)
        {
            App.Data.Stations.Add(new());
            Stations.SelectedIndex = App.Data.Stations.Count - 1;
        }

        private void Stations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StationEdit.IsEnabled = DeleteStation.IsEnabled = MoveStationUp.IsEnabled = MoveStationDown.IsEnabled = Stations.SelectedItems.Count > 0;
        }

        private void DeleteStation_Click(object sender, RoutedEventArgs e)
        {
            if (Stations.SelectedItem is not Station station)
                return;

            App.Data.Stations.Remove(station);
        }

        private void MoveStationUp_Click(object sender, RoutedEventArgs e)
        {
            if (Stations.SelectedItem is not Station station)
                return;
            
            int index = App.Data.Stations.IndexOf(station);
            if (index == 0)
                return;

            App.Data.Stations.RemoveAt(index);
            App.Data.Stations.Insert(index - 1, station);
        }

        private void MoveStationDown_Click(object sender, RoutedEventArgs e)
        {
            if (Stations.SelectedItem is not Station station)
                return;

            int index = App.Data.Stations.IndexOf(station);
            if (index == App.Data.Stations.Count - 1)
                return;

            App.Data.Stations.RemoveAt(index);
            App.Data.Stations.Insert(index + 1, station);
        }
    }
}
