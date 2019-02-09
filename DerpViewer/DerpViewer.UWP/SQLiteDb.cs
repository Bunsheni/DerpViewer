using System;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using Xamarin.Forms;
using Windows.Storage;
using DerpViewer.Windows;
using Windows.Storage.Streams;

[assembly: Dependency(typeof(SQLiteDb))]
namespace DerpViewer.Windows
{
    public class SQLiteDb : ISQLiteDb
    {
        StorageFolder StorageFolder;

        public SQLiteAsyncConnection GetConnection(string name)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        	var path = Path.Combine(localFolder.Path, name);
            return new SQLiteAsyncConnection(path);
        }

        public string GetDocumentsPath()
        {
            StorageFolder storageFolder = KnownFolders.PicturesLibrary;
            return storageFolder.Path + "\\" + "DerpViewer";
        }

        public async Task<string> CreateDirectory(string folderName)
        {
            StorageFolder = await KnownFolders.PicturesLibrary.GetFolderAsync(folderName);
            if (StorageFolder == null)
                StorageFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync(folderName);
            return StorageFolder.Path;
        }
        
        public async Task<Stream> GetNewFileStream(string fileName)
        {
            StorageFile newFile = await StorageFolder.CreateFileAsync(fileName);
            return await newFile.OpenStreamForWriteAsync();
        }

    }
}

