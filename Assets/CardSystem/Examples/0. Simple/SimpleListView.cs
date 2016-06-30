using UnityEngine;
using System.Collections;

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

    public void ScrollNext() {
        dataOffset++;
        UpdateList();
    }
    public void ScrollPrev() {
        dataOffset--;
        UpdateList();
    }
}
