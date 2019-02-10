using DerpViewer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace DerpViewer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class WebImageListPage : ContentPage
    {
        ObservableCollection<DerpImage> Images;
        int CurrentIndex;

        public WebImageListPage(ObservableCollection<DerpImage> images, int index)
        {
            InitializeComponent();
            Images = images;
            CurrentIndex = index;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}