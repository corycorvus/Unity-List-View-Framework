using UnityEngine;

namespace ListView {
    public class SimpleListView : MonoBehaviour {
        public GameObject prefab;
        public int dataOffset;

        public float itemHeight = 1;
        public int range = 5;

        public string[] data;

        private TextMesh[] items;

        void Start() {
            items = new TextMesh[range];
            for (int i = 0; i < range; i++) {
                items[i] = Instantiate(prefab).GetComponent<TextMesh>();
                items[i].transform.position = transform.position + Vector3.down * i * itemHeight;
                items[i].transform.parent = transform;
            }
            UpdateList();
        }

        void UpdateList() {
            for (int i = 0; i < range; i++) {
                int dataIdx = i + dataOffset;
                if (dataIdx >= 0 && dataIdx < data.Length) {
                    items[i].text = data[dataIdx];
                } else {
                    items[i].text = "";
                }
            }
        }

        void OnGUI() {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            GUILayout.Label("This is an overly simplistic list view. Click the buttons below to scroll, or modify Data Offset in the inspector");
            if (GUILayout.Button("Scroll Next")) {
                ScrollNext();
            }
            if (GUILayout.Button("Scroll Prev")) {
                ScrollPrev();
            }
            GUILayout.EndArea();
        }

        void ScrollNext() {
            dataOffset++;
            UpdateList();
        }

        void ScrollPrev() {
            dataOffset--;
            UpdateList();
        }
    }
}