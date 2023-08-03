using System;
using Avalonia.Platform.Storage;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using Avalonia.Threading;

namespace jarinfectionscanneruniversal
{
	public class Scanner
	{
		private readonly IStorageItem? scanDirectory;
		private readonly TextBlock outputTextBlock;
		
		private static readonly byte[][] fileSignatures = new byte[][]
		{
			new byte[] { 0x38, 0x54, 0x59, 0x04, 0x10, 0x35, 0x54, 0x59, 0x05, 0x10, 0x2E, 0x54, 0x59, 0x06, 0x10, 0x32, 0x54, 0x59, 0x07, 0x10, 0x31, 0x54, 0x59, 0x08, 0x10, 0x37, 0x54, 0x59, 0x10, 0x06, 0x10, 0x2E, 0x54, 0x59, 0x10, 0x07, 0x10, 0x31, 0x54, 0x59, 0x10, 0x08, 0x10, 0x34, 0x54, 0x59, 0x10, 0x09, 0x10, 0x34, 0x54, 0x59, 0x10, 0x0A, 0x10, 0x2E, 0x54, 0x59, 0x10, 0x0B, 0x10, 0x31, 0x54, 0x59, 0x10, 0x0C, 0x10, 0x33, 0x54, 0x59, 0x10, 0x0D, 0x10, 0x30, 0x54, 0xB7 },
			new byte[] { 0x68, 0x54, 0x59, 0x04, 0x10, 0x74, 0x54, 0x59, 0x05, 0x10, 0x74, 0x54, 0x59, 0x06, 0x10, 0x70, 0x54, 0x59, 0x07, 0x10, 0x3a, 0x54, 0x59, 0x08, 0x10, 0x2f, 0x54, 0x59, 0x10, 0x06, 0x10, 0x2f, 0x54, 0x59, 0x10, 0x07, 0x10, 0x66, 0x54, 0x59, 0x10, 0x08, 0x10, 0x69, 0x54, 0x59, 0x10, 0x09, 0x10, 0x6c, 0x54, 0x59, 0x10, 0x0a, 0x10, 0x65, 0x54, 0x59, 0x10, 0x0b, 0x10, 0x73, 0x54, 0x59, 0x10, 0x0c, 0x10, 0x2e, 0x54, 0x59, 0x10, 0x0a, 0x10, 0x73, 0x54, 0x59, 0x10, 0x0e, 0x10, 0x6b, 0x54, 0x59, 0x10, 0x0f, 0x10, 0x79, 0x54, 0x59, 0x10, 0x10, 0x10, 0x72, 0x54, 0x59, 0x10, 0x11, 0x10, 0x61, 0x54, 0x59, 0x10, 0x12, 0x10, 0x67, 0x54, 0x59, 0x10, 0x13, 0x10, 0x65, 0x54, 0x59, 0x10, 0x14, 0x10, 0x2e, 0x54, 0x59, 0x10, 0x15, 0x10, 0x64 },
			new byte[] { 0x2d, 0x54, 0x59, 0x04, 0x10, 0x6a, 0x54, 0x59, 0x05, 0x10, 0x61, 0x54, 0x59, 0x06, 0x10, 0x72 }
		};
		public readonly List<string> caughtFiles = new();
		public readonly List<string> problematicFiles = new();
		private bool detectedFile = false;
		
		public Scanner(IStorageItem? _scanDirectory, TextBlock _outputTextBlock)
		{
			scanDirectory = _scanDirectory;
			outputTextBlock = _outputTextBlock;
			detectedFile = false;
		}
		
		public async Task<bool> Scan()
		{
			if (scanDirectory != null)
			{
				try {
					await Task.Run(() =>
					{
						#pragma warning disable IDE0057
						string[] files = Directory.GetFiles(scanDirectory.Path.ToString().Substring(7), "*.jar", SearchOption.AllDirectories);
						#pragma warning restore IDE0057
						// Directory.GetFiles(scanDirectory.Path.ToString().Substring(7), "*.jar", SearchOption.AllDirectories).ToList().ForEach(file =>
						foreach (string file in files)
						{
							SendMessageToOutput(string.Format("Scanning File: {0}", file));
							ZipArchive zipArchive = ZipFile.OpenRead(file);

							bool flagged = false;

							foreach (ZipArchiveEntry entry in zipArchive.Entries)
							{
								if (!entry.Name.EndsWith(".class"))
									continue;

								Stream fileStream = entry.Open();
								if (entry.Length > int.MaxValue)
								{
									SendMessageToOutput(string.Format("WARN: The file [0] contains a class larger than 2 GB. This could be either terrible coding by the jarfile developer, or a sign of infection", file));
									problematicFiles.Add(file);
								}
								byte[] buffer = new byte[(int)entry.Length];

								fileStream.Read(buffer, 0, (int)entry.Length);

								foreach (byte[] signature in fileSignatures)
								{
									bool eof = false;
									int index = 0;
									while (!eof)
									{
										int matchedIndex = Array.IndexOf(buffer, signature[0], index);
										if (matchedIndex != -1)
										{
											// Possible trace
											Range range = new(matchedIndex, matchedIndex + signature.Length);
											if (buffer.Take(range).SequenceEqual(signature))
											{
												// MATCH! This indicates a signature was found in the file
												flagged = true;
												eof = true;
												detectedFile = true;
												SendMessageToOutput(string.Format("!!!!! FOUND VIRUS SIGNATURE IN {0}!", file));
												caughtFiles.Add(file);
												break;
											} else index = matchedIndex + 1;
										} else eof = true;
									}
									if (flagged) break;

								}
								if (flagged) break;
								
							}
							if (!flagged) SendMessageToOutput(string.Format("File is clean: {0}", file));
							zipArchive.Dispose();
						}
					});
				}
				catch (UnauthorizedAccessException e)
				{
					SendMessageToOutput(string.Format("We do not have access to the folder or one of its subfolders: \n{0}", e.Message));
				}
				catch (Exception e)
				{
					SendMessageToOutput(string.Format("An error occured: \n{0}", e));
				}
				return detectedFile;
			}
			return detectedFile;
		}
		void SendMessageToOutput(string msg)
		{
			Dispatcher.UIThread.Invoke(() => {
				outputTextBlock.Text += string.Format("\n[{0}]: {1}", DateTime.Now.ToString(MainWindow.dateFormat), msg);
			});
		}
	}
}