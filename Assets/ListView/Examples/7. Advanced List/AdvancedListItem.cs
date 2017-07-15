using System.Collections.Generic;
using UnityEngine;

namespace ListView
{
    class AdvancedListItem : ListViewItem<AdvancedListItemData, int>
    {
        public TextMesh title;

        protected AdvancedList m_List;

        public override void Setup(AdvancedListItemData data)
        {
            base.Setup(data);
            m_List = data.list;
            title.text = data.title;
        }
    }

    //[System.Serializable]     //Will cause warnings, but helpful for debugging
    class AdvancedListItemData : ListViewItemNestedData<AdvancedListItemData, int>
    {
        public string title, description, model;
        public AdvancedList list;

        public void FromJSON(JSONObject obj, AdvancedList list)
        {
            this.list = list;
            obj.GetField(ref title, "title");
            obj.GetField(ref description, "description");
            obj.GetField(ref model, "model");
            var template = "";
            obj.GetField(ref template, "template");
            this.template = template;
            obj.GetField("children", delegate(JSONObject _children)
            {
                children = new List<AdvancedListItemData>(_children.Count);
                for (var i = 0; i < _children.Count; i++)
                {
                    var child = new AdvancedListItemData();
                    child.FromJSON(_children[i], list);
                    children.Add(child);
                }
            });
        }
    }
}
