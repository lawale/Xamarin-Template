﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XamarinTemplate.Controls;

namespace XamarinTemplate.iOS.Renderers
{
    [Preserve(AllMembers = true)]
    public class RefreshLayoutRenderer : ViewRenderer<RefreshLayout, UIView>
    {
        /// <summary>
        /// Used for registration with dependency service
        /// </summary>
        public static void Init(string value = null)
        {
            var temp = DateTime.Now;

        }

        UIRefreshControl refreshControl;
        UIView refreshControlParent;

        /// <summary>
        /// Raises the element changed event.
        /// </summary>
        /// <param name="e">E.</param>
        protected override void OnElementChanged(ElementChangedEventArgs<RefreshLayout> e)
        {

            base.OnElementChanged(e);
            if (e?.OldElement != null || Element == null)
                return;
            refreshControl = new UIRefreshControl();
            refreshControl.ValueChanged += OnRefresh;
            this.refreshControlParent = this;

            UpdateColors();
            UpdateIsRefreshing();
            UpdateIsSwipeToRefreshEnabled();
        }

        bool set;
        nfloat origininalY;

        bool TryOffsetRefresh(UIView view, bool refreshing, int index = 0)
        {
            if (view is UITableView)
            {
                var uiTableView = view as UITableView;

                if (!set)
                {
                    origininalY = uiTableView.ContentOffset.Y;
                    set = true;

                }

                if (origininalY < 0)
                    return true;

                if (refreshing)
                    uiTableView.SetContentOffset(new CoreGraphics.CGPoint(0, origininalY - refreshControl.Frame.Size.Height), true);
                else
                    uiTableView.SetContentOffset(new CoreGraphics.CGPoint(0, origininalY), true);
                return true;
            }

            if (view is UICollectionView)
            {
                var uiCollectionView = view as UICollectionView;

                if (!set)
                {
                    origininalY = uiCollectionView.ContentOffset.Y;
                    set = true;
                }

                if (origininalY < 0)
                    return true;

                if (refreshing)
                    uiCollectionView.SetContentOffset(new CoreGraphics.CGPoint(0, origininalY - refreshControl.Frame.Size.Height), true);
                else
                    uiCollectionView.SetContentOffset(new CoreGraphics.CGPoint(0, origininalY), true);
                return true;
            }

            if (view is UIWebView)
            {
                //can't do anything
                return true;
            }

            if (view is UIScrollView)
            {
                var uiScrollView = view as UIScrollView;

                if (!set)
                {
                    origininalY = uiScrollView.ContentOffset.Y;
                    set = true;
                }

                if (origininalY < 0)
                    return true;

                if (refreshing)
                    uiScrollView.SetContentOffset(new CoreGraphics.CGPoint(0, origininalY - refreshControl.Frame.Size.Height), true);
                else
                    uiScrollView.SetContentOffset(new CoreGraphics.CGPoint(0, origininalY), true);
                return true;
            }
            if (view.Subviews == null)
                return false;

            for (int i = 0; i < view.Subviews.Length; ++i)
            {
                var control = view.Subviews[i];
                if (TryOffsetRefresh(control, refreshing, i))
                    return true;
            }
            return false;
        }

        bool TryInsertRefresh(UIView view, int index = 0)
        {
            this.refreshControlParent = view;

            if (view is UITableView)
            {
                var uiTableView = view as UITableView;
                uiTableView = view as UITableView;
                view.InsertSubview(refreshControl, index);
                return true;
            }

            if (view is UICollectionView)
            {
                var uiCollectionView = view as UICollectionView;
                uiCollectionView = view as UICollectionView;
                view.InsertSubview(refreshControl, index);
                return true;
            }

            if (view is UIWebView)
            {
                var uiWebView = view as UIWebView;
                uiWebView.ScrollView.InsertSubview(refreshControl, index);
                return true;
            }

            if (view is UIScrollView)
            {
                var uiScrollView = view as UIScrollView;
                view.InsertSubview(refreshControl, index);
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

        BindableProperty rendererProperty;

        /// <summary>
        /// Gets the bindable property.
        /// </summary>
        /// <returns>The bindable property.</returns>
        BindableProperty RendererProperty
        {
            get
            {
                if (rendererProperty != null)
                    return rendererProperty;

                var type = Type.GetType("Xamarin.Forms.Platform.iOS.Platform, Xamarin.Forms.Platform.iOS");
                var prop = type.GetField("RendererProperty");
                var val = prop.GetValue(null);
                rendererProperty = val as BindableProperty;
                return rendererProperty;
            }
        }

        void UpdateColors()
        {
            if (RefreshView == null)
                return;

            if (RefreshView.RefreshColor != Color.Default)
                refreshControl.TintColor = RefreshView.RefreshColor.ToUIColor();

            if (RefreshView.RefreshBackgroundColor != Color.Default)
                refreshControl.BackgroundColor = RefreshView.RefreshBackgroundColor.ToUIColor();
        }

        void UpdateIsRefreshing()
        {
            IsRefreshing = RefreshView.IsRefreshing;
        }

        void UpdateIsSwipeToRefreshEnabled()
        {
            if (RefreshView.IsPullToRefreshEnabled)
            {
                this.TryInsertRefresh(this.refreshControlParent);
            }
            else
            {
                if (this.refreshControl.Superview != null)
                {
                    this.refreshControl.RemoveFromSuperview();
                }
            }
        }

        /// <summary>
        /// Helpers to cast our element easily
        /// Will throw an exception if the Element is not correct
        /// </summary>
        /// <value>The refresh view.</value>
        public RefreshLayout RefreshView => Element;


        bool isRefreshing;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is refreshing.
        /// </summary>
        /// <value><c>true</c> if this instance is refreshing; otherwise, <c>false</c>.</value>
        public bool IsRefreshing
        {
            get => isRefreshing;
            set
            {
                bool changed = IsRefreshing != value;
                isRefreshing = value;
                if (isRefreshing)
                    refreshControl.BeginRefreshing();
                else
                    refreshControl.EndRefreshing();
                if (changed)
                    TryOffsetRefresh(this, IsRefreshing);
            }
        }

        /// <summary>
        /// The refresh view has been refreshed
        /// </summary>
        void OnRefresh(object sender, EventArgs e)
        {
            if (RefreshView?.RefreshCommand?.CanExecute(RefreshView?.RefreshCommandParameter) ?? false)
            {
                RefreshView.RefreshCommand.Execute(RefreshView?.RefreshCommandParameter);
            }
        }



        /// <summary>
        /// Raises the element property changed event.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e?.PropertyName == RefreshLayout.IsPullToRefreshEnabledProperty.PropertyName)
                UpdateIsSwipeToRefreshEnabled();

            else if (e.PropertyName == RefreshLayout.IsRefreshingProperty.PropertyName)
                UpdateIsRefreshing();

            else if (e.PropertyName == RefreshLayout.RefreshColorProperty.PropertyName)
                UpdateColors();

            else if (e.PropertyName == RefreshLayout.RefreshBackgroundColorProperty.PropertyName)
                UpdateColors();
        }

        /// <summary>
        /// Dispose the specified disposing.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (refreshControl != null)
                refreshControl.ValueChanged -= OnRefresh;
            this.refreshControlParent = null;
        }
    }
}