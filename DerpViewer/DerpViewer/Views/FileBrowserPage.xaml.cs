using DerpViewer.Models;
using DerpViewer.Services;
using DerpViewer.ViewModels;
using FFImageLoading.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DerpViewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FileBrowserPage : ContentPage
    {
        private bool renameMode = false;
        private CtFileItem renameFolder;
        private Thread SuggestionThread;
        public FileBrowserViewModel viewModel;

        public FileBrowserPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new FileBrowserViewModel();
            listView.RefreshCommand = new Command(async () =>
            {
                await viewModel.UpdateDirectory();
                listView.IsRefreshing = false;
            });
        }
        protected async override void OnAppearing()
        {
            await viewModel.MoveRootDirectory();
            base.OnAppearing();
        }

        private async void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            CtFileItem item = (CtFileItem)e.Item;

            if (item.Name == "/")
            {
                await viewModel.BackDirectory();
            }
            else
            {
                if (item.IsDirectory)
                {
                    string[] menus = new string[] { "열기", "이름 바꾸기", "삭제" };
                    string select1 = await DisplayActionSheet(item.Name, "취소", null, menus);
                    switch (select1)
                    {
                        case "열기":
                            await viewModel.MoveDirectory(item.Name);
                            break;
                        case "이름 바꾸기":
                            renameMode = true;
                            renameFolder = item;
                            foldernameentry.Text = item.Name;
                            overlay.IsVisible = true;
                            break;
                        case "삭제":
                            if (await DisplayAlert("알림", "폴더를 지우시겠습니까?", "확인", "취소"))
                            {
                                var number = await viewModel.GetSubItemNumber(item);
                                if (0 < number)
                                {
                                    if (await DisplayAlert("알림", $"{number}개의 하위항목이 존재합니다. 정말로 폴더를 지우시겠습니까?", "확인", "취소"))
                                    {
                                        viewModel.DeleteFolder(item);
                                        await viewModel.UpdateDirectory();
                                    }
                                }
                                else if (number < 0)
                                {
                                    await DisplayAlert("알림", "오류 발생", "확인");
                                }
                                else
                                {
                                    viewModel.DeleteFolder(item);
                                    await viewModel.UpdateDirectory();
                                }
                            }
                            else
                            {
                                return;
                            }
                            break;
                    }
                }
                else
                {
                    string[] menus = new string[] { "열기", "삭제" };
                    string select1 = await DisplayActionSheet(item.Name, "취소", null, menus);
                    switch (select1)
                    {
                        case "열기":
                            viewModel.OpenFile(item);
                            break;
                        case "삭제":
                            if (await DisplayAlert("알림", "파일을 지우시겠습니까?", "확인", "취소"))
                            {
                                viewModel.DeleteFile(item);
                                await viewModel.UpdateDirectory();
                            }
                            else
                            {
                                return;
                            }
                            break;
                    }
                }
            }
        }

        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
        }

        private async void ClassifyMyImageFiles_Clicked(object sender, EventArgs e)
        {
            var res = await DisplayAlert("알림", "하위 모든 이미지를 태그에 따라 알맞은 폴더로 이동시킵니다.", "확인", "취소");
            if (res)
            {
                await Task.Run(async () =>
                {
                    await viewModel.ClassifyAll();
                });
            }
        }

        private async void InitializeFileLocation_Clicked(object sender, EventArgs e)
        {
            var res = await DisplayAlert("알림", "하위 폴더의 모든 이미지를 현재 폴더로 이동시킵니다.", "확인", "취소");
            if (res)
            {
                await Task.Run(async () =>
                {
                    await viewModel.PopulationAll();
                });
            }
        }

        private void NewFolder_Clicked(object sender, EventArgs e)
        {
            renameMode = false;
            renameFolder = null;
            foldernameentry.Text = string.Empty;
            overlay.IsVisible = true;
        }

        private void NewFolderCancel_Clicked(object sender, EventArgs e)
        {
            overlay.IsVisible = false;
        }

        private async void NewFolderOk_Clicked(object sender, EventArgs e)
        {
            try
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
                    if (renameMode)
                        await viewModel.RenameFolder(renameFolder, foldernameentry.Text);
                    else
                        await viewModel.CreateFolder(foldernameentry.Text);

                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("알림", ex.Message, "확인");
            }
        }

        private void Foldernameentry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (foldernameentry.Text.Trim().Length != 0 && foldernameentry.Text.Trim().Last() != ',')
            {
                if (SuggestionThread != null && SuggestionThread.ThreadState == ThreadState.Running)
                {
                    viewModel.SuggestionLock = true;
                    SuggestionThread.Join();
                }
                SuggestionThread = new Thread(new ThreadStart(viewModel.GetSuggestionItem));
                SuggestionThread.Start();
                searchView.IsVisible = true;
            }
            else
            {
                searchView.IsVisible = false;
            }
        }

        private void suggestionListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var tabbedSuggestionItem = ((DerpSuggestionItem)e.Item);
            if (tabbedSuggestionItem != null)
            {
                var key = foldernameentry.Text;
                foldernameentry.Text = key.Substring(0, key.LastIndexOf(',') + 1) + tabbedSuggestionItem.Tag.NameEn;
            }
            searchView.IsVisible = false;
        }

        private void ImageTapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            var file = viewModel.FileList.Single(i => i.Name == ((CachedImage)sender).ClassId);
            viewModel.OpenFile(file);
        }
    }
}