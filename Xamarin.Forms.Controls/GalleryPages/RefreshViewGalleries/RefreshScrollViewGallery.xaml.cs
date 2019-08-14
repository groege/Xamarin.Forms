namespace Xamarin.Forms.Controls.GalleryPages.RefreshViewGalleries
{
	public partial class RefreshScrollViewGallery : ContentPage
	{
		public RefreshScrollViewGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}
	}
}