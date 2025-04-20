using GvdEdit.Models;
using GvdEdit.ViewModels;
using System;
using System.Collections.Specialized;
using System.Windows;

namespace GvdEdit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static GvdData Data
        {
            get => field;
            set
            {
                if (field is not null)
                    field.Stations.CollectionChanged -= Stations_CollectionChanged;
                
                field = value;

                Stations_CollectionChanged(null!, null!);

                if (field is not null)
                    field.Stations.CollectionChanged += Stations_CollectionChanged;
            }
        } = new();

        internal new static MainWindow MainWindow { get; set; } = null!;

        private static void Stations_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (MainWindow is null)
                return;

            MainWindow.StationsChanged();
        }

        internal static Guid SelectedTrain => MainWindow.Trains.SelectedItem is TrainVM train ? train.ID : Guid.Empty;

        internal static string? FileName { get; set; } = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.SystemAware);
        }
    }

}
