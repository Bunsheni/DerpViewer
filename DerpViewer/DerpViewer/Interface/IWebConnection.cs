using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DerpViewer
{
    public interface IWebConnection
    {
        Task<string> GetWebClintContentsAsync(string url);
        Task<string> TransWebBrowserInitAsync(string text, string from, string to);
        Task<string> SearchImage(string image);
    }
}
