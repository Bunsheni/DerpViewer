using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using Android;
using Android.Support.Design.Widget;
using Android.Content;
using Android.Support.V4.Content;
using System.Threading;

namespace DerpViewer.Droid
{
    [Activity(Label = "DerpViewer", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private string[] permissions = { Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage, Manifest.Permission.Internet };//권한 설정 변수
        private readonly static int MULTIPLE_PERMISSIONS = 101;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted ||
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted ||
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.Internet) != (int)Permission.Granted
                )
            {
                ActivityCompat.RequestPermissions(this, permissions, MULTIPLE_PERMISSIONS);
                while (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted ||
                    ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted ||
                    ContextCompat.CheckSelfPermission(this, Manifest.Permission.Internet) != (int)Permission.Granted
                    ) ;
            }

            //added
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(enableFastRenderer: true);
            var config = new FFImageLoading.Config.Configuration()
            {
                VerboseLogging = false,
                VerbosePerformanceLogging = false,
                VerboseMemoryCacheLogging = false,
                VerboseLoadingCancelledLogging = false,
                FadeAnimationForCachedImages = false,
                MaxMemoryCacheSize = 2000000000,
                FadeAnimationDuration = 500
            };
            FFImageLoading.ImageService.Instance.Initialize(config);

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        public override void OnTrimMemory([GeneratedEnum] TrimMemory level)
        {
            FFImageLoading.ImageService.Instance.InvalidateMemoryCache();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            base.OnTrimMemory(level);
        }
    }
}