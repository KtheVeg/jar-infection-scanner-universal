using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace jarinfectionscanneruniversal
{
	public partial class MainWindow : Window
	{
		private TextBox? pathTextBlock;
		private readonly TextBlock? outputTextBlock;
		private readonly ScrollViewer? scrollViewer;
		private readonly ProgressBar? progressBar;

		public static readonly string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";
		
		public MainWindow()
		{
			// Generated with Avalonia.NameGenerator
			InitializeComponent();

			// Get components
			pathTextBlock = this.FindControl<TextBox>("pathInput");
			outputTextBlock = this.FindControl<TextBlock>("output");
			scrollViewer = this.FindControl<ScrollViewer>("outputScroll");
			progressBar = this.FindControl<ProgressBar>("progress");
		}


		public async void BrowseFilesClick(object sender, RoutedEventArgs e)
		{
			// Create open folder dialog.
			FolderPickerOpenOptions options = new();
			IReadOnlyList<IStorageFolder> paths = await this.StorageProvider.OpenFolderPickerAsync(options);
			
			pathTextBlock ??= this.FindControl<TextBox>("pathInput");
			
			if (paths.Count > 0 && pathTextBlock != null)
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					pathTextBlock.Text = paths[0].Path.ToString().Substring(8);
				}
				else
				{
					pathTextBlock.Text = paths[0].Path.ToString().Substring(7);
				}
		}
		public async void ScanClick(object sender, RoutedEventArgs e)
		{

			#pragma warning disable CS8602 // Dereference of a possibly null reference.
			if (pathTextBlock.Text != null)
			{
				pathTextBlock.Text ??= "";
    			pathTextBlock.Text = pathTextBlock.Text.Replace("\"","");
				// Parse the directory entered by the user
				IStorageItem? scanDirectory = await this.StorageProvider.TryGetFolderFromPathAsync(pathTextBlock.Text);
				

				if (scanDirectory != null)
				{
					this.FindControl<Button>("scanButton").IsEnabled = false;
					// Good directory. Start with scanning
					outputTextBlock.Text = string.Format("\n[{0}] Scanning: {1}...", DateTime.Now.ToString(dateFormat), scanDirectory.Path.ToString().Substring(7));
					Scanner scanner = new(scanDirectory, outputTextBlock, scrollViewer, progressBar);
					progressBar.ShowProgressText = true;
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
						outputTextBlock.Text += "\n\nIn addition to the found files, there were problematic files. Consider deleting these files and redownloading them.";
						foreach (string file in scanner.problematicFiles)
						{
							outputTextBlock.Text += "\n" + file;
							outputTextBlock.Text += "\n" + scanner.problematicReason[scanner.problematicFiles.IndexOf(file)];
							scrollViewer.ScrollToEnd();
						}
					} else {
						if (scanner.problematicFiles.Count != 0)
						{
							outputTextBlock.Text += "\n\nThere were a few files that had problems scanning. Consider deleting these files and redownloading them.";
							foreach (string file in scanner.problematicFiles)
							{
								outputTextBlock.Text += "\n" + file;
								outputTextBlock.Text += "\n(Reason: " + scanner.problematicReason[scanner.problematicFiles.IndexOf(file)] + ")";
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
					this.FindControl<Button>("scanButton").IsEnabled = true;
				} else // Bad directory, let user know
					outputTextBlock.Text += string.Format("\n[{0}] The path you provided is invalid.", DateTime.Now.ToString(dateFormat));
			} else
				outputTextBlock.Text += string.Format("\n[{0}] The path you provided is invalid.", DateTime.Now.ToString(dateFormat));
			#pragma warning restore CS8602 // Dereference of a possibly null reference.

			
		}
		private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
	}
}