using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinTemplate.Services;
using XamarinTemplate.Views;
using XF.Material.Forms;

namespace XamarinTemplate
{
    public partial class App : Application
    {
        private readonly INavigationService navigationService;
        public App()
        {
            InitializeComponent();
            Material.Init(this);
            navigationService = Startup.ServiceProvider.GetService<INavigationService>();
            navigationService.Start(typeof(MainView), RegisterViews);
        }

        private void RegisterViews()
        {
            navigationService.RegisterView(typeof(MainView));
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
