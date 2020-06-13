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
            string str = Path.Combine(path.Replace("/", "\\").Split('\\'));
            return await DependencyService.Get<ISQLiteDb>().GetSubList(str);
        }

        public async Task<bool> MoveDirectory(string src, string dest)
        {
            return await DependencyService.Get<ISQLiteDb>().MoveDirectory(src, dest);
        }
        public async Task<bool> MoveFile(string src, string dest)
        {
            return await DependencyService.Get<ISQLiteDb>().MoveFile(src, dest);
        }

        public async Task<Stream> GetNewFileStream(string name)
        {
            return await DependencyService.Get<ISQLiteDb>().GetNewFileStream(name);
        }

        public async Task<Stream> GetReadFileStream(string name)
        {
            return await DependencyService.Get<ISQLiteDb>().GetReadFileStream(name);
        }

        public async Task<List<CtFileItem>> GetSubFiles(string directoryName)
        {
            var items = await GetSubList(directoryName);
            var files = items.FindAll(i => !i.IsDirectory);
            var directories = items.FindAll(i => i.IsDirectory);
            var allfiles = new List<CtFileItem>(files);

            foreach (var dir in directories)
            {
                var tmp = await GetSubFiles(dir.ShortName);
                allfiles.AddRange(tmp);
            }
            return allfiles;
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
        public bool IsSelected { get; set; }
        public string Type
        {
            get
            {
                if (Name == "/")
                    return string.Empty;
                else
                    return IsDirectory ? "폴더" : "파일";
            }
        }
        public string CreationTimeStr
        {
            get
            {
                if (Name == "/")
                    return string.Empty;
                else
                    return CreationTime.ToString();
            }
        }
        public string LengthStr
        {
            get
            {
                if (Name == "/" || IsDirectory)
                    return string.Empty;
                else
                    return Library.ByteUnitTransform(Length);
            }
        }

        public string Thumbnail
        {
            get
            {
                return IsDirectory ? "folder.png" : "file.png";
            }
        }
    }
}
