using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace MindManagerTags.Common
{
    class FilesManager : IFilesManager
    {
        readonly FilePicker _filePicker = new FilePicker();

        public async Task<StorageFile> BrowseMapAsync()
        {
            var file = await _filePicker.PickSingleFileAsync(new[] {".mmap"});

            ClearHistory();
            StorageApplicationPermissions.FutureAccessList.Add(file);

            return file;
        }

        public async Task<StorageFile> ReloadLastMapAsync()
        {
            if (StorageApplicationPermissions.FutureAccessList.Entries.Count == 0)
                return null;

            var lastFileToken = StorageApplicationPermissions.FutureAccessList.Entries[0].Token;

            return await StorageApplicationPermissions.FutureAccessList.GetFileAsync(lastFileToken, AccessCacheOptions.None);
        }

        public IStorageFile GetStorageFile(IStorageFile file)
        {
            ClearHistory();
            StorageApplicationPermissions.FutureAccessList.Add(file);

            return file;
        }

        public void ClearHistory()
        {
            StorageApplicationPermissions.FutureAccessList.Clear();
        }
    }
}
