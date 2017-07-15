using UnityEngine;

namespace ListView
{
    class JSONItem : ListViewItem<JSONItemData, int>
    {
        public TextMesh label;

        public override void Setup(JSONItemData data)
        {
            base.Setup(data);
            label.text = data.text;
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