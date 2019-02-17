using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DerpViewer.Views;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace DerpViewer
{
    public partial class App : Application
    {
        private const string UserAPIKeyKey = "UserAPIKey";
        private const string UsingFilterKey = "UsingFilter";
        private const string HideTopbarKey = "HideTopbar";
        private const string TagCountKey = "TagCount";

        public string UserAPIKey
        {
            get
            {
                if (Properties.ContainsKey(UserAPIKeyKey) && Properties[UserAPIKeyKey] != null)
                {
                    return Properties[UserAPIKeyKey].ToString();
                }
                else
                {
                    Properties[UserAPIKeyKey] = string.Empty;
                    SavePropertiesAsync();
                    return Properties[UserAPIKeyKey].ToString();
                }
            }
            set
            {
                Properties[UserAPIKeyKey] = value;
                SavePropertiesAsync();
            }
        }

        public bool UsingFilter
        {
            get
            {
                if (Properties.ContainsKey(UsingFilterKey)
                    && Properties[UsingFilterKey] != null)
                    return (bool)Properties[UsingFilterKey];
                else
                    return false;
            }
            set
            {
                Properties[UsingFilterKey] = value;
                SavePropertiesAsync();
            }
        }
        public bool HideTopbar
        {
            get
            {
                if (Properties.ContainsKey(HideTopbarKey)
                    && Properties[HideTopbarKey] != null)
                    return (bool)Properties[HideTopbarKey];
                else
                    return false;
            }
            set
            {
                Properties[HideTopbarKey] = value;
                SavePropertiesAsync();
            }
        }
        public int TagCount
        {
            get
            {
                if (Properties.ContainsKey(TagCountKey)
                    && Properties[TagCountKey] != null)
                    return (int)Properties[TagCountKey];
                else
                    return 10000;
            }
            set
            {
                Properties[TagCountKey] = value;
                SavePropertiesAsync();
            }
        }

        public App()
        {
            InitializeComponent();


            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            ((MainPage)MainPage).GetDerpSQLiteDb().Close();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
