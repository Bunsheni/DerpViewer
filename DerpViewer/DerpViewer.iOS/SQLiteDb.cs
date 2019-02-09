using System;
using System.IO;
using SQLite;
using Xamarin.Forms;
using DerpViewer.iOS;
using System.Threading.Tasks;

[assembly: Dependency(typeof(SQLiteDb))]
namespace DerpViewer.iOS
{
    public class SQLiteDb : ISQLiteDb
    {
        DirectoryInfo directory;

        public SQLiteAsyncConnection GetConnection(string name)
        {
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); 
            var path = Path.Combine(documentsPath, name);
            return new SQLiteAsyncConnection(path);
        }

        public string GetDocumentsPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        public async Task<string> CreateDirectory(string folderName)
        {
            string dir = Path.Combine(GetDocumentsPath(), folderName);
            directory = Directory.CreateDirectory(dir);
            return directory.FullName;
        }

        public async Task<Stream> GetNewFileStream(string fileName)
        {
            string dir = Path.Combine(directory.FullName, fileName);
            return File.OpenWrite(dir);
        }
    }
}

