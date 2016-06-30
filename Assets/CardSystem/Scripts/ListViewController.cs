using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CardSystem {
    public class ListViewController : ListViewController<ListViewItemInspectorData, ListViewItem> {}

    public abstract class ListViewControllerBase : MonoBehaviour {

        //Public variables
        public float scrollOffset;
        public float padding = 0.01f;
		public float range = 1;
        public GameObject[] templates;

        //Protected variables
        protected int dataOffset;
		protected int numItems;
		protected Vector3 leftSide;
        protected Vector3 _itemSize;
        protected readonly Dictionary<string, ListViewItemTemplate> _templates = new Dictionary<string, ListViewItemTemplate>();

        //Public properties
        public Vector3 itemSize {
	        get {
	            return _itemSize;
	        }
	    }

        void Start() {
			Setup();
		}

		void Update() {
			ViewUpdate();
		}

        protected virtual void Setup() {
            if (templates.Length < 1) {
                Debug.LogError("No templates!");
            }
            foreach (GameObject template in templates) {
                if (_templates.ContainsKey(template.name))
                    Debug.LogError("Two templates cannot have the same name");
                _templates[template.name] = new ListViewItemTemplate(template);
            }
        }

        protected virtual void ViewUpdate() {
            ComputeConditions();
            UpdateItems();
        }

        protected virtual void ComputeConditions() {
            if (templates.Length > 0) {
                //Use first template to get item size
                _itemSize = GetObjectSize(templates[0]);
            }
            //Resize range to nearest multiple of item width
            numItems = Mathf.RoundToInt(range / _itemSize.x); //Number of cards that will fit
            range = numItems * _itemSize.x;

            //Get initial conditions. This procedure is done every frame in case the collider bounds change at runtime
            leftSide = transform.position + Vector3.left * range * 0.5f;

            dataOffset = (int)(scrollOffset / itemSize.x);
            if (scrollOffset < 0)
                dataOffset--;
        }

	    protected abstract void UpdateItems();

        public virtual void ScrollNext() {
            scrollOffset += _itemSize.x;
        }
        public virtual void ScrollPrev() {
            scrollOffset -= _itemSize.x;
        }
        public virtual void ScrollTo(int index) {
            scrollOffset = index * itemSize.x;
        }
        protected virtual void Positioning(Transform t, int offset) {
            t.position = leftSide + (offset * _itemSize.x + scrollOffset) * Vector3.right;
        }

        protected virtual Vector3 GetObjectSize(GameObject g) {
            Vector3 itemSize = Vector3.one;
            //TODO: Better method for finding object size
            Renderer rend = g.GetComponentInChildren<Renderer>();
            if (rend) {
                itemSize.x = Vector3.Scale(g.transform.lossyScale, rend.bounds.extents).x * 2 + padding;
                itemSize.y = Vector3.Scale(g.transform.lossyScale, rend.bounds.extents).y * 2 + padding;
                itemSize.z = Vector3.Scale(g.transform.lossyScale, rend.bounds.extents).z * 2 + padding;
            }
            return itemSize;
        }

        protected virtual void RecycleItem(string template, MonoBehaviour item) {
            if (item == null || template == null)
                return;
            _templates[template].pool.Add(item);
            item.gameObject.SetActive(false);
        }
    }

    //I'm actually kind of shocked I can use the same name here
    public abstract class ListViewController<DataType, ItemType> : ListViewControllerBase
            where DataType : ListViewItemData
			where ItemType : ListViewItem<DataType> {
		public DataType[] data;

        protected override void UpdateItems() {
            for (int i = 0; i < data.Length; i++) {
                if (i + dataOffset < 0) {
                    ExtremeLeft(data[i]);
                } else if (i + dataOffset > numItems) {
                    ExtremeRight(data[i]);
                } else {
                    ListMiddle(data[i], i);
                }
            }
        }

        protected virtual void ExtremeLeft(DataType data) {
			RecycleItem(data.template, data.item);
			data.item = null;
		}
		protected virtual void ExtremeRight(DataType data) {
			RecycleItem(data.template, data.item);
			data.item = null;
		}
		protected virtual void ListMiddle(DataType data, int offset) {
			if (data.item == null) {
				data.item = GetItem(data);
			}
			Positioning(data.item.transform, offset);
		}

        protected virtual ItemType GetItem(DataType data) {
			if (data == null) {
				Debug.LogWarning("Tried to get item with null data");
				return null;
			}
			if (!_templates.ContainsKey(data.template)) {
				Debug.LogWarning("Cannot get item, template " + data.template + " doesn't exist");
				return null;
			}
			ItemType item = null;
			if (_templates[data.template].pool.Count > 0) {
				item = (ItemType)_templates[data.template].pool[0];
				_templates[data.template].pool.RemoveAt(0);

				item.gameObject.SetActive(true);
				item.Setup(data);
			} else {
				item = Instantiate(_templates[data.template].prefab).GetComponent<ItemType>();
				item.transform.parent = transform;
				item.Setup(data);
			}
			return item;
		}
	}
}