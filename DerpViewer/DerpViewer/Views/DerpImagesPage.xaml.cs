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
using System.Threading;

namespace DerpViewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DerpImagesPage : ContentPage
    {
        //뷰 모델
        private DerpImagesViewModel viewModel;
        // 더블 탭을 위한 탭 액션 지연시간
        private TimeSpan _tt = new TimeSpan(5000000);
        // 더블 탭을 위한 탭 카운트
        private int _tabCount = 0;
        //Sort 메뉴 스트립 문자열
        private static readonly string[] sortbyen = new string[] { "Latest", "Oldest", "High Score", "Low Score" };
        private static readonly string[] sortbykr = new string[] { "최신순", "오래된순", "높은 점수순", "낮은 점수순" };

        private bool pageLock, viewMode;
        private DerpSuggestionItem tabbedSuggestionItem;
        private DerpImage tabbedImage;
        private ImageSource tabbedImageSource;
        private int _lastItemAppearedIdx;
        private bool _lastItemLock;
        private DerpImage selectedItem;
        private Thread SuggestionThread, SearchThread;

        protected App RootApp { get => Application.Current as App; }
        protected MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        public string[] SortByStrArray => RootApp.Korean ? sortbykr : sortbyen;

        public DerpImagesPage() : this(false)
        {
        }

        public DerpImagesPage(bool favorite)
        {
            InitializeComponent();
            if (!favorite)
            {
                this.ToolbarItems.Remove(toolGetMyFavorite);
                this.ToolbarItems.Remove(toolGetMyFiles);
                //this.ToolbarItems.Remove(toolSearchMyFiles);
            }

            BindingContext = viewModel = new DerpImagesViewModel(favorite, webView);
            listView3.RefreshCommand = listView2.RefreshCommand = listView.RefreshCommand = new Command(async () =>
            {
                await viewModel.ExecuteLoadItemsCommand();
                listView.IsRefreshing = false;
            });
            LockHideNavigationBar(5000000);
        }

        protected override async void OnAppearing()
        {
            if (listView.ItemsSource == null)
            {
                await RootPage.GetDerpTagSQLiteDb().GetTagsAsync();
                await viewModel.ExecuteLoadItemsCommand();
            }
            base.OnAppearing();
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
                Device.StartTimer(_tt, TestHandleFuncAsync);
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
                        Application.Current.MainPage.Navigation.PushModalAsync(new ImagePage(tabbedImage, tabbedImageSource));
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
                if(SuggestionThread != null && SuggestionThread.ThreadState == ThreadState.Running)
                {
                    viewModel.SuggestionLock = true;
                    SuggestionThread.Join();
                }
                SuggestionThread = new Thread(new ThreadStart(viewModel.GetSuggestionItem));
                SuggestionThread.Start();
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
                AddToSearchBox(new DerpTag(temp), false);
            }
            contentView.IsVisible = true;
        }

        private void SelectionChanged()
        {
            int selected = viewModel.GetSelectedImages().Count;
            if (selected > 0)
            {
                clearSelectToolbarItem.Text = selected + " Selected";
                clearSelectToolbarItem.Icon = "Icon\\selected.png";
            }
            else
            {
                clearSelectToolbarItem.Text = "ClearSelect";
                clearSelectToolbarItem.Icon = "Icon\\nonselected.png";
            }
        }

        private void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ((DerpImage)e.Item).IsSelected ^= true;
            listView.SelectedItem = null;
            SelectionChanged();
        }


        private void listView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            selectedItem = e.Item as DerpImage;
            viewModel.listViewItemAppearing(e.Item);

            if (viewModel.RootApp.HideTopbar)
            {
                var currentIdx = viewModel.Images.IndexOf((DerpImage)e.Item);

                if (!_lastItemLock || _lastItemAppearedIdx == 0)
                {
                    if (currentIdx > _lastItemAppearedIdx && _lastItemAppearedIdx != 0)
                    {
                        HideNavigationBar();
                    }
                    else
                    {
                        DisplayNavigationBar();
                    }
                    LockHideNavigationBar(5000000);
                }
                _lastItemAppearedIdx = viewModel.Images.IndexOf((DerpImage)e.Item);
            }
            else if(!viewModel.HasNavigationBar)
            {
                viewModel.HasNavigationBar = true;
                searchBar.IsVisible = true;
                if (viewModel.Downloading)
                    progressView.IsVisible = true;
                searchBox.IsVisible = true;
                _lastItemLock = false;
            }
        }

        private void DisplayNavigationBar()
        {
            viewModel.HasNavigationBar = true;
            searchBar.IsVisible = true;
            if (viewModel.Downloading)
                progressView.IsVisible = true;
            searchBox.IsVisible = true;
        }

        private void HideNavigationBar()
        {
            viewModel.HasNavigationBar = false;
            searchBar.IsVisible = false;
            progressView.IsVisible = false;
            searchBox.IsVisible = false;
        }

        private void LockHideNavigationBar(long tick)
        {
            _lastItemLock = true;
            TimeSpan tt = new TimeSpan(tick);
            Device.StartTimer(tt, TimeHandleFuncAsync);
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
                    tabbedSuggestionItem = ((DerpSuggestionItem)e.Item);
                }
                catch
                {
                    return;
                }
                Device.StartTimer(_tt, SuggestionHandleFuncAsync);
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
                    temp = "-" + tabbedSuggestionItem.Tag.Name;
                    tabbedSuggestionItem.Tag.Sub = true;
                }
                else
                {
                    temp = tabbedSuggestionItem.Tag.Name;
                    tabbedSuggestionItem.Tag.Sub = false;
                }
                if (!viewModel.ExistItem(temp))
                {
                    AddToSearchBox(tabbedSuggestionItem.Tag, false);
                }
            }
            searchView.IsVisible = false;
            contentView.IsVisible = true;
            _tabCount = 0;
            return false;
        }

        public void AddToSearchBox(DerpTag item, bool clear)
        {
            DisplayNavigationBar();
            if (clear)
            {
                viewModel.ClearFilterItem();
                searchBox.Children.Clear();
            }
            viewModel.AddFilterItem(item);
            searchBox.Children.Add(new SearchBoxLabel(item, viewModel));
            searchBox.ForceLayout();
        }

        private async void LabelTapped(object sender, EventArgs e)
        {
            if (pageLock) return;
            pageLock = true;
            List<DerpTag> models = new List<DerpTag>();
            if (((Label)sender).Text != "unknown artist" && ((Label)sender).Text != "no content" && ((Label)sender).Text != "no character" && ((Label)sender).Text != "no tag")
            {
                foreach (string str in Library.StringDivider(((Label)sender).Text, ", "))
                {
                    DerpTag tag = await viewModel.DerpTagDb.GetTagFromNameAsync(str);
                    if (tag != null)
                        models.Add(tag);
                    else
                        models.Add(new DerpTag(str));
                }
                if (models.Count > 0)
                {
                    TagListDisplayAndSearch(models);
                }
            }
            pageLock = false;
        }

        public async void UpdateListView()
        {
            await viewModel.ExecuteLoadItemsCommand();
        }

        public async Task Download(string folderName)
        {
            int count = 0;
            await Task.Run(async () => {
                count = await viewModel.Download(folderName);
            });
            if (0 <= count)
                await DisplayAlert("알림", $"{count}개 이미지가 다운로드 완료되었습니다.", "확인");
        }

        private async void FavoriteTappedAsync(object sender, EventArgs e)
        {
            DerpImage img = ((Label)sender).BindingContext as DerpImage;
            if(img.IsFavorite)
            {
                if (await DisplayAlert("알림", "내 목록에서 제거합니까?", "확인", "취소"))
                {
                    img.IsFavorite = !img.IsFavorite;
                    await viewModel.DeleteFromMyImageListAsync(img);
                }
            }
            else
            {
                img.IsFavorite = !img.IsFavorite;
                await viewModel.AddToMyImageListAsync(img);
            }
            RootPage.FavoriteImageView.UpdateListView();
        }

        public async void TagListDisplayAndSearch(List<DerpTag> models)
        {
            List<string> dis = new List<string>();
            foreach (DerpTag model in models)
            {
                dis.Add(model.Name);
            }
            string select = await DisplayActionSheet(null, null, null, dis.ToArray());
            int selectedindex = dis.FindIndex(i => i == select);
            if (selectedindex < 0) return;
            DerpTag selectedModel = models[selectedindex];
            if (!viewModel.ExistItem(selectedModel.NameEn))
            {
                AddToSearchBox(selectedModel, false);
            }
        }


        private void SearchBox_ChildAdded(object sender, ElementEventArgs e)
        {
            SearchAction();
        }

        //viewModel이 검색을 수행하고
        private async void SearchAction()
        {
            await viewModel.Search();
            SelectionChanged();
        }

        private async void Download_Clicked(object sender, EventArgs e)
        {
            await Download(string.Empty);
        }

        private async void FolderDownload_Clicked(object sender, EventArgs e)
        {
            if (viewModel.Downloading)
            {
                await DisplayAlert("알림", "진행중인 다운로드가 완료된 후 시도하십시오.", "확인");
            }
            else
            {
                var imgs = viewModel.GetSelectedImages();
                if (imgs.Count > 0)
                {
                    foldernameentry.Text = viewModel.CurrentKey;
                    overlay.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("알림", "다운로드할 이미지를 선택하십시오.", "확인");
                }
            }
        }

        private void ClearSelect_Clicked(object sender, EventArgs e)
        {
            viewModel.ClearSelect();
            SelectionChanged();
        }
        private void ClearFilter_Clicked(object sender, EventArgs e)
        {
            if (searchBox.Children.Count > 0 || viewModel.Key.Length > 0)
            {
                viewModel.ClearFilterItem();
                searchBox.Children.Clear();
                //SearchAction();
            }
        }

        private async void Sort_Clicked(object sender, EventArgs e)
        {
            string temp = await DisplayActionSheet(viewModel.RootApp.Korean ? "정렬":"SortBy", viewModel.RootApp.Korean ? "취소" : "Cancle", null, SortByStrArray);
            if (temp != null)
            {
                int tempint = SortByStrArray.ToList().FindIndex(i => string.Compare(i, temp) == 0);
                viewModel.Sort(tempint);
                SearchAction();
            }
        }

        private void LinkCopy_Clicked(object sender, EventArgs e)
        {
            viewModel.LinkCopy();
        }

        private void HtmlCopy_Clicked(object sender, EventArgs e)
        {
            viewModel.HtmlCopy();
        }
                
        private async void View_Clicked(object sender, EventArgs e)
        {
            string temp = await DisplayActionSheet(viewModel.RootApp.Korean ? "보기" : "View", viewModel.RootApp.Korean ? "취소" : "Cancle", null, new string[] { "Small", "Large", "List"});
            if (temp != null)
            {
                if (temp == "Small")
                {
                    listView.IsVisible = true;
                    listView2.IsVisible = false;
                    listView3.IsVisible = false;
                    listView.ScrollTo(selectedItem, ScrollToPosition.Center, false);
                }
                else if (temp == "Large")
                {
                    listView.IsVisible = false;
                    listView2.IsVisible = true;
                    listView3.IsVisible = false;
                    listView2.ScrollTo(selectedItem, ScrollToPosition.Center, false);
                }
                else if (temp == "List")
                {
                    listView.IsVisible = false;
                    listView2.IsVisible = false;
                    listView3.IsVisible = true;
                    listView3.ScrollTo(selectedItem, ScrollToPosition.Center, false);
                }
            }
        }

        private async void GetMyFavoriteItem_Clicked(object sender, EventArgs e)
        {
            await viewModel.GetMyFavorite();
            await DisplayAlert("알림", "완료되었습니다.", "확인");
        }

        private async void GetMyFilesItem_Clicked(object sender, EventArgs e)
        {
            await viewModel.GetMyFiles();
            await DisplayAlert("알림", "완료되었습니다.", "확인");
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            overlay.IsVisible = false;
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            if (foldernameentry.Text.Trim().Length == 0)
            {
                await DisplayAlert("알림", "폴더 이름은 공백일 수 없습니다.", "확인");
            }
            else if (foldernameentry.Text.Contains('\\')
                || foldernameentry.Text.Contains('/') 
                || foldernameentry.Text.Contains(':') 
                || foldernameentry.Text.Contains('*')
                || foldernameentry.Text.Contains('?')
                || foldernameentry.Text.Contains('"')
                || foldernameentry.Text.Contains('>')
                || foldernameentry.Text.Contains('<')
                || foldernameentry.Text.Contains('|'))
            {
                await DisplayAlert("알림", "폴더 이름에는 다음 문자를 사용할 수 없습니다. \\ / : * ? \" > < |", "확인");
            }
            else
            {
                overlay.IsVisible = false;
                string name = foldernameentry.Text;
                await Download(name);
            }
        }

        private async void ToolSearchMyFiles_Clicked(object sender, EventArgs e)
        {
            webView.IsVisible = true;
            contentView.IsVisible = false;
            await viewModel.SearchMyFiles();
            await DisplayAlert("알림", "완료되었습니다.", "확인");
            //contentView.IsVisible = true;
            //webView.IsVisible = false;
        }

        private void ListView_SizeChanged(object sender, EventArgs e)
        {
            if (viewMode)
                DerpImage.staticWidth = listView2.Width;
            else
                DerpImage.staticWidth = listView.Width;
        }
    }

    class SearchBoxLabel : Label
    {
        static Thickness margin = new Thickness(5, 0);
        DerpTag model;
        DerpImagesViewModel viewModel;
        public SearchBoxLabel(DerpTag model, DerpImagesViewModel viewModel)
        {
            this.Margin = margin;
            this.model = model;
            this.viewModel = viewModel;
            this.Text = (model.Sub ? "-":string.Empty) + model.Name;
            this.GestureRecognizers.Add(new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    viewModel.RemoveFilterItem(model);
                    ((FlexLayout)Parent).Children.Remove(this);
                }),
                NumberOfTapsRequired = 1
            });
        }
    }
}