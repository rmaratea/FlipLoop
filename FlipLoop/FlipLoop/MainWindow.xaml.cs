using Microsoft.Win32;
using System.Windows;
using FlipLoop.Audio;
using System.IO;

namespace FlipLoop;

public partial class MainWindow : Window
{
    private AudioBuffer? _buffer;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dlg = new();

        dlg.Filter = "Audio|*.wav;*.mp3";

        if (dlg.ShowDialog() == true)
        {
            LoadAudio(dlg.FileName);
        }
    }

    private void Window_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effects = DragDropEffects.Copy;
        else
            e.Effects = DragDropEffects.None;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        string[] files =
            (string[])e.Data.GetData(DataFormats.FileDrop)!;

        if (files.Length == 0)
            return;

        string ext = Path.GetExtension(files[0]).ToLowerInvariant();

        if (ext != ".mp3" && ext != ".wav")
            return;

        LoadAudio(files[0]);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void LoadAudio(string file)
    {
        _buffer = AudioLoader.Load(file);

        FileNameLabel.Text = _buffer.FileName;

        InfoLabel.Text =
            $"{_buffer.Duration:mm\\:ss\\.fff}   " +
            $"{_buffer.SampleRate} Hz   " +
            $"{_buffer.Channels} canali";
    }

}