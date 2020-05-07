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
using SQLite;


namespace DerpViewer.Models
{
    public class DerpList
    {
        [JsonProperty("images")]
        public DerpImageCpt[] Images { get; set; }
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
        [JsonProperty("aspect_ratio")]
        public float AspectRatio { get; set; }
        [JsonProperty("comment_count")]
        public int CommentCount { get; set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("deletion_reason")]
        public string DeletionReason { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("downvotes")]
        public int Downvotes { get; set; }
        [JsonProperty("duplicate_of")]
        public string DuplicateOf { get; set; }
        [JsonProperty("faves")]
        public int Faves { get; set; }
        [JsonProperty("first_seen_at")]
        public DateTime FirstSeenAt { get; set; }
        [JsonProperty("format")]
        public string Format { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("hidden_from_users")]
        public bool HiddonFromUsers { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("intensities")]
        public object Intensities { get; set; }
        [JsonProperty("mime_type")]
        public string Mime_type { get; set; }
        [JsonProperty("orig_sha512_hash")]
        public string OrigSha512Hash { get; set; }
        [JsonProperty("processed")]
        public bool Processed { get; set; }
        [JsonProperty("representations")]
        public Representations Representations { get; set; }
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("sha512_hash")]
        public string Sha512Hash { get; set; }
        [JsonProperty("source_url")]
        public string SourceURL { get; set; }
        [JsonProperty("spoilered")]
        public bool Spoilered { get; set; }
        [JsonProperty("tag_count")]
        public int TagCount { get; set; }
        [JsonProperty("tag_ids")]
        public string[] TagIds { get; set; }
        [JsonProperty("tags")]
        public string[] Tags { get; set; }
        [JsonProperty("thumbnails_generated")]
        public bool ThumbnailsGenerated { get; set; }
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonProperty("uploader")]
        public string Uploader { get; set; }
        [JsonProperty("uploader_id")]
        public string UploaderID { get; set; }
        [JsonProperty("upvotes")]
        public int Upvotes { get; set; }
        [JsonProperty("view_url")]
        public string ViewUrl { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("wilson_score")]
        public float WilsonScore { get; set; }
        
        [JsonIgnore]
        public string ThumbTinyUrl
        {
            get
            {
                if (Format == "webm")
                    return string.Empty;
                else
                    return Representations.ThumbTiny;
            }
        }

        [JsonIgnore]
        public string ThumbUrl
        {
            get
            {
                if (Format == "webm")
                    return Representations.Thumb.Replace("webm","gif");
                else
                    return Representations.ThumbSmall;
            }
        }

        [JsonIgnore]
        public string ImageUrl
        {
            get
            {
                return ViewUrl;
            }
        }

        [JsonIgnore]
        public string SmallUrl
        {
            get
            {
                return Representations.Small;
            }
        }

        [JsonIgnore]
        public string MediumUrl
        {
            get
            {
                return Representations.Medium;
            }
        }

        [JsonIgnore]
        public string LargeUrl
        {
            get
            {
                return Representations.Large;
            }
        }

        [JsonIgnore]
        public string TallUrl
        {
            get
            {
                return Representations.Tall;
            }
        }
    }

    public class ImageArray
    {
        [PrimaryKey]
        public string Id { get; set; }
        public byte[] ByteArray { get; set; }
    }


    public class DerpImage : INotifyPropertyChanged
    {

        public DerpImage()
        {
        }

        public DerpImage(DerpImageCpt derp)
        {
            this.Id = derp.Id;
            this.CreatedAt = derp.CreatedAt;
            this.Score = derp.Score;
            this.OriginalFormat = derp.Format;
            this.Image = derp.ViewUrl;
            this.ThumbUrl = derp.ThumbUrl;
            this.SmallUrl = derp.SmallUrl;
            this.MediumUrl = derp.MediumUrl;
            this.LargeUrl = derp.LargeUrl;
            this.TallUrl = derp.TallUrl;
            this.ImageUrl = derp.ImageUrl;
            this.RatingStr = derp.Tags.Single(i => DerpTag.RatingEnStr.ToList().Exists(j => j == i.Trim())).Trim();
            this.Tags = string.Empty;
            foreach (var tag in derp.Tags)
            {
                if (this.Tags.Length == 0)
                    this.Tags = tag;
                else
                    this.Tags += ',' + tag;
            }
            this.Discription = derp.Description;
            this.AspectRatio = derp.AspectRatio;
            this.Width = derp.Width;
            this.Height = derp.Height;
        }

        public static double staticWidth = 0;
        public static double staticHeight = 0;

        [PrimaryKey]
        public string Id { get; set; }
        public string OriginalFormat { get; set; }
        [Ignore]
        public int Score { get; set; }
        [Ignore]
        public string IdScoreCreatedAt
        {
            get
            {
                return $"{Id} {Score} {CreatedAt}";
            }
        }
        public DateTime CreatedAt { get; set; }
        public string Image { get; set; }
        public string ThumbUrl { get; set; }
        public string SmallUrl { get; set; }
        public string MediumUrl { get; set; }
        public string LargeUrl { get; set; }
        public string TallUrl { get; set; }
        public string ImageUrl { get; set; }
        public float AspectRatio { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Tags { get; set; }
        public string Discription { get; set; }

        public static List<DerpTag> ContentTags;

        public string _artists;
        [Ignore]
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
        [Ignore]
        public string Characters
        {
            get
            {
                if (_characters == null && ContentTags != null)
                {
                    GetContents();
                }
                return _characters ?? "no service";
            }
        }

        public string _contents;
        [Ignore]
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
        [Ignore]
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
        public DerpRating Rating
        {
            get; set;
        }
        public string RatingStr
        {
            get
            {
                return DerpTag.RatingEnStr[(int)Rating];
            }
            set
            {
                Rating = (DerpRating)DerpTag.RatingEnStr.ToList().IndexOf(value);
            }
        }
        bool _isfavorite;
        public bool IsFavorite
        {
            get
            {
                return _isfavorite;
            }
            set
            {
                _isfavorite = value;
                OnPropertyChanged();
            }
        }

        [Ignore]
        public string FitSizeUrl
        {
            get
            {
                if (OriginalFormat == "gif")
                    return SmallUrl;
                else if (AspectRatio < 0.6)
                    return ImageUrl;
                else if (AspectRatio < 0.8)
                    return TallUrl;
                else if (AspectRatio < 1)
                    return LargeUrl;
                else
                    return MediumUrl;
            }
        }
        [Ignore]
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
                    if (Width < staticWidth)
                        return Width;
                    else
                        return staticWidth;
                }
                else
                {
                    return (int) (StaticHeight * AspectRatio);
                }
            }
        }

        [Ignore]
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
                    if (Height < staticHeight)
                        return Height;
                    else
                        return staticHeight;
                }
                else
                {
                    return (int)(StaticWidth / AspectRatio);
                }
            }
        }
               
        private bool _isselected, _isdownloaded;

        public event PropertyChangedEventHandler PropertyChanged;

        [Ignore]
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

        [Ignore]
        public bool IsDownloaded
        {
            get
            {
                return _isdownloaded;
            }
            set
            {
                _isdownloaded = value;
                OnPropertyChanged();
                OnPropertyChanged("BackgroundColor");
            }
        }

        [Ignore]
        public Color BackgroundColor
        {
            get
            {
                return IsSelected ? Color.LightGray : IsDownloaded ? Color.AliceBlue : Color.Transparent;
            }
        }
        public void GetContents()
        {
            var tagstr = Tags.Split(',');
            List<DerpTag> tags = new List<DerpTag>();
            List<string> tagstr2 = new List<string>();

            _artists = _characters = _anothers = _contents = string.Empty;
            try
            {

                for (int i = 0; i < tagstr.Length; i++)
                {
                    DerpTag tag = ContentTags.Find(item => item.NameEn == tagstr[i]);
                    string name;
                    if (tag != null)
                        name = tag.Name;
                    else
                        name = tagstr[i];
                    if (tag != null && tag.Category == DerpTagCategory.RATING)
                    {
                        if (_anothers.Length == 0)
                            _anothers = name;
                        else
                            _anothers += ", " + name;
                    }
                    else if (tagstr[i] == "artist needed" || tagstr[i].StartsWith("artist:") || tagstr[i].StartsWith("editor:") || tagstr[i].StartsWith("commissioner:"))
                    {
                        if (_artists.Length == 0)
                            _artists = name;
                        else
                            _artists += ", " + name;
                    }
                    else if (tagstr[i].StartsWith("spoiler:") || tagstr[i].StartsWith("comic:") || tagstr[i].StartsWith("fanfic:") || tagstr[i].StartsWith("game:") || tagstr[i].StartsWith("art pack:") || tagstr[i].StartsWith("series:"))
                    {
                        if (_contents.Length == 0)
                            _contents = name;
                        else
                            _contents += ", " + name;
                    }
                    else if (tagstr[i] == "oc" || tagstr[i] == "oc only")
                    {
                        if (_characters.Length == 0)
                            _characters = name;
                        else
                            _characters += ", " + name;
                    }
                    else if (tag != null && 
                        (tag.Category == DerpTagCategory.ARTCATEGORY || 
                        tag.Category == DerpTagCategory.CHARACTERFEATURE || 
                        tag.Category == DerpTagCategory.RACE || 
                        tag.Category == DerpTagCategory.POSE || 
                        tag.Category == DerpTagCategory.BODYPART||
                        tag.Category == DerpTagCategory.BEHAVIOR))
                    {
                        tags.Add(tag);
                    }
                    else
                    {
                        tagstr2.Add(tagstr[i]);
                    }
                }

                foreach(var tag in tags.OrderBy(i => i.Category))
                {
                    if (_anothers.Length == 0)
                        _anothers = tag.Name;
                    else
                        _anothers += ", " + tag.Name;
                }

                for (int i = 0; i < tagstr2.Count; i++)
                {
                    DerpTag tag = ContentTags.Find(item => item.NameEn == tagstr2[i]);
                    string name;
                    if (tag != null)
                        name = tag.Name;
                    else
                        name = tagstr2[i];

                    if (tag != null && tag.Category == DerpTagCategory.ORIGIN)
                    {
                        if (_artists.Length == 0)
                            _artists = tag.Name;
                        else
                            _artists += ", " + tag.Name;
                    }
                    else if (tag != null && (tag.Category == DerpTagCategory.CONTENTOFFICIAL || tag.Category == DerpTagCategory.CONTENTFANMADE))
                    {
                        if (_contents.Length == 0)
                            _contents = tag.Name;
                        else
                            _contents += ", " + tag.Name;
                    }
                    else if (tagstr2[i].StartsWith("oc:"))
                    {
                        if (_characters.Length == 0)
                            _characters = tagstr2[i];
                        else
                            _characters += ", " + tagstr2[i];
                    }
                    else if (tag != null && (tag.Category == DerpTagCategory.OC || tag.Category == DerpTagCategory.CHARACTER))
                    {
                        if (_characters.Length == 0)
                            _characters = tag.Name;
                        else
                            _characters += ", " + tag.Name;
                    }
                    else if (tag != null)
                    {
                        if (_anothers.Length == 0)
                            _anothers = tag.Name;
                        else
                            _anothers += ", " + tag.Name;
                    }
                    else
                    {
                        string org = tagstr2[i];
                        int acc = 0;
                        string temp2;

                        if (DerpTag.Korean)
                        {
                            while (true)
                            {
                                int space = org.IndexOf(' ', acc);
                                if (space > 0 && space < org.Length - 1)
                                {
                                    string word1 = org.Substring(0, space);
                                    string word2 = org.Substring(space + 1);

                                    var tag1 = ContentTags.Find(item => item.NameEn == word1);
                                    var tag2 = ContentTags.Find(item => item.NameEn == word2);

                                    if (tag1 != null && tag2 != null)
                                    {
                                        temp2 = tag1.Name + ' ' + tag2.Name;
                                        break;
                                    }
                                    else
                                    {
                                        acc = word1.Length + 1;
                                    }
                                }
                                else
                                {
                                    temp2 = tagstr2[i];
                                    break;
                                }
                            }
                        }
                        else
                        {
                            temp2 = tagstr2[i];
                        }


                        if (_anothers.Length == 0)
                            _anothers = temp2;
                        else
                            _anothers += ", " + temp2;
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

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
