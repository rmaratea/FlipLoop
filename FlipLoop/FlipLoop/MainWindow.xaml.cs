using Microsoft.Win32;
using System.Windows;

namespace FlipLoop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dlg = new();

        dlg.Filter =
            "Audio|*.wav;*.mp3";

        if (dlg.ShowDialog() == true)
        {
            MessageBox.Show(
                $"File selezionato:\n{dlg.FileName}");
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}