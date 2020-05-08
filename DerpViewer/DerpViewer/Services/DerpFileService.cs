using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DerpViewer.Services
{
    public class DerpFileService
    {
        public DerpFileService()
        {
        }

        public string GetBaseDirectory()
        {
            return DependencyService.Get<ISQLiteDb>().GetDocumentsPath();
        }

        public async Task<string> CreateDirectory(string name)
        {
            return await DependencyService.Get<ISQLiteDb>().CreateDirectory(name);
        }

        public async Task<List<CtFileItem>> GetSubList(string path)
        {
            string str = Path.Combine(path.Split('\\'));
            return await DependencyService.Get<ISQLiteDb>().GetSubList(str);
        }

        public async Task<Stream> GetNewFileStream(string name)
        {
            return await DependencyService.Get<ISQLiteDb>().GetNewFileStream(name);
        }

        public async Task<Stream> GetReadFileStream(string name)
        {
            return await DependencyService.Get<ISQLiteDb>().GetReadFileStream(name);
        }

    }
    public class CtFileItem
    {
        public CtFileItem()
        {
            Name = "/";
        }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public bool IsDirectory { get; set; }
        public long Length { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
