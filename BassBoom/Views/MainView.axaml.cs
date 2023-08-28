
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
using Avalonia.Input;
using Avalonia.Interactivity;
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
using System.Threading.Tasks;

namespace BassBoom.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new BassBoomData(this);
        PathToMp3.TextChanged += CheckPath;
    }

    public void CheckPath(object sender, TextChangedEventArgs e)
    {
        if (File.Exists(PathToMp3.Text) && (!string.IsNullOrEmpty(((BassBoomData)DataContext).selectedDevice)))
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
}

public class BassBoomData
{
    private readonly MainView view;
    internal string selectedDriver = "";
    internal string selectedDevice = "";

    public void GetDuration()
    {
        try
        {
            FileTools.OpenFile(view.PathToMp3.Text);
            int duration = AudioInfoTools.GetDuration(true);
            int durationNoScan = AudioInfoTools.GetDuration(false);
            view.DurationLabel.Text = $"Duration: [{duration} with scan, {durationNoScan} no scan]";
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
            FileTools.OpenFile(view.PathToMp3.Text);
            view.PlayButton.IsEnabled = false;
            view.GetDuration.IsEnabled = false;
            view.SelectDevice.IsEnabled = false;
            view.SelectDriver.IsEnabled = false;
            await PlaybackTools.PlayAsync();
            view.PlayButton.IsEnabled = true;
            view.GetDuration.IsEnabled = true;
            view.SelectDevice.IsEnabled = true;
            view.SelectDriver.IsEnabled = true;
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
            if (FileTools.IsOpened)
                FileTools.CloseFile();
        }
    }

    public void SelectDriver()
    {
        try
        {
            var drivers = DeviceTools.GetDrivers();
            var driverArray = drivers.Keys.ToArray();
            var selection = new SelectionWindow(new ObservableCollection<string>(driverArray));
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
            var selection = new SelectionWindow(new ObservableCollection<string>(deviceArray));
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

    internal BassBoomData(MainView window)
    {
        view = window;
    }
}
