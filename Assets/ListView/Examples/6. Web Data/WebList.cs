using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ListView
{
    class WebList : ListViewController<WebItemData, WebItem, string>
    {
        //Ideas for a better/different example web service are welcome
        //Note: the github API has a rate limit. After a couple of tries, you won't see any results :(

        [SerializeField]
        string m_URLFormatString = "https://api.github.com/gists/public?page={0}&per_page={1}";

        [SerializeField]
        string m_DefaultTemplate = "JSONItem";

        [SerializeField]
        int m_BatchSize = 15;

        [SerializeField]
        float m_Range;

        delegate void WebResult(List<WebItemData> data);

        int m_BatchOffset;
        bool m_WebLock;
        bool m_Loading;
        List<WebItemData> m_Cleanup;

        protected override float listHeight { get { return Mathf.Infinity; } }

        void Awake()
        {
            size = Vector3.forward * m_Range;
        }

        protected override void Setup()
        {
            base.Setup();
            StartCoroutine(GetBatch(0, m_BatchSize * 3, data => { this.data = data; }));
        }

        IEnumerator GetBatch(int offset, int range, WebResult result)
        {
            if (m_WebLock)
                yield break;

            m_WebLock = true;

            var items = new List<WebItemData>(range);
            var www = new WWW(string.Format(m_URLFormatString, offset, range));
            while (!www.isDone)
            {
                yield return null;
            }

            var response = new JSONObject(www.text);
            for (var i = 0; i < response.list.Count; i++)
            {
                var item = new WebItemData { template = m_DefaultTemplate};
                item.FromJSON(response[i]);
                items.Add(item);
            }

            result(items);

            m_WebLock = false;
            m_Loading = false;
        }

        protected override void ComputeConditions()
        {
            base.ComputeConditions();
            m_StartPosition = m_Extents - itemSize * 0.5f;

            var dataOffset = (int)(-scrollOffset / itemSize.z);

            var currBatch = dataOffset / m_BatchSize;
            if (dataOffset > (m_BatchOffset + 2) * m_BatchSize)
            {
                //Check how many batches we jumped
                if (currBatch == m_BatchOffset + 2) // Just one batch, fetch only the previous one
                {
                    StartCoroutine(GetBatch((m_BatchOffset + 3) * m_BatchSize, m_BatchSize, words =>
                    {
                        data.RemoveRange(0, m_BatchSize);
                        data.AddRange(words);
                        m_BatchOffset++;
                    }));
                }
                else if (currBatch != m_BatchOffset) // Jumped multiple batches. Get a whole new dataset
                {
                    if (!m_Loading)
                        m_Cleanup = data;

                    m_Loading = true;
                    StartCoroutine(GetBatch((currBatch - 1) * m_BatchSize, m_BatchSize * 3, words =>
                    {
                        data = words;
                        m_BatchOffset = currBatch - 1;
                    }));
                }
            }
            else if (m_BatchOffset > 0 && dataOffset < (m_BatchOffset + 1) * m_BatchSize)
            {
                if (currBatch == m_BatchOffset) // Just one batch, fetch only the next one
                {
                    StartCoroutine(GetBatch((m_BatchOffset - 1) * m_BatchSize, m_BatchSize, words =>
                    {
                        data.RemoveRange(m_BatchSize * 2, m_BatchSize);
                        words.AddRange(data);
                        m_Data = words;
                        m_BatchOffset--;
                    }));
                }
                else if (currBatch != m_BatchOffset) // Jumped multiple batches. Get a whole new dataset
                {
                    if (!m_Loading)
                        m_Cleanup = data;

                    m_Loading = true;
                    if (currBatch < 1)
                        currBatch = 1;
                    StartCoroutine(GetBatch((currBatch - 1) * m_BatchSize, m_BatchSize * 3, words =>
                    {
                        data = words;
                        m_BatchOffset = currBatch - 1;
                    }));
                }
            }

            if (m_Cleanup != null)
            {
                //Clean up all visible items
                foreach (var data in m_Cleanup)
                {
                    if (data == null)
                        continue;

                    var index = data.index;
                    WebItem item;
                    if (m_ListItems.TryGetValue(index, out item))
                        Recycle(index);
                }

                m_Cleanup = null;
            }
        }

        protected override void UpdateItems()
        {
            if (m_Data == null)
                return;

            var doneSettling = true;

            var offset = 0f;
            var order = 0;
            var itemSizeZ = itemSize.z;
            for (var i = 0; i < m_Data.Count; i++)
            {
                var datum = m_Data[i];
                var localOffset = offset + scrollOffset + m_BatchOffset * m_BatchSize * itemSizeZ;
                if (localOffset + itemSizeZ < 0 || localOffset > m_Size.z)
                    Recycle(datum.index);
                else
                    UpdateVisibleItem(datum, order++, localOffset, ref doneSettling);

                offset += itemSizeZ;
            }

            if (m_Settling && doneSettling)
                EndSettling();
        }
    }
}
