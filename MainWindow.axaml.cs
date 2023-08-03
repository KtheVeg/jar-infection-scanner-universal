using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace jarinfectionscanneruniversal
{
	public partial class MainWindow : Window
	{
		private TextBox? pathTextBlock;
		private readonly TextBlock? outputTextBlock;
		private readonly ScrollViewer? scrollViewer;

		public static readonly string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
		
		public MainWindow()
		{
			// Generated with Avalonia.NameGenerator
			InitializeComponent();

			// Get components
			pathTextBlock = this.FindControl<TextBox>("pathInput");
			outputTextBlock = this.FindControl<TextBlock>("output");
			scrollViewer = this.FindControl<ScrollViewer>("outputScroll");
		}

		// public void Button_Click(object sender, RoutedEventArgs e)
		// {
		// 	// Change button text when button is clicked.
		// 	Button button = (Button)sender;
		// 	button.Content = "Hello, Avalonia!";
		// }

		public async void BrowseFilesClick(object sender, RoutedEventArgs e)
		{
			// Create open folder dialog.
			FolderPickerOpenOptions options = new();
			IReadOnlyList<IStorageFolder> paths = await this.StorageProvider.OpenFolderPickerAsync(options);
			
			pathTextBlock ??= this.FindControl<TextBox>("pathInput");
			
			if (paths.Count > 0 && pathTextBlock != null)
				pathTextBlock.Text = paths[0].Path.ToString().Substring(7);
		}
		public async void ScanClick(object sender, RoutedEventArgs e)
		{
			// pathTextBlock ??= this.FindControl<TextBox>("pathInput");
			// outputTextBlock ??= this.FindControl<TextBlock>("output");
			
			// Construct an IStorageItem from the path provided in the text box.
			// if (pathTextBlock != null && outputTextBlock != null)
			// {
            //     string path = pathTextBlock.Text ?? "";
			// 	IStorageItem? scanDirectory = await this.StorageProvider.TryGetFolderFromPathAsync(path);
				
			// 	if (scanDirectory != null)
			// 	{
			// 		outputTextBlock.Text += "\nScanning " + scanDirectory.Path.ToString().Substring(7) + "...";
			// 		Scanner scanner = new Scanner(scanDirectory);
			// 	} else {
			// 		DateTime now = DateTime.Now;
			// 		outputTextBlock.Text += string.Format("\n[{0}] ", now.ToString(dateFormat));
			// 		outputTextBlock.Text += "The path you provided is invalid.";
			// 	}
				
			// } else throw new NullReferenceException("Path block somehow doesn't exist or is null.");

			#pragma warning disable CS8602 // Dereference of a possibly null reference.
			if (pathTextBlock.Text != null)
			{
				// Parse the directory entered by the user
				IStorageItem? scanDirectory = await this.StorageProvider.TryGetFolderFromPathAsync(pathTextBlock.Text);
				

				if (scanDirectory != null)
				{
					// Good directory. Start with scanning
					outputTextBlock.Text += string.Format("\n[{0}] Scanning: {1}...", DateTime.Now.ToString(dateFormat), scanDirectory.Path.ToString().Substring(7));
					Scanner scanner = new(scanDirectory, outputTextBlock, scrollViewer);
					await scanner.Scan();
					outputTextBlock.Text += string.Format("\n[{0}] Scan Finished.", DateTime.Now.ToString(dateFormat));
					if (scanner.caughtFiles.Count != 0)
					{
						outputTextBlock.Text += string.Format("\n\n\n\n\n[{0}] WARNING: ONE OR MORE INFECTED FILES WERE FOUND.\nView below for the affected files", DateTime.Now.ToString(dateFormat));
						foreach (string file in scanner.caughtFiles)
						{
							outputTextBlock.Text += "\n" + file;
							scrollViewer.ScrollToEnd();
						}
						outputTextBlock.Text += "\n\nIn addition to the found files, there were problematic files \nYou should delete these files.";
						foreach (string file in scanner.problematicFiles)
						{
							outputTextBlock.Text += "\n" + file;
							scrollViewer.ScrollToEnd();
						}
					} else {
						if (scanner.problematicFiles.Count != 0)
						{
							outputTextBlock.Text += "\n\nThere were a few files that had problems scanning. \nYou should delete these files.";
							foreach (string file in scanner.problematicFiles)
							{
								outputTextBlock.Text += "\n" + file;
								scrollViewer.ScrollToEnd();
							}
							outputTextBlock.Text += "\n\nThere were no infected files found.";
						}
						else
						{
							outputTextBlock.Text += "\n\nThere was no infected files found.";
							scrollViewer.ScrollToEnd();
						}
					}
				} else // Bad directory, let user know
					outputTextBlock.Text += string.Format("\n[{0}] The path you provided is invalid.", DateTime.Now.ToString(dateFormat));
			} else
				outputTextBlock.Text += string.Format("\n[{0}] The path you provided is invalid.", DateTime.Now.ToString(dateFormat));
			#pragma warning restore CS8602 // Dereference of a possibly null reference.

			
		}
		private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
	}
}