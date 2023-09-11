
//   BassBoom  Copyright (C) 2023  Aptivi
// 
//   This file is part of BassBoom
// 
//   BassBoom is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
// 
//   BassBoom is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
// 
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using BassBoom.Basolia;
using BassBoom.Basolia.Devices;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Basolia.Lyrics;
using BassBoom.Basolia.Playback;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BassBoom.Views;

public partial class MainView : UserControl
{
    internal bool sliderUpdatedByCode = false;

    public MainView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            // These functions depend on Basolia being initialized.
            DataContext = new BassBoomData(this);
            PathToMp3.TextChanged += CheckPath;
            DetermineDevice.IsCheckedChanged += MakeDeviceDeterministic;
            durationRemain.ValueChanged += HandleDurationValueChange;
            volumeSlider.ValueChanged += HandleVolumeValueChange;
            volumeSlider.Value = PlaybackTools.GetVolume().baseLinear;
        }
    }

    internal void EnablePlay()
    {
        if (File.Exists(PathToMp3.Text) &&
            ((!DetermineDevice.IsChecked.Value && !string.IsNullOrEmpty(((BassBoomData)DataContext).selectedDevice)) ||
               DetermineDevice.IsChecked.Value))
        {
            PlayButton.IsEnabled = true;
            FileTools.OpenFile(PathToMp3.Text);
            AudioInfoTools.GetId3Metadata(out var v1, out var v2);
            GotArtistLabel.Text =
                !string.IsNullOrEmpty(v2.Artist) ? v2.Artist :
                !string.IsNullOrEmpty(v1.Artist) ? v1.Artist :
                "";
            GotGenreLabel.Text =
                !string.IsNullOrEmpty(v2.Genre) ? v2.Genre :
                v1.GenreIndex >= 0 ? $"{v1.Genre} [{v1.GenreIndex}]" :
                "";
            GotTitleLabel.Text =
                !string.IsNullOrEmpty(v2.Title) ? v2.Title :
                !string.IsNullOrEmpty(v1.Title) ? v1.Title :
                "";
            BassBoomData.duration = AudioInfoTools.GetDuration(true);
            BassBoomData.durationSpan = AudioInfoTools.GetDurationSpanFromSamples(BassBoomData.duration).ToString();
            BassBoomData.v1 = v1;
            BassBoomData.v2 = v2;
            GotDurationLabel.Text = $"00:00:00/{BassBoomData.durationSpan}";
            if (FileTools.IsOpened)
                FileTools.CloseFile();
            SongInfo.IsEnabled = true;
        }
        else
        {
            PlayButton.IsEnabled = false;
            SongInfo.IsEnabled = false;
            GotArtistLabel.Text = "";
            GotGenreLabel.Text = "";
            GotTitleLabel.Text = "";
        }
    }

    private void CheckPath(object sender, TextChangedEventArgs e) =>
        EnablePlay();

    private void MakeDeviceDeterministic(object sender, RoutedEventArgs e) =>
        ((BassBoomData)DataContext).HandleDeviceButtons();

    private void HandleDurationValueChange(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (sliderUpdatedByCode)
            return;
        Debug.WriteLine($"Changed. {e.OldValue} -> {e.NewValue}");
        PlaybackPositioningTools.SeekToFrame((int)e.NewValue);
    }

    private void HandleVolumeValueChange(object sender, RangeBaseValueChangedEventArgs e)
    {
        Debug.WriteLine($"Vol. changed. {e.OldValue} -> {e.NewValue}");
        PlaybackTools.SetVolume(e.NewValue);
    }
}

public class BassBoomData
{
    internal string selectedDriver;
    internal string selectedDevice;
    internal bool paused = false;
    internal static int duration;
    internal static string durationSpan;
    internal static Id3V1Metadata v1 = null;
    internal static Id3V2Metadata v2 = null;
    private Thread sliderUpdate = new(UpdateSlider);
    private readonly MainView view;
    private static Lyric lyricInstance = null;

    public async Task PlayAsync()
    {
        try
        {
            if (!paused)
                FileTools.OpenFile(view.PathToMp3.Text);
            paused = false;

            // Enable and disable necessary buttons
            view.PlayButton.IsEnabled = false;
            view.SelectDevice.IsEnabled = false;
            view.SelectDriver.IsEnabled = false;
            view.DetermineDevice.IsEnabled = false;
            view.PathToMp3.IsEnabled = false;
            view.PauseButton.IsEnabled = true;
            view.StopButton.IsEnabled = true;

            // Try to open the lyrics
            string lyricsPath = Path.GetDirectoryName(view.PathToMp3.Text) + "/" + Path.GetFileNameWithoutExtension(view.PathToMp3.Text) + ".lrc";
            try
            {
                if (File.Exists(lyricsPath))
                    lyricInstance = LyricReader.GetLyrics(lyricsPath);
            }
            catch (Exception ex)
            {
                var dialog = MessageBoxManager.GetMessageBoxStandard(
                    "Basolia Warning!",
                    "Basolia has encountered a failure trying to open the lyrics file. You may not be able to view the song lyrics.\n\n" +
                   $"{ex.Message}", ButtonEnum.Ok);
                await dialog.ShowAsync();
            }

            // Determine the duration
            view.durationRemain.Maximum = duration;
            view.durationRemain.IsSnapToTickEnabled = true;
            view.durationRemain.TickFrequency = AudioInfoTools.GetBufferSize();
            sliderUpdate.Start(view);
            await PlaybackTools.PlayAsync();
        }
        catch (BasoliaException bex)
        {
            var dialog = MessageBoxManager.GetMessageBoxStandard(
                "Basolia Error!",
                "We apologize for your inconvenience, but BassBoom can't perform this operation as Basolia encountered the following error:\n\n" +
               $"{bex.Message}", ButtonEnum.Ok);
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            var dialog = MessageBoxManager.GetMessageBoxStandard(
                "BassBoom Error!",
                "We apologize for your inconvenience, but BassBoom can't perform this operation:\n\n" +
               $"{ex.Message}", ButtonEnum.Ok);
            await dialog.ShowAsync();
        }
        finally
        {
            if (FileTools.IsOpened && !paused)
                FileTools.CloseFile();
            view.PlayButton.IsEnabled = true;
            view.PauseButton.IsEnabled = false;
            view.StopButton.IsEnabled = false;
            HandleDeviceButtons();
        }
    }

    public void Pause()
    {
        try
        {
            PlaybackTools.Pause();
            sliderUpdate = new(UpdateSlider);
        }
        catch (BasoliaException bex)
        {
            var dialog = MessageBoxManager.GetMessageBoxStandard(
                "Basolia Error!",
                "We apologize for your inconvenience, but BassBoom can't perform this operation as Basolia encountered the following error:\n\n" +
               $"{bex.Message}", ButtonEnum.Ok);
            dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            var dialog = MessageBoxManager.GetMessageBoxStandard(
                "BassBoom Error!",
                "We apologize for your inconvenience, but BassBoom can't perform this operation:\n\n" +
               $"{ex.Message}", ButtonEnum.Ok);
            dialog.ShowAsync();
        }
        finally
        {
            view.PlayButton.IsEnabled = true;
            view.SelectDevice.IsEnabled = false;
            view.SelectDriver.IsEnabled = false;
            view.DetermineDevice.IsEnabled = true;
            view.PathToMp3.IsEnabled = false;
            view.PauseButton.IsEnabled = false;
            view.StopButton.IsEnabled = true;
            paused = true;
        }
    }

    public void Stop()
    {
        try
        {
            PlaybackTools.Stop();
            sliderUpdate = new(UpdateSlider);
        }
        catch (BasoliaException bex)
        {
            var dialog = MessageBoxManager.GetMessageBoxStandard(
                "Basolia Error!",
                "We apologize for your inconvenience, but BassBoom can't perform this operation as Basolia encountered the following error:\n\n" +
               $"{bex.Message}", ButtonEnum.Ok);
            dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            var dialog = MessageBoxManager.GetMessageBoxStandard(
                "BassBoom Error!",
                "We apologize for your inconvenience, but BassBoom can't perform this operation:\n\n" +
               $"{ex.Message}", ButtonEnum.Ok);
            dialog.ShowAsync();
        }
        finally
        {
            if (FileTools.IsOpened)
                FileTools.CloseFile();
            view.PlayButton.IsEnabled = true;
            view.PauseButton.IsEnabled = false;
            view.StopButton.IsEnabled = false;
            view.lyricLine.Text = "";
            HandleDeviceButtons();
        }
    }

    public void SelectDriver()
    {
        try
        {
            var drivers = DeviceTools.GetDrivers();
            var driverArray = drivers.Keys.ToArray();
            var selection = new SelectionWindow();
            selection.selection.ItemsSource = driverArray;
            selection.SelectInfo.Text = $"Select a driver. {driverArray.Length} drivers found in your system.";
            selection.Closed += (s, e) =>
            {
                string answer = selection.SelectionInput;
                selectedDriver = answer;
                DeviceTools.SetActiveDriver(selectedDriver);
                view.SelectDevice.IsEnabled = true;
            };
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                selection.ShowDialog(desktop.MainWindow);
        }
        catch (BasoliaException bex)
        {
            var dialog = MessageBoxManager.GetMessageBoxStandard(
                "Basolia Error!",
                "We apologize for your inconvenience, but BassBoom can't perform this operation as Basolia encountered the following error:\n\n" +
               $"{bex.Message}", ButtonEnum.Ok);
            dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            var dialog = MessageBoxManager.GetMessageBoxStandard(
                "BassBoom Error!",
                "We apologize for your inconvenience, but BassBoom can't perform this operation:\n\n" +
               $"{ex.Message}", ButtonEnum.Ok);
            dialog.ShowAsync();
        }
    }

    public void SelectDevice()
    {
        try
        {
            if (string.IsNullOrEmpty(selectedDriver))
            {
                var dialog = MessageBoxManager.GetMessageBoxStandard(
                "BassBoom Error!",
                "Select a driver first.", ButtonEnum.Ok);
                dialog.ShowAsync();
                return;
            }
            string activeDevice = selectedDevice;
            var devices = DeviceTools.GetDevices(selectedDriver, ref activeDevice);
            var deviceArray = devices.Keys.ToArray();
            var selection = new SelectionWindow();
            selection.selection.ItemsSource = deviceArray;
            selection.SelectInfo.Text = $"Select a device for the {selectedDriver} driver. {deviceArray.Length} devices found.";
            selection.Closed += (s, e) =>
            {
                string answer = selection.SelectionInput;
                selectedDevice = answer;
                DeviceTools.SetActiveDevice(selectedDriver, selectedDevice);
            };
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                selection.ShowDialog(desktop.MainWindow);
        }
        catch (BasoliaException bex)
        {
            var dialog = MessageBoxManager.GetMessageBoxStandard(
                "Basolia Error!",
                "We apologize for your inconvenience, but BassBoom can't perform this operation as Basolia encountered the following error:\n\n" +
               $"{bex.Message}", ButtonEnum.Ok);
            dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            var dialog = MessageBoxManager.GetMessageBoxStandard(
                "BassBoom Error!",
                "We apologize for your inconvenience, but BassBoom can't perform this operation:\n\n" +
               $"{ex.Message}", ButtonEnum.Ok);
            dialog.ShowAsync();
        }
    }

    public void SongInfo()
    {
        try
        {
            var info = new DynamicInfoWindow();
            info.DynamicGrid.Children.AddRange(
                new[]
                {
                    new TextBlock() { Text = $"Artist: {(!string.IsNullOrEmpty(v2.Artist) ? v2.Artist : !string.IsNullOrEmpty(v1.Artist) ? v1.Artist : "")}" },
                    new TextBlock() { Text = $"Title: {(!string.IsNullOrEmpty(v2.Title) ? v2.Title : !string.IsNullOrEmpty(v1.Title) ? v1.Title : "")}" },
                    new TextBlock() { Text = $"Album: {(!string.IsNullOrEmpty(v2.Album) ? v2.Album : !string.IsNullOrEmpty(v1.Album) ? v1.Album : "")}" },
                    new TextBlock() { Text = $"Genre: {(!string.IsNullOrEmpty(v2.Genre) ? v2.Genre : !string.IsNullOrEmpty(v1.Genre.ToString()) ? v1.Genre.ToString() : "")}" },
                    new TextBlock() { Text = $"Comment: {(!string.IsNullOrEmpty(v2.Comment) ? v2.Comment : !string.IsNullOrEmpty(v1.Comment) ? v1.Comment : "")}" },
                    new TextBlock() { Text = $"Duration: {durationSpan}" },
                    new TextBlock() { Text = $"Lyrics: {(lyricInstance is not null ? $"{lyricInstance.Lines.Count} lines" : "No lyrics")}" },
                }
            );
            for (int i = 0; i < info.DynamicGrid.Children.Count; i++)
            {
                info.DynamicGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                info.DynamicGrid.Children[i].SetValue(Grid.RowProperty, i);
            }
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                info.ShowDialog(desktop.MainWindow);
        }
        catch (BasoliaException bex)
        {
            var dialog = MessageBoxManager.GetMessageBoxStandard(
                "Basolia Error!",
                "We apologize for your inconvenience, but BassBoom can't perform this operation as Basolia encountered the following error:\n\n" +
               $"{bex.Message}", ButtonEnum.Ok);
            dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            var dialog = MessageBoxManager.GetMessageBoxStandard(
                "BassBoom Error!",
                "We apologize for your inconvenience, but BassBoom can't perform this operation:\n\n" +
               $"{ex.Message}", ButtonEnum.Ok);
            dialog.ShowAsync();
        }
    }

    internal void HandleDeviceButtons()
    {
        if (view.DetermineDevice.IsChecked.Value)
        {
            selectedDevice = null;
            selectedDriver = null;
            view.SelectDevice.IsEnabled = false;
            view.SelectDriver.IsEnabled = false;
        }
        else
        {
            if (!string.IsNullOrEmpty(selectedDriver))
                view.SelectDevice.IsEnabled = true;
            view.SelectDriver.IsEnabled = true;
        }
        view.DetermineDevice.IsEnabled = true;
        if (!paused)
            view.PathToMp3.IsEnabled = true;
        view.EnablePlay();
    }

    private static void UpdateSlider(object obj)
    {
        SpinWait.SpinUntil(() => PlaybackTools.Playing);
        var view = (MainView)obj;
        Dispatcher.UIThread.Invoke(() => {
            view.sliderUpdatedByCode = true;
            view.durationRemain.Value = 0;
            view.GotDurationLabel.Text = $"00:00:00/{durationSpan}";
            view.sliderUpdatedByCode = false;
        });
        while (PlaybackTools.Playing)
        {
            int position = PlaybackPositioningTools.GetCurrentDuration();
            var positionSpan = PlaybackPositioningTools.GetCurrentDurationSpan();
            string positionSpanString = positionSpan.ToString();
            string positionLyric = lyricInstance is not null ? lyricInstance.GetLastLineCurrent() : "";
            Dispatcher.UIThread.Invoke(() => {
                view.sliderUpdatedByCode = true;
                view.durationRemain.Value = position;
                view.GotDurationLabel.Text = $"{positionSpanString}/{durationSpan}";
                view.lyricLine.Text = positionLyric;
                view.sliderUpdatedByCode = false;
            });
        }
    }

    internal BassBoomData(MainView window)
    {
        view = window;
    }
}
