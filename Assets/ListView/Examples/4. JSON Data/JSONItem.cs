using UnityEngine;

namespace ListView
{
    class JSONItem : ListViewItem<JSONItemData, int>
    {
        [SerializeField]
        TextMesh m_Label;

        public override void Setup(JSONItemData data)
        {
            base.Setup(data);
            m_Label.text = data.text;
        }
    }

    class JSONItemData : CubeItemData
    {
        public void FromJSON(JSONObject obj)
        {
            obj.GetField(ref text, "text");
        }
    }
}
