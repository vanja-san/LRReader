﻿using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.UWP.Extensions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace LRReader.UWP.Views.Items
{
	public sealed partial class ArchiveImage : UserControl
	{
		private ImagePageSet _oldUrl = new ImagePageSet("", 0);
		private Container Data = new Container();
		private bool _loading;

		public ArchiveImage()
		{
			this.InitializeComponent();
		}

		private async void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (args.NewValue == null)
				return;
			var n = args.NewValue as ImagePageSet;
			if (!_oldUrl.Equals(n))
			{
				await ReloadImage(n);
				_oldUrl = n;
			}
		}

		private async void Reload_Click(object sender, RoutedEventArgs e) => await ReloadImage(_oldUrl);

		private async Task ReloadImage(ImagePageSet n)
		{
			if (_loading)
				return;
			_loading = true;
			Image.SetVisualOpacity(0);
			Image.Source = null;
			Ring.IsActive = true;
			Data.MissingImage = false;
			PageCount.Text = n.Page.ToString();

			var image = new BitmapImage();
			image.DecodePixelType = DecodePixelType.Logical;
			image.DecodePixelHeight = 275;
			image = await Service.ImageProcessing.ByteToBitmap(await Service.Images.GetImageCached(n.Image), image, n.Image.EndsWith("avif")) as BitmapImage;
			Ring.IsActive = false;
			/*if (image.PixelHeight != 0 && image.PixelWidth != 0)
			{
				if (Math.Abs(ActualHeight / ActualWidth - image.PixelHeight / image.PixelWidth) > .65)
					Image.Stretch = Stretch.Uniform;
				else
					Image.Stretch = Stretch.UniformToFill;
			}*/
			Image.Source = image;

			if (image != null)
				Image.FadeIn();
			else
				Data.MissingImage = true;
			_loading = false;
		}

		private class Container : ObservableObject
		{
			private bool _missingImage;
			public bool MissingImage
			{
				get => _missingImage;
				set
				{
					_missingImage = value;
					OnPropertyChanged("MissingImage");
				}
			}
		}

		private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e) => VisualStateManager.GoToState(this, "PointerOver", true);

		private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e) => VisualStateManager.GoToState(this, "Normal", true);

		private void UserControl_PointerCaptureLost(object sender, PointerRoutedEventArgs e) => VisualStateManager.GoToState(this, "Normal", true);
	}

}
