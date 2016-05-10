using System.Xml.Linq;

namespace MindManagerTags.DataModel
{
    /// <summary>
    /// Represent a mind map topic used in the application
    /// </summary>
    public class Topic
    {
        public Topic()
        {
            
        }

        /// <summary>
        /// Create a topic from an Xml topic node
        /// </summary>
        public Topic(XElement e)
        {
            // The caller must ensure that TextLabelName attribute is not null
            Name = e.Element(XName.Get("Text", MindMap.MindManagerNS)).Attribute("PlainText").Value;

            if (e.Parent != null && e.Parent.Name.LocalName != "OneTopic")
                Path = GetTopicPath(e.Parent.Parent);

            var topicNotes = e.Element(XName.Get("NotesGroup", MindMap.MindManagerNS));
            if (topicNotes != null)
            {
                var topicNote = topicNotes.Element(XName.Get("NotesXhtmlData", MindMap.MindManagerNS));
                if (topicNote != null && topicNote.Attribute("PreviewPlainText") != null)
                    NotesPreview = topicNote.Attribute("PreviewPlainText").Value.Replace("<br>", ".");
            }
        }

        public Topic(string name, string path = null, string notesPreview = null)
        {
            Name = name;
            Path = path;
            NotesPreview = notesPreview;
        }

        /// <summary>
        /// The name of the topic.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The path of this topic from the main topic.
        /// For exemple : Topic1 > Subtopic 1
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Preview of the topic associated note.
        /// </summary>
        public string NotesPreview { get; set; }


        /// <summary>
        /// Return a topic's path from an Xml node. Check the Path property for more information.
        /// </summary>
        /// <param name="topic">Topic's Xml element</param>
        /// <returns>Topic's path</returns>
        public string GetTopicPath(XElement topic)
        {
            if (topic == null || topic.Parent == null || topic.Parent.Name.LocalName == "OneTopic")
            {
                // Main topic, we ignore it to avoid repetition in all topics
                return string.Empty;
            }

            var topicText = topic.Element(XName.Get("Text", MindMap.MindManagerNS)) != null
                            && topic.Element(XName.Get("Text", MindMap.MindManagerNS)).Attribute("PlainText") != null
                ? topic.Element(XName.Get("Text", MindMap.MindManagerNS)).Attribute("PlainText").Value
                : "";

            var parentPath = GetTopicPath(topic.Parent.Parent);
            
            return parentPath == string.Empty ? topicText : parentPath + (topicText == string.Empty ? string.Empty : " > ") + topicText;
        }

        #region Equals override. To make tests easy.

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (Path != null ? Path.GetHashCode() : 0);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Topic) obj);
        }

        protected bool Equals(Topic other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Path, other.Path);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Path);
        }
    }
}