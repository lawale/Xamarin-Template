using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XamarinTemplate.Controls;
using XamarinTemplate.Droid.Renderers;

[assembly: ExportRenderer(typeof(RefreshLayout), typeof(RefreshLayoutRenderer))]
namespace XamarinTemplate.Droid.Renderers
{
    public class RefreshLayoutRenderer : SwipeRefreshLayout, IVisualElementRenderer, SwipeRefreshLayout.IOnRefreshListener
    {
        public RefreshLayoutRenderer(Context context)
            : base(context)
        {

        }

        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
        public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

        bool init;
        IVisualElementRenderer packed;

        public void SetElement(VisualElement element)
        {
            var oldElement = Element;

            //unregister old and re-register new
            if (oldElement != null)
                oldElement.PropertyChanged -= HandlePropertyChanged;

            Element = element;
            if (Element != null)
            {
                UpdateContent();
                Element.PropertyChanged += HandlePropertyChanged;
            }

            if (!init)
            {
                init = true;
                //sizes to match the forms view
                //updates properties, handles visual element properties
                Tracker = new VisualElementTracker(this);
                SetOnRefreshListener(this);
            }

            UpdateColors();
            UpdateIsRefreshing();
            UpdateIsSwipeToRefreshEnabled();
            ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, this.Element));
        }

        public SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
        {
            packed.View.Measure(widthConstraint, heightConstraint);

            //Measure child here and determine size
            return new SizeRequest(new Size(packed.View.MeasuredWidth, packed.View.MeasuredHeight));
        }



        void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Content")
                UpdateContent();
            else if (e.PropertyName == RefreshLayout.IsPullToRefreshEnabledProperty.PropertyName)
                UpdateIsSwipeToRefreshEnabled();
            else if (e.PropertyName == RefreshLayout.IsRefreshingProperty.PropertyName)
                UpdateIsRefreshing();
            else if (e.PropertyName == RefreshLayout.RefreshColorProperty.PropertyName)
                UpdateColors();
            else if (e.PropertyName == RefreshLayout.RefreshBackgroundColorProperty.PropertyName)
                UpdateColors();
        }

        void UpdateContent()
        {
            if (RefreshView.Content == null)
                return;

            if (packed != null)
                RemoveView(packed.View);

            packed = Platform.CreateRendererWithContext(RefreshView.Content, Context);

            try
            {
                RefreshView.Content.SetValue(RendererProperty, packed);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Unable to sent renderer property, maybe an issue: " + ex);
            }

            AddView(packed.View, LayoutParams.MatchParent);

        }

        BindableProperty rendererProperty = null;

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

                var type = Type.GetType("Xamarin.Forms.Platform.Android.Platform, Xamarin.Forms.Platform.Android");
                var prop = type.GetField("RendererProperty", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
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
                SetColorSchemeColors(RefreshView.RefreshColor.ToAndroid());
            if (RefreshView.RefreshBackgroundColor != Color.Default)
                SetProgressBackgroundColorSchemeColor(RefreshView.RefreshBackgroundColor.ToAndroid());
        }

        bool refreshing;

        public override bool Refreshing
        {
            get
            {
                return refreshing;
            }
            set
            {
                try
                {
                    refreshing = value;
                    //this will break binding :( sad panda we need to wait for next version for this
                    //right now you can't update the binding.. so it is 1 way
                    if (RefreshView != null && RefreshView.IsRefreshing != refreshing)
                        RefreshView.IsRefreshing = refreshing;

                    if (base.Refreshing == refreshing)
                        return;

                    base.Refreshing = refreshing;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        void UpdateIsRefreshing() =>
            Refreshing = RefreshView.IsRefreshing;


        void UpdateIsSwipeToRefreshEnabled() =>
            Enabled = RefreshView.IsPullToRefreshEnabled;



        /// <summary>
        /// Determines whether this instance can child scroll up.
        /// We do this since the actual swipe refresh can't figure it out
        /// </summary>
        /// <returns><c>true</c> if this instance can child scroll up; otherwise, <c>false</c>.</returns>
        public override bool CanChildScrollUp() =>
            CanScrollUp(packed.View);


        bool CanScrollUp(Android.Views.View view)
        {
            var viewGroup = view as ViewGroup;
            if (viewGroup == null)
                return base.CanChildScrollUp();

            var sdk = (int)global::Android.OS.Build.VERSION.SdkInt;
            if (sdk >= 16)
            {
                //is a scroll container such as listview, scroll view, gridview
                if (viewGroup.IsScrollContainer)
                {
                    return base.CanChildScrollUp();
                }
            }

            //if you have something custom and you can't scroll up you might need to enable this
            //for instance on a custom recycler view where the code above isn't working!
            for (int i = 0; i < viewGroup.ChildCount; i++)
            {
                var child = viewGroup.GetChildAt(i);
                if (child is Android.Widget.AbsListView)
                {
                    if (child is Android.Widget.AbsListView list)
                    {
                        if (list.FirstVisiblePosition == 0)
                        {
                            var subChild = list.GetChildAt(0);

                            return subChild != null && subChild.Top != 0;
                        }

                        //if children are in list and we are scrolled a bit... sure you can scroll up
                        return true;
                    }

                }
                else if (child is Android.Widget.ScrollView)
                {
                    var scrollview = child as Android.Widget.ScrollView;
                    return (scrollview.ScrollY <= 0.0);
                }
                else if (child is Android.Webkit.WebView)
                {
                    var webView = child as Android.Webkit.WebView;
                    return (webView.ScrollY > 0.0);
                }
                else if (child is Android.Support.V4.Widget.SwipeRefreshLayout)
                {
                    return CanScrollUp(child as ViewGroup);
                }
                //else if something else like a recycler view?

            }

            return false;
        }


        /// <summary>
        /// Helpers to cast our element easily
        /// Will throw an exception if the Element is not correct
        /// </summary>
        /// <value>The refresh view.</value>
        public RefreshLayout RefreshView =>
            Element == null ? null : (RefreshLayout)Element;

        /// <summary>
        /// The refresh view has been refreshed
        /// </summary>
        public void OnRefresh()
        {
            if (RefreshView?.RefreshCommand?.CanExecute(RefreshView?.RefreshCommandParameter) ?? false)
            {
                RefreshView.RefreshCommand.Execute(RefreshView?.RefreshCommandParameter);
            }
        }

        public void SetLabelFor(int? id)
        {

        }

        /// <summary>
        /// Gets the tracker.
        /// </summary>
        /// <value>The tracker.</value>
        public VisualElementTracker Tracker { get; private set; }


        /// <summary>
        /// Gets the view group.
        /// </summary>
        /// <value>The view group.</value>
        public Android.Views.ViewGroup ViewGroup => this;


        public Android.Views.View View => this;

        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <value>The element.</value>
        public VisualElement Element { get; private set; }

        /// <summary>
        /// Cleanup layout.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            /*if (disposing)
            {
                if (Element != null)
                {
                    Element.PropertyChanged -= HandlePropertyChanged;
                }
                if (packed != null)
                    RemoveView(packed.ViewGroup);
            }
            packed?.Dispose();
            packed = null;
            Tracker?.Dispose();
            Tracker = null;
            
            if (rendererProperty != null)
            {
                rendererProperty = null;
            }
            init = false;*/
        }

        public void UpdateLayout() => Tracker?.UpdateLayout();
    }
}