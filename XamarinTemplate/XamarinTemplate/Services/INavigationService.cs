using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinTemplate.Services
{
    public interface INavigationService
    {
        Task<Page> ModalNavigateBackward();
        Task ModalNavigateForward(Type viewModel, bool setBindingContext = true);
        Task ModalNavigateForwardWithInstance<T>(T viewModel);
        Task<Page> NavigateBackward();
        Task NavigateBackwardToRoot();
        Task NavigateForward(Type viewModel, bool setBindingContext = true);
        Task NavigateForwardWithInstance<T>(T viewModel);
        void RegisterView(Type view, bool viewHasViewmodel = true);
        void Start(Type intialView, Action ViewRegistration);
        void StartWithInstance(Page initialView, Action ViewRegistration);
    }
}