using Avalonia;
using Avalonia.Platform.Storage;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System;

namespace jarinfectionscanneruniversal
{
    public partial class MainWindow : Window
    {
        TextBox? pathTextBlock;
        public MainWindow()
        {
            // Generated with Avalonia.NameGenerator
            InitializeComponent();
        }

        public void button_Click(object sender, RoutedEventArgs e)
        {
            // Change button text when button is clicked.
            var button = (Button)sender;
            button.Content = "Hello, Avalonia!";
        }
        public async void browseFiles_Click(object sender, RoutedEventArgs e)
        {
            // Create open folder dialog.
            FolderPickerOpenOptions options = new FolderPickerOpenOptions();
            IReadOnlyList<IStorageFolder> paths = await this.StorageProvider.OpenFolderPickerAsync(options);
            if (pathTextBlock == null)
            {
                pathTextBlock = this.FindControl<TextBox>("pathInput");
            }
            if (paths.Count > 0 && pathTextBlock != null)
            {

                pathTextBlock.Text = paths[0].Path.ToString();

            }
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}