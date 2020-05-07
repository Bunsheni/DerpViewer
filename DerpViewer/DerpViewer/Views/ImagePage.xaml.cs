using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using FFImageLoading;
using DerpViewer.Models;

namespace DerpViewer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ImagePage : ContentPage
    {
        private bool flag;
        private DerpImage image;

        public ImagePage (DerpImage image, ImageSource loadingimage)
		{
			InitializeComponent ();
            this.image = image;
            if (image.Width < image.Height)
            {
                imageStack.Orientation = StackOrientation.Vertical;
            }
            else
            {
                imageStack.Orientation = StackOrientation.Horizontal;
            }
            imageView.LoadingPlaceholder = loadingimage;
            imageView.Source = image.ImageUrl;
            BindingContext = this;
            textView.Text = image.Discription == null || image.Discription.Length == 0 ? "No Description" : image.Discription;
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            if (!flag)
            {
                flag = true;
                //await Navigation.PopModalAsync();
                if (imageScroll.IsVisible)
                {
                    imageView.Source = imageView2.Source;
                    normalView.IsVisible = true;
                    imageScroll.IsVisible = false;
                }
                else
                {
                    imageView2.Source = imageView.Source;
                    imageView2.WidthRequest = image.StaticWidth;
                    imageView2.HeightRequest = image.StaticHeight;
                    imageScroll.IsVisible = true;
                    normalView.IsVisible = false;
                }
                flag = false;
            }
        }

        private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
        {
            if (e.Direction == SwipeDirection.Up)
            {
                textView.IsVisible = true;
                imageView.Opacity = 0.3;
                imageView.Aspect = Aspect.AspectFill;
            }
            else if (e.Direction == SwipeDirection.Down)
            {
                textView.IsVisible = false;
                imageView.Opacity = 1;
                imageView.Aspect = Aspect.AspectFit;
            }
        }

        private void ImageScroll_SizeChanged(object sender, EventArgs e)
        {
            if (image.AspectRatio < 1)
            {
                DerpImage.staticWidth = imageScroll.Width;
                DerpImage.staticHeight = 0;
            }
            else
            {
                DerpImage.staticHeight = imageScroll.Height;
                DerpImage.staticWidth = 0;
            }
        }
    }

    public class PinchToZoomContainer : ContentView
    {
        double currentScale = 1;
        double startScale = 1;
        double xOffset = 0;
        double yOffset = 0;

        public PinchToZoomContainer()
        {
            //var pinchGesture = new PinchGestureRecognizer();
            //pinchGesture.PinchUpdated += OnPinchUpdated;
            //GestureRecognizers.Add(pinchGesture);
        }

        void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                // Store the current scale factor applied to the wrapped user interface element,
                // and zero the components for the center point of the translate transform.
                startScale = Content.Scale;
                Content.AnchorX = 0;
                Content.AnchorY = 0;
            }
            if (e.Status == GestureStatus.Running)
            {
                // Calculate the scale factor to be applied.
                currentScale += (e.Scale - 1) * startScale;
                currentScale = Math.Max(1, currentScale);

                // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                // so get the X pixel coordinate.
                double renderedX = Content.X + xOffset;
                double deltaX = renderedX / Width;
                double deltaWidth = Width / (Content.Width * startScale);
                double originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

                // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                // so get the Y pixel coordinate.
                double renderedY = Content.Y + yOffset;
                double deltaY = renderedY / Height;
                double deltaHeight = Height / (Content.Height * startScale);
                double originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

                // Calculate the transformed element pixel coordinates.
                double targetX = xOffset - (originX * Content.Width) * (currentScale - startScale);
                double targetY = yOffset - (originY * Content.Height) * (currentScale - startScale);

                // Apply translation based on the change in origin.
                Content.TranslationX = targetX.Clamp(-Content.Width * (currentScale - 1), 0);
                Content.TranslationY = targetY.Clamp(-Content.Height * (currentScale - 1), 0);

                // Apply scale factor
                Content.Scale = currentScale;
                Console.WriteLine(currentScale);
            }
            if (e.Status == GestureStatus.Completed)
            {
                // Store the translation delta's of the wrapped user interface element.
                xOffset = Content.TranslationX;
                yOffset = Content.TranslationY;
            }
        }
    }

    public static class DoubleExtensions
    {
        public static double Clamp(this double self, double min, double max)
        {
            return Math.Min(max, Math.Max(self, min));
        }
    }
}