using System.Linq;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Timers;
using System.IO;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using Slideshow.View.ViewModels;

namespace Slideshow.View
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		public MainViewModel ViewModel { get { return (MainViewModel) DataContext; } }

		private static readonly string[] AcceptedFileExtentions =
		{
			".jpg",
			".jpeg",
			".png"
		};

		private int _pictureIndex;
		private readonly IList<string> _pictures = new List<string>();
		private readonly Timer _pictureTimer = new Timer();

		private void btnGoooo_Click(object sender, RoutedEventArgs e)
		{
			var selectedPath = txtPath.Text;

			if (string.IsNullOrEmpty(selectedPath))
			{
				var fbd = new System.Windows.Forms.FolderBrowserDialog();
				if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
				selectedPath = txtPath.Text = fbd.SelectedPath;
			}

			// Start
			_pictures.Clear();
			var paths = selectedPath.Split('%');
			foreach (var path in paths)
				FindFilesInDirectory(path);

			_pictures.Shuffle();
			_pictureTimer.Interval = 1;
			_pictureTimer.Elapsed += (o, args) =>
			{
				_pictureTimer.Stop();
				_pictureTimer.Interval = 1800;

				if (_pictures.Count > _pictureIndex)
				{
					// turn down for shuffle
					_pictures.Shuffle();
					_pictures.Shuffle();
					_pictures.Shuffle();

					_pictureIndex = 0;
				}

				Dispatcher.Invoke(
					new Action(() =>
					{
						pictureBoxSlideshowMask.Source =
							pictureBoxSlideshow.Source =
								new BitmapImage(
									new Uri(_pictures[_pictureIndex]));

						PathTextBox.Text = _pictures[_pictureIndex];

						_pictureTimer.Start();
					}));

				_pictureIndex++;
			};
			_pictureTimer.Start();
		}

		public void FindFilesInDirectory(string directory)
		{
			var real = Directory.Exists(directory);

			foreach (var file in Directory.GetFiles(directory))
			{
				var isRelevant = false;

				var path = file;
				foreach (var extension in AcceptedFileExtentions.Where(extension => path.ToLowerInvariant().EndsWith(extension)))
					isRelevant = true;

				if (isRelevant)
					_pictures.Add(file);
			}

			foreach (var direc in Directory.GetDirectories(directory))
				FindFilesInDirectory(direc);
		}

		private void PathTextBox_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(PathTextBox.Text))
				return;

			Process.Start("explorer.exe", string.Format("/select,\"{0}\"", PathTextBox.Text));
		}
	}

	public static class List
	{
		public static void Shuffle<T>(this IList<T> list)
		{
			var rng = new Random();
			var n = list.Count;
			while (n > 1)
			{
				n--;
				var k = rng.Next(n + 1);
				var value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}
