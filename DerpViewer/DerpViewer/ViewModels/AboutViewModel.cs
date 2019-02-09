using System;
using System.Windows.Input;

using Xamarin.Forms;

namespace DerpViewer.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            OpenWebCommand = new Command(() => Device.OpenUri(new Uri("https://derpibooru.org")));
        }

        public ICommand OpenWebCommand { get; }
    }
}