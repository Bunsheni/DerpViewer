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
        /// <summary>
        /// 폴더를 생성하고 전체 경로를 반환
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        Task<string> CreateDirectory(string folderName);
        Task<List<CtFileItem>> GetSubList(string name);
        Task<Stream> GetNewFileStream(string fileName);
        Task<Stream> GetReadFileStream(string fileName);
    }
}
