﻿using LRReader.Shared.Services;
using LRReader.Shared.ViewModels.Base;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace LRReader.Shared.ViewModels.Items
{
	public partial class ArchiveItemViewModel : ArchiveBaseViewModel
	{
		[ObservableProperty]
		private bool _missingImage = false;

		public ArchiveItemViewModel(SettingsService settings, ArchivesService archives, ApiService api, PlatformService platform, TabsService tabs) : base(settings, archives, api, platform, tabs)
		{
		}
	}
}
