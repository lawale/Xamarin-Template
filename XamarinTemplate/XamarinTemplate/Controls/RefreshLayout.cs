using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace XamarinTemplate.Controls
{
    public class RefreshLayout : ContentView
    {
        public RefreshLayout()
        {
            IsClippedToBounds = true;
            VerticalOptions = LayoutOptions.FillAndExpand;
            HorizontalOptions = LayoutOptions.FillAndExpand;
        }

        /// <summary>
        /// The is refreshing property.
        /// </summary>
        public static readonly BindableProperty IsRefreshingProperty =
            BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(RefreshLayout), false);

        /// <summary>
        /// Gets or sets a value indicating whether this instance is refreshing.
        /// </summary>
        /// <value><c>true</c> if this instance is refreshing; otherwise, <c>false</c>.</value>
        public bool IsRefreshing
        {
            get => (bool)GetValue(IsRefreshingProperty);
            set
            {
                if ((bool)GetValue(IsRefreshingProperty) == value)
                    OnPropertyChanged(nameof(IsRefreshing));

                SetValue(IsRefreshingProperty, value);
            }
        }

        /// <summary>
        /// The is pull to refresh enabled property.
        /// </summary>
        public static readonly BindableProperty IsPullToRefreshEnabledProperty =
            BindableProperty.Create(nameof(IsPullToRefreshEnabled), typeof(bool), typeof(RefreshLayout), true);

        /// <summary>
        /// Gets or sets a value indicating whether this instance is pull to refresh enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is pull to refresh enabled; otherwise, <c>false</c>.</value>
        public bool IsPullToRefreshEnabled
        {
            get => (bool)GetValue(IsPullToRefreshEnabledProperty);
            set => SetValue(IsPullToRefreshEnabledProperty, value);
        }


        /// <summary>
        /// The refresh command property.
        /// </summary>
        public static readonly BindableProperty RefreshCommandProperty =
            BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(RefreshLayout));

        /// <summary>
        /// Gets or sets the refresh command.
        /// </summary>
        /// <value>The refresh command.</value>
        public ICommand RefreshCommand
        {
            get => (ICommand)GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }

        /// <summary>
        /// Gets the Refresh command 
        /// </summary>
        public static readonly BindableProperty RefreshCommandParameterProperty =
            BindableProperty.Create(nameof(RefreshCommandParameter),
                typeof(bool),
                typeof(RefreshLayout),
                false);

        /// <summary>
        /// Gets or sets the Refresh command parameter
        /// </summary>
        public bool RefreshCommandParameter
        {
            get => (bool)GetValue(RefreshCommandParameterProperty);
            set => SetValue(RefreshCommandParameterProperty, value);
        }

        /// <summary>
        /// Executes if enabled or not based on can execute changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        void RefreshCommandCanExecuteChanged(object sender, EventArgs eventArgs)
        {
            ICommand cmd = RefreshCommand;
            if (cmd != null)
                IsEnabled = cmd.CanExecute(RefreshCommandParameter);
        }

        /// <summary>
        /// Color property of refresh spinner color 
        /// </summary>
        public static readonly BindableProperty RefreshColorProperty =
            BindableProperty.Create(nameof(RefreshColor), typeof(Color), typeof(RefreshLayout), Color.Default);

        /// <summary>
        /// Refresh  color
        /// </summary>
        public Color RefreshColor
        {
            get => (Color)GetValue(RefreshColorProperty);
            set => SetValue(RefreshColorProperty, value);
        }

        /// <summary>
        /// Color property of refresh background color
        /// </summary>
        public static readonly BindableProperty RefreshBackgroundColorProperty =
            BindableProperty.Create(nameof(RefreshBackgroundColor), typeof(Color), typeof(RefreshLayout), Color.Default);

        /// <summary>
        /// Refresh background color
        /// </summary>
        public Color RefreshBackgroundColor
        {
            get => (Color)GetValue(RefreshBackgroundColorProperty);
            set => SetValue(RefreshBackgroundColorProperty, value);
        }


        /// <param name="widthConstraint">The available width for the element to use.</param>
        /// <param name="heightConstraint">The available height for the element to use.</param>
        /// <summary>
        /// Optimization as we can get the size here of our content all in DIP
        /// </summary>
        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (Content == null)
                return new SizeRequest(new Size(100, 100));

            return base.OnMeasure(widthConstraint, heightConstraint);
        }
    }
}
