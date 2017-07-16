using System.Collections.Generic;
using UnityEngine;

//Uses JSONObject http://u3d.as/1Rh

namespace ListView
{
    class JSONList : ListViewController<JSONItemData, JSONItem, int>
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
                data = new List<JSONItemData>(obj.Count);
                for (var i = 0; i < length; i++)
                {
                    var child = new JSONItemData();
                    child.FromJSON(obj[i]);
                    child.template = m_DefaultTemplate;
                    child.idx = i;
                    data.Add(child);
                }
            }
            else
            {
                data = null;
            }
        }
    }
}
