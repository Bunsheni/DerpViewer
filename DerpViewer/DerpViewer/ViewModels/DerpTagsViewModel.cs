using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using DerpViewer.Models;

namespace DerpViewer.ViewModels
{
    class DerpTagsViewModel : BaseViewModel
    {
        string key = string.Empty;       
        
        public string Key { get { return key; } set { key = value; OnPropertyChanged(); } }
    }
}
