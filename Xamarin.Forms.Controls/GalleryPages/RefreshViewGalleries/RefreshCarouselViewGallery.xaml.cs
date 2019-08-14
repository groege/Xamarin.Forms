namespace Xamarin.Forms.Controls.GalleryPages.RefreshViewGalleries
{
	public partial class RefreshCarouselViewGallery : ContentPage
	{
		public RefreshCarouselViewGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}
	}
}