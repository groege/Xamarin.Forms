using System;
using Android.Content;

namespace Xamarin.Forms.Platform.Android
{
	public class GroupableItemsViewAdapter : SelectableItemsViewAdapter
	{
		protected readonly GroupableItemsView GroupableItemsView;

		internal GroupableItemsViewAdapter(GroupableItemsView groupableItemsView, 
			Func<View, Context, ItemContentView> createView = null) : base(groupableItemsView, createView)
		{
			GroupableItemsView = groupableItemsView;
		}
	}
}