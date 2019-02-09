using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using FFImageLoading;
namespace DerpViewer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ImagePage : ContentPage
    {
        public ImagePage (string source, ImageSource loadingimage)
		{
			InitializeComponent ();
            imageView.LoadingPlaceholder = loadingimage;
            imageView.Source = source;
            BindingContext = this;
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}