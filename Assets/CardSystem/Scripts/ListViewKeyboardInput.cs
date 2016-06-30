using UnityEngine;
using System.Collections;

namespace CardSystem {
	public class ListViewKeyboardInput : ListViewInputHandler {
		
		protected override void HandleInput() {
			if (Input.GetKeyUp(KeyCode.LeftArrow)) {
			    //listView.scrollNext();
			}
			if (Input.GetKeyUp(KeyCode.RightArrow)) {
				//listView.dataOffset++;
			}
		}
	}
}