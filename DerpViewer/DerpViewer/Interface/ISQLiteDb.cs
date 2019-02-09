using SQLite;
using System.IO;
using System.Threading.Tasks;

namespace DerpViewer
{
    public interface ISQLiteDb
    {
        SQLiteAsyncConnection GetConnection(string name);
        string GetDocumentsPath();
        Task<string> CreateDirectory(string folderName);
        Task<Stream> GetNewFileStream(string fileName);
    }
}
