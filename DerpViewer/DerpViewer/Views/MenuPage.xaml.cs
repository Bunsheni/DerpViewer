using DerpViewer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DerpViewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        List<HomeMenuItem> menuItems;
        public MenuPage()
        {
            InitializeComponent();

            menuItems = new List<HomeMenuItem>
            {
                new HomeMenuItem {Id = MenuItemType.ImageBrowser, Title="Images" },
                new HomeMenuItem {Id = MenuItemType.FavoriteBrowser, Title="Favorite" },
                new HomeMenuItem {Id = MenuItemType.TagBrowser, Title="Tags" },
                new HomeMenuItem {Id = MenuItemType.FileBrowser, Title="Files" },
                new HomeMenuItem {Id = MenuItemType.User, Title="User" },
                new HomeMenuItem {Id = MenuItemType.About, Title="About" }
            };

            ListViewMenu.ItemsSource = menuItems;

            ListViewMenu.SelectedItem = menuItems[0];
            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                var id = (int)((HomeMenuItem)e.SelectedItem).Id;
                await RootPage.NavigateFromMenu(id);
            };
        }

        public async Task MoveMenu(int id)
        {
            ListViewMenu.SelectedItem = menuItems[id];
            await RootPage.NavigateFromMenu(id);
        }
    }
}