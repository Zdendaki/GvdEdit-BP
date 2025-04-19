using GvdEdit.Models;
using System.Windows;

namespace GvdEdit.Windows
{
    /// <summary>
    /// Interakční logika pro StationWindow.xaml
    /// </summary>
    public partial class StationWindow : Window
    {
        public Station Station { get; }

        public StationWindow(Station station)
        {
            InitializeComponent();

            DataContext = Station = station;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
