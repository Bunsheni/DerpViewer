using System;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using DerpViewer.Models;
using DerpViewer.ViewModels;
using DerpViewer.Services;
using FFImageLoading.Forms;
using System.Collections.Generic;
using FFImageLoading;
using FFImageLoading.Cache;

namespace DerpViewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DerpImagesPage : ContentPage, IWebConnection
    {
        protected App RootApp { get => Application.Current as App; }
        protected MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        DerpImagesViewModel viewModel;
        int _tabCount;
        bool pageLock, viewMode;
        string tabbedSuggestionItem;
        DerpImage tabbedImage;
        ImageSource tabbedImageSource;

        public DerpImagesPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new DerpImagesViewModel(this as IWebConnection);
            listView.RefreshCommand = new Command(async () =>
            {
                await viewModel.ExecuteLoadItemsCommand();
                listView.IsRefreshing = false;
            });
            listView2.RefreshCommand = new Command(async () =>
            {
                await viewModel.ExecuteLoadItemsCommand();
                listView2.IsRefreshing = false;
            });
        }

        protected override async void OnAppearing()
        {
            if (listView.ItemsSource == null)
            {
                await RootPage.GetDerpSQLiteDb().GetTagsAsync();
                await viewModel.ExecuteLoadItemsCommand();
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

        private void ImageTapGestureRecognizer_Tapped(object sender, EventArgs e)
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
            searchView.IsVisible = false;
            string temp = searchBar.Text.Trim();
            if (searchBar.Text.Trim().Length != 0 && !viewModel.ExistItem(temp))
            {
                AddToSearchBox(temp, false);
            }
            contentView.IsVisible = true;
        }

        private void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((DerpImage)e.Item).IsSelected ^= true;
            listView.SelectedItem = null;
            int selected = viewModel.GetSelectedImages().Count;
            clearSelectToolbarItem.Text = selected > 0 ? selected +" Selected" : "ClearSelect";
        }

        int _lastItemAppearedIdx;
        bool _lastItemLock;
        private void listView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            viewModel.listViewItemAppearing(e.Item);
            var currentIdx = viewModel.Images.IndexOf((DerpImage)e.Item);

            if (!_lastItemLock || _lastItemAppearedIdx == 0)
            {
                if (currentIdx > _lastItemAppearedIdx)
                {
                    if (_lastItemAppearedIdx != 0)
                    {
                        viewModel.HasNavigationBar = false;
                        searchBar.IsVisible = false;
                        progressView.IsVisible = false;
                        searchBox.IsVisible = false;
                    }
                    else
                    {
                        _lastItemAppearedIdx = viewModel.Images.IndexOf((DerpImage)e.Item);
                        return;
                    }
                }
                else
                {
                    viewModel.HasNavigationBar = true;
                    searchBar.IsVisible = true;
                    progressView.IsVisible = true;
                    searchBox.IsVisible = true;
                }
                _lastItemLock = true;
                TimeSpan tt = new TimeSpan(5000000);
                Device.StartTimer(tt, TimeHandleFuncAsync);
            }

            _lastItemAppearedIdx = viewModel.Images.IndexOf((DerpImage)e.Item);
        }
        private bool TimeHandleFuncAsync()
        {
            _lastItemLock = false;
            return false;
        }

        private void ListView_ItemDisappearing(object sender, ItemVisibilityEventArgs e)
        {
        }

        private void suggestionListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (_tabCount < 1)
            {
                try
                {
                    tabbedSuggestionItem = ((DerpTag)e.Item).NameEn;
                }
                catch
                {
                    return;
                }
                TimeSpan tt = new TimeSpan(5000000);
                Device.StartTimer(tt, SuggestionHandleFuncAsync);
            }
            _tabCount++;
        }

        private bool SuggestionHandleFuncAsync()
        {
            if (tabbedSuggestionItem != null)
            {
                string temp;
                if (_tabCount > 1)
                {
                    temp = "-" + tabbedSuggestionItem;
                }
                else
                {
                    temp = tabbedSuggestionItem;
                }
                if (!viewModel.ExistItem(temp))
                {
                    AddToSearchBox(temp, false);
                }
            }
            searchView.IsVisible = false;
            contentView.IsVisible = true;
            _tabCount = 0;
            return false;
        }

        public void AddToSearchBox(string item, bool clear)
        {
            if (clear)
            {
                viewModel.ClearFilterItem();
                searchBox.Children.Clear();
            }
            viewModel.AddFilterItem(item);
            searchBox.Children.Add(new SearchBoxLabel(item, viewModel));
        }

        private async void LabelTapped(object sender, EventArgs e)
        {
            if (pageLock) return;
            pageLock = true;
            List<DerpTag> models = new List<DerpTag>();
            if (((Label)sender).Text != "unknown artist" && ((Label)sender).Text != "no content" && ((Label)sender).Text != "no character" && ((Label)sender).Text != "no tag")
            {
                foreach (string str in Library.stringDivider(((Label)sender).Text, ", "))
                {
                    DerpTag tag = await viewModel.DerpDb.GetTagFromNameAsync(str);
                    if (tag != null)
                        models.Add(tag);
                    else
                        models.Add(new DerpTag(str));
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
            if (!viewModel.ExistItem(selectedModel.NameEn))
            {
                AddToSearchBox(selectedModel.NameEn, false);
            }
        }

        private void SearchBox_ChildAdded(object sender, ElementEventArgs e)
        {
            SearchAction();
        }

        private async void SearchAction()
        {
            listView.IsRefreshing = true;
            listView2.IsRefreshing = true;
            await viewModel.Search();
            listView.IsRefreshing = false;
            listView2.IsRefreshing = false;
        }

        private void Download_Clicked(object sender, EventArgs e)
        {
            Task.Run(() => viewModel.Download());
        }

        private async void Sort_Clicked(object sender, EventArgs e)
        {
            string temp = await DisplayActionSheet("SortBy", "Cancle", null, DerpibooruService.sortbyen);
            if (temp != null)
            {
                int tempint = DerpibooruService.sortbyen.ToList().FindIndex(i => string.Compare(i, temp) == 0);
                viewModel.Sort(tempint);
                SearchAction();
            }
        }

        private void ClearSelect_Clicked(object sender, EventArgs e)
        {
            viewModel.ClearSelect();
            clearSelectToolbarItem.Text = "ClearSelect";
        }

        private void LinkCopy_Clicked(object sender, EventArgs e)
        {
            viewModel.LinkCopy();
        }

        private void HtmlCopy_Clicked(object sender, EventArgs e)
        {
            viewModel.HtmlCopy();
        }

        private void ListView_SizeChanged(object sender, EventArgs e)
        {
            if(viewMode)
                DerpImage.staticWidth = listView2.Width;
            else
                DerpImage.staticWidth = listView.Width;

        }

        private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
        {
            switch (e.Direction)
            {
                case SwipeDirection.Left:
                    break;
                case SwipeDirection.Right:
                    break;
                case SwipeDirection.Up:
                    {
                        viewModel.HasNavigationBar = false;
                        searchBar.IsVisible = false;
                        progressView.IsVisible = false;
                        searchBox.IsVisible = false;
                    }
                    break;
                case SwipeDirection.Down:
                    {
                        viewModel.HasNavigationBar = true;
                        searchBar.IsVisible = true;
                        progressView.IsVisible = true;
                        searchBox.IsVisible = true;
                    }
                    break;
            }
        }

        private void HideTapped(object sender, EventArgs e)
        {
            if (viewModel.HasNavigationBar)
            {
                viewModel.HasNavigationBar = false;
                searchBar.IsVisible = false;
                progressView.IsVisible = false;
                searchBox.IsVisible = false;
            }
            else
            {
                viewModel.HasNavigationBar = true;
                searchBar.IsVisible = true;
                progressView.IsVisible = true;
                searchBox.IsVisible = true;
            }

        }

        private void View_Clicked(object sender, EventArgs e)
        {
            viewMode = !viewMode;
            if (viewMode)
            {
                listView.IsVisible = false;
                listView2.IsVisible = true;
            }
            else
            {
                listView.IsVisible = true;
                listView2.IsVisible = false;
            }
        }
    }

    class SearchBoxLabel : Label
    {
        static Thickness margin = new Thickness(5, 0);
        string key;
        DerpImagesViewModel viewModel;
        public SearchBoxLabel(string model, DerpImagesViewModel viewModel)
        {
            this.Margin = margin;
            this.key = model;
            this.viewModel = viewModel;
            this.Text = model;
            this.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    viewModel.RemoveFilterItem(key);
                    ((FlexLayout)Parent).Children.Remove(this);
                }),
                NumberOfTapsRequired = 1
            });
        }
    }
}