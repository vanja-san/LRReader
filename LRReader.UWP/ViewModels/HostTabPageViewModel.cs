﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using LRReader.Internal;
using LRReader.Shared.Internal;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.UWP.ViewModels
{
	public class HostTabPageViewModel : ViewModelBase
	{
		private CustomTab _currentTab;
		public CustomTab CurrentTab
		{
			get => _currentTab;
			set
			{
				if (_currentTab != value)
				{
					_currentTab = value;
					RaisePropertyChanged("CurrentTab");
				}
			}
		}
		private bool _fullscreen = false;
		public bool FullScreen
		{
			get => _fullscreen;
			set
			{
				if (_fullscreen != value)
				{
					_fullscreen = value;
					RaisePropertyChanged("FullScreen");
					RaisePropertyChanged("Windowed");
				}
			}
		}
		public bool Windowed
		{
			get => !_fullscreen;
		}
		public ControlFlags ControlFlags
		{
			get => SharedGlobal.ControlFlags;
		}

	}
}
