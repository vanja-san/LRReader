﻿using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace LRReader.UWP.Views.Controls
{
	[ContentProperty(Name = "Items")]
	public sealed partial class ModernExpander : Expander
	{
		public ModernExpander()
		{
			this.InitializeComponent();
			Items = new List<object>();
		}

		public object Input
		{
			get => GetValue(InputProperty);
			set => SetValue(InputProperty, value);
		}

		public string Title
		{
			get => GetValue(TitleProperty) as string;
			set => SetValue(TitleProperty, value);
		}
		public string Description
		{
			get => GetValue(DescriptionProperty) as string;
			set => SetValue(DescriptionProperty, value);
		}

		public IList<object> Items
		{
			get => GetValue(ItemsProperty) as IList<object>;
			set => SetValue(ItemsProperty, value);
		}

		public string Glyph
		{
			get => GetValue(GlyphProperty) as string;
			set => SetValue(GlyphProperty, value);
		}

		public IconElement Icon
		{
			get => GetValue(IconProperty) as IconElement;
			set => SetValue(IconProperty, value);
		}

		public string ToolTip
		{
			get => GetValue(ToolTipProperty) as string;
			set => SetValue(ToolTipProperty, value);
		}

		public static readonly DependencyProperty InputProperty = DependencyProperty.Register("Input", typeof(object), typeof(ModernExpander), new PropertyMetadata(null));
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ModernExpander), new PropertyMetadata(""));
		public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(ModernExpander), new PropertyMetadata(""));
		public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IList<object>), typeof(ModernExpander), new PropertyMetadata(null));
		public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(ModernExpander), new PropertyMetadata(null));
		public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(IconElement), typeof(ModernExpander), new PropertyMetadata(null));
		public static readonly DependencyProperty ToolTipProperty = DependencyProperty.Register("ToolTip", typeof(string), typeof(ModernExpander), new PropertyMetadata(null));
	}

	public class ItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate LastItem { get; set; }
		public DataTemplate OtherItem { get; set; }

		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
			return (itemsControl.IndexFromContainer(container) == (itemsControl.ItemsSource as IList<object>).Count - 1) ? LastItem : OtherItem;
		}
	}
}
