using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

            await LoadProgress.Dispatcher.BeginInvoke(() => LoadProgress.IsIndeterminate = true);

            try
            {
                DirectoryInfo di = new(ofd.FolderName);
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
                    await LoadTrain(file.FullName);
                    await LoadProgress.Dispatcher.BeginInvoke(() => LoadProgress.Value++);
                }
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

        private async Task LoadTrain(string fileName)
        {
            using FileStream fs = new(fileName, FileMode.Open, FileAccess.Read);
            using XmlReader reader = XmlReader.Create(fs);

            XmlDocument document = new();
            document.Load(reader);


        }
    }
}
