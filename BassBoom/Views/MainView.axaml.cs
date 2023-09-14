
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
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using BassBoom.Basolia;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Basolia.Lyrics;
using BassBoom.Basolia.Playback;
using BassBoom.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BassBoom.Views;

public partial class MainView : UserControl
{
    public IStorageProvider StorageProvider
    {
        get
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                return desktop.MainWindow.StorageProvider;
            return null;
        }
    }
    internal bool sliderUpdatedByCode = false;

    public MainView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            // These functions depend on Basolia being initialized.
            MainViewModel.view = this;
            DataContext = new MainViewModel();
            PathsToMp3.SelectionChanged += (_, _) => EnablePlay();
            durationRemain.ValueChanged += HandleDurationValueChange;
            volumeSlider.ValueChanged += HandleVolumeValueChange;
            volumeSlider.Value = PlaybackTools.GetVolume().baseLinear;
        }
    }

    internal void EnablePlay()
    {
        if (PathsToMp3.SelectedValue is null)
            return;
        string selected = (string)PathsToMp3.SelectedValue;
        if (File.Exists(selected))
        {
            PlayButton.IsEnabled = true;
            SongInfo.IsEnabled = true;
            ((MainViewModel)DataContext).Populate();
        }
        else
        {
            PlayButton.IsEnabled = false;
            SongInfo.IsEnabled = false;
        }
    }

    private void CheckPath(object sender, TextChangedEventArgs e) =>
        EnablePlay();

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
