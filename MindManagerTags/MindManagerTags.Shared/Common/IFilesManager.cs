using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MindManagerTags.Common
{
    interface IFilesManager
    {
        Task<StorageFile> BrowseMapAsync();

        /// <summary>
        /// Refresh the last loaded map from storage
        /// </summary>
        /// <returns></returns>
        Task<StorageFile> ReloadLastMapAsync();

        /// <summary>
        /// Add the file in the FutureAccessList and return the same file
        /// </summary>
        IStorageFile GetStorageFile(IStorageFile file);

        /// <summary>
        /// Clears the FutureAccessList
        /// </summary>
        void ClearHistory();
    }
}
