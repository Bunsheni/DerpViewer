using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace DerpViewer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class WebPage : ContentPage
    {
        public WebPage(string url)
        {
            InitializeComponent();
            webView.Source = new UrlWebViewSource() { Url = url };
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}