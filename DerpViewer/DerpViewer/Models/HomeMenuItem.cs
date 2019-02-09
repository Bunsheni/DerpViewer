using System;
using System.Collections.Generic;
using System.Text;

namespace DerpViewer.Models
{
    public enum MenuItemType
    {
        ImageBrowser,
        TagBrowse,
        User,
        About
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }
    }
}
