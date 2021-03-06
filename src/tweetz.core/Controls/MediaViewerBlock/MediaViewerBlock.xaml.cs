﻿using System.Windows.Controls;
using System.Windows.Input;
using tweetz.core.ViewModels;

namespace tweetz.core.Controls.MediaViewerBlock
{
    public partial class MediaViewerBlock : UserControl
    {
        public MediaViewerBlock()
        {
            InitializeComponent();
            DataContextChanged += UpdateDataContext;
        }

        private MediaViewerBlockViewModel ViewModel => (MediaViewerBlockViewModel)DataContext;

        private void UpdateDataContext(object _, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            MediaControls.DataContext = e.NewValue;
            ViewModel.PropertyChanged += (_, e) => { if (string.CompareOrdinal(e.PropertyName, nameof(MediaViewerBlockViewModel.Uri)) == 0) ShowLoadingIndicator(); };
        }

        private void ShowLoadingIndicator()
        {
            MediaElement.Stop();
            ViewModel.ErrorMessage = null;
            MediaControls.Visibility = System.Windows.Visibility.Collapsed;
            LoadingIndicator.Visibility = System.Windows.Visibility.Visible;
        }

        private void ShowMediaControls()
        {
            LoadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
            MediaControls.Visibility = System.Windows.Visibility.Visible;
        }

        private void Popup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.Uri = null;
            e.Handled = true;
        }

        private void Popup_Opened(object sender, System.EventArgs e)
        {
            ShowLoadingIndicator();
        }

        private void Popup_Closed(object sender, System.EventArgs e)
        {
            ViewModel.Uri = null;
        }

        private void Popup_KeyDown(object sender, KeyEventArgs e)
        {
            ViewModel.Uri = null;
            e.Handled = true;
        }

        private void MediaElement_MediaOpened(object sender, System.Windows.RoutedEventArgs e)
        {
            ShowMediaControls();
            MediaElement.Play();
        }

        private void MediaElement_MediaFailed(object sender, System.Windows.ExceptionRoutedEventArgs e)
        {
            LoadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
            ViewModel.ErrorMessage = e.ErrorException.Message;
        }
    }
}