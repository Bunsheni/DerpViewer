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
        private DerpTagSQLiteDb _derpTagSQLiteDb;
        private DerpImageSQLiteDb _derpImageSQLiteDb;
        public Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();
        public DerpImagesPage MainImageView => MenuPages[(int)MenuItemType.ImageBrowser].RootPage as DerpImagesPage;
        public DerpImagesPage FavoriteImageView => MenuPages[(int)MenuItemType.FavoriteBrowser].RootPage as DerpImagesPage;

        public MainPage()
        {
            InitializeComponent();
            _derpTagSQLiteDb = new DerpTagSQLiteDb();
            _derpImageSQLiteDb = new DerpImageSQLiteDb();
            MenuPages.Add((int)MenuItemType.ImageBrowser, (NavigationPage)Detail);
            MenuPages.Add((int)MenuItemType.FavoriteBrowser, new NavigationPage(new DerpImagesPage(true)));
            MenuPages.Add((int)MenuItemType.TagBrowse, new NavigationPage(new DerpTagsPage()));
            MasterBehavior = MasterBehavior.Popover;
        }

        protected async override void OnAppearing()
        {
            if (!_derpTagSQLiteDb.IsLoaded)
            {
                await _derpTagSQLiteDb.Load();
                DerpImage.ContentTags = (await _derpTagSQLiteDb.GetTagsAsync());                
            }
            base.OnAppearing();
        }

        public DerpTagSQLiteDb GetDerpTagSQLiteDb()
        {
            return _derpTagSQLiteDb;
        }

        public DerpImageSQLiteDb GetDerpImageSQLiteDb()
        {
            return _derpImageSQLiteDb;
        }

        public async Task MoveMenu(int id)
        {
            await MenuPageView.MoveMenu(id);
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
                    case (int)MenuItemType.FavoriteBrowser:
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