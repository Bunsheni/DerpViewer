using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using DerpViewer.Models;
using DerpViewer.Services;

namespace DerpViewer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DerpTagDetailPage : ContentPage
    {
        public App RootApp { get => Application.Current as App; }
        public MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        public DerpTagSQLiteDb DerpDb { get => RootPage.GetDerpTagSQLiteDb(); }

        private DerpTag _derpTag, _originTag;

        public string Type { get { return DerpTag.Korean ? DerpTag._catkr[(int)_derpTag.Category] : DerpTag._caten[(int)_derpTag.Category]; }
            set
            {
                if (value != null)
                {
                    int index = Array.FindIndex(DerpTag._catkr, i => i == value);
                    if (index < 0)
                    {
                        index = Array.FindIndex(DerpTag._caten, i => i == value);
                    }

                    if (index > 0)
                    {
                        _derpTag.Category = (DerpTagCategory)index;
                    }
                    else
                    {
                        _derpTag.Category = DerpTagCategory.NONE;
                    }
                }
                else
                {
                    _derpTag.Category = DerpTagCategory.NONE;
                }
                OnPropertyChanged();
            }
        }
        public string NameEn { get { return _derpTag.NameEn; } set { _derpTag.NameEn = value; OnPropertyChanged(); } }
        public string NameKr { get { return _derpTag.NameKr; } set { _derpTag.NameKr = value; OnPropertyChanged(); } }
        public string DescriptionEn { get { return _derpTag.DescriptionEn; } set { _derpTag.DescriptionEn = value; OnPropertyChanged(); } }
        public string DescriptionKr { get { return _derpTag.DescriptionKr; } set { _derpTag.DescriptionKr = value; OnPropertyChanged(); } }
        public string Synonym { get { return _derpTag.Synonym; } set { _derpTag.Synonym = value; OnPropertyChanged(); } }

        public DerpTagDetailPage (DerpTag model)
        {
            InitializeComponent ();
            SetModel(model);
        }

        private void SetModel(DerpTag model)
        {
            _originTag = model;
            _derpTag = model.Clone();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            webView.Source = new UrlWebViewSource() { Url = "https://www.google.co.kr/webhp?sourceid=chrome&ie=UTF-8#q=" + _derpTag.NameEn.Replace(' ', '+') };
        }

        public async Task<string> TransWebBrowserInitAsync(string text, string from, string to)
        {
            return await webView.TransWebBrowserInitAsync(text, from, to);
        }

        public async Task<string> GetWebClintContentsAsync(string url)
        {
            return await webView.GetWebClintContentsAsync(url);
        }

        private async void TranslateItem_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Notice", "Only for developer.", "OK");
            return;
            FieldBox.IsVisible = false;
            try
            {
                string temp = NameEn + "///" + DescriptionEn;
                string res = await TransWebBrowserInitAsync(NameEn + "///" + DescriptionEn, "en", "ko");
                if (string.Compare(temp, res) == 0)
                {
                    res = await TransWebBrowserInitAsync(NameEn + "///" + DescriptionEn, "en", "ko");
                }
                List<string> tempList = Library.StringDivider(res, "///");
                if (tempList.Count == 2)
                {
                    NameKr = tempList[0].Trim();
                    DescriptionKr = tempList[1].Trim();
                }
            }
            catch
            {
                await DisplayAlert("알림", "문제가 발생하였습니다. 잠시 후 시도하십시오.", "확인");
            }
            webView.Source = new UrlWebViewSource() { Url = "https://www.google.co.kr/webhp?sourceid=chrome&ie=UTF-8#q=" + _derpTag.NameEn.Replace(' ', '+') };
            await webView.WaitAsync();
            FieldBox.IsVisible = true;
        }

        private void ClearItem_Clicked(object sender, EventArgs e)
        {
            SetModel(_originTag);
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            DerpTagCategory[] cats = new DerpTagCategory[] {
                DerpTagCategory.NONE,
                DerpTagCategory.RATING,
                DerpTagCategory.SPOILER,
                DerpTagCategory.ARTIST,
                DerpTagCategory.CHARACTER,
                DerpTagCategory.OC,
                DerpTagCategory.CONTENTOFFICIAL,
                DerpTagCategory.CONTENTFANMADE,
                DerpTagCategory.ORIGIN,
                DerpTagCategory.ARTCATEGORY, DerpTagCategory.CHARACTERFEATURE, DerpTagCategory.RACE, DerpTagCategory.POSE, DerpTagCategory.BODYPART, DerpTagCategory.BEHAVIOR };

            string[] strs = Array.ConvertAll(cats, i => DerpTag.Korean ? DerpTag._catkr[(int)i] : DerpTag._caten[(int)i]);

            string temp = await DisplayActionSheet(null, null, null, strs);
            if (temp != null)
            {
                Type = temp;
            }
        }

        private async void SaveItem_Clicked(object sender, EventArgs e)
        {
            await DerpDb.UpdateTagAsync(_derpTag);
            await DisplayAlert("알림", "저장되었습니다.", "확인");
        }
    }
}