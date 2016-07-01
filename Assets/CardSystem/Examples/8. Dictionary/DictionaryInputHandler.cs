using UnityEngine;
using System.Collections;
using CardSystem;

public class DictionaryInputHandler : ListViewScroller {
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
		} else {
			StopScrolling();
		}
	    screenPoint.z = listDepth;
        Scroll(Camera.main.ScreenToWorldPoint(screenPoint));
	}
    protected override void StartScrolling(Vector3 position) {
        base.StartScrolling(position);
        ((DictionaryList)listView).OnStartScrolling();
    }
    protected override void Scroll(Vector3 position) {
        if (scrolling) {
            listView.scrollOffset = startOffset + startPosition.y - position.y;
        }
    }
    protected override void StopScrolling() {
        base.StopScrolling();
        ((DictionaryList)listView).OnStopScrolling();
    }
}
