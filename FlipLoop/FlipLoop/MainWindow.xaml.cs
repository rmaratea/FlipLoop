using FlipLoop.Audio;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace FlipLoop;

public partial class MainWindow : Window
{

    private readonly AudioEngine _engine = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Audio Files|*.wav;*.mp3"
        };

        if (dlg.ShowDialog() == true)
        {
            LoadAudio(dlg.FileName);
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void LoadAudio(string file)
    {
        try
        {
            _engine.Load(file);

            if (_engine.CurrentBuffer == null)
                return;

            var buffer = _engine.CurrentBuffer;

            FileNameLabel.Text = buffer.FileName;

            StatusText.Text =
                $"{buffer.Duration:mm\\:ss\\.fff}    " +
                $"{buffer.SampleRate} Hz    " +
                $"{buffer.Channels} ch";

            Waveform.AudioBuffer = buffer;

            BpmLabel.Text = "--";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Errore caricamento audio",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void Window_DragEnter(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;

        if (files.Length == 0)
            return;

        var ext = Path.GetExtension(files[0]).ToLowerInvariant();

        if (ext is ".wav" or ".mp3")
        {
            LoadAudio(files[0]);
        }
    }
}
