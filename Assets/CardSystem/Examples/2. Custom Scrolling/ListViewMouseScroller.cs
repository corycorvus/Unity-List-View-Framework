using UnityEngine;

namespace ListView {
    public class ListViewMouseScroller : ListViewScroller {
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
    }
}