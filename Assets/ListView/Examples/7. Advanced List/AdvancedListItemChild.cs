using UnityEngine;

namespace ListView
{
    class AdvancedListItemChild : AdvancedListItem
    {
        [SerializeField]
        TextMesh m_Description;

        [SerializeField]
        Transform m_ModelTransform;

        GameObject m_Model;

        public GameObject model { get { return m_Model; } }

        public override void Setup(AdvancedListItemData data)
        {
            base.Setup(data);
            m_Description.text = data.description;
            m_Model = m_List.GetModel(data.model);
            m_Model.transform.parent = m_ModelTransform;
            m_Model.transform.localPosition = Vector3.zero;
            m_Model.transform.localScale = Vector3.one;
            m_Model.transform.localRotation = Quaternion.identity;
        }
    }
}
