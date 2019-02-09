using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DerpViewer.Services
{
    class DerpFileService
    {
        public string Path { get; set; }
        public DerpFileService()
        {
        }

        public async Task<string> CreateDirectory(string name)
        {
            return await DependencyService.Get<ISQLiteDb>().CreateDirectory(name);
        }

        public async Task<Stream> GetNewFileStream(string name)
        {
            return await DependencyService.Get<ISQLiteDb>().GetNewFileStream(name);
        }

    }
}
