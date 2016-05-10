using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace MindManagerTags.Controls
{
    /// <summary>
    /// This release split the items on two rows
    /// It uses 2 items control to dispaly the rows.
    /// In the future we remove the code from xaml and instanciate the controls in the code
    /// </summary>
    public sealed class WrapPanel : ItemsControl
    {
        public WrapPanel()
        {
           RowItems = new ObservableCollection<object>[2];

            for (var i = 0; i < RowItems.Length; i++)
            {
                RowItems[i] = new ObservableCollection<object>();
            }

            DefaultStyleKey = typeof(WrapPanel);
        }

        /// <summary>
        /// We just split the items in rows.
        /// 1 st version, not optimized. I don't expect a huge collection.
        /// </summary>
        protected override void OnItemsChanged(object e)
        {
            foreach (var row in RowItems)
            {
                row.Clear();
            }

            if (Items == null)
                return;

            var i = 0;
            foreach (var item in Items)
            {
                RowItems[i++].Add(item);
                i = i%RowItems.Length;
            }
        }

        public ObservableCollection<object>[] RowItems { get; set; }
    }
}
