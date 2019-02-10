using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace DerpViewer.Models
{
    class DerpList
    {
        [JsonProperty("images")]
        public DerpImageCpt[] Images { get; set; }
        [JsonProperty("top_scoring")]
        public DerpImageCpt[] TopScoring { get; set; }
        [JsonProperty("search")]
        public DerpImageCpt[] Search { get; set; }
    }
    /*
    DuplicateReport is a duplicate image report.
    */
    public struct DuplicateReport
    {
        //[JsonProperty("id")]
        //int ID;
        //[JsonProperty("state")]
        //string State;
        //[JsonProperty("reason")]
        //string Reason;
        //[JsonProperty("image_id_number")]
        //int ImageIdNumber;
        //[JsonProperty("target_image_id_number")]
        //int TargetImageIdNumber;
        //[JsonProperty("user")]
        //string User;
        //[JsonProperty("created_at")]
        //string CreatedAt;
        //[JsonProperty("modifier")]
        //DupeReportModifier Modifier;


    }
    /*
    DupeReportModifier is the weighting of the an image duplicate report.
    */
    public struct DupeReportModifier
    {
        //[JsonProperty("id")]
        //long ID;
        //[JsonProperty("name")]
        //string Name;
        //[JsonProperty("avatar")]
        //string Avatar;
        //[JsonProperty("comment_count")]
        //int CommentCount;
        //[JsonProperty("upload_count")]
        //int UploadCount;
        //[JsonProperty("post_count")]
        //int PostCount;
        //[JsonProperty("topic_count")]
        //int TopicCount;
    }

    public class Representations
    {
        [JsonProperty("thumb_tiny")]
        public string ThumbTiny { get; set; }
        [JsonProperty("thumb_small")]
        public string ThumbSmall { get; set; }
        [JsonProperty("thumb")]
        public string Thumb { get; set; }
        [JsonProperty("small")]
        public string Small { get; set; }
        [JsonProperty("medium")]
        public string Medium { get; set; }
        [JsonProperty("large")]
        public string Large { get; set; }
        [JsonProperty("tall")]
        public string Tall { get; set; }
        [JsonProperty("full")]
        public string Full { get; set; }
    }

    public class DerpImageCpt
    {
        public static List<DerpTag> ContentTags;

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("duplicate_reports")]
        public DuplicateReport[] DuplicateReport { get; set; }

        [JsonProperty("first_seen_at")]
        public DateTime FirstSeenAt { get; set; }

        [JsonProperty("uploader_id")]
        public string UploaderID { get; set; }

        [JsonProperty("file_name")]
        public string FileName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("uploader")]
        public string Uploader { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("upvotes")]
        public int Upvotes { get; set; }

        [JsonProperty("downvotes")]
        public int Downvotes { get; set; }

        [JsonProperty("faves")]
        public int Faves { get; set; }

        [JsonProperty("comment_count")]
        public int CommentCount { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("tag_ids")]
        public string[] TagIds { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("aspect_ratio")]
        public float AspectRatio { get; set; }

        [JsonProperty("original_format")]
        public string OriginalFormat { get; set; }

        [JsonProperty("mime_type")]
        public string MimeType { get; set; }

        [JsonProperty("sha512_hash")]
        public string Sha512Hash { get; set; }

        [JsonProperty("orig_sha512_hash")]
        public string OrigSha512Hash { get; set; }

        [JsonProperty("source_url")]
        public string SourceURL { get; set; }

        [JsonProperty("representations")]
        public Representations Representations { get; set; }

        [JsonProperty("is_rendered")]
        public bool IsRendered { get; set; }

        [JsonProperty("is_optimized")]
        public bool IsOptimized { get; set; }

        [JsonProperty("interactions")]
        public string Interactions { get; set; }
        
        public string ThumbTinyUrl
        {
            get
            {
                if (OriginalFormat == "webm")
                    return string.Empty;
                else
                    return $"https:{Representations.ThumbTiny}";
            }
        }
        public string ThumbUrl
        {
            get
            {
                if (OriginalFormat == "webm")
                    return string.Empty;
                else
                    return $"https:{Representations.ThumbSmall}";
            }
        }
        public string ImageUrl
        {
            get
            {
                return $"https:{Image}";
            }
        }
        public string SmallUrl
        {
            get
            {
                return $"https:{Representations.Small}";
            }
        }

        public string MediumUrl
        {
            get
            {
                return $"https:{Representations.Medium}";
            }
        }

        public string LargeUrl
        {
            get
            {
                return $"https:{Representations.Large}";
            }
        }

        public string TallUrl
        {
            get
            {
                return $"https:{Representations.Tall}";
            }
        }

        public string _artists;
        public string Aritsts
        {
            get
            {
                if (_artists == null && ContentTags != null)
                {
                    GetContents();
                }
                return _artists ?? "no service";
            }
        }

        public string _characters;
        public string Characters
        {
            get
            {
                if(_characters == null && ContentTags != null)
                {
                    GetContents();
                }
                return _characters ?? "no service";
            }
        }

        public string _contents;
        public string Contents
        {
            get
            {
                if (_contents == null && ContentTags != null)
                {
                    GetContents();
                }
                return _contents ?? "no service";
            }
        }

        public string _anothers;
        public string Anothers
        {
            get
            {
                if (_anothers == null && ContentTags != null)
                {
                    GetContents();
                }
                return _anothers ?? "no service";
            }
        }

        public string IdScoreCreatedAt
        {
            get
            {
                return $"{Id} {Score} {CreatedAt}";
            }
        }

        public void GetContents()
        {
            List<string> tagstr = Library.stringDivider(Tags, ", ");
            _artists = _characters = _anothers = _contents = string.Empty;
            try
            {

                for (int i = 0; i < tagstr.Count; i++)
                {
                    DerpTag tag = ContentTags.Find(item => item.NameEn == tagstr[i]);
                    if (tag != null && tag.Category == DerpTagCategory.RATING)
                    {
                        if (_anothers.Length == 0)
                            _anothers = tag.NameEn;
                        else
                            _anothers += ", " + tag.NameEn;
                    }
                    else if (tagstr[i].StartsWith("artist:") || tagstr[i].StartsWith("editor:"))
                    {
                        if (_artists.Length == 0)
                            _artists = tagstr[i];
                        else
                            _artists += ", " + tagstr[i];
                    }
                    else if (tagstr[i].StartsWith("spoiler:"))
                    {
                        if (_contents.Length == 0)
                            _contents = tagstr[i].Substring(8);
                        else
                            _contents += ", " + tagstr[i].Substring(8);
                    }
                    else if (tagstr[i] == "oc" || tagstr[i] == "oc only")
                    {
                        if (_characters.Length == 0)
                            _characters = tagstr[i];
                        else
                            _characters += ", " + tagstr[i];
                    }
                }
                for (int i = 0; i < tagstr.Count; i++)
                {
                    DerpTag tag = ContentTags.Find(item => item.NameEn == tagstr[i]);

                    if (tag != null && tag.Category == DerpTagCategory.RATING)
                    {
                    }
                    else if (tagstr[i].StartsWith("artist:") || tagstr[i].StartsWith("editor:") || tagstr[i].StartsWith("spoiler:") || tagstr[i] == "oc" || tagstr[i] == "oc only")
                    {
                    }
                    else if (tag != null && tag.Category == DerpTagCategory.ORIGIN)
                    {
                        if (_artists.Length == 0)
                            _artists = tag.NameEn;
                        else
                            _artists += ", " + tag.NameEn;
                    }
                    else if (tag != null && (tag.Category == DerpTagCategory.CONTENTOFFICIAL || tag.Category == DerpTagCategory.CONTENTFANMADE))
                    {
                        if (_contents.Length == 0)
                            _contents = tag.NameEn;
                        else
                            _contents += ", " + tag.NameEn;
                    }
                    else if (tagstr[i].StartsWith("oc:") || (tag != null && (tag.Category == DerpTagCategory.OC || tag.Category == DerpTagCategory.CHARACTER)))
                    {
                        if (_characters.Length == 0)
                            _characters = tagstr[i];
                        else
                            _characters += ", " + tagstr[i];
                    }
                    else
                    {
                        if (_anothers.Length == 0)
                            _anothers = tagstr[i];
                        else
                            _anothers += ", " + tagstr[i];
                    }
                }

            }
            catch
            { }
            _artists = _artists.Length == 0 ? "unknown artist" : _artists;
            _contents = _contents.Length == 0 ? "no content" : _contents;
            _characters = _characters.Length == 0 ? "no character" : _characters;
            _anothers = _anothers.Length == 0 ? "no tag" : _anothers;
        }
    }

    public class DerpImage : INotifyPropertyChanged
    {

        public DerpImage(DerpImageCpt derp)
        {
            this.Id = derp.Id;
            this.OriginalFormat = derp.OriginalFormat;
            this.IdScoreCreatedAt = derp.IdScoreCreatedAt;
            this.IdScoreCreatedAt = derp.IdScoreCreatedAt;
            this.Image = derp.Image;
            this.ThumbUrl = derp.ThumbUrl;
            this.MediumUrl = derp.MediumUrl;
            this.LargeUrl = derp.LargeUrl;
            this.TallUrl = derp.TallUrl;
            this.SmallUrl = derp.SmallUrl;
            this.ImageUrl = derp.ImageUrl;
            this.Aritsts = derp.Aritsts;
            this.Characters = derp.Characters;
            this.Contents = derp.Contents;
            this.Anothers = derp.Anothers;
            this.AspectRatio = derp.AspectRatio;
            this.Width = derp.Width;
            this.Height = derp.Height;
        }

        public static double staticWidth = 0;
        public static double staticHeight = 0;
        public string Id { get; set; }
        public string OriginalFormat { get; set; }
        public string IdScoreCreatedAt { get; set; }
        public string Image { get; set; }
        public string ThumbUrl { get; set; }
        public string SmallUrl { get; set; }
        public string MediumUrl { get; set; }
        public string LargeUrl { get; set; }
        public string TallUrl { get; set; }
        public string ImageUrl { get; set; }
        public string FitSizeUrl
        {
            get
            {
                if (OriginalFormat == "gif")
                    return SmallUrl;
                else if (AspectRatio < 0.5)
                    return LargeUrl;
                else if (AspectRatio < 0.3)
                    return TallUrl;
                else
                    return MediumUrl;
            }
        }
        public string Aritsts { get; set; }
        public string Characters { get; set; }
        public string Contents { get; set; }
        public string Anothers { get; set; }
        public float AspectRatio { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double StaticWidth
        {
            get
            {
                if(staticWidth == 0 && staticHeight == 0)
                {
                    return Height;
                }
                else if(staticWidth != 0)
                {
                    return staticWidth;
                }
                else
                {
                    return (int) (staticHeight * AspectRatio);
                }
            }
        }
        public double StaticHeight
        {
            get
            {
                if (staticWidth == 0 && staticHeight == 0)
                {
                    return Height;
                }
                else if (staticHeight != 0)
                {
                    return staticHeight;
                }
                else
                {
                    return (int)(staticWidth / AspectRatio);
                }
            }
        }



        private bool _isselected;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get
            {
                return _isselected;
            }
            set
            {
                _isselected = value;
                OnPropertyChanged();
                OnPropertyChanged("BackgroundColor");
            }
        }
        public Color BackgroundColor
        {
            get
            {
                return IsSelected ? Color.LightGray : Color.Transparent;
            }
        }
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
