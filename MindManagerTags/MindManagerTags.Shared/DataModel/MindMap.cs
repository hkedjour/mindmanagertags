using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace MindManagerTags.DataModel
{
    /// <summary>
    /// Encapsulate a mind manager map as used by the application.
    /// </summary>
    public class MindMap
    {
        public const string MindManagerNS = "http://schemas.mindjet.com/MindManager/Application/2003";

        /// <summary>
        /// The original map as loaded from mmap file.
        /// </summary>
        private XDocument _map;

        public async Task LoadMapAsync(IStorageFile file)
        {
            using (var sr = await file.OpenStreamForReadAsync())
            {
                using (var zip = new ZipArchive(sr, ZipArchiveMode.Read))
                {
                    var entry = zip.GetEntry("Document.xml");
                    await LoadMapAsync(entry.Open());
                }
            }
        }

        /// <summary>
        /// Loads a map from xml stream
        /// </summary>
        /// <param name="sr">xml stream that contains the mind map</param>
        public virtual async Task LoadMapAsync(Stream sr)
        {
            await Task.Factory.StartNew(() => _map = XDocument.Load(sr));
        }

        /// <summary>
        /// The Async version of GetActiveTags. It returns a list of all topics that are used in the current map.
        /// Tags that are declared but not used in topics are not returned;
        /// </summary>
        public Task<List<Tag>> GetActiveTagsAsync()
        {
            return Task<List<Tag>>.Factory.StartNew(GetActiveTags);
        }

        /// <summary>
        /// Return a list of all topics that are used in the current map.
        /// Tags that are declared but not used in topics are not returned;
        /// </summary>
        public List<Tag> GetActiveTags()
        {
            try
            {
                var lbl = XName.Get("TextLabel", MindManagerNS);

                var labels = _map.Descendants(lbl).Where(e => e.Attribute("TextLabelName") != null)
                    .Select(e => e.Attribute("TextLabelName").Value)
                    .Distinct()
                    .Select(s => new Tag(s))
                    .ToList();

                return labels;
            }
            catch (Exception ex)
            {
                return new[] {new Tag("Error while extracting tags :" + ex.Message)}.ToList();
            }
        }


        /// <summary>
        /// The async version of GetMarkedTopicsByTags. It returns all topics marked by provided tags.
        /// </summary>
        /// <param name="tags">Labels to look for</param>
        /// <param name="includeAll">If true, the topic is returned only if it's marked by all tags. Otherwize if it marked by at least one label.</param>
        public async Task<List<Topic>> GetMarkedTopicsByTagsAsync(IList<Tag> tags, bool includeAll)
        {
            return await Task<List<Topic>>.Factory.StartNew(() => GetMarkedTopicsByTags(tags, includeAll));
        }

        /// <summary>
        /// Return all topics marked by provided tags.
        /// </summary>
        /// <param name="tags">Labels to look for</param>
        /// <param name="includeAll">If true, the topic is returned only if it's marked by all tags. Otherwize if it marked by at least one label.</param>
        public List<Topic> GetMarkedTopicsByTags(IList<Tag> tags, bool includeAll)
        {
            try
            {
                var tagsNames = tags.Select(t => t.Name).ToList();

                var topics = _map.Descendants(XName.Get("Topic", MindManagerNS))
                    .Where(t => t.Element(XName.Get("TextLabels", MindManagerNS)) != null
                                && t.Element(XName.Get("Text", MindManagerNS)) != null
                                && t.Element(XName.Get("Text", MindManagerNS)).Attribute("PlainText") != null
                                && t.Element(XName.Get("Text", MindManagerNS)).Attribute("PlainText").Value != string.Empty)
                    // Up to this point we selected all topics that have a nom empty text content
                    .Where(t => t.Element(XName.Get("TextLabels", MindManagerNS))
                        .Elements(XName.Get("TextLabel", MindManagerNS))
                        .Select(tl => tl.Attribute("TextLabelName").Value)
                        .Intersect(tagsNames, StringComparer.CurrentCultureIgnoreCase)
                        .Count() >= (includeAll ? tagsNames.Count : 1))
                    .Select(t => new Topic(t))
                    .ToList();

                return topics;
            }
            catch (Exception ex)
            {
                return new[] {new Topic("Error filtring topics", ex.Message)}.ToList();
            }
        }

        /// <summary>
        /// Return map's title (aka the central topic text)
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetMapTitle()
        {
            return await Task<string>.Factory.StartNew(() =>
            {
                try
                {
                    var titleElement = _map.Root.Element(XName.Get("OneTopic", MindManagerNS))
                        .Element(XName.Get("Topic", MindManagerNS))
                        .Element(XName.Get("Text", MindManagerNS));

                    if (titleElement != null &&
                        titleElement.Attribute("PlainText") != null)
                    {
                        return titleElement.Attribute("PlainText").Value;
                    }

                    return string.Empty;
                }
                catch (Exception ex)
                {
                    return "Error loading map";
                }

            });
        }
    }
}