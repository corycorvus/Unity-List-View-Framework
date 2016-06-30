using UnityEngine;
using System.Collections;
using CardSystem;

public class ListViewExpander : ListViewInputHandler {
    protected override void HandleInput() {
        if (Input.GetMouseButtonUp(1)) {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                NestedJSONItem item = hit.collider.GetComponent<NestedJSONItem>();
                if (item) {
                    item.data.expanded = !item.data.expanded;
                }
            }
        }
    }
}
