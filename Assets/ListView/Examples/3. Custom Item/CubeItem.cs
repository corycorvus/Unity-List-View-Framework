using UnityEngine;

namespace ListView
{
    class CubeItem : ListViewItem<CubeItemData, int>
    {
        [SerializeField]
        TextMesh m_Label;

        public override void Setup(CubeItemData data)
        {
            base.Setup(data);
            m_Label.text = data.text;
        }
    }

    [System.Serializable]
    class CubeItemData : ListViewItemData<int>
    {
        public string text;
        public int idx { set { index = value; } }
    }
}
