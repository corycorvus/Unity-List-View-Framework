﻿using UnityEngine;
using System.Collections;

namespace CardSystem {
	public class CubeList : ListViewController<CubeItemData, CubeItem> {
		protected override void Setup() {
			base.Setup();
			for(int i = 0; i < data.Length; i++) {
				data[i].text = i + "";
			}
		}
	}
}