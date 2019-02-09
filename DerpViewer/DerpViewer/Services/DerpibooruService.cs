using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

using DerpViewer.Models;
using DerpViewer.Views;
using Xamarin.Forms;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DerpViewer.Services
{
    enum DerpSortBy { CREATE, SCORE }
    enum DerpSortOrder { ASC, DESC }
    class DerpibooruService : INotifyPropertyChanged
    {


        private static readonly string[] sortbyfieldName = new string[]{ "created_at", "score"};
        private static readonly string[] sortbyorder = new string[] { "asc", "desc" };
        public static readonly string[] sortbyen = new string[] { "Latest", "Oldest", "High Score", "Low Score" };
        public static readonly string[] sortbykr = new string[] { "생성된 날짜", "점수" };

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



        public static readonly int MinSearchLength = 5;
        private HttpClient _client = new HttpClient();

        IWebConnection _web;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DerpibooruService(IWebConnection web)
        {
            _web = web;
        }

        public async Task<List<DerpImage>> GetLastedImage(string userkey, int index)
        {
            string orl = "https://derpibooru.org/images.json";
            string url = $"{orl}?key={userkey}&perpage=50&page={index}";
            var response = await _client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            var content = await response.Content.ReadAsStringAsync();

            List<DerpImage> res = new List<DerpImage>();
            foreach (DerpImageCpt img in JsonConvert.DeserializeObject<DerpList>(content).Images)
            {
                res.Add(new DerpImage(img));
            }
            return res;
        }
                
        public async Task<List<DerpImage>> GetSearchImage(string userkey, string key, int index, DerpSortBy sf, DerpSortOrder sd)
        {
            string orl = "https://derpibooru.org/search.json";
            string url = $"{orl}?key={userkey}&perpage=50&q={key.Replace(' ', '+').ToLower()}&page={index}&sf={sortbyfieldName[(int)sf]}&sd={sortbyorder[(int)sd]}";
            var response = await _client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            var content = await response.Content.ReadAsStringAsync();

            List<DerpImage> res = new List<DerpImage>();
            foreach (DerpImageCpt img in JsonConvert.DeserializeObject<DerpList>(content).Search)
            {
                res.Add(new DerpImage(img));
            }
            return res;
        }

        protected MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        public List<DerpTag> GetTagInfoFromDerpibooru(List<DerpTag> oldTags, int max)
        {
            List<DerpTag> newTags = new List<DerpTag>();

            using (WebClient webClient = new WebClient())
            {
                int pageindex = 1;
                string url = "https://derpibooru.org/tags?page=";
                string infostr;
                string tempstr;
                string tempstr2;

                ProgressBarHeight = 8;
                ProgressBarIsVisible = true;

                while (true)
                {
                    infostr = webClient.DownloadString(url + pageindex);
                    infostr = Library.extractionString(infostr, "<div class=\"tag-list\">", "</div>");

                    if (infostr.Contains("<span class=\"tag dropdown\" "))
                    {
                        while (infostr.Contains("<span class=\"tag dropdown\" "))
                        {
                            try
                            {
                                Library.extractionString(infostr, out infostr, "<span class=\"tag dropdown\" ", "Filter</a></span></span>", out tempstr);
                                DerpTag tag = new DerpTag();
                                tag.CategoryStrEn = Library.extractionString(tempstr, "data-tag-category=\"", "\"");
                                tag.Id = Library.extractionString(tempstr, "data-tag-id=\"", "\"");
                                tempstr2 = Library.extractionString(tempstr, "data-tag-name=\"", "\"");
                                tag.NameEn = tag.NameKr = System.Web.HttpUtility.HtmlDecode(tempstr2).Replace("&#39;", "'");
                                tempstr2 = Library.extractionString(tempstr, "title=\"", "\"");
                                tag.DescriptionEn = tag.DescriptionKr = System.Web.HttpUtility.HtmlDecode(tempstr2);
                                //tag.DescriptionKr = await _web.TransWebBrowserInitAsync(tag.CategoryStrEn, "en", "ko");
                                DerpTag temp = oldTags.Find(i => i.Id == tag.Id);
                                if (temp == null)
                                {
                                    newTags.Add(tag);
                                }
                                else
                                {
                                    newTags.Add(tag);
                                    oldTags.Remove(temp);
                                }
                                Progress = (float)newTags.Count / max;
                                if (newTags.Count >= max)
                                {
                                    foreach (DerpTag tag2 in oldTags)
                                    {
                                        tag2.NameEn = System.Web.HttpUtility.HtmlDecode(tag2.NameEn);
                                    }
                                    newTags.AddRange(oldTags);
                                    return newTags;
                                }
                            }
                            catch (Exception ex)
                            {
                                newTags.AddRange(oldTags);
                                Console.WriteLine(ex);
                                return newTags;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                    pageindex++;
                }

                ProgressBarHeight = 0;
                ProgressBarIsVisible = false;
            }
            return newTags;
        }
    }

}
