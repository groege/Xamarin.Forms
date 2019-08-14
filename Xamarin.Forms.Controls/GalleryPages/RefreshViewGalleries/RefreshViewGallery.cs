using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Xamarin.Forms.Controls.GalleryPages.RefreshViewGalleries
{
	public class RefreshViewGallery : ContentPage
	{
		public RefreshViewGallery()
		{
			Title = "RefreshView Gallery";
			Content = new StackLayout
			{
				Children =
				{
					new Button { Text ="Enable CollectionView", AutomationId = "EnableCollectionView", Command = new Command(() => Device.SetFlags(new[] { ExperimentalFlags.CollectionViewExperimental })) },
					GalleryBuilder.NavButton("Refresh Layout Gallery", () => new RefreshLayoutGallery(), Navigation),
					GalleryBuilder.NavButton("Refresh ScrollView Gallery", () => new RefreshScrollViewGallery(), Navigation),
					GalleryBuilder.NavButton("Refresh ListView Gallery", () => new RefreshListViewGallery(), Navigation),
					GalleryBuilder.NavButton("Refresh CollectionView Gallery", () => new RefreshCollectionViewGallery(), Navigation),
					GalleryBuilder.NavButton("Refresh CarouselView Gallery", () => new RefreshCarouselViewGallery(), Navigation)
				}
			};
		}
	}

	public class RefreshItem
	{
		public string Name { get; set; }
		public Color Color { get; set; }
	}

	public class RefreshViewModel : BindableObject
	{
		const int RefreshDuration = 2;

		private readonly Random _random;
		private bool _isRefresing;
		private ObservableCollection<RefreshItem> _items;

		public RefreshViewModel()
		{
			_random = new Random();
			Items = new ObservableCollection<RefreshItem>();
			LoadItems();
		}

		public bool IsRefreshing
		{
			get { return _isRefresing; }
			set
			{
				_isRefresing = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<RefreshItem> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public ICommand RefreshCommand => new Command(ExecuteRefresh);

		private void LoadItems()
		{
			for (int i = 0; i < 100; i++)
			{
				Items.Add(new RefreshItem
				{
					Color = Color.FromRgb(_random.Next(0, 255), _random.Next(0, 255), _random.Next(0, 255)),
					Name = DateTime.Now.AddMinutes(i).ToString("F")
				});
			}
		}

		private void ExecuteRefresh()
		{
			IsRefreshing = true;

			Items.Clear();

			Device.StartTimer(TimeSpan.FromSeconds(RefreshDuration), () =>
			{
				LoadItems();

				IsRefreshing = false;

				return false;
			});
		}
	}
}