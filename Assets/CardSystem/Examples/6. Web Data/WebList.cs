using System;
using UnityEngine;
using System.Collections;

namespace ListView {
	public class WebList : ListViewController<JSONItemData, JSONItem> {
        //Ideas for a better/different example web service are welcome
        //Note: the github API has a rate limit. After a couple of tries, you won't see any results :(
	    public string URLFormatString = "https://api.github.com/gists/public?page={0}&per_page={1}";
	    public string defaultTemplate = "JSONItem";
	    public int batchSize = 15;

        private delegate void WebResult(JSONItemData[] data);

	    private int batchOffset;
	    private bool webLock;
	    private bool loading;
	    private JSONItemData[] cleanup;

        protected override void Setup() {
			base.Setup();
		    StartCoroutine(GetBatch(0, batchSize * 3, data => {
		        this.data = data;
		    }));
		}

	    IEnumerator GetBatch(int offset, int range, WebResult result) {
	        if (webLock)
	            yield break;
	        webLock = true;
            JSONItemData[] items = new JSONItemData[range];
	        WWW www = new WWW(string.Format(URLFormatString, offset, range));
	        while (!www.isDone) {
	            yield return null;
	        }
            JSONObject response = new JSONObject(www.text);
	        for(int i = 0; i < response.list.Count; i++) {
	            items[i] = new JSONItemData {template = defaultTemplate};
	            response[i].GetField(ref items[i].text, "description");
	        }
	        result(items);
	        webLock = false;
	        loading = false;
	    }

        protected override void ComputeConditions() {
            if (templates.Length > 0) {
                //Use first template to get item size
                _itemSize = GetObjectSize(templates[0]);
            }
            //Resize range to nearest multiple of item width
            numItems = Mathf.RoundToInt(range / _itemSize.y); //Number of cards that will fit
            range = numItems * _itemSize.y;

            //Get initial conditions. This procedure is done every frame in case the collider bounds change at runtime
            leftSide = transform.position + Vector3.up * range * 0.5f + Vector3.left * _itemSize.x * 0.5f;

            dataOffset = (int)(scrollOffset / itemSize.y);
            if (scrollOffset < 0)
                dataOffset--;

            int currBatch = -dataOffset / batchSize;
            if (-dataOffset > (batchOffset + 2) * batchSize) {
                //Check how many batches we jumped
                if (currBatch == batchOffset + 2) { //Just one batch, fetch only the next one
                    StartCoroutine(GetBatch((batchOffset + 3) * batchSize, batchSize, words => {
                        Array.Copy(data, batchSize, data, 0, batchSize * 2);
                        Array.Copy(words, 0, data, batchSize * 2, batchSize);
                        batchOffset++;
                    }));
                } else if (currBatch != batchOffset) { //Jumped multiple batches. Get a whole new dataset
                    if (!loading)
                        cleanup = data;
                    loading = true;
                    StartCoroutine(GetBatch((currBatch - 1) * batchSize, batchSize * 3, words => {
                        data = words;
                        batchOffset = currBatch - 1;
                    }));
                }
            } else if (batchOffset > 0 && -dataOffset < (batchOffset + 1) * batchSize) {
                if (currBatch == batchOffset) { //Just one batch, fetch only the next one
                    StartCoroutine(GetBatch((batchOffset - 1) * batchSize, batchSize, words => {
                        Array.Copy(data, 0, data, batchSize, batchSize * 2);
                        Array.Copy(words, 0, data, 0, batchSize);
                        batchOffset--;
                    }));
                } else if (currBatch != batchOffset) { //Jumped multiple batches. Get a whole new dataset
                    if (!loading)
                        cleanup = data;
                    loading = true;
                    if (currBatch < 1)
                        currBatch = 1;
                    StartCoroutine(GetBatch((currBatch - 1) * batchSize, batchSize * 3, words => {
                        data = words;
                        batchOffset = currBatch - 1;
                    }));
                }
            }
            if (cleanup != null) {
                //Clean up all existing gameobjects
                foreach (JSONItemData item in cleanup) {
                    if (item == null)
                        continue;
                    if (item.item != null) {
                        RecycleItem(item.template, item.item);
                        item.item = null;
                    }
                }
                cleanup = null;
            }
        }

        protected override void UpdateItems() {
            if (data == null || data.Length == 0 || loading) {
                return;
            }
            for (int i = 0; i < data.Length; i++) {
                if (data[i] == null)
                    continue;
                if (i + dataOffset + batchOffset * batchSize < -1) {        //Checking against -1 lets the first element overflow
                    ExtremeLeft(data[i]);
                } else if (i + dataOffset + batchOffset * batchSize > numItems) {
                    ExtremeRight(data[i]);
                } else {
                    ListMiddle(data[i], i + batchOffset * batchSize);
                }
            }
        }

        protected override void Positioning(Transform t, int offset) {
            t.position = leftSide + (offset * _itemSize.y + scrollOffset) * Vector3.down;
        }
    }
}