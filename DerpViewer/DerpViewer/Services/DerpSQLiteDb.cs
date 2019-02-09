using DerpViewer.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DerpViewer.Services
{
    public class DerpSQLiteDb
    {
        private SQLiteAsyncConnection _connection;
        public AutoResetEvent _waitforListComplete;
        AsyncLock myLock = new AsyncLock();
        public bool IsLoaded;

        private List<DerpTag> _derpTags;

        public DerpSQLiteDb()
        {
            _waitforListComplete = new AutoResetEvent(false);
        }

        public SQLiteAsyncConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = DependencyService.Get<ISQLiteDb>().GetConnection("DerpTag.db3");                
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

        public async Task CreatDerpTagTable()
        {
            try
            {
                await GetConnection().CreateTableAsync<DerpTag>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task DropDerpTagTable()
        {
            await GetConnection().DropTableAsync<DerpTag>();
        }
        
        public async Task Load()
        {
            IsLoaded = true;
            _waitforListComplete.Reset();

            await CreatDerpTagTable();
            _derpTags = (await GetConnection().Table<DerpTag>().ToListAsync()).ToList();

            IEnumerable<DerpTag> temps = _derpTags.FindAll(i => i.NameEn.StartsWith("spoiler:s")).OrderByDescending(i => i.NameEn);
            _derpTags.RemoveAll(i => i.NameEn.StartsWith("spoiler:s"));
            _derpTags.AddRange(temps);
            temps = _derpTags.FindAll(i => i.Category == DerpTagCategory.RATING);
            _derpTags.RemoveAll(i => i.Category == DerpTagCategory.RATING);
            _derpTags.InsertRange(0, temps);

            _waitforListComplete.Set();

            if (_derpTags == null)
            {
                IsLoaded = false;
            }
        }

        public async Task<List<DerpTag>> GetTagsAsync()
        {
            using (var releaser = await myLock.LockAsync())
            {
                if (_derpTags == null)
                {
                    await Task.Run(() => { _waitforListComplete.WaitOne(); });
                }
                return _derpTags;
            }
        }

        public async Task<DerpTag> GetTagAsync(string workid)
        {
            return (await GetTagsAsync()).Find(i => i.Id == workid);
        }

        public async Task<DerpTag> GetTagFromNameAsync(string workid)
        {
            return (await GetTagsAsync()).Find(i => i.NameEn == workid);
        }

        public async Task InsertTagAsync(DerpTag tag)
        {
            await GetConnection().InsertAsync(tag);
            (await GetTagsAsync()).Insert(0, tag);
        }

        public async Task AddTagsAsync(List<DerpTag> tags)
        {
            (await GetTagsAsync()).AddRange(tags);
            await GetConnection().InsertAllAsync(tags);
        }

        public async Task InsertTagsAsync(List<DerpTag> tags)
        {
            foreach(DerpTag tag in tags)
            {
                await GetConnection().InsertOrReplaceAsync(tag);
                (await GetTagsAsync()).Insert(0, tag);
            }
        }

        public async Task DeleteTagAsync(string workid)
        {
            DerpTag comic = _derpTags.Find(i => i.Id == workid);
            await GetConnection().DeleteAsync(comic);
            _derpTags.Remove(comic);
        }

        public async Task UpdateTagAsync(DerpTag tag)
        {
            using (var releaser = await myLock.LockAsync())
            {
                if (tag != _derpTags.Find(i => i.Id == tag.Id))
                {
                    int i = _derpTags.FindIndex(j => j.Id == tag.Id);
                    _derpTags[i] = tag;
                }
                await GetConnection().UpdateAsync(tag);
            }
        }

        public List<DerpTag> DerpTagSuggestion(string key)
        {
            if(_derpTags != null)
            {
                List<DerpTag> res;
                if(key.Length > 2)
                {
                    res =_derpTags.FindAll(i => i.Contain(key) || i.Suggestion(key));
                }
                else
                {
                    res = _derpTags.FindAll(i => i.Suggestion(key));
                }
                return res;
            }
            else
            {
                return new List<DerpTag>();
            }
        }
    }


    public class AsyncSemaphore
    {
        private readonly static Task s_completed = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> m_waiters = new Queue<TaskCompletionSource<bool>>();
        private int m_currentCount;

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0) throw new ArgumentOutOfRangeException("initialCount");
            m_currentCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (m_waiters)
            {
                if (m_currentCount > 0)
                {
                    --m_currentCount;
                    return s_completed;
                }
                else
                {
                    var waiter = new TaskCompletionSource<bool>();
                    m_waiters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (m_waiters)
            {
                if (m_waiters.Count > 0)
                    toRelease = m_waiters.Dequeue();
                else
                    ++m_currentCount;
            }
            if (toRelease != null)
                toRelease.SetResult(true);
        }
    }

    public class AsyncLock
    {
        private readonly AsyncSemaphore m_semaphore;
        private readonly Task<Releaser> m_releaser;

        public AsyncLock()
        {
            m_semaphore = new AsyncSemaphore(1);
            m_releaser = Task.FromResult(new Releaser(this));
        }
        public Task<Releaser> LockAsync()
        {
            var wait = m_semaphore.WaitAsync();
            return wait.IsCompleted ?
                m_releaser :
                wait.ContinueWith((_, state) => new Releaser((AsyncLock)state),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public struct Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;

            internal Releaser(AsyncLock toRelease) { m_toRelease = toRelease; }

            public void Dispose()
            {
                if (m_toRelease != null)
                    m_toRelease.m_semaphore.Release();
            }
        }
    }
}
