using System.Collections.Generic;
using UnityEngine;

namespace ListView
{
    class AdvancedListItem : NestedListViewItem<AdvancedListItemData, int>
    {
        [SerializeField]
        TextMesh m_Title;

        protected AdvancedList m_List;

        public override void Setup(AdvancedListItemData data)
        {
            base.Setup(data);
            m_List = data.list;
            m_Title.text = data.title;
        }
    }

    //[System.Serializable]     //Will cause warnings, but helpful for debugging
    class AdvancedListItemData : NestedListViewItemData<AdvancedListItemData, int>
    {
        public string title, description, model;
        public AdvancedList list;

        public void FromJSON(JSONObject obj, AdvancedList list, ref int index)
        {
            this.list = list;
            obj.GetField(ref title, "title");
            obj.GetField(ref description, "description");
            obj.GetField(ref model, "model");
            var template = "";
            obj.GetField(ref template, "template");
            this.template = template;
            this.index = index;
            var idx = index + 1;
            obj.GetField("children", delegate(JSONObject _children)
            {
                children = new List<AdvancedListItemData>(_children.Count);
                for (var i = 0; i < _children.Count; i++)
                {
                    var child = new AdvancedListItemData();
                    child.FromJSON(_children[i], list, ref idx);
                    children.Add(child);
                }
            });
            index = idx;
        }
    }
}
