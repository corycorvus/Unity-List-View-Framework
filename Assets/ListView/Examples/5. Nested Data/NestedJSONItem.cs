using System.Collections.Generic;
using UnityEngine;

namespace ListView
{
    class NestedJSONItem : NestedListViewItem<NestedJSONItemData, int>
    {
        [SerializeField]
        TextMesh m_Label;

        public override void Setup(NestedJSONItemData data)
        {
            base.Setup(data);
            m_Label.text = data.text;
        }
    }

    //[System.Serializable]     //Will cause warnings, but helpful for debugging
    class NestedJSONItemData : NestedListViewItemData<NestedJSONItemData, int>
    {
        public string text;

        public void FromJSON(JSONObject obj, string template, ref int index)
        {
            obj.GetField(ref text, "text");
            this.template = template;
            this.index = index;
            var idx = index + 1;
            obj.GetField("children", delegate(JSONObject _children)
            {
                children = new List<NestedJSONItemData>(_children.Count);
                for (int i = 0; i < _children.Count; i++)
                {
                    var child = new NestedJSONItemData();
                    child.FromJSON(_children[i], template, ref idx);
                    children.Add(child);
                }
            });
            index = idx;
        }
    }
}
