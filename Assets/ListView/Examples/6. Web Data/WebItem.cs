using UnityEngine;

namespace ListView
{
    class WebItem : ListViewItem<WebItemData, string>
    {
        [SerializeField]
        TextMesh m_Label;

        public override void Setup(WebItemData data)
        {
            base.Setup(data);
            m_Label.text = data.text;
        }
    }

    class WebItemData : ListViewItemData<string>
    {
        public string text;
        public void FromJSON(JSONObject obj)
        {
            obj.GetField(ref text, "description");
            var idx = string.Empty;
            obj.GetField(ref idx, "id");
            index = idx;
        }
    }
}
