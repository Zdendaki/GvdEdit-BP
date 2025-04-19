using GvdEdit.Models;
using GvdEdit.Windows;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

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
            _gvdForm.UpdateSize();
        }

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
    }
}