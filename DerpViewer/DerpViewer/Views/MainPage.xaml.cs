using DerpViewer.Models;
using DerpViewer.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DerpViewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage
    {
        private DerpTagSQLiteDb _derpSQLiteDb;
        Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();

        public MainPage()
        {
            InitializeComponent();

            _derpSQLiteDb = new DerpTagSQLiteDb();
            MenuPages.Add((int)MenuItemType.ImageBrowser, (NavigationPage)Detail);
            MenuPages.Add((int)MenuItemType.TagBrowse, new NavigationPage(new DerpTagsPage()));
            MasterBehavior = MasterBehavior.Popover;
        }

        protected async override void OnAppearing()
        {
            if (!_derpSQLiteDb.IsLoaded)
            {
                await _derpSQLiteDb.Load();
                DerpImageCpt.ContentTags = (await _derpSQLiteDb.GetTagsAsync());
            }
            base.OnAppearing();
        }

        public DerpTagSQLiteDb GetDerpSQLiteDb()
        {
            return _derpSQLiteDb;
        }

        public async Task NavigateFromMenu(int id)
        {
            if (!MenuPages.ContainsKey(id))
            {
                switch (id)
                {
                    case (int)MenuItemType.ImageBrowser:
                        MenuPages.Add(id, new NavigationPage(new DerpImagesPage()));
                        break;
                    case (int)MenuItemType.TagBrowse:
                        MenuPages.Add(id, new NavigationPage(new DerpTagsPage()));
                        break;
                    case (int)MenuItemType.User:
                        MenuPages.Add(id, new NavigationPage(new UserPage()));
                        break;
                    case (int)MenuItemType.About:
                        MenuPages.Add(id, new NavigationPage(new AboutPage()));
                        break;
                }
            }

            var newPage = MenuPages[id];

            if (newPage != null && Detail != newPage)
            {
                Detail = newPage;

                if (Device.RuntimePlatform == Device.Android)
                    await Task.Delay(100);

                IsPresented = false;
            }
        }
    }
}