using System.Windows;

namespace GvdEdit.Windows
{
    /// <summary>
    /// Interakční logika pro SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(Window owner)
        {
            InitializeComponent();
            Owner = owner;
            DataContext = App.Data;
        }
    }
}
