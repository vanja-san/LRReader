﻿using LRReader.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LRReader.Shared.Services
{

	public enum Symbol
	{
		Favorite, Pictures
	}

	public enum Dialog
	{
		Generic, CategoryArchive, CreateCategory, Markdown, ProgressConflict, ServerProfile
	}

	public abstract class PlatformService
	{
		public abstract Version Version { get; }
		public abstract bool AnimationsEnabled { get; }
		public abstract uint HoverTime { get; }

		public abstract void Init();
		public abstract void ChangeTheme(AppTheme theme);
		public abstract string GetLocalizedString(string key);
		public abstract Task<bool> OpenInBrowser(Uri uri);
		public abstract void CopyToClipboard(string text);

		private readonly Dictionary<Dialog, Type> Dialogs = new Dictionary<Dialog, Type>();

		private readonly Dictionary<Symbol, object> SymbolToSymbol = new Dictionary<Symbol, object>();

		public void MapSymbolToSymbol(Symbol symbol, object backing) => SymbolToSymbol.Add(symbol, backing);

		public object? GetSymbol(Symbol symbol)
		{
			object symb;
			if (!SymbolToSymbol.TryGetValue(symbol, out symb))
				return null;
			return symb;
		}

		public void MapDialogToType(Dialog tab, Type type) => Dialogs.Add(tab, type);

		public Task<IDialogResult> OpenDialog(Dialog dialog, params object?[] args) => OpenDialog<IDialog>(dialog, args);

		public async Task<IDialogResult> OpenDialog<D>(Dialog dialog, params object?[] args) where D : IDialog
		{
			var newDialog = CreateDialog<D>(dialog, args);
			if (newDialog == null)
				return IDialogResult.None;
			return await newDialog.ShowAsync();
		}

		public D CreateDialog<D>(Dialog dialog, params object?[] args) where D : IDialog => (D)Activator.CreateInstance(Dialogs[dialog], args);

		public abstract Task<IDialogResult> OpenGenericDialog(string title = "", string primarybutton = "", string secondarybutton = "", string closebutton = "", object? content = null);
	}

	public class StubPlatformService : PlatformService
	{

		public override void Init()
		{
		}

		public override Version Version => new Version(0, 0, 0, 0);

		public override bool AnimationsEnabled => false;

		public override uint HoverTime => 300;

		public override Task<bool> OpenInBrowser(Uri uri)
		{
			return Task.Run(() => false);
		}

		public override string GetLocalizedString(string key) => key;

		public override void ChangeTheme(AppTheme theme) { }

		public override Task<IDialogResult> OpenGenericDialog(string title, string primarybutton, string secondarybutton, string closebutton, object? content) { return Task.Run(() => IDialogResult.None); }

		public override void CopyToClipboard(string text) { }
	}
}
