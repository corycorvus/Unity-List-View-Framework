using UnityEngine;

namespace ListView
{
    public class WebListInputHandler : ListViewScroller
    {
        [SerializeField]
        float m_ScrollWheelCoeff = 1;

        float m_ListDepth;

        protected override void HandleInput()
        {
            var screenPoint = Input.mousePosition;
            if (Input.GetMouseButton(0))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    var item = hit.collider.GetComponent<WebItem>();
                    if (item)
                    {
                        m_ListDepth = (hit.point - Camera.main.transform.position).magnitude;
                        screenPoint.z = m_ListDepth;
                        StartScrolling(Camera.main.ScreenToWorldPoint(screenPoint));
                    }
                }
            }
            screenPoint.z = m_ListDepth;
            var scrollPosition = Camera.main.ScreenToWorldPoint(screenPoint);
            Scroll(scrollPosition);
            if (!Input.GetMouseButton(0))
                StopScrolling();

            listView.scrollOffset += Input.mouseScrollDelta.y * m_ScrollWheelCoeff;
        }

        protected override void Scroll(Vector3 position)
        {
            if (m_Scrolling)
                listView.scrollOffset = m_StartOffset + m_StartPosition.y - position.y;
        }
    }
}
