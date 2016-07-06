using UnityEngine;
using System.Collections;
using ListView;

public class WebListInputHandler : ListViewScroller {
    public float scrollWheelCoeff = 1;
    private float listDepth;
    protected override void HandleInput() {
        Vector3 screenPoint = Input.mousePosition;
        if (Input.GetMouseButton(0)) {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                ListViewItemBase item = hit.collider.GetComponent<ListViewItemBase>();
                if (item) {
                    listDepth = (hit.point - Camera.main.transform.position).magnitude;
                    screenPoint.z = listDepth;
                    StartScrolling(Camera.main.ScreenToWorldPoint(screenPoint));
                }
            }
        }
        screenPoint.z = listDepth;
        Vector3 scrollPosition = Camera.main.ScreenToWorldPoint(screenPoint);
        Scroll(scrollPosition);
        if(!Input.GetMouseButton(0))
            StopScrolling();

        listView.scrollOffset += Input.mouseScrollDelta.y * scrollWheelCoeff;
    }
    protected override void Scroll(Vector3 position) {
        if (scrolling)
            listView.scrollOffset = startOffset + startPosition.y - position.y;
    }
}
