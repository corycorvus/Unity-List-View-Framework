using UnityEngine;

namespace ListView
{
    class AdvancedListItemChild : AdvancedListItem
    {
        [SerializeField]
        TextMesh m_Description;

        [SerializeField]
        Transform m_ModelTransform;

        public GameObject model { get; private set; }

        public override void Setup(AdvancedListItemData data)
        {
            base.Setup(data);
            m_Description.text = data.description;
            model = m_List.GetModel(data.model);
            model.transform.parent = m_ModelTransform;
            model.transform.localPosition = Vector3.zero;
            model.transform.localScale = Vector3.one;
            model.transform.localRotation = Quaternion.identity;
        }
    }
}
