using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace DerpViewer.Models
{
    public enum DerpTagCategory { NONE, RATING, ORIGIN, SPOILER, CONTENTOFFICIAL, CONTENTFANMADE, ARTIST, CHARACTER, OC, ERROR, UNKNOWN, ARTCATEGORY, CHARACTERFEATURE, RACE, POSE, BODYPART, BEHAVIOR }
    public enum DerpRating { SAFE, EXPLICIT, SUGGESTIVE, QUESTIONABLE, GRIMDARK, SEMIGRIMDARK, GROTESQUE, UNKNOWN }
    public class DerpTag : INotifyPropertyChanged
    {
        public static readonly string[] _caten = { string.Empty, "rating", "origin", "spoiler", "content-official", "content-fanmade", "artist", "character", "oc", "error", "unknown", "art category", "character feature", "race", "pose", "body part", "behavior"};
        public static readonly string[] _catkr = { string.Empty, "등급", "출처", "스포일러", "공식", "팬메이드", "작가", "캐릭터", "오씨", "에러", "알 수 없음", "예술 카테고리", "인물 특징", "종족", "자세", "신체 부분", "행동"};
        public static readonly string[] RatingEnStr = { "safe", "explicit", "suggestive", "questionable", "grimdark", "semi-grimdark", "grotesque" };
        public static readonly string[] RatingKrStr = { "safe", "explicit", "suggestive", "questionable", "grimdark", "semi-grimdark", "grotesque" };

        string nameen, namekr, synonym, desen, deskr;
        int index = 0;

        [PrimaryKey]
        public string Id { get; set; }
        public DerpTagCategory Category { get; set; }
        public string NameEn
        {
            get
            {
                return nameen;
            }
            set
            {
                nameen = value;
                OnPropertyChanged();
            }
        }

        public string NameKr
        {
            get
            {
                return namekr;
            }
            set
            {
                namekr = value;
                OnPropertyChanged();
            }
        }
        public string Synonym
        {
            get
            {
                return synonym;
            }
            set
            {
                synonym = value;
                OnPropertyChanged();
            }
        }
        public string DescriptionEn
        {
            get
            {
                return desen;
            }
            set
            {
                desen = value;
                OnPropertyChanged();
            }
        }
        public string DescriptionKr
        {
            get
            {
                return deskr;
            }
            set
            {
                deskr = value;
                OnPropertyChanged();
            }
        }
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
                OnPropertyChanged();
            }
        }

        public static bool Korean {get; set; }

        [Ignore]
        public bool Sub { get; set; }
        private bool _isselected;
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
        public Color BackgroundColor
        {
            get
            {
                return IsSelected ? Color.LightGray : Color.Transparent;
            }
        }

        [Ignore]
        public string Name
        {
            get
            {
                if(Korean)
                {
                    return NameKr;
                }
                else
                {
                    return NameEn;
                }
            }
        }
        [Ignore]
        public string Description
        {
            get
            {
                if (Korean)
                {
                    if (DescriptionKr.Length == 0)
                        return "No Description";
                    else
                        return DescriptionKr;
                }
                else
                {
                    if (DescriptionEn.Length == 0)
                        return "No Description";
                    else
                        return DescriptionEn;
                }
            }
        }
        [Ignore]
        public string CategoryStr => Korean ? CategoryStrKr : CategoryStrEn;
        [Ignore]
        public string CategoryStrEn
        {
            get
            {
                return _caten[(int)Category];
            }
            set
            {
                for (int i = 0; i < _caten.Length ; i++)
                {
                    if (_caten[i] == value || _catkr[i] == value)
                    {
                        Category = (DerpTagCategory)i;
                        return;
                    }
                }
                Category = DerpTagCategory.UNKNOWN;
            }
        }

        [Ignore]
        public string CategoryStrKr
        {
            get
            {
                return _catkr[(int)Category];
            }
            set
            {
                for (int i = 0; i < _caten.Length; i++)
                {
                    if (_caten[i] == value || _catkr[i] == value)
                    {
                        Category = (DerpTagCategory)i;
                        return;
                    }
                }
                Category = DerpTagCategory.UNKNOWN;
            }
        }
        public bool Contain(string key)
        {
            key = key.Replace(" ", "").ToLower();
            return NameEn.Replace(" ", "").ToLower().Contains(key) || NameKr.Replace(" ", "").ToLower().Contains(key);
        }
        public bool Suggestion(string key)
        {
            key = key.Replace(" ", "").ToLower();
            return NameEn.Replace(" ", "").ToLower().StartsWith(key) || NameKr.Replace(" ", "").ToLower().StartsWith(key);
        }

        public DerpTag()
        {
            Id = string.Empty;
            NameEn = NameKr = string.Empty;
            DescriptionEn = DescriptionKr = string.Empty;
            Category = DerpTagCategory.UNKNOWN;
            Synonym = string.Empty;
        }

        public DerpTag(string name)
        {
            Id = name;
            NameEn = NameKr = name;
            DescriptionEn = DescriptionKr = string.Empty;
            Category = DerpTagCategory.NONE;
            Synonym = string.Empty;
        }

        public DerpTag Clone()
        {
            DerpTag model = new DerpTag();
            model.Id = this.Id;
            model.Index = this.Index;
            model.Category = this.Category;
            model.NameEn = this.NameEn;
            model.NameKr = this.NameKr;
            model.DescriptionEn = this.DescriptionEn;
            model.DescriptionKr = this.DescriptionKr;
            model.Synonym = this.Synonym;
            model.IsFavorite = this.IsFavorite;
            return model;
        }

        public virtual List<string> GetTags()
        {
            List<string> temp = Library.StringDivider(Synonym, "/");
            if (NameEn != NameKr)
            {
                temp.Insert(0, NameKr);
            }
            temp.Insert(0, NameEn);
            return temp;
        }

        [Ignore]
        public Color CategoryColor {
            get
            {
                switch(Category)
                {
                    case DerpTagCategory.ARTIST:
                        return Color.FromHex("#393f85");
                    case DerpTagCategory.CHARACTER:
                        return Color.FromHex("#2d8677");
                    case DerpTagCategory.CONTENTFANMADE:
                        return Color.FromHex("#bb5496");
                    case DerpTagCategory.CONTENTOFFICIAL:
                        return Color.FromHex("#998e1a");
                    case DerpTagCategory.RATING:
                        return Color.FromHex("#267ead");
                    case DerpTagCategory.SPOILER:
                        return Color.FromHex("#c24523");
                    case DerpTagCategory.ORIGIN:
                        return Color.FromHex("#393f85");
                    case DerpTagCategory.OC:
                        return Color.FromHex("#9852a3");
                    case DerpTagCategory.ERROR:
                        return Color.FromHex("#ad263f");
                    case DerpTagCategory.NONE:
                        return Color.FromHex("#6f8f0e");
                    default:
                        return Color.Gray;
                }
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

        public bool Contains(string key)
        {
            if(key.Length > 1)
                return NameEn.ToLower().Contains(key.ToLower()) || NameKr.ToLower().Contains(key.ToLower()) || Synonym.ToLower().Contains(key.ToLower());
            else
                return NameEn.ToLower().StartsWith(key.ToLower()) || NameKr.ToLower().StartsWith(key.ToLower()) || Synonym.ToLower().StartsWith(key.ToLower());

        }
        
        public DerpTag (DerpTag org)
        {
            NameEn = org.NameEn;
            NameKr = org.NameKr;
            DescriptionEn = org.DescriptionEn;
            DescriptionKr = org.DescriptionKr;
            Synonym = org.Synonym;
            Index = org.Index;
            Category = org.Category;
    }

        public bool Equals(DerpTag other)
        {
            return (Id.Equals(other.Id));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;
            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
