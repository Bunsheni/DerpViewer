using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DerpViewer.Views
{
    public class CustomWebView : WebView
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
        private async Task WaitAsync()
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
                while(true)
                {
                    await WaitAsync();
                    string temp = (await EvaluateJavaScriptAsync("document.documentElement.outerHTML;")).Replace("\\u003C", "<").Replace("\\\"", "\"").Replace("\\n", "\n");
                    if (temp.Contains("target")) break;
                }
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
                Console.WriteLine("번역기 서버가 불안정합니다.", "알림");
            }
            if (res.Length == 0)
                res = text;

            return res;
        }
    }


}
