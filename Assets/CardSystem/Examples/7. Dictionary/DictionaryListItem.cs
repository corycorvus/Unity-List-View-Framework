using UnityEngine;
using System.Collections;
using CardSystem;

namespace CardSystem {
    public class DictionaryListItem : ListViewItem<DictionaryListItemData> {
        public TextMesh title;

        protected DictionaryList list;

        public override void Setup(DictionaryListItemData data) {
            base.Setup(data);
            list = data.list;
            title.text = data.title;
        }
    }

//[System.Serializable]     //Will cause warnings, but helpful for debugging
    public class DictionaryListItemData : ListViewItemNestedData<DictionaryListItemData> {
        public string title, description, model;
        public DictionaryList list;

        public void FromJSON(JSONObject obj, DictionaryList list) {
            this.list = list;
            obj.GetField(ref title, "title");
            obj.GetField(ref description, "description");
            obj.GetField(ref model, "model");
            obj.GetField(ref template, "template");
            obj.GetField("children", delegate(JSONObject _children) {
                children = new DictionaryListItemData[_children.Count];
                for (int i = 0; i < _children.Count; i++) {
                    children[i] = new DictionaryListItemData();
                    children[i].FromJSON(_children[i], list);
                }
            });
        }
    }
}