using GvdEdit.Models;
using System.Windows;

namespace GvdEdit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static GvdData Data { get; set; } = new();

        internal static string? FileName { get; set; } = null;
    }

}
