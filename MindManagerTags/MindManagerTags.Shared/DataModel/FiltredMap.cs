using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MindManagerTags.DataModel
{
    /// <summary>
    /// Encapsulates the operations needed to explore and filter the map.
    /// </summary>
    public class FiltredMap : MindMap, INotifyPropertyChanged
    {
        private bool _anyMarker;
        private ObservableCollection<Tag> _tags;
        private ObservableCollection<Topic> _filteredTopics;
        private string _title;

        public FiltredMap()
        {
            _tags = new ObservableCollection<Tag>();
            _filteredTopics = new ObservableCollection<Topic>();

            PropertyChanged += OnMyPropertyChanged;
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Current mapt tags list
        /// </summary>
        public ObservableCollection<Tag> Tags
        {
            get { return _tags; }
            set
            {
                if (value == _tags)
                    return;

                _tags = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Topics as filterd by the user
        /// </summary>
        public ObservableCollection<Topic> FilteredTopics
        {
            get { return _filteredTopics; }
            set
            {
                if (value == _filteredTopics)
                    return;

                _filteredTopics = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether to match all tags in a topic or just one
        /// </summary>
        public bool AnyMarker
        {
            get { return _anyMarker; }
            set
            {
                if (value == AnyMarker)
                    return;

                _anyMarker = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Just another way to make properties async!!!
        /// </summary>
        private async void OnMyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AnyMarker")
                await FilterTopics();
        }

        /// <summary>
        /// Loads a map from a stream and fills the Tags property
        /// </summary>
        /// <param name="sr">A stream representing the map</param>
        public override async Task LoadMapAsync(Stream sr)
        {
            await base.LoadMapAsync(sr);

            Title = await GetMapTitle();

            FilteredTopics.Clear();

            foreach (var tag in Tags)
                tag.PropertyChanged -= OnTagPropertyChanged;

            Tags.Clear();
            var tags = GetActiveTags();

            foreach (var tag in tags)
            {
                tag.PropertyChanged += OnTagPropertyChanged;
                Tags.Add(tag);
            }
        }

        private async void OnTagPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
                await FilterTopics();
        }

        /// <summary>
        /// Filter topics by currently selected tags.
        /// </summary>
        public async Task FilterTopics()
        {
            var selectedTags = GetSelectedTags();
            if (selectedTags.Count == 0)
            {
                FilteredTopics.Clear();
                return;
            }

            var topics = await GetMarkedTopicsByTagsAsync(selectedTags, !AnyMarker);

            FilteredTopics.Clear();

            foreach (var topic in topics)
                FilteredTopics.Add(topic);
        }

        /// <summary>
        /// Return a list of selected tags
        /// </summary>
        public List<Tag> GetSelectedTags()
        {
            return Tags.Where(t => t.IsSelected).ToList();
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public async Task SelectTags(List<string> tags)
        {
            if (tags.Count == 0)
                return;

            foreach (var tag in Tags)
            {
                if (tags.Contains(tag.Name))
                {
                    // I'll fix it in the next release
                    tag.PropertyChanged -= OnTagPropertyChanged;
                    tag.IsSelected = true;
                    tag.PropertyChanged += OnTagPropertyChanged;
                }
            }

            await FilterTopics();
        }
    }
}