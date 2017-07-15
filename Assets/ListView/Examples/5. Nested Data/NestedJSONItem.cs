using System.Collections.Generic;
using UnityEngine;

namespace ListView
{
    class NestedJSONItem : ListViewItem<NestedJSONItemData, int>
    {
        public TextMesh label;

        public override void Setup(NestedJSONItemData data)
        {
            base.Setup(data);
            label.text = data.text;
        }

        public void ToggleExpanded()
        {
            Debug.Log("WIP");
        }
    }

    //[System.Serializable]     //Will cause warnings, but helpful for debugging
    class NestedJSONItemData : ListViewItemNestedData<NestedJSONItemData, int>
    {
        public string text;

        public void FromJSON(JSONObject obj, string template)
        {
            obj.GetField(ref text, "text");
            this.template = template;
            obj.GetField("children", delegate(JSONObject _children)
            {
                children = new List<NestedJSONItemData>(_children.Count);
                for (int i = 0; i < _children.Count; i++)
                {
                    children[i] = new NestedJSONItemData();
                    children[i].FromJSON(_children[i], template);
                }
            });
        }
    }
}
