using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MindManagerTags.DataModel;

namespace MindManagerTags.Tests
{
    [TestClass]
    public class MindMapTests
    {
        /// <summary>
        /// Tests if tags are correctly extracted from a map
        /// </summary>
        [TestMethod]
        public void extract_used_tags()
        {
            var task =  Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("Assets\\TestMap.mmap").AsTask();
            var file = task.Result;

            var map = new MindMap();
            var t = map.LoadMapAsync(file);
            t.Wait();

            var tags = map.GetActiveTagsAsync().Result;

            Assert.AreEqual(5, tags.Count, "Only five tags are used");

            Assert.IsTrue(tags.Contains(new Tag("ASAP")), "ASAP is used.");
            Assert.IsTrue(tags.Contains(new Tag("Waiting For")), "Waiting For is used.");
            Assert.IsTrue(tags.Contains(new Tag("@Errands")), "@Errands is used.");
            Assert.IsTrue(tags.Contains(new Tag("@Home")), "@Home is used.");
            Assert.IsTrue(tags.Contains(new Tag("@Office")), "@Office is used.");
        }

        /// <summary>
        /// Tests if topics are correctly filtered from a map
        /// </summary>
        [TestMethod]
        public void search_topics_by_one_tag()
        {
            var task =  Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("Assets\\TestMap.mmap").AsTask();
            var file = task.Result;

            var map = new MindMap();
            var t = map.LoadMapAsync(file);
            t.Wait();

            var topics = map.GetMarkedTopicsByTagsAsync(new[] {new Tag("ASAP"), }, true).Result;

            Assert.AreEqual(8, topics.Count, "Only 8 topics are marked as 'ASAP'");

            Assert.IsTrue(topics.Contains(new Topic("Action 1", "Next Actions")), "Action1 is marked by ASAP");
            Assert.IsTrue(topics.Contains(new Topic("Action 2", "Next Actions")), "Action 2 is marked by ASAP");
            Assert.IsTrue(topics.Contains(new Topic("Action 3", "Next Actions")), "Action 3 is marked by ASAP");
            Assert.IsTrue(topics.Contains(new Topic("Action 4", "Next Actions")), "Action 4 is marked by ASAP");
            Assert.IsTrue(topics.Contains(new Topic("Task 1", "Projects > Project one > Phase 1")), "Task 1 is marked by ASAP");
            Assert.IsTrue(topics.Contains(new Topic("Task 21", "Projects > Project one > Phase 2 > Sub phase 1")), "Task 21 is marked by ASAP");
             Assert.IsTrue(topics.Contains(new Topic("Task 21", "Projects > Project one > Phase 2")), "Task 21 is marked by ASAP");
           Assert.IsTrue(topics.Contains(new Topic("Task 3", "Projects > Project one > Phase 2 > Sub phase 3")), "Task 3 is marked by ASAP");
       }

        /// <summary>
        /// Tests if topics are correctly filtered from a map. Any Marker is false.
        /// </summary>
        [TestMethod]
        public void search_topics_by_tag_inclusive()
        {
            var task =  Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("Assets\\TestMap.mmap").AsTask();
            var file = task.Result;

            var map = new MindMap();
            var t = map.LoadMapAsync(file);
            t.Wait();

            var topics = map.GetMarkedTopicsByTagsAsync(new[] { new Tag("ASAP"), new Tag("@Errands"), }, includeAll: true).Result;

            Assert.AreEqual(2, topics.Count, "Only 2 topics are marked as 'ASAP' and '@Errands'");

            Assert.IsTrue(topics.Contains(new Topic("Action 4", "Next Actions")), "Action 4 is marked by ASAP");
            Assert.IsTrue(topics.Contains(new Topic("Task 21", "Projects > Project one > Phase 2 > Sub phase 1")), "Task 21 is marked by ASAP");
       }

        /// <summary>
        /// Tests if topics are correctly filtered from a map. Any Marker is true.
        /// I don't know if exclusive is the right word!
        /// </summary>
        [TestMethod]
        public void search_topics_by_tag_exclusive()
        {
            var task =  Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("Assets\\TestMap.mmap").AsTask();
            var file = task.Result;

            var map = new MindMap();
            var t = map.LoadMapAsync(file);
            t.Wait();

            var topics = map.GetMarkedTopicsByTagsAsync(new[] { new Tag("Waiting For"), new Tag("@Errands"), }, includeAll: false).Result;

            Assert.AreEqual(3, topics.Count, "Only 3 topics are marked as 'ASAP' or '@Errands'");

            Assert.IsTrue(topics.Contains(new Topic("Action 1", "Next Actions")), "Action1 is marked by ASAP");
            Assert.IsTrue(topics.Contains(new Topic("Action 4", "Next Actions")), "Action 4 is marked by ASAP");
            Assert.IsTrue(topics.Contains(new Topic("Task 21", "Projects > Project one > Phase 2 > Sub phase 1")), "Task 21 is marked by ASAP");
       }
    }
}
