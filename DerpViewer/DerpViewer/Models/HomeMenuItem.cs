using System;
using System.Collections.Generic;
using System.Text;

namespace DerpViewer.Models
{
    public enum MenuItemType
    {
        ImageBrowser,
        FavoriteBrowser,
        TagBrowser,
        FileBrowser,
        User,
        About
    }
    public class HomeMenuItem
    {
        public MenuItemType Id { get; set; }

        public string Title { get; set; }
    }
}
