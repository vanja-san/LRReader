﻿#nullable enable
using LRReader.Shared.Messages;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.Shared.Services;
using LRReader.UWP.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace LRReader.UWP.ViewModels
{
	public partial class SettingsPageViewModel : ObservableObject
	{
		private readonly ImagesService Images;
		private readonly PlatformService Platform;
		private readonly UpdatesService Updates;
		private readonly ApiService Api;
		private readonly TabsService Tabs;

		private ResourceLoader lang = ResourceLoader.GetForCurrentView("Settings");

		public readonly string LRReader = "LRReader";

		public SettingsService SettingsManager;
		public Version Version => Platform.Version;
		public Version MinVersion => Updates.MIN_VERSION;
		public Version MaxVersion => Updates.MAX_VERSION;

		[ObservableProperty]
		private string _shinobuStatusText;
		[ObservableProperty]
		private string _shinobuPid;

		public ReleaseInfo ReleaseInfo;
		[ObservableProperty]
		private bool _showReleaseInfo;
		[ObservableProperty]
		private ServerInfo _serverInfo;

		public ObservableCollection<string> SortBy = new ObservableCollection<string>();
		private int _sortByIndex = -1;
		public int SortByIndex
		{
			get => SortBy.IndexOf(SettingsManager.SortByDefault);
			set
			{
				if (value != _sortByIndex)
				{
					_sortByIndex = value;
					if (value == -1)
						SettingsManager.SortByDefault = "title";
					else
						SettingsManager.SortByDefault = SortBy.ElementAt(value);
					OnPropertyChanged("SortByIndex");
				}
			}
		}
		public string ThumbnailCacheSize;
		[ObservableProperty]
		private bool _progressCache;

		public MinionJob thumbnailJob;

		public bool AvifMissing;
		public bool HeifMissing;

		public SettingsPageViewModel(SettingsService settings, ImagesService images, ArchivesService archives, PlatformService platform, UpdatesService updates, ApiService api, TabsService tabs)
		{
			SettingsManager = settings;
			Images = images;
			Platform = platform;
			Updates = updates;
			Api = api;
			Tabs = tabs;

			UpdateReleaseData();
			foreach (var n in archives.Namespaces)
				SortBy.Add(n);
			_sortByIndex = SortBy.IndexOf(SettingsManager.SortByDefault);
		}

		public async Task CheckForPackages()
		{
			SetProperty(ref AvifMissing, !await (Platform as UWPlatformService).CheckAppInstalled("Microsoft.AV1VideoExtension_8wekyb3d8bbwe"), nameof(AvifMissing));
			SetProperty(ref HeifMissing, !await (Platform as UWPlatformService).CheckAppInstalled("Microsoft.HEIFImageExtension_8wekyb3d8bbwe"), nameof(HeifMissing));
		}


		public async Task<DownloadPayload> DownloadDB()
		{
			return await DatabaseProvider.BackupJSON();
		}

		public async Task UpdateShinobuStatus()
		{
			var worker = lang.GetString("Server/WorkerStatus/Text");
			var status = lang.GetString("Server/WorkerUnknown/Text");
			var pid = "";
			if (SettingsManager.Profile.HasApiKey)
			{
				var result = await ShinobuProvider.GetShinobuStatus();
				if (result != null && result.pid != 0)
				{
					if (result.is_alive)
						status = lang.GetString("Server/WorkerRunning/Text");
					else
						status = lang.GetString("Server/WorkerStopped/Text");
					pid = $"PID: {result.pid}";
				}
			}
			ShinobuStatusText = $"{worker}{status}";
			ShinobuPid = pid;
		}

		public void UpdateReleaseData()
		{
			// TODO Updates here
			/*var info = await SharedGlobal.UpdatesManager.CheckUpdates(Platform.Version);
			if (info != null)
			{
				SetProperty(ref ReleaseInfo, info, nameof(ReleaseInfo));
				ShowReleaseInfo = true;
			}
			else
			{
				ShowReleaseInfo = false;
				SetProperty(ref ReleaseInfo, null, nameof(ReleaseInfo));
			}*/
		}

		public async Task UpdateServerInfo()
		{
			var info = await ServerProvider.GetServerInfo();
			if (info != null)
			{
				ServerInfo = info;
			}
		}

		public async Task CheckThumbnailJob()
		{
			if (thumbnailJob == null)
				return;
			var status = await ServerProvider.GetMinionStatus(thumbnailJob.job);
			if (status == null)
			{
				thumbnailJob = null;
				return;
			}
			if (status.state.Equals("finished"))
			{
				WeakReferenceMessenger.Default.Send(new ShowNotification("Thumbnail generation completed", ""));
				thumbnailJob = null;
			}
		}

		public async Task UpdateThumbnailCacheSize()
		{
			if (ProgressCache)
				return;
			ProgressCache = true;
			ThumbnailCacheSize = await Images.GetThumbnailCacheSize();
			OnPropertyChanged("ThumbnailCacheSize");
			ProgressCache = false;
		}

		[ICommand]
		private async Task AddProfile()
		{
			var dialog = Platform.CreateDialog<ICreateProfileDialog>(Dialog.ServerProfile, false);
			var result = await dialog.ShowAsync();
			if (result == IDialogResult.Primary)
			{
				var address = dialog.Address;
				if (!(address.StartsWith("http://") || address.StartsWith("https://")))
					address = "http://" + address;
				SettingsManager.AddProfile(dialog.Name, address, dialog.ApiKey);
			}
		}

		[ICommand]
		private async Task EditProfile(ServerProfile profile)
		{
			var dialog = Platform.CreateDialog<ICreateProfileDialog>(Dialog.ServerProfile, true);
			dialog.Name = profile.Name;
			dialog.Address = profile.ServerAddress;
			dialog.ApiKey = profile.ServerApiKey;
			var result = await dialog.ShowAsync();
			if (result == IDialogResult.Primary)
			{
				var address = dialog.Address;
				if (!(address.StartsWith("http://") || address.StartsWith("https://")))
					address = "http://" + address;
				SettingsManager.ModifyProfile(profile.UID, dialog.Name, address, dialog.ApiKey);
				Api.RefreshSettings(profile);
			}
		}

		[ICommand]
		private void RemoveProfile(ServerProfile profile)
		{
			SettingsManager.Profiles.Remove(profile);

			// TODO Change this once profiles page is up
			SettingsManager.Profile = SettingsManager.Profiles.FirstOrDefault();
		}

		[ICommand]
		private async Task RestartWorker() => await ShinobuProvider.RestartWorker();

		[ICommand]
		private async Task StopWorker() => await ShinobuProvider.StopWorker();

		[ICommand]
		private async Task ClearAllNew() => await DatabaseProvider.ClearAllNew();

		[ICommand]
		private async Task ResetSearch() => await SearchProvider.DiscardCache();

		[ICommand]
		private async Task RegenThumbnails(bool force)
		{
			if (thumbnailJob == null)
				thumbnailJob = await ArchivesProvider.RegenerateThumbnails(force);
		}

		[ICommand]
		private async Task ClearThumbnailCache()
		{
			if (ProgressCache)
				return;
			ProgressCache = true;
			await Images.DeleteThumbnailCache();
			ThumbnailCacheSize = await Images.GetThumbnailCacheSize();
			OnPropertyChanged("ThumbnailCacheSize");
			ProgressCache = false;
		}

		[ICommand]
		private void OpenUploadArchive() => Tabs.OpenTab(Tab.Web, SettingsManager.Profile.ServerAddressBrowser + "/upload");

		[ICommand]
		private void OpenBatchTagging() => Tabs.OpenTab(Tab.Web, SettingsManager.Profile.ServerAddressBrowser + "/batch");

		[ICommand]
		private void OpenSettings() => Tabs.OpenTab(Tab.Web, SettingsManager.Profile.ServerAddressBrowser + "/config");

		[ICommand]
		private void OpenPlugins() => Tabs.OpenTab(Tab.Web, SettingsManager.Profile.ServerAddressBrowser + "/config/plugins");

		[ICommand]
		private void OpenLogs() => Tabs.OpenTab(Tab.Web, SettingsManager.Profile.ServerAddressBrowser + "/logs");
	}
}
