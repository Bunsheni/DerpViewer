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
using System.Windows.Input;
using Xamarin.Forms;

namespace DerpViewer.ViewModels
{
    public class FileBrowserViewModel : BaseViewModel
    {
        private string _key = string.Empty;
        private bool progressBarIsVisible;
        private List<DerpSuggestionItem> suggestionItem;
        private DerpFileService fileService = new DerpFileService();
        private ObservableCollection<CtFileItem> fileList = new ObservableCollection<CtFileItem>();

        public bool ProgressBarIsVisible
        {
            get
            {
                return progressBarIsVisible;
            }
            set
            {
                progressBarIsVisible = value;
                OnPropertyChanged();
            }
        }
        int progressBarHeight;
        public int ProgressBarHeight
        {
            get
            {
                return progressBarHeight;
            }
            set
            {
                progressBarHeight = value;
                OnPropertyChanged();
            }
        }
        float progress1, progress2;
        public float Progress1
        {
            get
            {
                return progress1;
            }
            set
            {
                progress1 = value;
                OnPropertyChanged();
            }
        }
        public float Progress2
        {
            get
            {
                return progress2;
            }
            set
            {
                progress2 = value;
                OnPropertyChanged();
            }
        }
        public List<DerpSuggestionItem> SuggestionItems
        {
            get { return suggestionItem; }

            set
            {
                suggestionItem = value;
                OnPropertyChanged();
            }
        }

        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        public bool SuggestionLock = false;
        public void GetSuggestionItem()
        {
            try
            {
                string key;
                int i = Key.LastIndexOf(',');
                if (i < 0)
                {
                    key = Key;
                }
                else
                {
                    key = Key.Substring(i + 1);
                }
                var items = DerpTagDb.DerpTagSuggestion(key);
                if (!SuggestionLock)
                    SuggestionItems = items;
                else
                    SuggestionLock = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public ObservableCollection<CtFileItem> FileList
        {
            get { return fileList; }
            set { SetProperty(ref fileList, value); }
        }

        public string CurrentDirectory = string.Empty;

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


        public async Task<int> GetSubItemNumber(CtFileItem fileItem)
        {
            try
            {
                var subs = await fileService.GetSubList(fileItem.ShortName);
                return subs.Count;
            }
            catch
            {
                return -1;
            }
        }

        public bool DeleteFolder(CtFileItem fileItem)
        {
            try
            {
                if (Directory.Exists(fileItem.FullName))
                    Directory.Delete(fileItem.FullName, true);
                return true;
            }
            catch
            {
                return false;
            }
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
            List<CtFileItem> files = await fileService.GetSubList(CurrentDirectory);
            foreach (CtFileItem item in files)
            {
                FileList.Add(item);
            }
        }

        public async Task RenameFolder(CtFileItem item, string newname)
        {
            string src = item.ShortName;
            string dest = Path.Combine(CurrentDirectory, newname);
            await fileService.MoveDirectory(src, dest);
            await UpdateDirectory();
        }

        public async Task CreateFolder(string name)
        {
            await fileService.CreateDirectory(Path.Combine(CurrentDirectory, name));
            await UpdateDirectory();
        }

        public async Task ClassifyAll()
        {
            await DirectoryImageClassify(CurrentDirectory);
            await UpdateDirectory();
        }

        public async Task DirectoryImageClassify(string directoryName)
        {
            
            var items = await fileService.GetSubList(directoryName);
            var files = items.FindAll(i => !i.IsDirectory);
            var directories = items.FindAll(i => i.IsDirectory).OrderBy(j => j.Name.Length).ToArray();
            var directoryOthers = directories.SingleOrDefault(i => i.Name == "others");

            int index = 0, count = files.Count;

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
                    bool matchFlag = false;
                    foreach (var dir in directories)
                    {
                        var strl_dirtags = dir.Name.Replace(", ", ",").Split(',');
                        //var rest = strl_dirtags.Except(strl_tags).Any();
                        var rest = strl_tags.Any(i => strl_dirtags.Contains(i));
                        if (rest)
                        {
                            if (matchedDirectory == null)
                            {
                                matchedDirectory = dir;
                                matchFlag = true;
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
                    else if (!matchFlag && directoryOthers != null)
                    {
                        var src = file.FullName;
                        var dest = Path.Combine(directoryOthers.FullName, file.Name);
                        if (File.Exists(src) && !File.Exists(dest))
                        {
                            File.Move(src, dest);
                            DependencyService.Get<IMedia>().UpdateGallery(dest);
                        }
                    }
                }
                SetProgressBarValue(++index, count);
            }
            foreach (var dir in directories)
            {
                await DirectoryImageClassify(dir.ShortName);
            }
        }

        public async Task<List<CtFileItem>> PopulationFiles(string directory)
        {
            var items = await fileService.GetSubList(directory);
            var directories = items.FindAll(i => i.IsDirectory);
            var files = items.FindAll(i => !i.IsDirectory);
            foreach (var dir in directories)
            {
                var tmp = await PopulationFiles(dir.ShortName);
                files.AddRange(tmp);
            }
            return files;
        }

        public async Task PopulationAll()
        {
            var files = await PopulationFiles(CurrentDirectory);
            int index = 0, count = files.Count;
            foreach (var file in files)
            {
                var src = file.ShortName;
                var dest = Path.Combine(CurrentDirectory, file.Name);
                await fileService.MoveFile(src, dest);
                DependencyService.Get<IMedia>().UpdateGallery(dest);
                SetProgressBarValue(++index, count);
            }
            await UpdateDirectory();
        }

        public void SetProgressBarValue(int value, int Max)
        {
            if (value < Max)
            {
                ProgressBarHeight = 8;
                ProgressBarIsVisible = true;
            }
            else
            {
                value = Max;
                ProgressBarIsVisible = false;
                ProgressBarHeight = 0;
            }
            Progress2 = (float)value / Max;
        }
    }
}
