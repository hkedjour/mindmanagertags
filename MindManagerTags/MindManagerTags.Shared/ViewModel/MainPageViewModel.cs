using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Input;
using Windows.Storage.AccessCache;
using MindManagerTags.DataModel;
using MindManagerTags.Common;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;

namespace MindManagerTags.ViewModel
{
    class MainPageViewModel : INotifyPropertyChanged
    {
        private bool _canBrowseMap;
        private bool _canRefreshMap;
        private bool _isMapEmpty;

        /// <summary>
        /// Launch OpenFileDialog
        /// </summary>
        public ICommand BrowseMapCommand { get; set; }

        private bool CanBrowseMap
        {
            get { return _canBrowseMap; }
            set
            {
                if (value == _canBrowseMap)
                    return;
                _canBrowseMap = value;
                ((RelayCommand)BrowseMapCommand).RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Refresh the current map if it's loaded via Browsing
        /// </summary>
        public ICommand RefreshMapCommand { get; set; }

        /// <summary>
        /// Stores the currently used map
        /// </summary>
        public FiltredMap Map { get; set; }

        public IFilesManager FilesManager { get; set; }

        public MainPageViewModel()
        {
            FilesManager = new FilesManager();
            Map = new FiltredMap();
            IsMapEmpty = true;

            BrowseMapCommand = new RelayCommand(OnBrowseMap, () => CanBrowseMap);
            CanBrowseMap = true;

            RefreshMapCommand = new RelayCommand(OnRefreshMap, () => CanRefreshMap);
            CanRefreshMap = false;
        }

        public bool CanRefreshMap
        {
            get { return _canRefreshMap; }
            set
            {
                if (value == _canRefreshMap)
                    return;
                _canRefreshMap = value;
                ((RelayCommand)RefreshMapCommand).RaiseCanExecuteChanged();
            }
        }

        private async void OnRefreshMap()
        {
            await RefreshMapAsync();
        }

        /// <summary>
        /// Reload the current map
        /// </summary>
        private async Task RefreshMapAsync()
        {
            CanRefreshMap = false;

            var freshMap = await FilesManager.ReloadLastMapAsync();

            if (freshMap == null)
            {
                // Display an error message
                return;
            }

            await Map.LoadMapAsync(freshMap);

            CanRefreshMap = true;
            IsMapEmpty = Map.Tags.Count == 0;
        }

        private async void OnBrowseMap()
        {
            var mapFile = await FilesManager.BrowseMapAsync();

            await LoadMapAsync(mapFile);
        }

        /// <summary>
        /// Handle files that are launched from the browser.
        /// </summary>
        public async Task LoadFileAsync(IStorageFile file)
        {
            await LoadMapAsync(FilesManager.GetStorageFile(file));
        }

        public async Task LoadMapAsync(IStorageFile file)
        {
            try
            {
                CanBrowseMap = false;
                await Map.LoadMapAsync(file);
                CanBrowseMap = true;
                CanRefreshMap = true;

                IsMapEmpty = Map.Tags.Count == 0;
            }
            catch (Exception e)
            {
                
            }
        }

        public bool IsMapEmpty
        {
            get { return _isMapEmpty; }
            set
            {
                if (_isMapEmpty == value) return;
                _isMapEmpty = value;
                OnPropertyChanged();
            }
        }

        public async Task SaveState()
        {
            var settings = ApplicationData.Current.LocalSettings;

            settings.Values["AnyMarker"] = Map.AnyMarker;

            var serilizer = new DataContractSerializer(typeof (List<string>));

            using (var sr = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync("tags.xml", CreationCollisionOption.ReplaceExisting))
            {
                serilizer.WriteObject(sr, Map.GetSelectedTags().Select(t => t.Name).ToList());
            }
        }

        public async Task RestoreState()
        {
            await RefreshMapAsync();
            var settings = ApplicationData.Current.LocalSettings;

            try
            {
                Map.AnyMarker = (bool) settings.Values["AnyMarker"];

                var serilizer = new DataContractSerializer(typeof (List<string>));
                using (var sr = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("tags.xml"))
                {
                    await Map.SelectTags((List<string>) serilizer.ReadObject(sr));
                }
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
