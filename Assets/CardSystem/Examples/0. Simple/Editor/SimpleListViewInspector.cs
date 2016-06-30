using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SimpleListView))]
public class SimpleListViewInspector : Editor {
    public override void OnInspectorGUI() {
        SimpleListView listView = (SimpleListView) target;
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Prev")) {
                listView.ScrollPrev();
            }
            if (GUILayout.Button("Next")) {
                listView.ScrollNext();
            }
        }
        GUILayout.EndHorizontal();
        DrawDefaultInspector();
    }
}
