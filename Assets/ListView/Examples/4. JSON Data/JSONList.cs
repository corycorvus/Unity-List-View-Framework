using System.Collections.Generic;
using UnityEngine;

//Uses JSONObject http://u3d.as/1Rh

namespace ListView
{
    class JSONList : ListViewController<JSONItemData, JSONItem, int>
    {
        public string dataFile;
        public string defaultTemplate;

        protected override void Setup()
        {
            base.Setup();
            var text = Resources.Load<TextAsset>(dataFile);
            if (text)
            {
                var obj = new JSONObject(text.text);
                data = new List<JSONItemData>(obj.Count);
                for (var i = 0; i < data.Count; i++)
                {
                    var child = new JSONItemData();
                    child.FromJSON(obj[i]);
                    child.template = defaultTemplate;
                    data.Add(child);
                }
            }
            else
            {
                data = null;
            }
        }
    }
}