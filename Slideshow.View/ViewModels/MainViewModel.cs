using ExifLib;
using Slideshow.View.Common;
using Slideshow.View.Extentions;
using Slideshow.View.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Slideshow.View.ViewModels
{
	public class MainViewModel
		: ObservableObject
	{
		public readonly string[] AllowedFileExtentions = new[] {
			".png",
			".jpg",
			".jpeg",
			".gif"
		};

		public MainViewModel()
		{
			BeginSlideshowCommand = new RelayCommand(BeginSlideShow);
			DispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 3500);
			DispatcherTimer.Tick += SlideshowProgress;

			WriteDebugEntry("--- Main View Model Initalized ---");
			WriteDebugEntry("--- Init Datestamp: " + DateTime.UtcNow.ToString("MM/dd/yyyy") + "---");
			WriteDebugEntry("--- Debug logs are now enabled ---");
			WriteDebugEntry("----------------------------------");
		}

		public string DebugLog
		{
			get { return _debugLog; }
			set { SetValue(ref _debugLog, value); }
		}
		private string _debugLog;

		public ICommand BeginSlideshowCommand
		{
			get { return _beginSlideshowCommand; }
			set { SetValue(ref _beginSlideshowCommand, value); }
		}
		private ICommand _beginSlideshowCommand;

		public string SelectedPath
		{
			get { return _seletedPath; }
			set { SetValue(ref _seletedPath, value); }
		}
		private string _seletedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

		public BitmapSource CurrentPicture
		{
			get { return _currentPicture; }
			set
			{
				SetValue(ref _currentPicture, value);
				OnPropertyChanged("CurrentImagePath");
			}
		}
		private BitmapSource _currentPicture;

		public ObservableCollection<string> ImagePaths
		{
			get { return _imagePaths; }
			set { SetValue(ref _imagePaths, value); }
		}
		private ObservableCollection<string> _imagePaths = new ObservableCollection<string>();

		public string CurrentImagePath
		{
			get { return ImagePaths.Count == 0 ? string.Empty : ImagePaths[CurrentPictureIndex]; }
		}

		public int CurrentPictureIndex
		{
			get { return _currentPictureIndex; }
			set { SetValue(ref _currentPictureIndex, value); }
		}
		private int _currentPictureIndex = 0;

		public DispatcherTimer DispatcherTimer { get { return _dispatcherTimer; } }
		private DispatcherTimer _dispatcherTimer = new DispatcherTimer();

		public void BeginSlideShow()
		{
			// Reset Data
			CurrentPicture = null;
			ImagePaths.Clear();

			// Validate SelectedPath
			if (!Directory.Exists(SelectedPath))
			{
				MessageBox.Show("Specified file path is not a real directory. Please ammend.", "Path Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// Load Files
			var files = new List<string>();
			LoadFiles(SelectedPath, ref files);
			foreach (var file in files)
				ImagePaths.Add(file);

			// Shuffle
			ImagePaths.Shuffle();

			// TODO: Check speedy loading

			// TODO: settings

			// Progress
			SlideshowProgress(null, null);
		}

		private void SlideshowProgress(object sender, EventArgs e)
		{
			_dispatcherTimer.Stop();
			_dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 3500);

			if (CurrentPictureIndex >= ImagePaths.Count)
			{
				CurrentPictureIndex = 0;
				ImagePaths.Shuffle();
			}
			
			WriteDebugEntry("Loading Image " + ImagePaths[CurrentPictureIndex]);
			BitmapImage newImage = null;

			try
			{
				newImage = new BitmapImage(new Uri(ImagePaths[CurrentPictureIndex]));
				CurrentPicture = newImage;
			}
			catch (NotSupportedException)
			{
				WriteDebugEntry("Unable to load image.. skipping");

				CurrentPictureIndex++;
				_dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
				_dispatcherTimer.Start();
			}

			try
			{
				using (var reader = new ExifReader(ImagePaths[CurrentPictureIndex]))
				{
					object orignt = null;
					var orientation = reader.GetTagValue(ExifTags.Orientation, out orignt);

					if (orignt != null && ((ushort)orignt != 1))
					{
						var transformBitmap = new TransformedBitmap();
						transformBitmap.BeginInit();
						transformBitmap.Source = newImage;

						switch ((ushort)orignt)
						{
							case 8:
								WriteDebugEntry("  Image required rotation :: RotateTransform(-90)");
								transformBitmap.Transform = new RotateTransform(-90);
								break;
							case 6:
								WriteDebugEntry("  Image required rotation :: RotateTransform(90)");
								transformBitmap.Transform = new RotateTransform(90);
								break;

							default:
								break;
						}

						transformBitmap.EndInit();
						CurrentPicture = ((BitmapSource)transformBitmap).ToBitmapImage();
					}
				}
			}
			catch (ExifLibException ex)
			{

			}

			switch (ImagePaths[CurrentPictureIndex].Split('.').Last().ToLowerInvariant())
			{
				case "gif":
					WriteDebugEntry("  Image is gif :: setting up gif looping");

					TimeSpan duration = TimeSpan.FromTicks(0);
					var gif = GifBitmapDecoder.Create(new Uri(ImagePaths[CurrentPictureIndex]), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
					var delay = 0;
					foreach (var frame in gif.Frames)
					{
						frame.Freeze();
						delay += (ushort)((BitmapMetadata)frame.Metadata).GetQuery("/grctlext/Delay");
					}
					duration = TimeSpan.FromMilliseconds(delay * 10);

					if (duration < TimeSpan.FromSeconds(5))
						_dispatcherTimer.Interval = TimeSpan.FromTicks(duration.Ticks * 3);
					else if (duration < TimeSpan.FromSeconds(10))
						_dispatcherTimer.Interval = TimeSpan.FromTicks(duration.Ticks * 2);
					else
						_dispatcherTimer.Interval = duration;
					break;
			}
			
			CurrentPictureIndex++;
			_dispatcherTimer.Start();
		}

		private void LoadFiles(string selectedPath, ref List<string> files)
		{
			try
			{
				// Add files in directory
				var directoryFiles = Directory.GetFiles(selectedPath);
				foreach (var file in directoryFiles)
				{
					var fileInfo = new FileInfo(file);
					if (Array.FindIndex(AllowedFileExtentions, x => x == fileInfo.Extension.ToLowerInvariant()) > -1)
						files.Add(file);
				}

				// Find files in sub-directory
				var subDirectories = Directory.GetDirectories(selectedPath);
				foreach (var directory in subDirectories)
				{
					LoadFiles(directory, ref files);
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				Debug.WriteLine("'System.UnauthorizedAccessException' hit - {0}", ex.Message);
			}
		}

		private void WriteDebugEntry(string message)
		{
#if DEBUG
			DebugLog = message + Environment.NewLine + DebugLog;
#endif
		}
	}
}
