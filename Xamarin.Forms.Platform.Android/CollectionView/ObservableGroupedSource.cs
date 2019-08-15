using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal class ObservableGroupedSource : IGroupedItemsViewSource
	{
		readonly RecyclerView.Adapter _adapter;
		readonly IList _groupSource;
		List<IItemsViewSource> _groups = new List<IItemsViewSource>();
		bool _disposed;

		bool _hasGroupHeaders;
		bool _hasGroupFooters;

		public ObservableGroupedSource(GroupableItemsView groupableItemsView, RecyclerView.Adapter adapter)
		{
			var groupSource = groupableItemsView.ItemsSource;

			_adapter = adapter;
			_groupSource = groupSource as IList ?? new ListSource(groupSource);

			if (_groupSource is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged += CollectionChanged;
			}

			_hasGroupFooters = groupableItemsView.GroupFooterTemplate != null;
			_hasGroupHeaders = groupableItemsView.GroupHeaderTemplate != null;

			ResetGroupTracking();
		}

		public int Count
		{
			get
			{
				var groupContents = 0;

				for (int n = 0; n < _groups.Count; n++)
				{
					groupContents += _groups[n].Count;
				}

				return (HasHeader ? 1 : 0)
					 + (HasFooter ? 1 : 0)
					 + groupContents;
			}
		}

		public bool HasHeader { get; set; }
		public bool HasFooter { get; set; }

		public void Dispose()
		{
			Dispose(true);
		}

		public bool IsFooter(int position)
		{
			if (!HasFooter)
			{
				return false;
			}

			if (HasHeader)
			{
				return position == _groupSource.Count + 1;
			}

			return position == _groupSource.Count;
		}

		public bool IsHeader(int position)
		{
			return HasHeader && position == 0;
		}

		public bool IsGroupHeader(int position)
		{
			if (IsFooter(position) || IsHeader(position))
			{
				return false;
			}

			var (group, inGroup) = GetGroupIndex(position);

			return _groups[group].IsHeader(inGroup);
		}

		(int, int) GetGroupIndex(int position)
		{
			position = AdjustIndexRequest(position);

			var group = 0;
			var inGroup = 0;

			while (position > 0)
			{
				inGroup += 1;

				if (inGroup == _groups[group].Count)
				{
					group += 1;
					inGroup = 0;
				}

				position -= 1;
			}

			return (group, inGroup);
		}

		public bool IsGroupFooter(int position)
		{
			if (IsFooter(position) || IsHeader(position))
			{
				return false;
			}

			var (group, inGroup) = GetGroupIndex(position);

			return _groups[group].IsFooter(inGroup);
		}

		public int GetPosition(object item)
		{
			int previousGroupsOffset = 0;

			for (int groupIndex = 0; groupIndex < _groupSource.Count; groupIndex++)
			{
				if (_groupSource[groupIndex] == item)
				{
					return AdjustPositionIndex(groupIndex);
				}

				var group = _groups[groupIndex];
				var inGroup = group.GetPosition(item);

				if (inGroup > -1)
				{
					return AdjustPositionIndex(previousGroupsOffset + inGroup);
				}

				previousGroupsOffset += group.Count;
			}

			return -1;
		}

		public object GetItem(int position)
		{
			var (group, inGroup) = GetGroupIndex(position);

			if (IsGroupFooter(position) || IsGroupHeader(position))
			{
				// This is looping to find the group/index twice, need to make it less inefficient
				return _groupSource[group];
			}

			return _groups[group].GetItem(inGroup);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				ClearGroupTracking();

				if(_groupSource is INotifyCollectionChanged notifyCollectionChanged)
				{
					notifyCollectionChanged.CollectionChanged -= CollectionChanged;
				}
			}
		}

		int AdjustIndexRequest(int index)
		{
			return index - (HasHeader ? 1 : 0);
		}

		int AdjustPositionIndex(int index)
		{
			return index + (HasHeader ? 1 : 0);
		}

		void ResetGroupTracking()
		{
			ClearGroupTracking();

			for (int n = 0; n < _groupSource.Count; n++)
			{
				var source = ItemsSourceFactory.Create(_groupSource[n] as IEnumerable, _adapter);
				source.HasFooter = _hasGroupFooters;
				source.HasHeader = _hasGroupHeaders;
				_groups.Add(source);
			}
		}

		void ClearGroupTracking()
		{
			for (int n = _groups.Count - 1; n >= 0; n--)
			{
				_groups[n].Dispose();
				_groups.RemoveAt(n);
			}
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Add(args);
					break;
				case NotifyCollectionChangedAction.Remove:
					Remove(args);
					break;
				case NotifyCollectionChangedAction.Replace:
					Replace(args);
					break;
				case NotifyCollectionChangedAction.Move:
					Move(args);
					break;
				case NotifyCollectionChangedAction.Reset:
					Reload();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void Reload()
		{
			ResetGroupTracking();
			_adapter.NotifyDataSetChanged();
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _groupSource.IndexOf(args.NewItems[0]);
			startIndex = AdjustPositionIndex(startIndex);
			var count = args.NewItems.Count;

			// Adding a group will change the section index for all subsequent groups, so the easiest thing to do
			// is to reset all the group tracking to get it up-to-date
			ResetGroupTracking();

			// TODO ???????? ezhart These inserted indexes and ranges aren't quite right
			// They need to account for all the new items being added
			// So we need to look at the group at the index, and use its Count 
			if (count == 1)
			{
				_adapter.NotifyItemInserted(startIndex);
				return;
			}

			_adapter.NotifyItemRangeInserted(startIndex, count);
		}

		void Remove(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.OldStartingIndex;

			if (startIndex < 0)
			{
				// INCC implementation isn't giving us enough information to know where the removed items were in the
				// collection. So the best we can do is a ReloadData()
				Reload();
				return;
			}

			// If we have a start index, we can be more clever about removing the item(s) (and get the nifty animations)
			var count = args.OldItems.Count;

			// Removing a group will change the section index for all subsequent groups, so the easiest thing to do
			// is to reset all the group tracking to get it up-to-date
			ResetGroupTracking();

			if (count == 1)
			{
				_adapter.NotifyItemRemoved(startIndex);
				return;
			}

			// TODO ???????? ezhart These inserted indexes and ranges aren't quite right
			// They need to account for all the new items being added
			// So we need to look at the group at the index, and use its Count 
			_adapter.NotifyItemRangeRemoved(startIndex, count);
		}

		void Replace(NotifyCollectionChangedEventArgs args)
		{
			var newCount = args.NewItems.Count;

			if (newCount == args.OldItems.Count)
			{
				ResetGroupTracking();

				var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _groupSource.IndexOf(args.NewItems[0]);

				// TODO ???????? ezhart These inserted indexes and ranges aren't quite right
				// They need to account for all of the items (and headers and footers) in the sections
				// So we need to look at the group at the index, and use its Count 

				// We are replacing one set of items with a set of equal size; we can do a simple item or range 
				// notification to the adapter
				if (newCount == 1)
				{
					_adapter.NotifyItemChanged(startIndex);
				}
				else
				{
					_adapter.NotifyItemRangeChanged(startIndex, newCount);
				}
				return;
			}

			// The original and replacement sets are of unequal size; this means that everything currently in view will 
			// have to be updated. So we just have to use ReloadData and let the UICollectionView update everything
			_adapter.NotifyDataSetChanged();
		}

		void Move(NotifyCollectionChangedEventArgs args)
		{
			var count = args.NewItems.Count;

			ResetGroupTracking();

			var start = Math.Min(args.OldStartingIndex, args.NewStartingIndex);
			var end = Math.Max(args.OldStartingIndex, args.NewStartingIndex) + count;

			// TODO Item range notification should work, but we need to account for all the items inside the groups

			_adapter.NotifyItemRangeChanged(start, end);
		}
	}
}