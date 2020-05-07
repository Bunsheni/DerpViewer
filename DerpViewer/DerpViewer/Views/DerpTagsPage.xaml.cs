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
using Xamarin.Essentials;

namespace DerpViewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DerpTagsPage : ContentPage
    {
        protected App RootApp { get => Application.Current as App; }
        protected MainPage RootPage { get => Application.Current.MainPage as MainPage; }

        private DerpibooruService derpibooru;
        private DerpTagsViewModel viewModel;
        private DerpTag selectedTag;
        private bool _selectMode = false;

        private List<DerpTag> modifiedTags = new List<DerpTag>();

        public List<DerpTag> DerpTags
        {
            get
            {
                return listView.ItemsSource as List<DerpTag>;
            }
        }

        public DerpTagsPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new DerpTagsViewModel();
            derpibooru = new DerpibooruService(webView);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (selectedTag != null)
            {
                selectedTag = DerpTags.Single(i => i.NameEn == selectedTag.NameEn);
                if (selectedTag != null)
                    listView.ScrollTo(selectedTag, ScrollToPosition.Start, false);
            }
            await Load();
        }


        public async Task Load()
        {
            string key = searchBar.Text;
            List<DerpTag> models = await RootPage.GetDerpTagSQLiteDb().GetTagsAsync();
            if (_selectMode)
            {
                if (key.Length != 0)
                {
                    if (key.Contains("{character}"))
                    {
                        List<string> keycharacters = new List<string>();
                        var characters = models.FindAll(i => i.Category == DerpTagCategory.CHARACTER);
                        foreach(var character in characters)
                        {
                            var tempe = key.Replace("{character}", character.NameEn);
                            var tempk = key.Replace("{character}", character.NameKr);
                            keycharacters.Add(tempe);
                            keycharacters.Add(tempk);
                        }
                        listView.ItemsSource = models.FindAll(i => keycharacters.Contains(i.NameEn) || keycharacters.Contains(i.NameKr));
                    }
                    else
                    {
                        listView.ItemsSource = models.FindAll(i => i.NameKr.Contains(key) && (i.Category == DerpTagCategory.CHARACTER || i.Category == DerpTagCategory.NONE));
                    }
                    foreach (DerpTag tag in DerpTags)
                    {
                        tag.IsSelected = true;
                    }

                }
                else
                {
                    listView.ItemsSource = models.FindAll(i => i.Category == DerpTagCategory.CHARACTER || i.Category == DerpTagCategory.NONE);
                    foreach (DerpTag tag in DerpTags)
                    {
                        tag.IsSelected = false;
                    }
                }
            }
            else
            {
                if (key.Length != 0)
                {
                    listView.ItemsSource = models.FindAll(i => i.NameEn.Contains(key) || i.NameKr.Contains(key) || i.CategoryStrKr == key || i.CategoryStrEn == key);
                }
                else
                {
                    listView.ItemsSource = models;
                }
                foreach (DerpTag tag in DerpTags)
                {
                    tag.IsSelected = false;
                }
            }
        }


        private async void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsBusy)
            {
                IsBusy = true;
                try
                {
                    await Load();
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
            if (await DisplayAlert("알림", $"Derpibooru에서 태그를 검색합니다.", "확인", "취소"))
            {
                progressBar.BindingContext = derpibooru;
                progressView.IsVisible = true;
                progressView.HeightRequest = 4;
                int newcount = 0;

                await Task.Run(async () =>
                {
                    List<DerpTag> oldTags = await RootPage.GetDerpTagSQLiteDb().GetTagsAsync();
                    List<DerpTag> newTags = derpibooru.GetTagInfoFromDerpibooru(oldTags, RootApp.TagCount, this as IWebConnection, out newcount);
                    if (newTags != null && newTags.Count > 0)
                    {
                        await RootPage.GetDerpTagSQLiteDb().DropDerpTagTable();
                        await RootPage.GetDerpTagSQLiteDb().CreatDerpTagTable();
                        (await RootPage.GetDerpTagSQLiteDb().GetTagsAsync()).Clear();
                        await RootPage.GetDerpTagSQLiteDb().AddTagsAsync(newTags);
                    }
                });

                await DisplayAlert("알림", $"{newcount}개의 태그가 새롭게 추가되었습니다.", "확인");
                progressView.HeightRequest = 0;
                progressView.IsVisible = false;
                await Load();
            }
        }

        bool lockTap;
        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var tag = e.Item as DerpTag;
            if(_selectMode)
            {
                tag.IsSelected ^= true;
                listView.SelectedItem = null;
            }
            else if(!lockTap)
            {
                lockTap = true;
                string[] SortsEn = { "검색", "편집", "복사", "삭제" };
                string select = await DisplayActionSheet(null, null, null, SortsEn);
                if (select == "검색")
                {
                    await viewModel.RootPage.MoveMenu((int)MenuItemType.ImageBrowser);
                    viewModel.RootPage.MainImageView.AddToSearchBox(tag, true);
                }
                else if (select == "편집")
                {
                    await Navigation.PushAsync(new DerpTagDetailPage(tag));

                }
                else if (select == "복사")
                {
                    await Clipboard.SetTextAsync(tag.Id);
                }
                else if (select == "삭제")
                {
                    await viewModel.DerpTagDb.DeleteTagAsync(tag.Id);
                }
                lockTap = false;
            }
        }

        float _progress;
        public float Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }

        private async void TagTranslate_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Notice", "Only for developer.", "OK");
            return;
            if (await DisplayAlert("경고", "이 작업은 태그의 Discription 을 번역합니다. 상당히 긴 시간이 소요됩니다.", "확인", "취소"))
            {
                progressBar.BindingContext = this;
                progressView.IsVisible = true;
                progressView.HeightRequest = 4;

                List<DerpTag> tags = await RootPage.GetDerpTagSQLiteDb().GetTagsAsync();
                int i = 0;
                foreach (DerpTag tag in tags)
                {
                    if (tag.DescriptionEn.Length > 0 && tag.DescriptionEn == tag.DescriptionKr && tag.DescriptionEn.Length < 100)
                    {
                        tag.DescriptionKr = await webView.TransWebBrowserInitAsync(tag.DescriptionEn.Replace("\"", ""), "en", "ko");
                        await viewModel.DerpTagDb.UpdateTagAsync(tag);
                    }
                    i++;
                    Progress = (float)i / tags.Count;
                }

                progressView.HeightRequest = 0;
                progressView.IsVisible = false;
                await Load();
            }
        }

        private async void ReplaceItem_Clicked(object sender, EventArgs e)
        {
            replaceView.IsVisible = !replaceView.IsVisible;
            searchBar.IsVisible = !searchBar.IsVisible;
            _selectMode = !_selectMode;
            listView.SelectedItem = null;
            await Load();
        }

        private void OldText_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchBar.Text = oldText.Text;
        }

        private async void Apply_Clicked(object sender, EventArgs e)
        {
            int count = 0;
            string key = oldText.Text;
            string key2 = newText.Text;
            List<DerpTag> models = await RootPage.GetDerpTagSQLiteDb().GetTagsAsync();
            var characters = models.FindAll(i => i.Category == DerpTagCategory.CHARACTER);

            foreach (DerpTag tag in DerpTags)
            {
                if(tag.IsSelected)
                {
                    if (key.Contains("{character}"))
                    {
                        var character = characters.Find(i => key.Replace("{character}", i.NameEn) == tag.NameEn);
                        if (character != null)
                        {
                            tag.NameKr = key2.Replace("{character}", character.NameKr);
                            modifiedTags.Add(tag);
                            count++;
                        }
                    }
                    else
                    {
                        tag.NameKr = tag.NameKr.Replace(oldText.Text, newText.Text);
                        modifiedTags.Add(tag);
                        count++;
                    }
                }
            }
            await DisplayAlert("Notify", $"{count}개의 태그가 변경되었습니다.", "확인");
        }

        

        private async void Save_Clicked(object sender, EventArgs e)
        {
            if(modifiedTags.Count > 0)
            {
                bool res = await DisplayAlert(null, $"{modifiedTags.Count}개의 태그가 저장됩니다.", "확인", "취소");
                if (res)
                {
                    await viewModel.DerpTagDb.InsertTagsAsync(modifiedTags);
                    modifiedTags.Clear();
                }
            }
        }
    }
}
