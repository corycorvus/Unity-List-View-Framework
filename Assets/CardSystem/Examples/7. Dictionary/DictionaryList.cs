using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CardSystem {
	public class DictionaryList : ListViewController<DictionaryListItemData, DictionaryListItem> {
	    public string dataFile;
	    public GameObject[] models;

        readonly Dictionary<string, ModelPool> _models = new Dictionary<string, ModelPool>();
        protected override void Setup() {
			base.Setup();
		    TextAsset text = Resources.Load<TextAsset>(dataFile);
		    if (text) {
		        JSONObject obj = new JSONObject(text.text);
                data = new DictionaryListItemData[obj.Count];
                for (int i = 0; i < data.Length; i++) {
                    data[i] = new DictionaryListItemData();
                    data[i].FromJSON(obj[i], this);
                }
		    } else data = new DictionaryListItemData[0];

            if (models.Length < 1) {
                Debug.LogError("No models!");
            }
            foreach (GameObject model in models) {
                if (_models.ContainsKey(model.name))
                    Debug.LogError("Two templates cannot have the same name");
                _models[model.name] = new ModelPool(model);
            }
        }

        protected override void UpdateItems() {
            int count = 0;
            UpdateRecursively(data, ref count);
        }

	    void UpdateRecursively(DictionaryListItemData[] data, ref int count) {
	        foreach (DictionaryListItemData item in data) {
	            if (count + dataOffset < 0) {
	                ExtremeLeft(item);
	            } else if (count + dataOffset > numItems) {
	                ExtremeRight(item);
	            } else {
	                ListMiddle(item, count + dataOffset);
	            }
	            count++;
	            if (item.children != null) {
	                if (item.expanded) {
	                    UpdateRecursively(item.children, ref count);
	                } else {
	                    RecycleChildren(item);
	                }
	            }
	        }
	    }

	    void RecycleChildren(DictionaryListItemData data) {
            foreach (DictionaryListItemData child in data.children) {
                RecycleItem(child.template, child.item);
                child.item = null;
                if(child.children != null)
                    RecycleChildren(child);
            }
        }
        
        public GameObject GetModel(string name) {
            if (!_models.ContainsKey(name)) {
                Debug.LogWarning("Cannot get model, " + name + " doesn't exist");
                return null;
            }
            GameObject model = null;
            if (_models[name].pool.Count > 0) {
                model = _models[name].pool[0];
                _models[name].pool.RemoveAt(0);

                model.gameObject.SetActive(true);
            } else {
                model = Instantiate(_models[name].prefab);
                model.transform.parent = transform;
            }
	        return model;
	    }

        public class ModelPool {
            public readonly GameObject prefab;
            public readonly List<GameObject> pool = new List<GameObject>();

            public ModelPool(GameObject prefab) {
                if (prefab == null)
                    Debug.LogError("Template prefab cannot be null");
                this.prefab = prefab;
            }
        }
    }
}