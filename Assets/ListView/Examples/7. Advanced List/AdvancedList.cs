using System.Collections.Generic;
using UnityEngine;

namespace ListView
{
    class AdvancedList : NestedListViewController<AdvancedListItemData, AdvancedListItem, int>
    {
        class ModelPool
        {
            public readonly GameObject prefab;
            public readonly List<GameObject> pool = new List<GameObject>();

            public ModelPool(GameObject prefab)
            {
                if (prefab == null)
                    Debug.LogError("Template prefab cannot be null");

                this.prefab = prefab;
            }
        }

        [SerializeField]
        string m_DataFile;

        [SerializeField]
        GameObject[] m_Models;

        [SerializeField]
        float m_Range;

        readonly Dictionary<string, ModelPool> m_ModelDictionary = new Dictionary<string, ModelPool>();
        readonly Dictionary<string, Vector3> m_TemplateSizes = new Dictionary<string, Vector3>();

        void Awake()
        {
            size = m_Range * Vector3.forward;
        }

        protected override void Setup()
        {
            base.Setup();
            foreach (var kvp in m_TemplateDictionary)
            {
                m_TemplateSizes[kvp.Key] = GetObjectSize(kvp.Value.prefab);
            }

            var text = Resources.Load<TextAsset>(m_DataFile);
            if (text)
            {
                var obj = new JSONObject(text.text);
                var length = obj.Count;
                data = new List<AdvancedListItemData>(length);
                var index = 0;
                for (var i = 0; i < length; i++)
                {
                    var item = new AdvancedListItemData();
                    item.FromJSON(obj[i], this, ref index);
                    data.Add(item);
                }
            }
            else
            {
                data = null;
            }

            if (m_Models.Length < 1)
            {
                Debug.LogError("No models!");
            }

            foreach (var model in m_Models)
            {
                if (m_ModelDictionary.ContainsKey(model.name))
                    Debug.LogError("Two templates cannot have the same name");

                m_ModelDictionary[model.name] = new ModelPool(model);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(itemSize.x, m_Size.y, itemSize.z));
        }

        protected override void ComputeConditions()
        {
            base.ComputeConditions();
            m_StartPosition = m_Extents - itemSize * 0.5f;
        }

        protected override void UpdateRecursively(List<AdvancedListItemData> data, ref int order, ref float offset, ref bool doneSettling, int depth = 0)
        {
            for (var i = 0; i < data.Count; i++)
            {
                var datum = data[i];

                var index = datum.index;
                bool expanded;
                if (!m_ExpandStates.TryGetValue(index, out expanded))
                    m_ExpandStates[index] = false;

                m_ItemSize = m_TemplateSizes[datum.template];
                var itemSize = m_ItemSize.Value;

                var localOffset = offset + scrollOffset;
                if (localOffset + itemSize.z < 0 || localOffset > m_Size.z)
                    Recycle(index);
                else
                    UpdateNestedItem(datum, order++, localOffset, depth, ref doneSettling);

                offset += itemSize.z;

                if (datum.children != null)
                {
                    if (expanded)
                        UpdateRecursively(datum.children, ref order, ref offset, ref doneSettling, depth + 1);
                    else
                        RecycleChildren(datum);
                }
            }
        }

        protected override void RecycleItem(string template, AdvancedListItem item)
        {
            base.RecycleItem(template, item);

            var aItem = item as AdvancedListItemChild;

            if (!aItem)
                return;

            var model = aItem.model;
            m_ModelDictionary[aItem.data.model].pool.Add(model);
            model.transform.parent = transform;
            model.SetActive(false);
        }

        public GameObject GetModel(string name)
        {
            if (!m_ModelDictionary.ContainsKey(name))
            {
                Debug.LogWarning("Cannot get model, " + name + " doesn't exist");
                return null;
            }

            GameObject model;
            if (m_ModelDictionary[name].pool.Count > 0)
            {
                model = m_ModelDictionary[name].pool[0];
                m_ModelDictionary[name].pool.RemoveAt(0);

                model.gameObject.SetActive(true);
            }
            else
            {
                model = Instantiate(m_ModelDictionary[name].prefab);
                model.transform.parent = transform;
            }

            return model;
        }
    }
}
