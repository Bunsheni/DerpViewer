using DerpViewer.Services;
using DerpViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DerpViewer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FileBrowserPage : ContentPage
    {
        public FileBrowserViewModel viewModel;

        public FileBrowserPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new FileBrowserViewModel();
        }
        protected async override void OnAppearing()
        {
            await viewModel.MoveRootDirectory();
            base.OnAppearing();
        }

        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            CtFileItem item = (CtFileItem)e.SelectedItem;

            if (item.Name == "/")
            {
                await viewModel.BackDirectory();
            }
            else
            {
                if (item.IsDirectory)
                {
                    await viewModel.MoveDirectory(item.Name);
                }
                else
                {
                    string[] menus = new string[] { "열기", "삭제" };
                    string select1 = await DisplayActionSheet(null, "취소", null, menus);
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

        private async void ClassifyMyImageFiles_Clicked(object sender, EventArgs e)
        {
            await viewModel.ClassifyAll();
        }

        private async void InitializeFileLocation_Clicked(object sender, EventArgs e)
        {
            await viewModel.Population();
        }
    }
}