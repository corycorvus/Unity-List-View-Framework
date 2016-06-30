using UnityEngine;
using System.Collections;

namespace CardSystem {
	public class CubeItem : ListViewItem<CubeItemData> {
		public TextMesh label;

		public override void Setup(CubeItemData data) {
            base.Setup(data);
			label.text = data.text;
		}
	}

	[System.Serializable]
	public class CubeItemData : ListViewItemData {
		public string text;
	}
}
