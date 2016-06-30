using UnityEngine;
using System.Collections;
using CardSystem;

namespace CardSystem {
    public class AdvancedListItem : ListViewItem<AdvancedListItemData> {
        public TextMesh title;

        protected AdvancedList list;

        public override void Setup(AdvancedListItemData data) {
            base.Setup(data);
            list = data.list;
            title.text = data.title;
        }
    }

//[System.Serializable]     //Will cause warnings, but helpful for debugging
    public class AdvancedListItemData : ListViewItemNestedData<AdvancedListItemData> {
        public string title, description, model;
        public AdvancedList list;

        public void FromJSON(JSONObject obj, AdvancedList list) {
            this.list = list;
            obj.GetField(ref title, "title");
            obj.GetField(ref description, "description");
            obj.GetField(ref model, "model");
            obj.GetField(ref template, "template");
            obj.GetField("children", delegate(JSONObject _children) {
                children = new AdvancedListItemData[_children.Count];
                for (int i = 0; i < _children.Count; i++) {
                    children[i] = new AdvancedListItemData();
                    children[i].FromJSON(_children[i], list);
                }
            });
        }
    }
}