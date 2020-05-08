using DerpViewer.Models;
using DerpViewer.Services;
using DerpViewer.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DerpViewer.ViewModels
{
    public class FileBrowserViewModel : BaseViewModel
    {
        public string CurrentDirectory = string.Empty;

        private DerpFileService fileService = new DerpFileService();
        private ObservableCollection<CtFileItem> fileList = new ObservableCollection<CtFileItem>();

        public ObservableCollection<CtFileItem> FileList
        {
            get { return fileList; }
            set { SetProperty(ref fileList, value); }
        }

        public FileBrowserViewModel()
        {
        }

        public async Task UpdateDirectory()
        {
            FileList.Clear();
            if (CurrentDirectory.Length > 0)
            {
                FileList.Add(new CtFileItem());
            }
            await DisplayFiles();
        }

        public async Task BackDirectory()
        {
            CurrentDirectory = CurrentDirectory.Substring(0, CurrentDirectory.LastIndexOf('/'));
            FileList.Clear();
            if (CurrentDirectory.Length > 0)
            {
                FileList.Add(new CtFileItem());
            }
            await DisplayFiles();
        }

        public async Task MoveRootDirectory()
        {
            CurrentDirectory = string.Empty;
            FileList.Clear();
            await DisplayFiles();
        }

        public async Task MoveDirectory(string name)
        {
            if (name.Length == 0) return;
            CurrentDirectory = CurrentDirectory + '/' + name;
            FileList.Clear();
            FileList.Add(new CtFileItem());
            await DisplayFiles();
        }

        public bool ExistFile(CtFileItem fileItem)
        {
            var res = File.Exists(fileItem.FullName);
            return res;
        }

        public bool DeleteFile(CtFileItem fileItem)
        {
            try
            {
                File.Delete(fileItem.FullName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async void OpenFile(CtFileItem fileItem)
        {
            Stream stream = await fileService.GetReadFileStream(fileItem.ShortName);
            if (stream != null)
            {
                ImageSource source = ImageSource.FromStream(() => stream);
                if (source != null)
                    await CurrentPage.Navigation.PushAsync(new ImagePage(source));
            }
        }

        public async Task DisplayFiles()
        {
            List<CtFileItem> files = await fileService.GetSubList(CurrentDirectory.Trim('/').Replace("/", "\\"));
            foreach (CtFileItem item in files)
            {
                FileList.Add(item);
            }
        }

        public async Task ClassifyAll()
        {
            await DirectoryImageClassify(string.Empty);
            await MoveRootDirectory();
        }

        public async Task DirectoryImageClassify(string directoryName)
        {
            
            var items = await fileService.GetSubList(directoryName);
            var files = items.FindAll(i => !i.IsDirectory);
            var directories = items.FindAll(i => i.IsDirectory).OrderBy(j => j.Name.Length).ToArray();

            foreach (var file in files)
            {
                var i___ = file.Name.IndexOf("__");
                var i_dot = file.Name.LastIndexOf(".");
                if (0 < i___ && 0 <= i_dot && i___ < i_dot)
                {
                    var i_tagstrlen = i_dot - i___;
                    var str_tags = file.Name.Substring(i___ + 2, i_tagstrlen).Replace("+", " ").Replace("-colon-", "#");
                    var strl_tags = str_tags.Split('_').ToArray();
                    CtFileItem matchedDirectory = null;
                    foreach (var dir in directories)
                    {
                        var strl_dirtags = dir.Name.Split(',');
                        var rest = strl_tags.Any(i => strl_dirtags.Contains(i));
                        if (rest)
                        {
                            if (matchedDirectory == null)
                            {
                                matchedDirectory = dir;
                            }
                            else
                            {
                                matchedDirectory = null;
                                break;
                            }
                        }
                    }
                    if (matchedDirectory != null)
                    {
                        var src = file.FullName;
                        var dest = Path.Combine(matchedDirectory.FullName, file.Name);
                        if (File.Exists(src) && !File.Exists(dest))
                        {
                            File.Move(src, dest);
                            DependencyService.Get<IMedia>().UpdateGallery(dest);
                        }
                    }
                    else if (directories.Length > 0)
                    {
                        var othersDirectoryName = await fileService.CreateDirectory(Path.Combine(directoryName, "others"));
                        var src = file.FullName;
                        var dest = Path.Combine(othersDirectoryName, file.Name);
                        if (File.Exists(src) && !File.Exists(dest))
                        {
                            File.Move(src, dest);
                            DependencyService.Get<IMedia>().UpdateGallery(dest);
                        }
                    }
                }
            }
            foreach (var dir in directories)
            {
                await DirectoryImageClassify(dir.ShortName);
            }
        }

        public async Task Population()
        {
            var items = await fileService.GetSubList(string.Empty);
            var directories = items.FindAll(i => i.IsDirectory).OrderBy(j => j.Name.Length).ToArray();

            var files = new List<CtFileItem>();
            foreach (var dir in directories)
            {
                var tmp = await GetSubFiles(dir.ShortName);
                files.AddRange(tmp);
            }

            foreach (var file in files)
            {
                var src = file.FullName;
                var dest = Path.Combine(fileService.GetBaseDirectory(), file.Name);
                if (File.Exists(src) && !File.Exists(dest))
                {
                    File.Move(src, dest);
                    DependencyService.Get<IMedia>().UpdateGallery(dest);
                }
            }
        }

        public async Task<CtFileItem[]> GetSubFiles(string directoryName)
        {
            var items = await fileService.GetSubList(directoryName);
            var files = items.FindAll(i => !i.IsDirectory);
            var directories = items.FindAll(i => i.IsDirectory).OrderBy(j => j.Name.Length).ToArray();
            var allfiles = new List<CtFileItem>(files);
            
            foreach(var dir in directories)
            {
                var tmp = await GetSubFiles(dir.ShortName);
                allfiles.AddRange(tmp);
            }
            return allfiles.ToArray();
        }
    }
}
