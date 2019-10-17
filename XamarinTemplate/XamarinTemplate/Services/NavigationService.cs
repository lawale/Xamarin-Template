using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System.Reflection;

namespace XamarinTemplate.Services
{
    public class NavigationService : INavigationService
    {
        private readonly ILogger<NavigationService> logger;

        private readonly Dictionary<string, Type> viewDictionary = new Dictionary<string, Type>();

        public NavigationService(ILogger<NavigationService> logger)
        {
            this.logger = logger;
        }

        public void StartWithInstance(Page initialView, Action ViewRegistration)
        {
            ViewRegistration?.Invoke();
            Application.Current.MainPage = initialView as Page;
        }

        public void Start(Type initialView, Action ViewRegistration)
        {
            ViewRegistration?.Invoke();
            var key = initialView.Name + "Model";
            var viewModelName = $"{Assembly.GetAssembly(typeof(Startup)).GetName().Name}.ViewModels.{key}";
            var viewModel = Assembly.GetAssembly(typeof(Startup)).CreateInstance(viewModelName);
            Application.Current.MainPage = GetPageWithInstance(viewModel);
        }

        public void RegisterView(Type view, bool viewHasViewmodel = true)
        {
            if (view == null)
            {
                var message = "Cannot register null type as view.";
                logger.LogInformation(message);
                throw new ArgumentNullException(message);
            }

            if (!view.IsSubclassOf(typeof(Page)))
            {
                var message = "view does not derive from page";
                logger.LogInformation(message);
                throw new ArgumentException(message);
            }

            if (!view.Name.EndsWith("View"))
            {
                var message = "View name does not follow convention used in this project.  View Names should end with \"View\".";
                logger.LogInformation(message);
                throw new ArgumentException(message);
            }

            var key = view.Name + "Model";

            if (viewDictionary.ContainsKey(key))
            {
                var message = $"The viewmodel {key} is already assigned to a view.";
                logger.LogInformation(message);
                throw new ArgumentException(message);
            }

            viewDictionary[key] = view;

            logger.LogInformation($"{key} has been assigned as the viewmodel for view {view.Name}");
        }

        public async Task NavigateForwardWithInstance<T>(T viewModel)
        {
            var view = GetPageWithInstance(viewModel);
            await CurrentPage().Navigation.PushAsync(view);
            logger.LogInformation($"Completed Navigation to {view.GetType().Name}");
        }

        public async Task NavigateForward(Type viewModel, bool setBindingContext = true)
        {
            var view = GetPage(viewModel, setBindingContext);
            await CurrentPage().Navigation.PushAsync(view);
            logger.LogInformation($"Completed Navigation to {view.GetType().Name}");
        }

        public async Task ModalNavigateForward(Type viewModel, bool setBindingContext = true)
        {
            var view = GetPage(viewModel, setBindingContext);
            await CurrentPage().Navigation.PushModalAsync(view);
            logger.LogInformation($"Completed Navigation to {view.GetType().Name}");
        }

        public async Task ModalNavigateForwardWithInstance<T>(T viewModel)
        {
            var view = GetPageWithInstance(viewModel);
            await CurrentPage().Navigation.PushModalAsync(view);
            logger.LogInformation($"Completed Navigation to {view.GetType().Name}");
        }

        public async Task<Page> ModalNavigateBackward() => await CurrentPage().Navigation.PopModalAsync(true);

        public async Task<Page> NavigateBackward() => await CurrentPage().Navigation.PopAsync(true);

        public async Task NavigateBackwardToRoot() => await CurrentPage().Navigation.PopToRootAsync(true);

        private Page CurrentPage() => Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();

        private Page GetPageWithInstance<T>(T viewModel)
        {
            if (viewModel == null)
            {
                var message = "Null ViewModel.";
                logger.LogInformation(message);
                throw new ArgumentNullException(message);
            }

            var key = viewModel.GetType().Name;

            if (!viewDictionary.ContainsKey(key))
            {
                var message = $"The Type {key} or its associated View does not exist in NavigationService.";
                var modelStringIndex = key.LastIndexOf("Model");
                var viewName = key.Remove(modelStringIndex);
                //logger.LogError(exception, $"The Expected View {viewName} to be associated with {ViewModel.GetType().FullName} has not been registered. Regsiter {viewName} with the RegisterView method.");
                logger.LogInformation(message);
                throw new KeyNotFoundException(message);
            }

            var viewType = viewDictionary[key];
            var view = Activator.CreateInstance(viewType) as Page;
            view.BindingContext = viewModel;
            return view;
        }

        private Page GetPage(Type viewModel, bool setBindingContext)
        {
            var key = viewModel.Name;
            if (!viewDictionary.ContainsKey(key))
            {
                var message = $"The Type {key} or its associated View does not exist in NavigationService.";
                var modelStringIndex = key.LastIndexOf("Model");
                var viewName = key.Remove(modelStringIndex);
                logger.LogInformation(message);
                //logger.(exception, $"The Expected View {viewName} to be associated with {ViewModel.FullName} has not been registered. Regsiter {viewName} with the RegisterView method.");
                throw new KeyNotFoundException(message);
            }


            var viewType = viewDictionary[key];
            var view = Activator.CreateInstance(viewType) as Page;
            if (setBindingContext)
            {
                view.BindingContext = Startup.ServiceProvider.GetService(viewModel);
            }

            return view;
        }
    }
}
