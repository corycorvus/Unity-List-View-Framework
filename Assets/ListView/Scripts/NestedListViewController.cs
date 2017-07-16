﻿#if UNITY_EDITOR
using System.Collections.Generic;

namespace ListView
{
    class NestedListViewController<TData, TItem, TIndex> : ListViewController<TData, TItem, TIndex>
        where TData : NestedListViewItemData<TData, TIndex>
        where TItem : NestedListViewItem<TData, TIndex>
    {

        protected override float listHeight { get { return m_ExpandedDataLength; } }

        protected float m_ExpandedDataLength;

        protected readonly Dictionary<TIndex, bool> m_ExpandStates = new Dictionary<TIndex, bool>();

        public override List<TData> data
        {
            get { return base.data; }
            set
            {
                m_Data = value;

                // Update visible rows
                var items = new Dictionary<TIndex, TItem>(m_ListItems);
                foreach (var row in items)
                {
                    var index = row.Key;
                    var newData = GetRowRecursive(m_Data, index);
                    if (newData != null)
                        row.Value.Setup(newData);
                    else
                        Recycle(index);
                }
            }
        }

        static TData GetRowRecursive(List<TData> data, TIndex index)
        {
            foreach (var datum in data)
            {
                if (datum.index.Equals(index))
                    return datum;

                if (datum.children != null)
                {
                    var result = GetRowRecursive(datum.children, index);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        protected void RecycleRecursively(TData data)
        {
            Recycle(data.index);

            if (data.children != null)
            {
                foreach (var child in data.children)
                {
                    RecycleRecursively(child);
                }
            }
        }

        protected override void UpdateItems()
        {
            var doneSettling = true;
            var count = 0f;
            var order = 0;

            UpdateRecursively(m_Data, ref order, ref count, ref doneSettling);
            m_ExpandedDataLength = count;

            if (m_Settling && doneSettling)
                EndSettling();
        }

        protected virtual void UpdateRecursively(List<TData> data, ref int order, ref float offset, ref bool doneSettling, int depth = 0)
        {
            for (var i = 0; i < data.Count; i++)
            {
                var datum = data[i];

                var index = datum.index;
                bool expanded;
                if (!m_ExpandStates.TryGetValue(index, out expanded))
                    m_ExpandStates[index] = false;

                var itemSize = m_ItemSize.Value;

                var localOffset = offset + scrollOffset;
                if (localOffset + itemSize.z < 0 || localOffset > m_Size.z)
                    Recycle(index);
                else
                    UpdateNestedItem(datum, order++, localOffset, depth, ref doneSettling);

                offset += itemSize.z;

                if (datum.children != null)
                {
                    if (expanded)
                        UpdateRecursively(datum.children, ref order, ref offset, ref doneSettling, depth + 1);
                    else
                        RecycleChildren(datum);
                }
            }
        }

        protected virtual void UpdateNestedItem(TData data, int order, float count, int depth, ref bool doneSettling)
        {
            UpdateVisibleItem(data, order, count, ref doneSettling);
        }

        protected void RecycleChildren(TData data)
        {
            foreach (var child in data.children)
            {
                Recycle(child.index);

                if (child.children != null)
                    RecycleChildren(child);
            }
        }

        protected bool GetExpanded(TIndex index)
        {
            bool expanded;
            m_ExpandStates.TryGetValue(index, out expanded);
            return expanded;
        }

        protected void SetExpanded(TIndex index, bool expanded)
        {
            m_ExpandStates[index] = expanded;
            StartSettling();
        }

        protected void ScrollToIndex(TData container, TIndex targetIndex, ref float scrollHeight)
        {
            var index = container.index;
            if (index.Equals(targetIndex))
            {
                if (-scrollOffset > scrollHeight || -scrollOffset + m_Size.z < scrollHeight)
                    scrollOffset = -scrollHeight;
                return;
            }

            scrollHeight += itemSize.z;

            if (GetExpanded(index))
            {
                if (container.children != null)
                {
                    foreach (var child in container.children)
                    {
                        ScrollToIndex(child, targetIndex, ref scrollHeight);
                    }
                }
            }
        }

        protected override TItem GetItem(TData data)
        {
            var item = base.GetItem(data);
            item.toggleExpanded -= ToggleExpanded;
            item.toggleExpanded += ToggleExpanded;
            return item;
        }

        void ToggleExpanded(TData data)
        {
            bool expanded;
            m_ExpandStates.TryGetValue(data.index, out expanded);
            m_ExpandStates[data.index] = !expanded;
            StartSettling();
        }
    }
}
#endif
