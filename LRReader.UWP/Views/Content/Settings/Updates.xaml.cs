﻿using LRReader.Shared.Services;
using LRReader.UWP.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Content.Settings
{
	public sealed partial class Updates : Page
	{
		private SettingsPageViewModel Data;

		public Updates()
		{
			this.InitializeComponent();
			Data = DataContext as SettingsPageViewModel;
		}

		private async void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri link))
			{
				await Service.Platform.OpenInBrowser(link);
			}
		}

	}
}
