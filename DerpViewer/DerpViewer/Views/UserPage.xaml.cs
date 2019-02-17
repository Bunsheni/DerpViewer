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
	public partial class UserPage : ContentPage
    {
        protected App RootApp { get => Application.Current as App; }
        public string UserAPIKey
        {
            get
            {
                return RootApp.UserAPIKey;
            }
            set
            {
                RootApp.UserAPIKey = value.Trim();
            }
        }
        public string TagCount
        {
            get
            {
                return RootApp.TagCount.ToString();
            }
            set
            {
                RootApp.TagCount = int.Parse(value.Trim());
            }
        }
        public bool UsingFilter
        {
            get
            {
                return RootApp.UsingFilter;
            }
            set
            {
                RootApp.UsingFilter = value;
            }
        }
        public bool HideTopbar
        {
            get
            {
                return RootApp.HideTopbar;
            }
            set
            {
                RootApp.HideTopbar = value;
            }
        }

        public UserPage ()
		{
			InitializeComponent ();
            BindingContext = this;
		}
	}
}