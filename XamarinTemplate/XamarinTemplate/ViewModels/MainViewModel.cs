using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;

namespace XamarinTemplate.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public ICommand ShowAlert { get; }

        public MainViewModel()
        {
            ShowAlert = new Command(ExecuteShowAlert);
        }

        private void ExecuteShowAlert()
        {
            MaterialDialog.Instance.AlertAsync("This is an Alert");
        }
    }
}
