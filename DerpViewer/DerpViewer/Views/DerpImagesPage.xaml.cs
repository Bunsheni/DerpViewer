﻿using System;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using DerpViewer.Models;
using DerpViewer.ViewModels;
using DerpViewer.Services;
using FFImageLoading.Forms;
using System.Collections.Generic;

namespace DerpViewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DerpImagesPage : ContentPage, IWebConnection
    {
        protected App RootApp { get => Application.Current as App; }
        protected MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        DerpImagesViewModel viewModel;
        int _tabCount;
        bool pageLock;
        public DerpImagesPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new DerpImagesViewModel(this as IWebConnection);
            listView.RefreshCommand = new Command(async () =>
            {
                await viewModel.LastedView();
                listView.IsRefreshing = false;
            });
        }

        protected override async void OnAppearing()
        {
            if (listView.ItemsSource == null)
            {
                await RootPage.GetDerpSQLiteDb().GetTagsAsync();
                await viewModel.LastedView();
            }
            base.OnAppearing();
        }


        public async Task<string> TransWebBrowserInitAsync(string text, string from, string to)
        {
            return await webView.TransWebBrowserInitAsync(text, from, to);
        }

        public async Task<string> GetWebClintContentsAsync(string url)
        {
            return await webView.GetWebClintContentsAsync(url);
        }

        DerpImage tabbedImage;
        ImageSource tabbedImageSource;
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (_tabCount < 1)
            {
                try
                {
                    tabbedImage = viewModel.Images.Single(i => i.Id == ((CachedImage)sender).ClassId);
                    tabbedImageSource = ((CachedImage)sender).Source;
                }
                catch
                {
                    return;
                }
                TimeSpan tt = new TimeSpan(3000000);
                Device.StartTimer(tt, TestHandleFuncAsync);
            }
            _tabCount++;
        }
        private bool TestHandleFuncAsync()
        {
            if (tabbedImage != null)
            {
                if (_tabCount > 1)
                {
                    Application.Current.MainPage.Navigation.PushModalAsync(new WebPage($"https://derpibooru.org/{tabbedImage.Id}"));
                }
                else
                {
                    if(tabbedImage.OriginalFormat == "webm")
                        Application.Current.MainPage.Navigation.PushModalAsync(new WebPage(tabbedImage.ImageUrl));
                    else
                        Application.Current.MainPage.Navigation.PushModalAsync(new ImagePage(tabbedImage.ImageUrl, tabbedImageSource));
                }
                tabbedImage = null;
                _tabCount = 0;
            }
            return false;
        }


        private void searchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchBar.Text.Trim().Length != 0 && searchBar.Text.Trim().Last() != ',')
            {
                Task.Run(() => viewModel.GetSuggestionItem());
                searchView.IsVisible = true;
                contentView.IsVisible = false;
            }
            else
            {
                searchView.IsVisible = false;
                contentView.IsVisible = true;
            }
        }

        private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            viewModel.Search();
            searchView.IsVisible = false;
            contentView.IsVisible = true;
        }

        private void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((DerpImage)e.Item).IsSelected ^= true;
            listView.SelectedItem = null;
        }
        private void Download_Clicked(object sender, EventArgs e)
        {
            Task.Run(() => viewModel.Download());
        }

        private async void Sort_Clicked(object sender, EventArgs e)
        {
            string temp = await DisplayActionSheet("SortBy", "Cancle", null, DerpibooruService.sortbyen);
            int tempint = DerpibooruService.sortbyen.ToList().FindIndex(i => string.Compare(i, temp) == 0);
            viewModel.Sort(tempint);
        }

        private void listView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            viewModel.listViewItemAppearing(e.Item);
        }

        private void ClearSelect_Clicked(object sender, EventArgs e)
        {
            viewModel.ClearSelect();
        }

        string tabbedItem;
        private void suggestionListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            suggestionListView.SelectedItem = null;
        }

        private void TextCell_Tapped(object sender, EventArgs e)
        {
            if (_tabCount < 1)
            {
                try
                {
                    tabbedItem = ((TextCell)sender).Text;
                }
                catch
                {
                    return;
                }
                TimeSpan tt = new TimeSpan(3000000);
                Device.StartTimer(tt, SuggestionHandleFuncAsync);
            }
            _tabCount++;
        }

        private bool SuggestionHandleFuncAsync()
        {
            if (tabbedItem != null)
            {
                if (_tabCount > 1)
                {
                    viewModel.AddKey("-" + tabbedItem);
                }
                else
                {
                    viewModel.AddKey(tabbedItem);
                }
            }
            searchView.IsVisible = false;
            contentView.IsVisible = true;
            searchBar.Focus();
            _tabCount = 0;
            return false;
        }

        private void listView_ItemDisappearing(object sender, ItemVisibilityEventArgs e)
        {
        }

        private async void LabelTapped(object sender, EventArgs e)
        {
            if (pageLock) return;
            pageLock = true;
            List<DerpTag> models = new List<DerpTag>();
            if (((Label)sender).Text != "unknown artist" && ((Label)sender).Text != "no cotent" && ((Label)sender).Text != "no character" && ((Label)sender).Text != "no tag")
            {
                foreach (string str in Library.stringDivider(((Label)sender).Text, ", "))
                {
                    DerpTag tag = await viewModel.DerpDb.GetTagFromNameAsync(str);
                    if(tag != null)
                        models.Add(tag);
                }
                if (models.Count > 0)
                {
                    DisplayAndSearch(models);
                }
            }
            pageLock = false;
        }

        public async void DisplayAndSearch(List<DerpTag> models)
        {
            List<string> dis = new List<string>();
            foreach (DerpTag model in models)
            {
                dis.Add(model.NameEn);
            }
            string select = await DisplayActionSheet(null, null, null, dis.ToArray());
            int selectedindex = dis.FindIndex(i => i == select);
            if (selectedindex < 0) return;
            DerpTag selectedModel = models[selectedindex];
            viewModel.AddKey(selectedModel.NameEn);
            searchBar.Focus();
        }

        private void SearchBox_ChildAdded(object sender, ElementEventArgs e)
        {

        }
    }
}