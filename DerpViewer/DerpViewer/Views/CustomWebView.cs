using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DerpViewer.Views
{
    public class CustomWebView : WebView, IWebConnection
    {
        AutoResetEvent waitForNavComplete;
        public CustomWebView()
        {
            this.Navigated += WebView_Navigated;
            waitForNavComplete = new AutoResetEvent(false);
        }
        private void WebView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            waitForNavComplete.Set();
        }
        public async Task WaitAsync()
        {
            await Task.Run(() => { waitForNavComplete.WaitOne(); });
            waitForNavComplete.Reset();
        }

        public async Task<string> GetWebClintContentsAsync(string url)
        {
            string res = string.Empty;
            try
            {
                Source = url;
                await WaitAsync();
                res = (await EvaluateJavaScriptAsync("document.documentElement.outerHTML;")).Replace("\\u003C", "<").Replace("\\\"", "\"").Replace("\\n", "\n");
                return res;

            }
            catch
            {
                Console.WriteLine("번역기 서버가 불안정합니다.", "알림");
            }
            return res;
        }

        public async Task<string> TransWebBrowserInitAsync(string text, string from, string to)
        {
            string res = string.Empty;
            try
            {
                Source = "http://text-to-speech-translator.paralink.com/default.asp";
                await WaitAsync();
                await EvaluateJavaScriptAsync(string.Format("document.getElementById(\"{0}\").value = \"{1}\"", "langs1", from));
                await EvaluateJavaScriptAsync(string.Format("DDMENU(0)"));
                await EvaluateJavaScriptAsync(string.Format("document.getElementById(\"{0}\").value = \"{1}\"", "langs2", to));
                await EvaluateJavaScriptAsync(string.Format("DDMENU(1)"));
                await EvaluateJavaScriptAsync("LABLE('google');");
                await EvaluateJavaScriptAsync(string.Format("document.getElementById(\"{0}\").value = \"{1}\"", "source", text));
                await EvaluateJavaScriptAsync("translateTEXTonload(GEBI('source').value);");
                await WaitAsync();
                res = await EvaluateJavaScriptAsync("document.getElementById(\"target\").value;");

            }
            catch
            {
                Console.WriteLine("WebView Fail.", "Notice");
            }
            if (res.Length == 0)
                res = text;
            return res;
        }


        public async Task<string> SearchImage(string image)
        {
            Source = "https://derpibooru.org/search/reverse";
            await WaitAsync();


            await EvaluateJavaScriptAsync(string.Format("document.getElementById(\"{0}\").click()", "image"));

            //var url = "https://derpicdn.net/img/view/2012/1/2/0.jpg";
            //await EvaluateJavaScriptAsync(string.Format("document.getElementById(\"{0}\").value = \"{1}\"", "scraper_url", url));
            //await EvaluateJavaScriptAsync(string.Format("document.getElementById(\"{0}\").click()", "js-scraper-preview"));
            ////await WaitAsync();

            //var d = await EvaluateJavaScriptAsync(string.Format("document.getElementById(\"{0}\")[0]", "js-image-upload-previews"));
            //var s = await EvaluateJavaScriptAsync(string.Format("document.getElementsByClassName(\"{0}\")[0].getAttribute(\"src\")", "scraper-preview--image"));
            //var a = string.Format("document.getElementsByClassName(\"{0}\")[0].setAttribute(\"src\", \"{1}\")", "scraper-preview--image", image);
            //await EvaluateJavaScriptAsync(a);

            //string a = string.Format("var image = document.createElement(\"img\"); image.src = \"{0}\"; document.getElementById(\"image\").files.appendChild(image);", path);

            //a = "document.querySelector('[type=file]').files;";
            //await EvaluateJavaScriptAsync(string.Format("document.getElementById(\"{0}\").click()", "image"));
            //await WaitAsync();

            await EvaluateJavaScriptAsync(string.Format("document.getElementsByName(\"{0}\")[0].click()", "commit"));
            await WaitAsync();
            return "";
        }

    }


}
