/*
 * Copyright (C) 2015 Refractored LLC & James Montemagno: 
 * http://github.com/JamesMontemagno
 * http://twitter.com/JamesMontemagno
 * http://refractored.com
 * 
 * The MIT License (MIT) see GitHub For more information
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.ComponentModel;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	[Preserve(AllMembers = true)]
    public class RefreshViewRenderer : ViewRenderer<RefreshView, UIView>
	{
		bool _isRefreshing;
		bool _set;
		nfloat _origininalY;
		BindableProperty _rendererProperty; 
        UIView _refreshControlParent;
		UIRefreshControl _refreshControl;

		public RefreshView RefreshView
		{
			get { return Element; }
		}

		public bool IsRefreshing
		{
			get { return _isRefreshing; }
			set
			{
				bool changed = IsRefreshing != value;

				_isRefreshing = value;

				if (_isRefreshing)
					_refreshControl.BeginRefreshing();
				else
					_refreshControl.EndRefreshing();

				if (changed)
					TryOffsetRefresh(this, IsRefreshing);
			}
		}

		public BindableProperty RendererProperty
		{
			get
			{
				if (_rendererProperty != null)
					return _rendererProperty;

				var type = Type.GetType("Xamarin.Forms.Platform.iOS.Platform, Xamarin.Forms.Platform.iOS");
				var prop = type.GetField("RendererProperty");
				var val = prop.GetValue(null);
				_rendererProperty = val as BindableProperty;

				return _rendererProperty;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<RefreshView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
                return;

            _refreshControl = new UIRefreshControl();

            _refreshControl.ValueChanged += OnRefresh;

            _refreshControlParent = this;

            UpdateColors();
            UpdateIsRefreshing();
            UpdateIsSwipeToRefreshEnabled();
        }

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsSwipeToRefreshEnabled();
			else if (e.PropertyName == RefreshView.IsRefreshingProperty.PropertyName)
				UpdateIsRefreshing();
			else if (e.IsOneOf(RefreshView.RefreshColorProperty, VisualElement.BackgroundColorProperty))
				UpdateColors();
		}

		protected override void SetBackgroundColor(Color color)
		{
			if (_refreshControl == null)
				return;

			if (color != Color.Default)
				_refreshControl.BackgroundColor = color.ToUIColor();
			else
				_refreshControl.BackgroundColor = null;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (_refreshControl != null)
				{
					_refreshControl.ValueChanged -= OnRefresh;
					_refreshControl = null;
				}

				_refreshControlParent = null;
			}
		}

		bool TryOffsetRefresh(UIView view, bool refreshing)
        {
            if (view is UITableView)
            {
                var uiTableView = view as UITableView;
                if (!_set)
                {
                    _origininalY = uiTableView.ContentOffset.Y;
                    _set = true;
                }

                if (_origininalY < 0)
                    return true;

                if (refreshing)
                    uiTableView.SetContentOffset(new CoreGraphics.CGPoint(0, _origininalY - _refreshControl.Frame.Size.Height), true);
                else
                    uiTableView.SetContentOffset(new CoreGraphics.CGPoint(0, _origininalY), true);
                return true;
            }

            if (view is UICollectionView)
            {
                var uiCollectionView = view as UICollectionView;
                if (!_set)
                {
                    _origininalY = uiCollectionView.ContentOffset.Y;
                    _set = true;
                }

                if (_origininalY < 0)
                    return true;

                if (refreshing)
                    uiCollectionView.SetContentOffset(new CoreGraphics.CGPoint(0, _origininalY - _refreshControl.Frame.Size.Height), true);
                else
                    uiCollectionView.SetContentOffset(new CoreGraphics.CGPoint(0, _origininalY), true);
                return true;
            }

            if (view is UIWebView)
            {
                // Can't do anything
                return true;
            }

            if (view is UIScrollView)
            {
                var uiScrollView = view as UIScrollView;

                if (!_set)
                {
                    _origininalY = uiScrollView.ContentOffset.Y;
                    _set = true;
                }

                if (_origininalY < 0)
                    return true;

                if (refreshing)
                    uiScrollView.SetContentOffset(new CoreGraphics.CGPoint(0, _origininalY - _refreshControl.Frame.Size.Height), true);
                else
                    uiScrollView.SetContentOffset(new CoreGraphics.CGPoint(0, _origininalY), true);
                return true;
            }

            if (view.Subviews == null)
                return false;

            for (int i = 0; i < view.Subviews.Length; i++)
            {
                var control = view.Subviews[i];
                if (TryOffsetRefresh(control, refreshing))
                    return true;
            }

            return false;
        }


        bool TryInsertRefresh(UIView view, int index = 0)
        {
            _refreshControlParent = view;
            
            if (view is UITableView || view is UICollectionView)
            {
                view.InsertSubview(_refreshControl, index);
                return true;
            }

            if (view is UIWebView)
            {
                var uiWebView = view as UIWebView;
                uiWebView.ScrollView.InsertSubview(_refreshControl, index);
                return true;
            }

            if (view is UIScrollView)
            {
                var uiScrollView = view as UIScrollView;
                view.InsertSubview(_refreshControl, index);
                uiScrollView.AlwaysBounceVertical = true;
                return true;
            }
            
			if (view.Subviews == null)
                return false;

            for (int i = 0; i < view.Subviews.Length; i++)
            {
                var control = view.Subviews[i];
                if (TryInsertRefresh(control, i))
                    return true;
            }

            return false;
        }

        void UpdateColors()
        {
            if (RefreshView == null || _refreshControl == null)
                return;

            if (RefreshView.RefreshColor != Color.Default)
                _refreshControl.TintColor = RefreshView.RefreshColor.ToUIColor();

			SetBackgroundColor(RefreshView.BackgroundColor);
        }
       
		void UpdateIsRefreshing()
        {
            IsRefreshing = RefreshView.IsRefreshing;
        }

        void UpdateIsSwipeToRefreshEnabled()
        {
            if (RefreshView.IsEnabled)
            {
                TryInsertRefresh(_refreshControlParent);
            }
            else
            {
                if (_refreshControl.Superview != null)
                {
                    _refreshControl.RemoveFromSuperview();
                }
            }
        }
                       
        void OnRefresh(object sender, EventArgs e)
        {
            if (RefreshView?.Command?.CanExecute(RefreshView?.CommandParameter) ?? false)
            {
                RefreshView.Command.Execute(RefreshView?.CommandParameter);
            }
        }
    }
}