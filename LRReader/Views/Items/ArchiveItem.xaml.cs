﻿using LRReader.Internal;
using LRReader.Models.Main;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace LRReader.Views.Items
{
	public sealed partial class ArchiveItem : UserControl
	{

		private Archive Archive;
		private string _oldID = "";

		public ArchiveItem()
		{
			this.InitializeComponent();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			Archive = args.NewValue as Archive;
			if (!_oldID.Equals(Archive.arcid))
			{
				Title.Opacity = 0;
				Thumbnail.Visibility = Visibility.Collapsed;
				Ring.Visibility = Visibility.Visible;
				/*StorageFile file = await Global.ImageManager.DownloadThumbnailAsync(Archive.arcid);

				using (var ras = await file.OpenAsync(FileAccessMode.Read))
				{
					var image = new BitmapImage();
					await image.SetSourceAsync(ras);
					if (image.PixelHeight != 0 && image.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
							Thumbnail.Stretch = Stretch.Uniform;
					Thumbnail.Source = image;
					Thumbnail.Visibility = Visibility.Visible;
					Ring.Visibility = Visibility.Collapsed;
					Title.Opacity = 1;
				}*/
				using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
				{
					byte[] bytes = await Global.ImageManager.DownloadThumbnailRuntime(Archive.arcid);
					await stream.WriteAsync(bytes.AsBuffer());
					stream.Seek(0);
					var image = new BitmapImage();
					await image.SetSourceAsync(stream);
					if (image.PixelHeight != 0 && image.PixelWidth != 0)
						if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
							Thumbnail.Stretch = Stretch.Uniform;
					Thumbnail.Source = image;
				}
				Thumbnail.Visibility = Visibility.Visible;
				Ring.Visibility = Visibility.Collapsed;
				Title.Opacity = 1;
				_oldID = Archive.arcid;
				Bindings.Update();
			}
		}

	}
}
