using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CardSystem {
	public class AdvancedList : ListViewController<AdvancedListItemData, AdvancedListItem> {
	    public string dataFile;
	    public GameObject[] models;

        readonly Dictionary<string, ModelPool> _models = new Dictionary<string, ModelPool>();
        readonly Dictionary<string, Vector3> templateSizes = new Dictionary<string, Vector3>();
	    private float scrollReturn = float.MaxValue;
	    private float itemHeight;

        protected override void Setup() {
			base.Setup();
            foreach (KeyValuePair<string, ListViewItemTemplate> kvp in _templates) {
                templateSizes[kvp.Key] = GetObjectSize(kvp.Value.prefab);
            }

		    TextAsset text = Resources.Load<TextAsset>(dataFile);
		    if (text) {
		        JSONObject obj = new JSONObject(text.text);
                data = new AdvancedListItemData[obj.Count];
                for (int i = 0; i < data.Length; i++) {
                    data[i] = new AdvancedListItemData();
                    data[i].FromJSON(obj[i], this);
                }
		    } else data = new AdvancedListItemData[0];

            if (models.Length < 1) {
                Debug.LogError("No models!");
            }
            foreach (GameObject model in models) {
                if (_models.ContainsKey(model.name))
                    Debug.LogError("Two templates cannot have the same name");
                _models[model.name] = new ModelPool(model);
            }
        }

	    void OnDrawGizmos() {
	        Gizmos.DrawWireCube(transform.position, new Vector3(itemSize.x, range, itemSize.z));
	    }

        protected override void ComputeConditions() {
            if (templates.Length > 0) {
                //Use first template to get item size
                _itemSize = GetObjectSize(templates[0]);
            }
            //Resize range to nearest multiple of item width
            numItems = Mathf.RoundToInt(range / _itemSize.y); //Number of cards that will fit
            range = numItems * _itemSize.y;

            //Get initial conditions. This procedure is done every frame in case the collider bounds change at runtime
            leftSide = transform.position + Vector3.up * range * 0.5f + Vector3.left * itemSize.x * 0.5f;

            dataOffset = (int)(scrollOffset / itemSize.y);
            if (scrollOffset < 0)
                dataOffset--;
        }

        protected override void UpdateItems() {
            float totalOffset = 0;
            UpdateRecursively(data, ref totalOffset);
            totalOffset -= itemHeight;
            if (totalOffset < -scrollOffset) {
                scrollReturn = -totalOffset;
            }
        }

	    void UpdateRecursively(AdvancedListItemData[] data, ref float totalOffset) {
            foreach (AdvancedListItemData item in data) {
                itemHeight = templateSizes[item.template].y;
	            if (totalOffset + scrollOffset + itemHeight < 0) {
	                ExtremeLeft(item);
	            } else if (totalOffset + scrollOffset > range) {
	                ExtremeRight(item);
	            } else {
	                ListMiddle(item, totalOffset + scrollOffset);
	            }
                totalOffset += itemHeight;
	            if (item.children != null) {
	                if (item.expanded) {
	                    UpdateRecursively(item.children, ref totalOffset);
	                } else {
	                    RecycleChildren(item);
	                }
	            }
	        }
	    }
        protected void ListMiddle(AdvancedListItemData data, float offset) {
            if (data.item == null) {
                data.item = GetItem(data);
            }
            Positioning(data.item.transform, offset);
        }

        protected void Positioning(Transform t, float offset) {
            t.position = leftSide + offset * Vector3.down;
        }

	    public void OnStopScrolling() {
	        if (scrollOffset > 0) {
	            scrollOffset = 0;
	        }
	        if (scrollReturn < float.MaxValue) {
	            scrollOffset = scrollReturn;
	            scrollReturn = float.MaxValue;
	        }
	    }

        void RecycleChildren(AdvancedListItemData data) {
            foreach (AdvancedListItemData child in data.children) {
                RecycleItem(child.template, child.item);
                child.item = null;
                if(child.children != null)
                    RecycleChildren(child);
            }
        }

        protected override void RecycleItem(string template, MonoBehaviour item) {
            base.RecycleItem(template, item);
            try {
                AdvancedListItemChild aItem = (AdvancedListItemChild) item;
                if (aItem) {
                    _models[aItem.data.model].pool.Add(aItem.model);
                    aItem.model.transform.parent = null;
                    aItem.model.SetActive(false);
                }
            } catch { }
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