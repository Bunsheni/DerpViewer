using System;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using Xamarin.Forms;
using Windows.Storage;
using DerpViewer.Windows;
using Windows.Storage.Streams;
using System.Collections.Generic;
using DerpViewer.Services;

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
            StorageFolder = await KnownFolders.PicturesLibrary.TryGetItemAsync(folderName) as StorageFolder;
            if (StorageFolder == null)
                StorageFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync(folderName);
            return StorageFolder.Path;
        }

        public async Task<List<CtFileItem>> GetSubList(string name)
        {
            StorageFolder folder = name.Length == 0 ? StorageFolder : await StorageFolder.GetFolderAsync(name);
            if (folder != null)
            {
                List<CtFileItem> items = new List<CtFileItem>();
                foreach (IStorageItem item in await folder.GetItemsAsync())
                {
                    CtFileItem fitem = new CtFileItem();
                    fitem.Name = item.Name;
                    fitem.FullName = Path.Combine(StorageFolder.Path, name, item.Name);
                    if (item.IsOfType(StorageItemTypes.File))
                    {
                        fitem.IsDirectory = false;
                    }
                    else
                    {
                        fitem.IsDirectory = true;
                    }
                    items.Add(fitem);
                }
                return items;
            }
            return null;
        }

        public async Task<Stream> GetReadFileStream(string name)
        {
            StorageFile newFile;
            var temp1 = await StorageFolder.TryGetItemAsync(name);
            if (temp1 == null)
            {
                return null;
            }
            else
            {
                newFile = await StorageFolder.GetFileAsync(name);
            }
            return await newFile.OpenStreamForReadAsync();
        }

        public async Task<Stream> GetNewFileStream(string fileName)
        {
            StorageFile newFile;
            var temp1 = await StorageFolder.TryGetItemAsync(fileName);
            if (temp1 == null)
            {
                newFile = await StorageFolder.CreateFileAsync(fileName);
            }
            else
            {
                newFile = await StorageFolder.GetFileAsync(fileName);
            }
            return await newFile.OpenStreamForWriteAsync();
        }

    }
}

