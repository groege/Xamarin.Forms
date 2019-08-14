using System;
using System.ComponentModel;
using Android.Content;

namespace Xamarin.Forms.Platform.Android
{
	public class GroupableItemsViewRenderer : SelectableItemsViewRenderer
	{
		GroupableItemsView GroupableItemsView => (GroupableItemsView)ItemsView;
		GroupableItemsViewAdapter GroupableItemsViewAdapter => (GroupableItemsViewAdapter)ItemsViewAdapter;

		public GroupableItemsViewRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.Is(GroupableItemsView.IsGroupedProperty))
			{
				UpdateItemsSource();
			}
		}

		protected override ItemsViewAdapter CreateAdapter()
		{
			return new GroupableItemsViewAdapter(GroupableItemsView);
		}

		protected override void SetUpNewElement(ItemsView newElement)
		{
			if (newElement != null && !(newElement is GroupableItemsView))
			{
				throw new ArgumentException($"{nameof(newElement)} must be of type {typeof(GroupableItemsView).Name}");
			}

			base.SetUpNewElement(newElement);
		}
	}
}