using UnityEngine;

namespace ListView
{
    class CubeItem : ListViewItem<CubeItemData, int>
    {
        public TextMesh label;

        public override void Setup(CubeItemData data)
        {
            base.Setup(data);
            label.text = data.text;
        }
    }

    [System.Serializable]
    class CubeItemData : ListViewItemData<int>
    {
        public string text;
    }
}
