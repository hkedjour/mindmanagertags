using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MindManagerTags.DataModel
{
    /// <summary>
    /// Represent a Tag in a map
    /// </summary>
    public class Tag : INotifyPropertyChanged
    {
        private string _name;
        private bool _isSelected;

        public Tag()
        {
            
        }

        /// <summary>
        /// Create a tag from its name
        /// </summary>
        /// <param name="name">Tag's name</param>
        public Tag(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the user selected this tag or not
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        #region Equals override, used in tests

        public override int GetHashCode()
        {
            return (_name != null ? _name.GetHashCode() : 0);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Tag) obj);
        }

        protected bool Equals(Tag other)
        {
            return string.Equals(_name, other._name);
        }

        #endregion

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
