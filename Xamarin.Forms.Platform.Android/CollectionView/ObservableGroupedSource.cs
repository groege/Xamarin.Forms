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
	internal class ObservableGroupedSource : IItemsViewSource
	{
		readonly IList _groupSource;
		readonly RecyclerView.Adapter _adapter;
		List<ObservableItemsSource> _groups = new List<ObservableItemsSource>();

		private bool _disposed;

		public ObservableGroupedSource(IEnumerable groupSource, RecyclerView.Adapter adapter)
		{
			_adapter = adapter;
			_groupSource = groupSource as IList ?? new ListSource(groupSource);

			ResetGroupTracking();
		}

		public object this[int index] => _groupSource[AdjustIndexRequest(index)];

		public int Count => _groupSource.Count + (HasHeader ? 1 : 0) + (HasFooter ? 1 : 0);
		public bool HasHeader { get; set; }
		public bool HasFooter { get; set; }

		public void Dispose()
		{
			Dispose(true);	
		}

		public bool IsFooter(int index)
		{
			if (!HasFooter)
			{
				return false;
			}

			if (HasHeader)
			{
				return index == _groupSource.Count + 1;
			}

			return index == _groupSource.Count;
		}

		public bool IsHeader(int index)
		{
			return HasHeader && index == 0;
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

				((INotifyCollectionChanged)_groupSource).CollectionChanged -= CollectionChanged;
			}
		}

		int AdjustIndexRequest(int index)
		{
			return index - (HasHeader ? 1 : 0);
		}

		void ResetGroupTracking()
		{
			ClearGroupTracking();

			for (int n = 0; n < _groupSource.Count; n++)
			{
				if (_groupSource[n] is INotifyCollectionChanged && _groupSource[n] is IList list)
				{
					_groups.Add(new ObservableItemsSource(list, _adapter, n));
				}
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
//					Add(args);
					break;
				case NotifyCollectionChangedAction.Remove:
//					Remove(args);
					break;
				case NotifyCollectionChangedAction.Replace:
//					Replace(args);
					break;
				case NotifyCollectionChangedAction.Move:
//					Move(args);
					break;
				case NotifyCollectionChangedAction.Reset:
//					Reload();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}


	}
}