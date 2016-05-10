using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MindManagerTags.Common
{
    public class FilePicker
    {

#if WINDOWS_PHONE_APP

        private bool _initialized;
        private TaskCompletionSource<StorageFile> _filePickedCompletionSource;

        private void Initialize()
        {
            if (_initialized)
                return;

             _initialized = true;
            CoreApplication.GetCurrentView().Activated += OnViewActivated;
        }

        private void OnViewActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            var continueArgs = args as FileOpenPickerContinuationEventArgs;

            if (continueArgs != null && continueArgs.Files.Count > 0 && _filePickedCompletionSource != null)
            {
                var selectedFile = continueArgs.Files[0];

                _filePickedCompletionSource.SetResult(selectedFile);

                _filePickedCompletionSource = null;
            }
        }

#endif

        /// <summary>
        /// Call this methode to open a file
        /// </summary>
        public async Task<StorageFile> PickSingleFileAsync(string[] filters)
        {
#if WINDOWS_PHONE_APP

            Initialize();

#endif

            var picker = new FileOpenPicker();

            foreach (var filter in filters)
                picker.FileTypeFilter.Add(filter);

#if WINDOWS_PHONE_APP

            _filePickedCompletionSource = new TaskCompletionSource<StorageFile>();
            
            picker.PickSingleFileAndContinue();

            var file = await _filePickedCompletionSource.Task;

#else   // WINDOWS_APP

            var file = await picker.PickSingleFileAsync();

#endif

            return file;
        }
    }
}