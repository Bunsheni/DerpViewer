using FFImageLoading;
using FFImageLoading.Cache;
using DerpViewer.Models;
using DerpViewer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DerpViewer.ViewModels
{
    class DerpImagesViewModel : BaseViewModel
    {
        DerpibooruService derpibooru;
        DerpFileService fileService = new DerpFileService();
        object lockobject = new object();
        object downlaodLockObject = new object();
        bool downloading;
        List<DerpImage> downloadList = new List<DerpImage>();
        int page = 0;
        DerpSortBy sortBy = DerpSortBy.CREATE;
        DerpSortOrder sortOrder = DerpSortOrder.DESC;
        bool searchmode;

        bool progressBarIsVisible;
        int progressBarHeight;
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

        float progress1, progress2;
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
        private ObservableCollection<DerpImage> _images;
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
        List<DerpTag> suggestionItem;
        public List<DerpTag> SuggestionItems
        {
            get { return suggestionItem; }

            set
            {
                suggestionItem = value;
                OnPropertyChanged();
            }
        }

        string CurrentKey;
        string key;
        public string Key { get { return key; } set { key = value; OnPropertyChanged(); } }

        public DerpImagesViewModel(IWebConnection web)
        {
            derpibooru = new DerpibooruService(web);
        }

        public async Task LastedView()
        {
            page = 1;
            endPage = false;
            searchmode = false;
            if(Images != null)
            {
                Images.Clear();
                GC.Collect();
                await ImageService.Instance.InvalidateCacheAsync(CacheType.All);
            }
            Images = new ObservableCollection<DerpImage>(await derpibooru.GetSearchImage(UserAPIKey, "*", page, sortBy, sortOrder));
        }
        
        public async void Search()
        {
            if (Key.Length != 0)
            {
                CurrentKey = Key.Trim(' ', ',');
                endPage = false;
                searchmode = true;
                page = 1;
                if (Images != null)
                {
                    Images.Clear();
                    GC.Collect();
                    await ImageService.Instance.InvalidateCacheAsync(CacheType.All);
                }
                Images = new ObservableCollection<DerpImage>(await derpibooru.GetSearchImage(UserAPIKey, CurrentKey, page, sortBy, sortOrder) ?? new List<DerpImage>());
            }
        }

        bool endPage;
        public async Task MoreView()
        {
            if(!endPage)
            {
                List<DerpImage> tempimages;
                page++;
                if (searchmode)
                {
                    tempimages = await derpibooru.GetSearchImage(UserAPIKey, CurrentKey, page, sortBy, sortOrder);
                }
                else
                {
                    tempimages = await derpibooru.GetSearchImage(UserAPIKey, "*", page, sortBy, sortOrder);
                }
                if(tempimages.Count == 0)
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
                GC.Collect();
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
        public void Download()
        {
            foreach (DerpImage image in GetSelectedImages())
            {
                lock (downlaodLockObject)
                {
                    if (!downloadList.Exists(i => i == image))
                        downloadList.Add(image);
                }
            }
            if (downloadList.Count > 0)
                DownloadRun();
        }

        public async void DownloadRun()
        {
            if (!downloading)
            {
                downloading = true;
                ProgressBarHeight = 8;
                ProgressBarIsVisible = true;
                int i = 0;
                while (true)
                {
                    await DownloadImageAsync(DependencyService.Get<ISQLiteDb>().GetDocumentsPath(), $"https:{downloadList[i].Image}");
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
                ProgressBarIsVisible = false;
                ProgressBarHeight = 0;
                downloading = false;
            }
        }

        private async Task DownloadImageAsync(string directory, string url)
        {
            await fileService.CreateDirectory("DerpViewer");
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
                        using (Stream outputStream = await fileService.GetNewFileStream(fileName))
                        {
                            try
                            {
                                int tempsize = 0;
                                byte[] buffer = new byte[4096];
                                int bytesRead;
                                while (true)
                                {
                                    bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                                    if (bytesRead != 0)
                                    {
                                        try
                                        {
                                            Progress1 = (float)tempsize / inputStream.Length;
                                        }
                                        catch { }
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
                                inputStream.Close();
                                outputStream.Close();
                                return;
                            }
                            inputStream.Close();
                            outputStream.Close();
                        }
                        if (Device.RuntimePlatform == Device.Android)
                        {
                            DependencyService.Get<IMedia>().UpdateGallery(filePath);
                        }
                    }
                }
                catch
                {
                }
            }
        }


        //private async Task DownloadImageAsync(string directory, string url)
        //{
        //    Directory.CreateDirectory(directory);
        //    string fd = Path.Combine(directory, url.Substring(url.LastIndexOf('/') + 1));
        //    string fd2 = fd + "_temp";
        //    string fd3 = fd;
        //    HttpWebRequest request;
        //    HttpWebResponse response;
        //    if (!File.Exists(fd3))
        //    {
        //        try
        //        {
        //            request = (HttpWebRequest)WebRequest.Create(url);
        //            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
        //            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";
        //            response = (HttpWebResponse)request.GetResponse();
        //            bool bImage = response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase);
        //            if ((response.StatusCode == HttpStatusCode.OK ||
        //                response.StatusCode == HttpStatusCode.Moved ||
        //                response.StatusCode == HttpStatusCode.Redirect)
        //                && bImage)
        //            {
        //                using (Stream inputStream = response.GetResponseStream())
        //                using (Stream outputStream = File.OpenWrite(fd2))
        //                {
        //                    try
        //                    {
        //                        int tempsize = 0;
        //                        byte[] buffer = new byte[4096];
        //                        int bytesRead;
        //                        while (true)
        //                        {
        //                            bytesRead = inputStream.Read(buffer, 0, buffer.Length);
        //                            if (bytesRead != 0)
        //                            {
        //                                try
        //                                {
        //                                    Progress1 = (float)tempsize / inputStream.Length;
        //                                }
        //                                catch { }
        //                                outputStream.Write(buffer, 0, bytesRead);
        //                                tempsize += bytesRead;
        //                            }
        //                            else
        //                            {
        //                                Progress1 = 0;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    catch
        //                    {
        //                        inputStream.Close();
        //                        outputStream.Close();
        //                        return;
        //                    }
        //                    inputStream.Close();
        //                    outputStream.Close();
        //                }
        //                File.Move(fd2, fd3);
        //                if (Device.RuntimePlatform == Device.Android)
        //                {
        //                    DependencyService.Get<IMedia>().UpdateGallery(fd3);
        //                }
        //            }
        //        }
        //        catch
        //        {
        //        }
        //    }
        //}

        public void GetSuggestionItem()
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
            SuggestionItems = DerpDb.DerpTagSuggestion(key);
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
            if(i<0)
            {
                Key = tag + ",";
            }
            else
            {
                Key = Key.Substring(0, Key.LastIndexOf(',') + 1) + tag + ",";
            }

        }

        public async void listViewItemAppearing(object item)
        {
            if (Images.Count > 0)
            {
                if ((Images.Count >= 49 && Images[Images.Count - 49] == item) || Images.Last() == item)
                    await MoreView();
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
    }
}
