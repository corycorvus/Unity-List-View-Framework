using UnityEngine;

namespace ListView
{
    class ListViewExpander : ListViewInputHandler
    {
        protected override void HandleInput()
        {
            if (Input.GetMouseButtonUp(1))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    var item = hit.collider.GetComponent<NestedJSONItem>();
                    if (item)
                        item.ToggleExpanded();
                }
            }
        }
    }
}
