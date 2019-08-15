using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Android.Support.V7.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal static class ItemsSourceFactory
	{
		public static IItemsViewSource Create(IEnumerable itemsSource, RecyclerView.Adapter adapter)
		{
			if (itemsSource == null)
			{
				return new EmptySource();
			}

			switch (itemsSource)
			{
				case IList _ when itemsSource is INotifyCollectionChanged:
					return new ObservableItemsSource(itemsSource as IList, adapter);
				case IEnumerable<object> generic:
					return new ListSource(generic);
			}

			return new ListSource(itemsSource);
		}

		public static IItemsViewSource Create(ItemsView itemsView, RecyclerView.Adapter adapter)
		{
			return Create(itemsView.ItemsSource, adapter);
		}

		public static IGroupedItemsViewSource Create(GroupableItemsView itemsView, RecyclerView.Adapter adapter)
		{
			return new ObservableGroupedSource(itemsView, adapter);
		}
	}
}