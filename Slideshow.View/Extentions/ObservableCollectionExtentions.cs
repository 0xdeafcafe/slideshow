using System;
using System.Collections.ObjectModel;

namespace Slideshow.View.Extentions
{
	public static class ObservableCollectionExtentions
	{
		public static void Shuffle<T>(this ObservableCollection<T> list)
		{
			Random rng = new Random();
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}
