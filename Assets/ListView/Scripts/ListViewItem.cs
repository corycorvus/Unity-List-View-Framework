using System;
using UnityEngine;

namespace ListView
{
    class ListViewItem : ListViewItem<ListViewItemInspectorData, int> { }

    public class ListViewItemBase : MonoBehaviour { }

    public class ListViewItem<TData, TIndex> : ListViewItemBase where TData : ListViewItemData<TIndex>
    {
        public TData data { get; set; }
        public Action<Action> startSettling { protected get; set; }
        public Action endSettling { protected get; set; }
        public Func<TIndex, ListViewItem<TData, TIndex>> getListItem { protected get; set; }

        public virtual void Setup(TData data)
        {
            this.data = data;
        }
    }

    public class NestedListViewItem<TData, TIndex> : ListViewItem<TData, TIndex> where TData : ListViewItemData<TIndex>
    {
        public event Action<TData> toggleExpanded;

        public void ToggleExpanded()
        {
            if (toggleExpanded != null)
                toggleExpanded(data);
        }
    }
}
