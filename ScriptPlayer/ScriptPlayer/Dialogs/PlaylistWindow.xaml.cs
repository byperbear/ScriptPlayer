﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ScriptPlayer.Shared;
using ScriptPlayer.ViewModels;

namespace ScriptPlayer.Dialogs
{
    /// <summary>
    /// Interaction logic for PlaylistWindow.xaml
    /// </summary>
    public partial class PlaylistWindow : Window
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(MainViewModel), typeof(PlaylistWindow), new PropertyMetadata(default(PlaylistViewModel)));

        public MainViewModel ViewModel
        {
            get => (MainViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public PlaylistWindow(MainViewModel viewmodel)
        {
            ViewModel = viewmodel;
            ViewModel.Playlist.SelectedEntryMoved += PlaylistOnSelectedEntryMoved;
            InitializeComponent();
        }

        private void PlaylistOnSelectedEntryMoved(object sender, EventArgs eventArgs)
        {
            if (ViewModel.Playlist.SelectedEntry == null)
                return;

            lstEntries.ScrollIntoView(ViewModel.Playlist.SelectedEntry);
        }

        private void PlaylistEntry_DoubleClicked(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (!(item?.DataContext is PlaylistEntry entry))
                return;

            ViewModel.Playlist.RequestPlayEntry(entry);
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ViewModel.Playlist.AddEntries(files);
        }

        private void LstEntries_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.Playlist.SetSelectedItems(((ListBox) sender).SelectedItems.Cast<PlaylistEntry>());
        }

        private void ToolTip_OnOpened(object sender, RoutedEventArgs e)
        {
            var tooltip = (ToolTip) sender;
            PlaylistEntry entry = ((FrameworkElement)tooltip.PlacementTarget).DataContext as PlaylistEntry;

            GifPlayer player = tooltip.GetChildOfType<GifPlayer>();
            TextBlock text = tooltip.GetChildOfType<TextBlock>();

            string gifFile = ViewModel.GetRelatedFile(entry.Fullname, new[] { "gif" });
            if (!string.IsNullOrEmpty(gifFile))
            {
                player.Load(gifFile);
            }

            text.Text = entry.Shortname + " [" + (entry.Duration?.ToString("hh\\:mm\\:ss") ?? "?") + "]";
        }

        private void ToolTip_OnClosed(object sender, RoutedEventArgs e)
        {
            var tooltip = (ToolTip)sender;
            GifPlayer player = tooltip.GetChildOfType<GifPlayer>();
            player.Close();
        }
    }

    public static class DependecyObjectExtensions
    {
        public static T GetChildOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
