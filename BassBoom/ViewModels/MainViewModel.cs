
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

using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using BassBoom.Basolia;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;
using BassBoom.Basolia.Lyrics;
using BassBoom.Basolia.Playback;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BassBoom.Views;
using System.Collections.Generic;
using BassBoom.Basolia.Format.Cache;

namespace BassBoom.ViewModels;

public class MainViewModel : ViewModelBase
{
    internal bool advance = false;
    internal static bool paused = false;
    internal static int duration;
    internal static string durationSpan;
    internal static Id3V1Metadata v1 = null;
    internal static Id3V2Metadata v2 = null;
    internal static FrameInfo frameInfo = null;
    internal static MainView view;
    internal static string selectedPath = "";
    private Thread sliderUpdate = new(UpdateSlider);
    private ObservableCollection<string> musicFileSelect = new();
    private static Lyric lyricInstance = null;
    private readonly List<CachedSongInfo> cachedInfos = new();

    private FilePickerFileType MusicFiles => new("Music files")
    {
        // TODO: When adding support for macOS, use this: AppleUniformTypeIdentifiers
        Patterns = new[] { "*.mp3", "*.mp2", "*.mpa", "*.mpg", "*.mpga" },
        MimeTypes = new[] { "audio/mpeg", "audio/x-mpeg", "audio/mpeg3", "audio/x-mpeg3" }
    };

    public ObservableCollection<string> MusicFileSelect =>
        musicFileSelect;

    public async Task AddSong()
    {
        var results = await view.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true,
            Title = "BassBoom - Add music files to playlist",
            FileTypeFilter = new FilePickerFileType[] { MusicFiles }.ToList(),
        });
        if (results.Count > 0)
        {
            foreach (var result in results)
            {
                var file = result.Path.LocalPath;
                MusicFileSelect.Add(file);
            }
            view.PathsToMp3.SelectedIndex = 0;
        }
    }

    public async Task PopulateAsync()
    {
        if (view.PathsToMp3.SelectedValue is null)
            return;
        if (PlaybackTools.State != PlaybackState.Stopped)
            return;
        string file = (string)view.PathsToMp3.SelectedValue;
        await PopulateSongInfoAsync(file);
    }

    public async Task PopulateSongInfoAsync(string file)
    {
        if (view.PathsToMp3.SelectedValue is null)
            return;
        if (PlaybackTools.State != PlaybackState.Stopped)
            return;
        if (cachedInfos.Any((csi) => csi.MusicPath == file))
        {
            var instance = cachedInfos.Single((csi) => csi.MusicPath == file);
            selectedPath = instance.MusicPath;
            duration = instance.Duration;
            durationSpan = instance.DurationSpan;
            v1 = instance.MetadataV1;
            v2 = instance.MetadataV2;
            frameInfo = instance.FrameInfo;
            lyricInstance = instance.LyricInstance;
        }
        else
        {
            FileTools.OpenFile(file);
            selectedPath = file;
            AudioInfoTools.GetId3Metadata(out var v1, out var v2);
            duration = AudioInfoTools.GetDuration(true);
            durationSpan = AudioInfoTools.GetDurationSpanFromSamples(duration).ToString();
            MainViewModel.v1 = v1;
            MainViewModel.v2 = v2;
            frameInfo = AudioInfoTools.GetFrameInfo();
            var formatInfo = FormatTools.GetFormatInfo();

            // Try to open the lyrics
            string lyricsPath = Path.GetDirectoryName(selectedPath) + "/" + Path.GetFileNameWithoutExtension(selectedPath) + ".lrc";
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

            if (FileTools.IsOpened)
                FileTools.CloseFile();
            var instance = new CachedSongInfo(file, v1, v2, duration, formatInfo, frameInfo, lyricInstance);
            cachedInfos.Add(instance);
        }
        view.durationRemain.IsEnabled = false;
    }

    public async Task PlayAsync()
    {
        advance = true;
        foreach (string file in musicFileSelect.Skip(view.PathsToMp3.SelectedIndex))
        {
            if (!advance)
                break;
            view.PathsToMp3.SelectedValue = file;
            await ProcessPlayAsync();
        }
    }

    public async Task ProcessPlayAsync()
    {
        try
        {
            if (view.PathsToMp3.SelectedValue is null)
                return;
            if (!paused)
                FileTools.OpenFile(selectedPath);
            paused = false;

            // Enable and disable necessary buttons
            view.PlayButton.IsEnabled = false;
            view.PauseButton.IsEnabled = true;
            view.StopButton.IsEnabled = true;
            view.durationRemain.IsEnabled = true;

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
            {
                PlaybackPositioningTools.SeekToTheBeginning();
                view.durationRemain.Value = 0;
                FileTools.CloseFile();
            }
            sliderUpdate = new(UpdateSlider);
            view.PlayButton.IsEnabled = true;
            view.PauseButton.IsEnabled = false;
            view.StopButton.IsEnabled = false;
            view.durationRemain.IsEnabled = false;
            lyricInstance = null;
            view.EnablePlay();
        }
    }

    public void Pause()
    {
        try
        {
            PlaybackTools.Pause();
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
            view.PauseButton.IsEnabled = false;
            view.StopButton.IsEnabled = true;
            view.lyricLine.Text = "";
            lyricInstance = null;
            paused = true;
            advance = false;
        }
    }

    public void Stop()
    {
        try
        {
            PlaybackTools.Stop();
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
            view.durationRemain.IsEnabled = false;
            view.lyricLine.Text = "";
            lyricInstance = null;
            advance = false;
            view.EnablePlay();
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
                    new TextBlock() { Text = $"" },
                    new TextBlock() { Text = $"--- Frame info ---" },
                    new TextBlock() { Text = $"Version: {frameInfo.Version}" },
                    new TextBlock() { Text = $"Layer: {frameInfo.Layer}" },
                    new TextBlock() { Text = $"Rate: {frameInfo.Rate}" },
                    new TextBlock() { Text = $"Mode: {frameInfo.Mode}" },
                    new TextBlock() { Text = $"Mode Ext: {frameInfo.ModeExt}" },
                    new TextBlock() { Text = $"Frame Size: {frameInfo.FrameSize}" },
                    new TextBlock() { Text = $"Flags: {frameInfo.Flags}" },
                    new TextBlock() { Text = $"Emphasis: {frameInfo.Emphasis}" },
                    new TextBlock() { Text = $"Bitrate: {frameInfo.BitRate}" },
                    new TextBlock() { Text = $"ABR Rate: {frameInfo.AbrRate}" },
                    new TextBlock() { Text = $"VBR: {frameInfo.Vbr}" },
                    new TextBlock() { Text = $"" },
                    new TextBlock() { Text = $"--- Texts and Extras ---" },
                }
            );
            foreach (var text in v2.Texts)
                info.DynamicGrid.Children.Add(new TextBlock() { Text = $"T - {text.Item1}: {text.Item2}" });
            foreach (var text in v2.Extras)
                info.DynamicGrid.Children.Add(new TextBlock() { Text = $"E - {text.Item1}: {text.Item2}" });
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

    private static void UpdateSlider(object obj)
    {
        SpinWait.SpinUntil(() => PlaybackTools.Playing);
        var view = (MainView)obj;
        Dispatcher.UIThread.Invoke(() => {
            view.sliderUpdatedByCode = true;
            view.durationRemain.Value = 0;
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
                view.lyricLine.Text = positionLyric;
                view.sliderUpdatedByCode = false;
            });
        }
    }
}
