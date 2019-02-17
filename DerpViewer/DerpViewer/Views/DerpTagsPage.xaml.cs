using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using DerpViewer.Models;
using DerpViewer.Services;
using DerpViewer.ViewModels;

namespace DerpViewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DerpTagsPage : ContentPage, IWebConnection
    {
        protected App RootApp { get => Application.Current as App; }
        protected MainPage RootPage { get => Application.Current.MainPage as MainPage; }

        DerpibooruService derpibooru;
        DerpTagsViewModel viewModel;

        public DerpTagsPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new DerpTagsViewModel();
            progressBar.BindingContext = derpibooru = new DerpibooruService(this as IWebConnection);
        }

        public async Task<string> TransWebBrowserInitAsync(string text, string from, string to)
        {
            return await webView.TransWebBrowserInitAsync(text, from, to);
        }

        public async Task<string> GetWebClintContentsAsync(string url)
        {
            return await webView.GetWebClintContentsAsync(url);
        }


        protected override async void OnAppearing()
        {
            await viewModel.Load();
            base.OnAppearing();
        }

        private void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;            
            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }

        private async void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsBusy)
            {
                IsBusy = true;
                try
                {
                    string key = searchBar.Text;
                    if (key.Length != 0)
                    {
                        List<DerpTag> models = await RootPage.GetDerpSQLiteDb().GetTagsAsync();
                        listView.ItemsSource = models.FindAll(i => i.NameEn.Contains(key) || i.NameKr.Contains(key));
                    }
                    else
                    {
                        listView.ItemsSource = await RootPage.GetDerpSQLiteDb().GetTagsAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async void TagUpdate_Clicked(object sender, EventArgs e)
        {
            progressView.IsVisible = true;
            progressView.HeightRequest = 4;

            await Task.Run(async () =>
            {             
                List<DerpTag> oldTags = (await RootPage.GetDerpSQLiteDb().GetTagsAsync()).FindAll(i => i.Category != DerpTagCategory.NONE && !i.NameEn.StartsWith("artist:") && !i.NameEn.StartsWith("oc:") && !i.NameEn.StartsWith("editor:") && i.Category != DerpTagCategory.ERROR);
                List<DerpTag> newTags = derpibooru.GetTagInfoFromDerpibooru(oldTags, RootApp.TagCount);
                if (newTags != null)
                {
                    await RootPage.GetDerpSQLiteDb().DropDerpTagTable();
                    await RootPage.GetDerpSQLiteDb().CreatDerpTagTable();
                    (await RootPage.GetDerpSQLiteDb().GetTagsAsync()).Clear();
                    await RootPage.GetDerpSQLiteDb().AddTagsAsync(newTags);
                }
            });

            progressView.HeightRequest = 0;
            progressView.IsVisible = false;
            await viewModel.Load();
        }
    }
}
