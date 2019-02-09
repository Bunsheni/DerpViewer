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
        string key;
        
        private ObservableCollection<DerpTag> _tags;
        public ObservableCollection<DerpTag> Tags
        {
            get
            {
                return _tags;
            }
            set
            {
                _tags = value;
                OnPropertyChanged();
            }
        }
        
        public string Key { get { return key; } set { key = value; OnPropertyChanged(); } }

        public async Task Load()
        {
            if(Tags != null)
            {
                Tags.Clear();
                GC.Collect();
            }
            Tags = new ObservableCollection<DerpTag>(await RootPage.GetDerpSQLiteDb().GetTagsAsync());
        }
    }
}
