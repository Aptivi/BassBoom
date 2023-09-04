
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
using Avalonia.Interactivity;
using Avalonia.Threading;
using BassBoom.Basolia;
using BassBoom.Basolia.Devices;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Basolia.Playback;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BassBoom.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new BassBoomData(this);
        PathToMp3.TextChanged += CheckPath;
        DetermineDevice.IsCheckedChanged += MakeDeviceDeterministic;
    }

    internal void EnablePlay()
    {
        if (File.Exists(PathToMp3.Text) &&
            ((!DetermineDevice.IsChecked.Value && !string.IsNullOrEmpty(((BassBoomData)DataContext).selectedDevice)) ||
               DetermineDevice.IsChecked.Value))
        {
            PlayButton.IsEnabled = true;
            GetDuration.IsEnabled = true;
        }
        else
        {
            PlayButton.IsEnabled = false;
            GetDuration.IsEnabled = false;
        }
    }

    private void CheckPath(object sender, TextChangedEventArgs e) =>
        EnablePlay();

    private void MakeDeviceDeterministic(object sender, RoutedEventArgs e) =>
        ((BassBoomData)DataContext).HandleDeviceButtons();
}

public class BassBoomData
{
    internal string selectedDriver;
    internal string selectedDevice;
    internal bool paused = false;
    internal static int duration;
    internal static string durationSpan;
    private Thread sliderUpdate = new(UpdateSlider);
    private readonly MainView view;

    public void GetDuration()
    {
        try
        {
            FileTools.OpenFile(view.PathToMp3.Text);
            durationSpan = AudioInfoTools.GetDurationSpan(true).ToString();
            string durationNoScan = AudioInfoTools.GetDurationSpan(false).ToString();
            view.GotDurationLabel.Text = $"[{durationSpan} with scan, {durationNoScan} no scan]";
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
        }
    }

    public async Task PlayAsync()
    {
        try
        {
            if (!paused)
                FileTools.OpenFile(view.PathToMp3.Text);
            paused = false;
            view.PlayButton.IsEnabled = false;
            view.GetDuration.IsEnabled = false;
            view.SelectDevice.IsEnabled = false;
            view.SelectDriver.IsEnabled = false;
            view.DetermineDevice.IsEnabled = false;
            view.PathToMp3.IsEnabled = false;
            view.PauseButton.IsEnabled = true;
            view.StopButton.IsEnabled = true;
            duration = AudioInfoTools.GetDuration(true);
            durationSpan = AudioInfoTools.GetDurationSpan(true).ToString();
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
            view.GetDuration.IsEnabled = true;
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
            view.GetDuration.IsEnabled = false;
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
            view.GetDuration.IsEnabled = true;
            view.PauseButton.IsEnabled = false;
            view.StopButton.IsEnabled = false;
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
            view.durationRemain.Value = 0;
            view.GotDurationLabel.Text = $"00:00:00/{durationSpan}";
        });
        while (PlaybackTools.Playing)
        {
            int position = PlaybackPositioningTools.GetCurrentDuration();
            string positionSpan = PlaybackPositioningTools.GetCurrentDurationSpan().ToString();
            double remaining = 100 * (position / (double)duration);
            Dispatcher.UIThread.Invoke(() => {
                view.durationRemain.Value = remaining;
                view.GotDurationLabel.Text = $"{positionSpan}/{durationSpan}";
            });
        }
    }

    internal BassBoomData(MainView window)
    {
        view = window;
    }
}
