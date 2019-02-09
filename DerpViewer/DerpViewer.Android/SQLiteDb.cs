using System;
using System.Linq;
using Xamarin.Forms;
using DerpViewer.Droid;
using SQLite;
using System.Reflection;
using Java.IO;
using System.IO;
using System.Threading.Tasks;

[assembly: Dependency(typeof(SQLiteDb))]

namespace DerpViewer.Droid
{
    public class SQLiteDb : ISQLiteDb, IMedia
    {
        DirectoryInfo directory;

        public SQLiteAsyncConnection GetConnection(string name)
        {
            Directory.CreateDirectory(GetDocumentsPath());
            var path = Path.Combine(GetDocumentsPath(), name);
            return new SQLiteAsyncConnection(path);

        }

        public string GetDocumentsPath()
        {
            var documentsPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            var path = System.IO.Path.Combine(documentsPath, "DerpViewer");
            return path;
        }

        public async Task<string> CreateDirectory(string folderName)
        {
            directory = Directory.CreateDirectory(GetDocumentsPath());
            return directory.FullName;
        }

        public async Task<Stream> GetNewFileStream(string fileName)
        {
            string dir = Path.Combine(directory.FullName, fileName);
            return System.IO.File.OpenWrite(dir);
        }

        public void UpdateGallery(string path)
        {
            Android.Content.Intent mediaScanIntent = new Android.Content.Intent(Android.Content.Intent.ActionMediaScannerScanFile);
            Java.IO.File file = new Java.IO.File(path);
            Android.Net.Uri contentUri = Android.Net.Uri.FromFile(file);
            mediaScanIntent.SetData(contentUri);
            Android.App.Application.Context.SendBroadcast(mediaScanIntent);
        }

        private static void CopyDatabaseIfNotExists(string dbPath)
        {
            if (!System.IO.File.Exists(dbPath))
            {
                using (var br = new BinaryReader(Android.App.Application.Context.Assets.Open("DerpTag.db3")))
                {
                    using (var bw = new BinaryWriter(new FileStream(dbPath, FileMode.Create)))
                    {
                        byte[] buffer = new byte[2048];
                        int length = 0;
                        while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, length);
                        }
                    }
                }
            }
        }

        public class ExternalSdStorageHelper
        {

            /// <summary>
            /// Remember to turn on the READ_EXTERNAL_STORAGE permission, or this just comes back empty
            /// Tries to establish whether there's an external SD card present. It's
            /// a little hacky; reads /proc/mounts and looks for /storage/ references,
            /// and iterates over those looking for things like 'ext' and 'sdcard' in
            /// the same line, e.g. /storage/extSdCard or /storage/externalSd or similar.
            /// For the moment, the existence of 'ext' as part of the path (not the file system type) 
            /// is a crucial flag. If it doesn't see that, it'll bail out and assume there 
            /// isn't one (even if there is and it's  named something else). 
            /// We may have to build a list over time. 
            /// Returns: The root of the mounted directory (with no trailing '/', or empty string if there isn't one)
            /// </summary>
            /// <returns>The root of the mounted directory (with NO trailing '/'), or empty string if there isn't one)</returns>
            public static string GetExternalSdCardPath()
            {
                string procMounts = ReadProcMounts();
                string sdCardEntry = ParseProcMounts(procMounts);

                // note that IsWritable may fail if the disk is mounted elsewhere, e.g. MTP to PC
                if (!string.IsNullOrWhiteSpace(sdCardEntry))
                {
                    return sdCardEntry;
                }
                return string.Empty;
            }

            /// <summary>
            /// Just returns the contents of /proc/mounts as a string.
            /// Note that you MAY need to wrap this call up in a try/catch if you
            /// anticipate permissions issues, but generally just reading from 
            /// this file is OK
            /// </summary>
            /// <returns></returns>
            public static string GetProcMountsContents()
            {
                return ReadProcMounts();
            }

            /// <summary>
            /// This is an expensive operation to call, because it physically tries to write to the media.
            /// Remember to turn on the WRITE_EXTERNAL_STORAGE permission, or this will always return false.
            /// </summary>
            /// <param name="pathToTest">The root path of the alleged SD card (e.g. /storage/externalSd), 
            /// or anywhere else you want to test (WITHOUT the trailing '/'). If you try to write to somewhere 
            /// you're not allowed to, you may get eaten by a dragon.</param>
            /// <returns>True if it could write to it, false if not</returns>
            public static bool IsWritable(string pathToTest)
            {
                bool result = false;

                if (!string.IsNullOrWhiteSpace(pathToTest))
                {
                    const string someTestText = "some test text";
                    try
                    {
                        string testFile = string.Format("{0}/{1}.txt", pathToTest, Guid.NewGuid());
                        Android.Util.Log.Info("ExternalSDStorageHelper", "Trying to write some test data to {0}", testFile);
                        System.IO.File.WriteAllText(testFile, someTestText);
                        Android.Util.Log.Info("ExternalSDStorageHelper", "Success writing some test data to {0}!", testFile);
                        System.IO.File.Delete(testFile);
                        Android.Util.Log.Info("ExternalSDStorageHelper", "Cleaned up test data file {0}", testFile);
                        result = true;
                    }
                    catch (Exception ex) // shut up about it and move on, we obviously can't have it, so it's dead to us, we can't use it.
                    {
                        Android.Util.Log.Error("ExternalSDStorageHelper", string.Format("Exception: {0}\r\nMessage: {1}\r\nStack Trace: {2}", ex, ex.Message, ex.StackTrace));
                    }
                }
                return result;
            }

            /// <summary>
            /// example entries from /proc/mounts on a Samsung Galaxy S2 looks like:
            /// dev/block/dm-1 /mnt/asec/com.touchtype.swiftkey-2 ext4 ro,dirsync,nosuid,nodev,blah
            /// dev/block/dm-2 /mnt/asec/com.mobisystems.editor.office_registered-2 ext4 ro,dirsync,nosuid, blah
            /// dev/block/vold/259:3 /storage/sdcard0 vfat rw,dirsync, blah (this is NOT an external SD card)
            /// dev/block/vold/179:9 /storage/extSdCard vfat rw,dirsync,nosuid, blah (this IS an external SD card)
            /// </summary>
            /// <param name="procMounts"></param>
            /// <returns></returns>
            private static string ParseProcMounts(string procMounts)
            {
                string sdCardEntry = string.Empty;
                if (!string.IsNullOrWhiteSpace(procMounts))
                {
                    var candidateProcMountEntries = procMounts.Split('\n', '\r').ToList();
                    candidateProcMountEntries.RemoveAll(s => s.IndexOf("storage", StringComparison.OrdinalIgnoreCase) < 0);
                    var bestCandidate = candidateProcMountEntries
                      .FirstOrDefault(s => s.IndexOf("ext", StringComparison.OrdinalIgnoreCase) >= 0
                                           && s.IndexOf("sd", StringComparison.OrdinalIgnoreCase) >= 0
                                           && s.IndexOf("fat", StringComparison.OrdinalIgnoreCase) >= 0); // you can have things like fat, vfat, exfat, texfat, etc.

                    // e.g. /dev/block/vold/179:9 /storage/extSdCard vfat rw,dirsync,nosuid, blah
                    if (!string.IsNullOrWhiteSpace(bestCandidate))
                    {
                        var sdCardEntries = bestCandidate.Split(' ');
                        sdCardEntry = sdCardEntries.FirstOrDefault(s => s.IndexOf("/storage/", System.StringComparison.OrdinalIgnoreCase) >= 0);
                        return !string.IsNullOrWhiteSpace(sdCardEntry) ? string.Format("{0}", sdCardEntry) : string.Empty;
                    }
                }
                return sdCardEntry;
            }

            /// <summary>
            /// This doesn't require you to add any permissions in your Manifest.xml, but you'll
            /// need to add READ_EXTERNAL_STORAGE at the very least to be able to determine if the external
            /// SD card is available and usable.
            /// </summary>
            /// <returns></returns>
            private static string ReadProcMounts()
            {
                Android.Util.Log.Info("ExternalSDStorageHelper", "Attempting to read '/proc/mounts' to see if there's an external SD card reference");
                try
                {
                    string contents = System.IO.File.ReadAllText("/proc/mounts");
                    return contents;
                }
                catch (Exception ex) // shut up about it and move on, we obviously can't have it, we can't use it.
                {
                    Android.Util.Log.Error("ExternalSDStorageHelper", string.Format("Exception: {0}\r\nMessage: {1}\r\nStack Trace: {2}", ex, ex.Message, ex.StackTrace));
                }

                return string.Empty; // expect to fail by default
            }            
        }
    }
}

