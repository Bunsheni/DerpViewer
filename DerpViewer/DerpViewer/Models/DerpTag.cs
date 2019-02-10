using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace DerpViewer.Models
{
    public enum DerpTagCategory { NONE, RATING, ORIGIN, SPOILER, CONTENTOFFICIAL, CONTENTFANMADE, ARTIST, CHARACTER, OC, ERROR, UNKNOWN  }
    public class DerpTag
    {
        private static readonly string[] _caten = { string.Empty, "rating", "origin", "spoiler", "content-official", "content-fanmade", "artist", "character", "oc", "error" };
        private static readonly string[] _catkr = { string.Empty, "등급", "origin", "spoiler", "content-official", "content-fanmade", "artist", "character", "oc", "error" };
        [PrimaryKey]
        public string Id { get; set; }
        public DerpTagCategory Category { get; set; }
        public string NameEn { get; set; }
        public string NameKr { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionKr { get; set; }
        [Ignore]
        public string Description
        {
            get
            {
                if (DescriptionKr.Length == 0)
                    return "No Description";
                else
                    return DescriptionKr;
            }
        }
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
        }

        public DerpTag(string name)
        {
            Id = string.Empty;
            NameEn = NameKr = name;
            DescriptionEn = DescriptionKr = string.Empty;
            Category = DerpTagCategory.UNKNOWN;
        }
    }
}
