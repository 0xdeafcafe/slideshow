using Slideshow.View.Common;
using Slideshow.View.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
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
			".jpeg"
		};

		public MainViewModel()
		{
			BeginSlideshowCommand = new RelayCommand(BeginSlideShow);
			DispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 3500);
			DispatcherTimer.Tick += SlideshowProgress;
		}
		
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
			
			if (CurrentPictureIndex >= ImagePaths.Count)
			{
				CurrentPictureIndex = 0;
				ImagePaths.Shuffle();
			}

			CurrentPicture = new BitmapImage(new Uri(ImagePaths[CurrentPictureIndex]));

			CurrentPictureIndex++;

			_dispatcherTimer.Start();
		}

		private void LoadFiles(string selectedPath, ref List<string> files)
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
	}
}
