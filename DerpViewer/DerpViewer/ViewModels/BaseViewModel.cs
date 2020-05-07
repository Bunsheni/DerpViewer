using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;

using DerpViewer.Models;
using DerpViewer.Services;
using DerpViewer.Views;

namespace DerpViewer.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public App RootApp { get => Application.Current as App; }
        public MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        public DerpTagSQLiteDb DerpTagDb { get => RootPage.GetDerpTagSQLiteDb(); }
        public DerpImageSQLiteDb DerpImageDb { get => RootPage.GetDerpImageSQLiteDb(); }

        bool isBusy = false;
        public string UserAPIKey
        {
            get
            {
                return RootApp.UsingFilter ? RootApp.UserAPIKey : string.Empty;
            }
        }
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
