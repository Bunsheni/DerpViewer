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
using System.Collections.ObjectModel;
using System.IO;

namespace DerpViewer.Services
{
    enum DerpSortBy { CREATE, SCORE }
    enum DerpSortOrder { ASC, DESC }
    class DerpibooruService : INotifyPropertyChanged
    {
        private static readonly string[] sortbyfieldName = new string[]{ "created_at", "score"};
        private static readonly string[] sortbyorder = new string[] { "asc", "desc" };

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
        int progressBarHeight;
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

        public async Task<List<DerpImage>> GetSearchImage(List<DerpImage> myList, List<CtFileItem> downloadedList, string userkey, string key, int index, DerpSortBy sf, DerpSortOrder sd)
        {
            string orl = "https://derpibooru.org/api/v1/json/";
            string url = $"{orl}search/images?key={userkey}&per_page=50&q={key.Replace(' ', '+').ToLower()}&page={index}&sf={sortbyfieldName[(int)sf]}&sd={sortbyorder[(int)sd]}";
            var response = await _client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            var content = await response.Content.ReadAsStringAsync();

            var imgs = JsonConvert.DeserializeObject<DerpList>(content).Images;

            List<DerpImage> res = new List<DerpImage>();
            foreach (DerpImageCpt img in imgs)
            {
                DerpImage myimg = new DerpImage(img);
                if(myList.Exists(i => i.Id == img.Id))
                {
                    myimg.IsFavorite = true;
                }
                if (downloadedList != null && downloadedList.Exists(i => i.Name == img.Id || i.Name.StartsWith(img.Id + "__")))
                {
                    myimg.IsDownloaded = true;
                }
                res.Add(myimg);
            }
            return res;
        }

        public async Task<List<DerpImage>> GetDerpFavoriteImages(string userkey)
        {
            List<DerpImage> res = new List<DerpImage>();
            string orl = "https://derpibooru.org/search.json";
            string url = $"{orl}?q=my:faves&key={userkey}";
            var response = await _client.GetAsync(url);
            if (response.StatusCode != HttpStatusCode.NotFound)
            {
                var content = await response.Content.ReadAsStringAsync();
                foreach (DerpImageCpt img in JsonConvert.DeserializeObject<DerpList>(content).Images)
                {
                    var myimg = new DerpImage(img);
                    res.Add(myimg);
                }
            }
            return res;
        }

        public async Task<DerpImage> GetDerpImage(string id)
        {
            string orl = "https://derpibooru.org/api/v1/json/";
            string url = $"{orl}/images/:{id}";
            var response = await _client.GetAsync(url);
            if (response.StatusCode != HttpStatusCode.NotFound)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    var img = JsonConvert.DeserializeObject<DerpImageCpt>(content);
                    if (img != null)
                    {
                        var derpimg = new DerpImage(img);
                        return derpimg;
                    }
                }
                catch
                {

                }
            }
            return null;
        }

        public List<DerpTag> GetTagInfoFromDerpibooru(List<DerpTag> oldTags, int max, IWebConnection _web, out int newcount)
        {
            newcount = 0;

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
                                DerpTag temp = oldTags.Find(i => i.Id == tag.Id);
                                if (temp != null)
                                {
                                    tag = temp;
                                }
                                else
                                {
                                    newcount++;
                                }
                                tag.Index = newTags.Count;
                                newTags.Add(tag);

                                Progress = (float)newTags.Count / max;
                                if (newTags.Count >= max)
                                {
                                    goto EndPoint;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                goto EndPoint;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                    pageindex++;
                }

            EndPoint:
                for (int season = 9; season > 0; season--)
                {
                    int fin = season == 3 ? 13 : 26;
                    var tags = new DerpTag($"spoiler:s{season.ToString("D2")}") { Category = DerpTagCategory.SPOILER, Index = newTags.Count };
                    newTags.RemoveAll(i => i.NameEn == tags.NameEn);
                    newTags.Add(tags);
                    for (int episod = fin; episod > 0; episod--)
                    {
                        var tage = new DerpTag($"spoiler:s{season.ToString("D2")}e{episod.ToString("D2")}") { Category = DerpTagCategory.SPOILER, Index = newTags.Count };
                        newTags.RemoveAll(i => i.NameEn == tage.NameEn);
                        newTags.Add(tage);
                    }
                }
                newTags.Add(new DerpTag("score.gt:100"));
                newTags.Add(new DerpTag("score.gt:500"));
                newTags.Add(new DerpTag("score.gt:1000"));
                newTags.Add(new DerpTag("score.gte:100"));
                newTags.Add(new DerpTag("score.gte:500"));
                newTags.Add(new DerpTag("score.gte:1000"));
                newTags.Add(new DerpTag("score.lt:100"));
                newTags.Add(new DerpTag("score.lt:500"));
                newTags.Add(new DerpTag("score.lt:1000"));
                newTags.Add(new DerpTag("score.lte:100"));
                newTags.Add(new DerpTag("score.lte:500"));
                newTags.Add(new DerpTag("score.lte:1000"));


                var olds = oldTags.FindAll(i => newTags.Find(j => j.NameEn == i.NameEn) == null);
                newTags.AddRange(olds);

                ProgressBarHeight = 0;
                ProgressBarIsVisible = false;
            }
            return newTags;
        }

        public async Task SearchImage(string base64)
        {
            await _web.SearchImage(base64);
        }
    }

}
