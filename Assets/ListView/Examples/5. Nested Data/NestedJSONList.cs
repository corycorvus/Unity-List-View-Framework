using System.Collections.Generic;
using UnityEngine;

namespace ListView
{
    class NestedJSONList : NestedListViewController<NestedJSONItemData, NestedJSONItem, int>
    {
        [SerializeField]
        string m_DataFile;

        [SerializeField]
        string m_DefaultTemplate;

        [SerializeField]
        float m_Range;

        void Awake()
        {
            size = Vector3.forward * m_Range;
        }

        protected override void Setup()
        {
            base.Setup();
            var text = Resources.Load<TextAsset>(m_DataFile);
            if (text)
            {
                var obj = new JSONObject(text.text);
                var length = obj.Count;
                data = new List<NestedJSONItemData>(length);
                var index = 0;
                for (var i = 0; i < length; i++)
                {
                    var item = new NestedJSONItemData();
                    item.FromJSON(obj[i], m_DefaultTemplate, ref index);
                    data.Add(item);
                }
            }
            else
            {
                data = null;
            }
        }
    }
}
