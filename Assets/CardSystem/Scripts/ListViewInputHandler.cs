using UnityEngine;
using System.Collections;

namespace CardSystem {
	public abstract class ListViewInputHandler : MonoBehaviour {
		
		public ListViewControllerBase listView;
		void Update() {
			HandleInput();
		}

	    protected abstract void HandleInput();
	}

	public abstract class ListViewScroller : ListViewInputHandler {
	    protected bool scrolling;
	    protected Vector3 startPosition;
	    protected float startOffset;
		protected virtual void StartScrolling(Vector3 start) {
		    if (scrolling)
		        return;
		    scrolling = true;
		    startPosition = start;
		    startOffset = listView.scrollOffset;
		}

        protected virtual void Scroll(Vector3 position) {
            if(scrolling)
                listView.scrollOffset = startOffset + position.x - startPosition.x;
        }

	    protected virtual void StopScrolling() {
            scrolling = false;
        }
	}
}