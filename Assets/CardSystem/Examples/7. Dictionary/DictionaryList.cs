using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Mono.Data.Sqlite;

namespace CardSystem {
	public class DictionaryList : ListViewController<DictionaryListItemData, DictionaryListItem> {
	    public string databasePath = "/CardSystem/Examples/7. Dictionary/wordnet30.db";
        public int batchSize = 15;
	    public string defaultTemplate = "DictionaryItem";
	    public GameObject loadingIndicator;
        
        delegate void WordsResult(DictionaryListItemData[] words);
	    private volatile bool dbLock;

	    private DictionaryListItemData[] cleanup;
	    private int batchOffset;        //Number of batches we are offset

        IDbConnection dbconn;

        protected override void Setup() {
			base.Setup();

            string conn = "URI=file:" + Application.dataPath + databasePath;
            
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open(); //Open connection to the database.

            data = null;
            //Start off with some data
            GetWords(0, batchSize * 3, words => {
                Debug.Log("result");
                data = words;
            });
        }

	    void OnDestroy() {
            dbconn.Close();
            dbconn = null;
        }

	    void GetWords(int offset, int range, WordsResult result) {
	        if (dbLock)
	            return;
	        if (result == null) {
	            Debug.LogError("Called GetWords without a result callback");
	            return;
	        }
	        dbLock = true;
            //Not sure what the current deal is with threads. Hopefully this is OK?
	        new Thread(() => {
	            DictionaryListItemData[] words = new DictionaryListItemData[range];
	            IDbCommand dbcmd = dbconn.CreateCommand();
	            string sqlQuery = string.Format("SELECT lemma, definition FROM word as W JOIN sense as S on W.wordid=S.wordid JOIN synset as Y on S.synsetid=Y.synsetid ORDER BY W.wordid limit {0} OFFSET {1}", range, offset);
	            dbcmd.CommandText = sqlQuery;
	            IDataReader reader = dbcmd.ExecuteReader();
	            int count = 0;
	            while (reader.Read()) {
	                string lemma = reader.GetString(0);
	                string definition = reader.GetString(1);
	                words[count] = new DictionaryListItemData();

	                words[count].template = defaultTemplate;
	                words[count].word = lemma;

	                //Wrap definition
	                string[] wrds = definition.Split(' ');
	                int charCount = 0;
	                foreach (string wrd in wrds) {
	                    charCount += wrd.Length + 1;
	                    if (charCount > 40) { //Guesstimate
	                        words[count].definition += "\n";
	                        charCount = 0;
	                    }
	                    words[count].definition += wrd + " ";
	                }
	                count++;
	            }
	            reader.Close();
	            reader = null;
	            dbcmd.Dispose();
	            dbcmd = null;
	            result(words);
	            dbLock = false;
	        }).Start();
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
	                GetWords((batchOffset + 3) * batchSize, batchSize, words => {
	                    Array.Copy(data, batchSize, data, 0, batchSize * 2);
	                    Array.Copy(words, 0, data, batchSize * 2, batchSize);
	                    batchOffset++;
	                });
	            } else {    //Jumped multiple batches. Get a whole new dataset
                    //Clean up all existing gameobjects
                    cleanup = data;
                    data = null;
                    GetWords((currBatch - 1) * batchSize, batchSize * 3, words => {
                        data = words;
                        batchOffset = currBatch - 1;
                    });
                }
	        }
            if (batchOffset > 0 && -dataOffset < (batchOffset + 1) * batchSize) {
                if (currBatch == batchOffset) { //Just one batch, fetch only the next one
                    GetWords((batchOffset - 1) * batchSize, batchSize, words => {
                        Array.Copy(data, 0, data, batchSize, batchSize * 2);
                        Array.Copy(words, 0, data, 0, batchSize);
                        batchOffset--;
                    });
                } else {    //Jumped multiple batches. Get a whole new dataset
                    //Clean up all existing gameobjects
                    cleanup = data;
                    data = null;
                    GetWords((currBatch - 1) * batchSize, batchSize * 3, words => {
                        data = words;
                        batchOffset = currBatch - 1;
                    });
                }
            }
	        if (cleanup != null) {
                foreach (DictionaryListItemData item in cleanup) {
                    if (item.item != null)
                        RecycleItem(item.template, item.item);
                }
	            cleanup = null;
	        }
        }
        protected override void UpdateItems() {
            if (data == null || data.Length == 0) {
                loadingIndicator.SetActive(true);
                return;
            }
            for (int i = 0; i < data.Length; i++) {
                if (i + dataOffset + batchOffset * batchSize < -1) {
                    ExtremeLeft(data[i]);
                } else if (i + dataOffset + batchOffset * batchSize > numItems) {
                    ExtremeRight(data[i]);
                } else {
                    ListMiddle(data[i], i + batchOffset * batchSize);
                }
            }
            loadingIndicator.SetActive(false);
        }

        protected override void Positioning(Transform t, int offset) {
            t.position = leftSide + (offset * _itemSize.y + scrollOffset) * Vector3.down;
        }
    }
}