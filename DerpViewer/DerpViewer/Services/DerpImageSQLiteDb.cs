using DerpViewer.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DerpViewer.Services
{
    public class DerpImageSQLiteDb
    {
        private SQLiteAsyncConnection _connection;
        public AutoResetEvent _waitforListComplete;
        AsyncLock myLock = new AsyncLock();
        public bool IsLoaded;

        private List<DerpImage> _derpImage;

        public DerpImageSQLiteDb()
        {
            _waitforListComplete = new AutoResetEvent(false);
        }

        public SQLiteAsyncConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = DependencyService.Get<ISQLiteDb>().GetConnection("DerpImage.db3");                
            }
            return _connection;
        }

        public void Close()
        {
            if(_connection != null)
            {
                _connection.GetConnection().Close();
                _connection.GetConnection().Dispose();
                SQLiteAsyncConnection.ResetPool();
                _connection = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public async Task CreatDerpImageInfoTable()
        {
            try
            {
                await GetConnection().CreateTableAsync<DerpImage>();
                await GetConnection().CreateTableAsync<ImageArray>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task DropDerpImageTable()
        {
            await GetConnection().DropTableAsync<DerpImage>();
        }

        private List<DerpImage> _derpImages;
        private List<ImageArray> _imageArray;
        public async Task Load()
        {
            IsLoaded = true;
            _waitforListComplete.Reset();

            await CreatDerpImageInfoTable();
            _derpImages = (await GetConnection().Table<DerpImage>().ToListAsync()).ToList();
            _waitforListComplete.Set();

            if (_derpImages == null)
            {
                IsLoaded = false;
            }
        }

        public async Task<List<DerpImage>> GetDerpImagesAsync()
        {
            using (var releaser = await myLock.LockAsync())
            {
                if (_derpImages == null)
                {
                    await Task.Run(() => { _waitforListComplete.WaitOne(); });
                }
                return _derpImages;
            }
        }

        public async Task<DerpImage> GetDerpImageAsync(string workid)
        {
            return (await GetDerpImagesAsync()).Find(i => i.Id == workid);
        }
        
        public async Task<ImageArray> GetImageArrayAsync(string workid)
        {
            return await GetConnection().Table<ImageArray>().Where(i => i.Id == workid).FirstOrDefaultAsync();
        }

        public async Task InsertImageArrayAsync(ImageArray image)
        {
            await DeleteImageArrayAsync(image.Id);
            await GetConnection().InsertAsync(image);
        }

        public async Task InsertDerpImageAsync(DerpImage image)
        {
            _derpImage = (await GetConnection().Table<DerpImage>().ToListAsync()).ToList();
            _derpImage.Add(image);
            await DeleteDerpImageAsync(image.Id);
            await GetConnection().InsertAsync(image);
        }

        public async Task DeleteDerpImageAsync(string workid)
        {
            DerpImage image = await GetConnection().Table<DerpImage>().Where(i => i.Id == workid).FirstOrDefaultAsync();
            if (image != null)
                await GetConnection().DeleteAsync(image);
        }

        public async Task DeleteImageArrayAsync(string workid)
        {
            ImageArray image = await GetConnection().Table<ImageArray>().Where(i => i.Id == workid).FirstOrDefaultAsync();
            if(image != null)
                await GetConnection().DeleteAsync(image);
        }

        public async Task DeleteInfoAsync(string workid)
        {
            DerpImage image = await GetConnection().Table<DerpImage>().Where(i => i.Id == workid).FirstOrDefaultAsync();
            if (image != null)
                await GetConnection().DeleteAsync(image);
        }

        public async Task UpdateImageAsync(DerpImage tag)
        {
            using (var releaser = await myLock.LockAsync())
            {
                if (tag != _derpImage.Find(i => i.Id == tag.Id))
                {
                    int i = _derpImage.FindIndex(j => j.Id == tag.Id);
                    _derpImage[i] = tag;
                }
                await GetConnection().UpdateAsync(tag);
            }
        }
    }
}
