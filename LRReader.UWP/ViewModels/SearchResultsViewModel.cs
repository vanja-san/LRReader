﻿using LRReader.Shared.Internal;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Providers;
using LRReader.UWP.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static LRReader.Shared.Services.Service;

namespace LRReader.UWP.ViewModels
{
	public class SearchResultsViewModel : ObservableObject
	{
		private bool _loadingArchives = true;
		public bool LoadingArchives
		{
			get => _loadingArchives;
			set
			{
				SetProperty(ref _loadingArchives, value);
				OnPropertyChanged("ControlsEnabled");
				OnPropertyChanged("HasNextPage");
				OnPropertyChanged("HasPrevPage");
			}
		}
		private bool _refreshOnErrorButton = false;
		public bool RefreshOnErrorButton
		{
			get => _refreshOnErrorButton;
			set
			{
				SetProperty(ref _refreshOnErrorButton, value);
				OnPropertyChanged("ControlsEnabled");
				OnPropertyChanged("HasNextPage");
				OnPropertyChanged("HasPrevPage");
			}
		}
		public ObservableCollection<Archive> ArchiveList = new ObservableCollection<Archive>();
		private int _page = 0;
		public int Page
		{
			get => _page;
			set
			{
				if (SetProperty(ref _page, value))
				{
					OnPropertyChanged("DisplayPage");
					OnPropertyChanged("HasNextPage");
					OnPropertyChanged("HasPrevPage");
				}
			}
		}
		public int DisplayPage => Page + 1;
		private int _totalArchives;
		public int TotalArchives
		{
			get => _totalArchives;
			set
			{
				SetProperty(ref _totalArchives, value);
			}
		}
		public bool HasNextPage => Page < TotalArchives / SharedGlobal.ServerInfo.archives_per_page && ControlsEnabled;
		public bool HasPrevPage => Page > 0 && ControlsEnabled;
		private bool _newOnly;
		public bool NewOnly
		{
			get => _newOnly;
			set => SetProperty(ref _newOnly, value);
		}
		private bool _untaggedOnly;
		public bool UntaggedOnly
		{
			get => _untaggedOnly;
			set => SetProperty(ref _untaggedOnly, value);
		}
		public string Query = "";
		public Category Category = new Category() { id = "", search = "" };
		private bool _controlsEnabled;
		public bool ControlsEnabled
		{
			get => _controlsEnabled && !RefreshOnErrorButton;
			set
			{
				SetProperty(ref _controlsEnabled, value);
				OnPropertyChanged("HasNextPage");
				OnPropertyChanged("HasPrevPage");
			}
		}
		protected bool _internalLoadingArchives;
		public ObservableCollection<string> Suggestions = new ObservableCollection<string>();
		public ObservableCollection<string> SortBy = new ObservableCollection<string>();
		private int _sortByIndex = -1;
		public int SortByIndex
		{
			get => _sortByIndex;
			set => SetProperty(ref _sortByIndex, value);
		}
		public Order OrderBy = Order.Ascending;

		public SearchResultsViewModel()
		{
			foreach (var n in SharedGlobal.ArchivesManager.Namespaces)
				SortBy.Add(n);
			SortByIndex = _sortByIndex = SortBy.IndexOf(Settings.SortByDefault);
			OrderBy = Settings.OrderByDefault;
			SharedGlobal.EventManager.DeleteArchiveEvent += DeleteArchive;
		}

		public async Task NextPage()
		{
			if (HasNextPage)
				await LoadPage(Page + 1);
		}

		public async Task PrevPage()
		{
			if (HasPrevPage)
				await LoadPage(Page - 1);
		}

		public async Task ReloadSearch()
		{
			await LoadPage(0);
		}

		public async Task LoadPage(int page)
		{
			if (_internalLoadingArchives)
				return;
			ControlsEnabled = false;
			_internalLoadingArchives = true;
			RefreshOnErrorButton = false;
			LoadingArchives = true;
			ArchiveList.Clear();
			Page = page;
			string sortby;
			if (SortByIndex == -1)
				sortby = "title";
			else
				sortby = SortBy.ElementAt(SortByIndex);
			var resultPage = await SearchProvider.Search(
				SharedGlobal.ServerInfo.archives_per_page, page, Query, string.IsNullOrEmpty(Category.search) ? Category.id : "", NewOnly, UntaggedOnly, sortby, OrderBy);
			if (resultPage != null)
			{
				await Task.Run(async () =>
				{
					foreach (var a in resultPage.data)
					{
						if (!CustomArchiveCheck(a))
							continue;
						var archive = SharedGlobal.ArchivesManager.GetArchive(a.arcid);
						if (archive != null)
							await DispatcherService.RunAsync(() => ArchiveList.Add(archive));
					}
				});
				TotalArchives = resultPage.recordsFiltered;
			}
			else
				RefreshOnErrorButton = true;
			LoadingArchives = false;
			_internalLoadingArchives = false;
			ControlsEnabled = true;
		}

		public void DeleteArchive(string id)
		{
			ArchiveList.Remove(SharedGlobal.ArchivesManager.GetArchive(id));
		}

		protected virtual bool CustomArchiveCheck(Archive archive) => true;
	}
}
