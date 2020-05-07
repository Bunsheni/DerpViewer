using System;
using System.Collections.Generic;
using System.Text;

namespace DerpViewer.Models
{
    public class DerpSuggestionItem
    {
        public string Key { get; set; }
        public DerpTag Tag { get; set; }
        public string Text
        {
            get
            {
                if (Tag.Name == Key)
                    return Tag.Name;
                else
                    return Tag.Name + "(" + Key + ")";
            }
        }

        public DerpSuggestionItem(string key, DerpTag obj)
        {
            Key = key;
            Tag = obj;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
