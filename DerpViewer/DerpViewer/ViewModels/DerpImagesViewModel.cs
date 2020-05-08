
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Essentials;

using FFImageLoading;
using FFImageLoading.Cache;
using DerpViewer.Models;
using DerpViewer.Services;
using FFImageLoading.Forms;
using DerpViewer.Views;
using System.Threading;

namespace DerpViewer.ViewModels
{
    class DerpImagesViewModel : BaseViewModel
    {
        private DerpibooruService derpibooru;
        private DerpFileService fileService = new DerpFileService();
        private object lockobject = new object();
        private object downlaodLockObject = new object();
        private List<DerpImage> downloadList = new List<DerpImage>();
        private int page = 0;
        private DerpSortBy sortBy = DerpSortBy.CREATE;
        private DerpSortOrder sortOrder = DerpSortOrder.DESC;

        private bool searchmode;
        private float progress1, progress2;
        private bool progressBarIsVisible, hasNavigationBar, listViewIsRefreshing;
        private int progressBarHeight;
        private ObservableCollection<DerpImage> _images;
        private ObservableCollection<DerpImage> _myimages;
        private List<DerpSuggestionItem> suggestionItem;
        private List<string> _tempKeys = new List<string>();
        private string _key = string.Empty;
        private List<CtFileItem> fileList = new List<CtFileItem>();
        private bool endPage;

        public int ThreadCount = 0;
        public string SortText => RootApp.Korean ? "정렬" : "Sort";
        public string FolderDownloadText => RootApp.Korean ? "폴더에 다운로드" : "Download at folder";
        public string LinkCopyText => RootApp.Korean ? "링크 복사" : "Link Copy";
        public string HtmlCopyText => RootApp.Korean ? "HTML 복사" : "HTML Copy";
        public string ViewText => RootApp.Korean ? "보기 변경" : "View";
        public string GetMyFavoriteText => RootApp.Korean ? "즐겨찾기 가져오기" : "Get My Favorite";
        public string GetMyFilesText => RootApp.Korean ? "내 파일 가져오기" : "Get My Files";
        public string SearchMyFilesText => RootApp.Korean ? "내 파일 검색하기" : "Search My Files";

        public bool IsFavoriteView { get; }

        public bool Downloading { get; private set; }

        public string CurrentKey { get; private set; }

        public bool ListViewIsRefreshing
        {
            get
            {
                return listViewIsRefreshing;
            }
            set
            {
                listViewIsRefreshing = value;
                OnPropertyChanged();
            }
        }


        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        public bool HasNavigationBar
        {
            get
            {
                return hasNavigationBar;
            }
            set
            {
                hasNavigationBar = value;
                OnPropertyChanged();
            }
        }

        public bool ProgressBarIsVisible
        {
            get
            {
                return progressBarIsVisible;
            }
            set
            {
                progressBarIsVisible = value;
                OnPropertyChanged();
            }
        }
        public int ProgressBarHeight
        {
            get
            {
                return progressBarHeight;
            }
            set
            {
                progressBarHeight = value;
                OnPropertyChanged();
            }
        }
        public float Progress1
        {
            get
            {
                return progress1;
            }
            set
            {
                progress1 = value;
                OnPropertyChanged();
            }
        }
        public float Progress2
        {
            get
            {
                return progress2;
            }
            set
            {
                progress2 = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<DerpImage> Images
        {
            get
            {
                return _images;
            }
            set
            {
                _images = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<DerpImage> MyImages
        {
            get
            {
                return _myimages;
            }
            set
            {
                _myimages = value;
                OnPropertyChanged();
            }
        }

        public List<DerpSuggestionItem> SuggestionItems
        {
            get { return suggestionItem; }

            set
            {
                suggestionItem = value;
                OnPropertyChanged();
            }
        }

        public DerpImagesViewModel(bool favorite, IWebConnection web)
        {
            IsFavoriteView = favorite;
            CurrentKey = string.Empty;
            HasNavigationBar = true;
            derpibooru = new DerpibooruService(web);
        }

        public ObservableCollection<DerpImage> SearchDerpImage(ObservableCollection<DerpImage> images, string key)
        {
            ObservableCollection<DerpImage> searchedimages = new ObservableCollection<DerpImage>();
            string[] keys = key.Split(',');
            foreach (DerpImage image in images)
            {
                var imagetags = image.Tags;
                bool flag = true;
                foreach (string str in keys)
                {
                    if (!imagetags.Contains(str))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    searchedimages.Add(image);
                }
            }
            return searchedimages;
        }

        public async Task<bool> ExecuteLoadItemsCommand()
        {
            try
            {
                int thread;
                lock (lockobject)
                {
                    thread = ++ThreadCount;
                }
                page = 1;
                endPage = false;
                searchmode = true;

                if (!DerpImageDb.IsLoaded)
                    await DerpImageDb.Load();

                var mylist = await DerpImageDb.GetDerpImagesAsync();
                var files = await fileService.GetSubList("");
                List<CtFileItem> filelist = null;
                if (files != null)
                {
                    filelist = files.FindAll(i => i.Name.Contains("__"));
                    if (mylist != null)
                    {
                        mylist.Reverse();
                        MyImages = new ObservableCollection<DerpImage>(mylist);
                    }
                    fileList.Clear();
                    if (filelist != null && filelist.Count > 0)
                    {
                        fileList.AddRange(filelist);
                    }
                }

                var imgs = new ObservableCollection<DerpImage>();

                if (IsFavoriteView)
                {
                    if (CurrentKey.Length == 0)
                    {
                        imgs = MyImages;
                    }
                    else
                    {
                        imgs = SearchDerpImage(MyImages, CurrentKey);
                    }
                }
                else
                {
                    if (CurrentKey.Length == 0)
                        CurrentKey = "*";
                    var temp = await derpibooru.GetSearchImage(mylist, filelist, UserAPIKey, CurrentKey, page, sortBy, sortOrder);
                    imgs = new ObservableCollection<DerpImage>(temp);
                }

                var flag = false;
                lock (lockobject)
                {
                    if (ThreadCount == thread)
                    {
                        Images = imgs;
                        ThreadCount = 0;
                        flag = true;
                    }
                }
                if (flag)
                {
                    await ImageService.Instance.InvalidateCacheAsync(CacheType.All);
                    GC.Collect();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// 마지막 아이템이 나타나면 다음 페이지를 검색한다.
        /// 검색모드가 아니면 
        /// </summary>
        /// <param name="item"></param>
        public async void listViewItemAppearing(object item)
        {
            try
            {
                if (ThreadCount == 0 && Images.Count > 0 && !IsFavoriteView)
                {
                    if ((Images.Count >= 49 && Images[Images.Count - 49] == item) || Images.Last() == item)
                    {
                        if (!endPage)
                        {
                            page++;
                            List<DerpImage> tempimages = await derpibooru.GetSearchImage(MyImages.ToList(), fileList, UserAPIKey, searchmode ? CurrentKey : "*", page, sortBy, sortOrder);
                            if (tempimages.Count == 0)
                            {
                                endPage = true;
                                page--;
                            }
                            else
                            {
                                foreach (DerpImage image in tempimages)
                                {
                                    lock (lockobject)
                                    {
                                        if (!Images.Any(i => i.Id == image.Id))
                                            Images.Add(image);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task Search()
        {
            ListViewIsRefreshing = true;
            CurrentKey = string.Empty;
            foreach (string str in _tempKeys)
            {
                if (CurrentKey.Length == 0)
                    CurrentKey = str;
                else
                    CurrentKey += ',' + str;
            }

            bool res = await ExecuteLoadItemsCommand();


            if (res)
            {
                ListViewIsRefreshing = false;
                if (Images.Count > 0)
                {
                    foreach (string str in _tempKeys)
                    {
                        if (!str.StartsWith("-"))
                        {
                            var temp = await DerpTagDb.GetTagFromNameAsync(str);
                            if (temp == null)
                            {
                                await DerpTagDb.InsertTagAsync(new DerpTag(str));
                            }
                        }
                    }
                }
            }
        }

        public List<DerpImage> GetSelectedImages()
        {
            List<DerpImage> res = new List<DerpImage>();
            lock (lockobject)
            {
                foreach (DerpImage image in Images)
                {
                    if (image.IsSelected)
                    {
                        res.Add(image);
                    }
                }
            }
            return res;
        }

        public async Task AddToMyImageListAsync(DerpImage img)
        {
            MyImages.Insert(0, img);
            await DerpImageDb.InsertDerpImageAsync(img);
        }

        public async Task DeleteFromMyImageListAsync(DerpImage img)
        {
            MyImages.Remove(img);
            await DerpImageDb.DeleteDerpImageAsync(img);
        }

        public async Task<int> Download(string foldername)
        {
            int res = -1;
            foreach (DerpImage image in GetSelectedImages())
            {
                lock (downlaodLockObject)
                {
                    if (!downloadList.Exists(i => i == image))
                        downloadList.Add(image);
                }
            }
            if (downloadList.Count > 0 && !Downloading)
            {
                res = await DownloadRun(foldername);
            }
            return res;
        }

        public async Task<int> DownloadRun(string foldername)
        {
            int count = 0;
            Downloading = true;
            ProgressBarIsVisible = true;
            ProgressBarHeight = 8;
            int i = 0;
            while (true)
            {
                if (await DownloadImageAsync(foldername, downloadList[i].Image))
                    count++;
                downloadList[i].IsDownloaded = true;
                lock (downlaodLockObject)
                {
                    i++;
                    Progress2 = (float)i / downloadList.Count;
                    if (i >= downloadList.Count)
                    {
                        downloadList.Clear();
                        break;
                    }
                }
            }
            Progress2 = 0;
            ProgressBarHeight = 0;
            ProgressBarIsVisible = false;
            Downloading = false;
            return count;
        }

        private async Task<bool> DownloadImageAsync(string foldername, string url)
        {
            string directory = await fileService.CreateDirectory(foldername);
            string fileName = url.Substring(url.LastIndexOf('/') + 1);
            string filePath = Path.Combine(directory, fileName);

            if (File.Exists(filePath)) return false;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";

                var response = (HttpWebResponse)request.GetResponse();

                bool bImage = response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase);
                if ((response.StatusCode == HttpStatusCode.OK ||
                    response.StatusCode == HttpStatusCode.Moved ||
                    response.StatusCode == HttpStatusCode.Redirect)
                    && bImage)
                {
                    using (Stream inputStream = response.GetResponseStream())
                    using (Stream outputStream = await fileService.GetNewFileStream(Path.Combine(foldername, fileName)))
                    {
                        try
                        {
                            long lenth;
                            int bytesRead, tempsize = 0;
                            byte[] buffer = new byte[1024];
                            try
                            {
                                lenth = inputStream.Length;
                            }
                            catch
                            {
                                lenth = response.ContentLength; //window case
                            }
                            while (true)
                            {
                                bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                                if (bytesRead != 0)
                                {
                                    Progress1 = (float)tempsize / lenth;
                                    outputStream.Write(buffer, 0, bytesRead);
                                    tempsize += bytesRead;
                                }
                                else
                                {
                                    Progress1 = 0;
                                    break;
                                }
                            }
                        }
                        catch
                        {
                        }
                        inputStream.Close();
                        outputStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            if (File.Exists(filePath))
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    DependencyService.Get<IMedia>().UpdateGallery(filePath);
                }
                return true;
            }
            else
                return false;
        }

        private byte[] GetByteArrayFromUrl(string directory, string url)
        {
            string fileName = url.Substring(url.LastIndexOf('/') + 1);
            string filePath = Path.Combine(directory, fileName);
            HttpWebRequest request;
            HttpWebResponse response;
            if (!File.Exists(filePath))
            {
                try
                {
                    request = (HttpWebRequest)WebRequest.Create(url);
                    request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
                    response = (HttpWebResponse)request.GetResponse();
                    bool bImage = response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase);
                    if ((response.StatusCode == HttpStatusCode.OK ||
                        response.StatusCode == HttpStatusCode.Moved ||
                        response.StatusCode == HttpStatusCode.Redirect)
                        && bImage)
                    {
                        using (Stream inputStream = response.GetResponseStream())
                        {
                            try
                            {
                                int tempsize = 0;
                                int tempsize2 = (int)inputStream.Length;
                                byte[] buffer = new byte[4096];
                                byte[] ImageLargeArray = new byte[inputStream.Length];
                                int bytesRead;
                                while (true)
                                {
                                    if (tempsize2 > 0)
                                    {
                                        bytesRead = inputStream.Read(ImageLargeArray, tempsize, tempsize2 < 4096 ? tempsize2 : 4096);
                                        try
                                        {
                                            Progress1 = (float)tempsize / inputStream.Length;
                                        }
                                        catch { }
                                        tempsize += bytesRead;
                                        tempsize2 -= bytesRead;
                                    }
                                    else
                                    {
                                        Progress1 = 0;
                                        break;
                                    }
                                }
                                inputStream.Close();
                                return ImageLargeArray;
                            }
                            catch
                            {
                                inputStream.Close();
                                return null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return null;
        }

        public bool SuggestionLock = false;
        public void GetSuggestionItem()
        {
            try
            {
                string key;
                int i = Key.LastIndexOf(',');
                if (i < 0)
                {
                    key = Key;
                }
                else
                {
                    key = Key.Substring(i + 1);
                }
                var items = DerpTagDb.DerpTagSuggestion(key);
                if (!SuggestionLock)
                    SuggestionItems = items;
                else
                    SuggestionLock = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void AddKey(string tag)
        {
            int i;
            if (Key != null && Key.Length > 0)
            {
                i = Key.LastIndexOf(',');
            }
            else
            {
                i = -1;
            }
            if (i < 0)
            {
                Key = tag + ",";
            }
            else
            {
                Key = Key.Substring(0, Key.LastIndexOf(',') + 1) + tag + ",";
            }

        }

        public void ClearSelect()
        {
            lock (lockobject)
            {
                foreach (DerpImage image in Images)
                {
                    if (image.IsSelected)
                    {
                        image.IsSelected = false;
                    }
                }
            }
        }

        public async void LinkCopy()
        {
            string res = string.Empty;
            lock (lockobject)
            {
                foreach (DerpImage image in Images)
                {
                    if (image.IsSelected)
                    {
                        var tmp = "https://derpibooru.org/" + image.Id;
                        if (res.Length == 0)
                            res = tmp;
                        else
                            res += tmp;
                    }
                }
            }
            await Clipboard.SetTextAsync(res);
        }

        public async void HtmlCopy()
        {
            string res = string.Empty;
            lock (lockobject)
            {
                foreach (DerpImage image in Images)
                {
                    if (image.IsSelected)
                    {
                        if (image.Rating == DerpRating.SUGGESTIVE)
                        {
                            res += $"<a href=\"{image.ImageUrl}\"><img src=\"https://derpicdn.net/media/2012/09/11/19_58_52_556_suggestive.png\" width=\"250\" height=\"250\"></a>";
                        }
                        else if (image.Rating == DerpRating.EXPLICIT)
                        {
                            res += $"<a href=\"{image.ImageUrl}\"><img src=\"https://derpicdn.net/media/2014/01/07/21_08_05_460_rect4.png\" width=\"250\" height=\"250\"></a>";
                        }
                        else if (image.Rating == DerpRating.GROTESQUE)
                        {
                            res += $"<a href=\"{image.ImageUrl}\"><img src=\"https://derpicdn.net/media/2014/01/07/21_08_05_460_rect4.png\" width=\"250\" height=\"250\"></a>";
                        }
                        else
                        {
                            res += $"<img src=\"{image.ImageUrl}\">";
                        }
                    }
                }
            }
            await Clipboard.SetTextAsync(res);
        }

        public void Sort(int i)
        {
            switch (i)
            {
                case 0:
                    sortBy = DerpSortBy.CREATE;
                    sortOrder = DerpSortOrder.DESC;
                    break;
                case 1:
                    sortBy = DerpSortBy.CREATE;
                    sortOrder = DerpSortOrder.ASC;
                    break;
                case 2:
                    sortBy = DerpSortBy.SCORE;
                    sortOrder = DerpSortOrder.DESC;
                    break;
                case 3:
                    sortBy = DerpSortBy.SCORE;
                    sortOrder = DerpSortOrder.ASC;
                    break;
                default:
                    break;
            }
        }

        public bool ExistItem(string tag)
        {
            return _tempKeys.Contains(tag);
        }

        public void AddFilterItem(DerpTag tag)
        {
            _tempKeys.Add((tag.Sub ? "-" : string.Empty) + tag.NameEn);
            Key = "";
        }

        public void RemoveFilterItem(DerpTag tag)
        {
            string key = (tag.Sub ? "-" : string.Empty) + tag.NameEn;
            _tempKeys.Remove(key);
        }

        public void ClearFilterItem()
        {
            _tempKeys.Clear();
            Key = "";
        }

        public async Task<bool> GetMyFavorite()
        {
            if (IsBusy)
                return false;
            IsBusy = true;
            try
            {
                if (IsFavoriteView)
                {
                    var myimages = new List<DerpImage>();
                    var mylist = await derpibooru.GetDerpFavoriteImages(UserAPIKey);
                    foreach (var image in mylist)
                    {
                        if (!MyImages.Any(i => i.Id == image.Id))
                        {
                            image.IsFavorite = true;
                            myimages.Add(image);
                        }
                    }
                    foreach (var img in myimages)
                    {
                        await DerpImageDb.InsertDerpImageAsync(img);
                    }
                    await ExecuteLoadItemsCommand();
                }
            }
            catch
            {
            }
            IsBusy = false;
            return true;
        }

        public async Task<bool> GetMyFiles()
        {
            bool res = false;
            if (IsBusy)
                return false;
            if (IsFavoriteView)
            {
                IsBusy = true;
                Downloading = true;
                ProgressBarIsVisible = true;
                ProgressBarHeight = 8;
                try
                {
                    int index = 0;
                    string directory = await fileService.CreateDirectory("");
                    var filelist = (await fileService.GetSubList("")).FindAll(i => i.Name.Contains("__"));
                    var myimages = new List<DerpImage>();
                    foreach (var file in filelist)
                    {
                        var name = file.Name;
                        if (name.Contains("__"))
                        {
                            int d;
                            string id = name.Substring(0, name.IndexOf('_'));
                            if (int.TryParse(id, out d))
                                if (!MyImages.Any(i => i.Id == id))
                                {
                                    var image = await derpibooru.GetDerpImage(id);
                                    if (image != null)
                                    {
                                        image.IsFavorite = true;
                                        myimages.Add(image);
                                    }
                                }
                        }
                        index++;
                        Progress2 = (float)index / filelist.Count;
                    }
                    foreach (var img in myimages)
                    {
                        await DerpImageDb.InsertDerpImageAsync(img);
                    }
                    res = true;
                }
                catch
                {
                    res = false;
                }
                await ExecuteLoadItemsCommand();
                Progress2 = 0;
                ProgressBarHeight = 0;
                ProgressBarIsVisible = false;
                Downloading = false;
                IsBusy = false;
            }
            return res;
        }

        public async Task<bool> SearchMyFiles()
        {
            string directory = await fileService.CreateDirectory("");
            var filelist = (await fileService.GetSubList("")).FindAll(i => !i.IsDirectory && i.Name.Contains("__"));

            foreach (var file in filelist)
            {
                var stream = await fileService.GetReadFileStream(file.Name);
                if (stream != null)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);
                    byte[] ss = memoryStream.ToArray();
                    string base64String = Convert.ToBase64String(ss);
                    string base64String2 = "data:image/png;base64," + base64String;
                    await derpibooru.SearchImage(base64String2);
                }
            }
            return true;
        }
    }
}
