using DerpViewer.Services;
using SQLite;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DerpViewer
{
    public interface ISQLiteDb
    {
        SQLiteAsyncConnection GetConnection(string name);
        string GetDocumentsPath();
        Task<string> CreateDirectory(string folderName);
        Task<List<CtFileItem>> GetSubList(string name);
        Task<Stream> GetNewFileStream(string fileName);
        Task<Stream> GetReadFileStream(string fileName);
    }
}
