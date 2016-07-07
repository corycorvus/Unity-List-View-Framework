using System;
using UnityEngine;
using System.Data;
using System.IO;
using System.Threading;
using Mono.Data.Sqlite;

//Borrows from http://answers.unity3d.com/questions/743400/database-sqlite-setup-for-unity.html
//Dictionary from https://wordnet.princeton.edu/
namespace ListView {
	public class DictionaryList : ListViewController<DictionaryListItemData, DictionaryListItem> {
        public const string editorDatabasePath = "ListView/Examples/8. Dictionary/wordnet30.db";
	    public const string databasePath = "wordnet30.db";
        public int batchSize = 15;
        public float scrollDamping = 15f;
	    public float maxMomentum = 200f;
        public string defaultTemplate = "DictionaryItem";
	    public GameObject loadingIndicator;

        public int maxWordCharacters = 30;          //Wrap word after 30 characters
        public int definitionCharacterWrap = 40;    //Wrap definition after 40 characters
	    public int maxDefinitionLines = 4;          //Max 4 lines per definition
        
        delegate void WordsResult(DictionaryListItemData[] words);
	    private volatile bool dbLock;

        private DictionaryListItemData[] cleanup;
	    private int dataLength;         //Total number of items in the data set
	    private int batchOffset;        //Number of batches we are offset
	    private bool scrolling;
	    private bool loading;
        private float scrollReturn = float.MaxValue;
	    private float scrollDelta;
	    private float lastScrollOffset;

        IDbConnection dbconn;

        protected override void Setup() {
			base.Setup();

#if UNITY_EDITOR
            string conn = "URI=file:" + Path.Combine(Application.dataPath, editorDatabasePath);
#else
            string conn = "URI=file:" + Path.Combine(Application.dataPath, databasePath);
#endif

            dbconn = new SqliteConnection(conn);
            dbconn.Open(); //Open connection to the database.

            if (maxWordCharacters < 4) {
                Debug.LogError("Max word length must be > 3");
            }

            try {
                IDbCommand dbcmd = dbconn.CreateCommand();
                string sqlQuery = "SELECT COUNT(lemma) FROM word as W JOIN sense as S on W.wordid=S.wordid JOIN synset as Y on S.synsetid=Y.synsetid";
                dbcmd.CommandText = sqlQuery;
                IDataReader reader = dbcmd.ExecuteReader();
                while (reader.Read()) {
                    dataLength = reader.GetInt32(0);
                }
                reader.Close();
                dbcmd.Dispose();
            } catch {
                Debug.LogError("DB error, couldn't get total data length");
            }

            data = null;
            //Start off with some data
            GetWords(0, batchSize * 3, words => {
                data = words;
            });
        }

	    void OnDestroy() {
            dbconn.Close();
            dbconn = null;
        }

	    void GetWords(int offset, int range, WordsResult result) {
	        if (dbLock) {
	            return;
	        }
	        if (result == null) {
	            Debug.LogError("Called GetWords without a result callback");
	            return;
	        }
	        dbLock = true;
            //Not sure what the current deal is with threads. Hopefully this is OK?
	        new Thread(() => {
	            try {
	                DictionaryListItemData[] words = new DictionaryListItemData[range];
	                IDbCommand dbcmd = dbconn.CreateCommand();
	                string sqlQuery = string.Format("SELECT lemma, definition FROM word as W JOIN sense as S on W.wordid=S.wordid JOIN synset as Y on S.synsetid=Y.synsetid ORDER BY W.wordid limit {0} OFFSET {1}", range, offset);
	                dbcmd.CommandText = sqlQuery;
	                IDataReader reader = dbcmd.ExecuteReader();
	                int count = 0;
	                while (reader.Read()) {
	                    string lemma = reader.GetString(0);
	                    string definition = reader.GetString(1);
	                    words[count] = new DictionaryListItemData {template = defaultTemplate};

	                    //truncate word if necessary
                        if (lemma.Length > maxWordCharacters) {
                            lemma = lemma.Substring(0, maxWordCharacters - 3) + "...";
                        }
                        words[count].word = lemma;

                        //Wrap definition
                        string[] wrds = definition.Split(' ');
	                    int charCount = 0;
	                    int lineCount = 0;
	                    foreach (string wrd in wrds) {
	                        charCount += wrd.Length + 1;
	                        if (charCount > definitionCharacterWrap) { //Guesstimate
	                            if (++lineCount >= maxDefinitionLines) {
                                    words[count].definition += "...";
	                                break;
	                            }
                                words[count].definition += "\n";
	                            charCount = 0;
	                        }
	                        words[count].definition += wrd + " ";
	                    }
	                    count++;
	                }
	                if (count < batchSize) {
	                    Debug.LogWarning("reached end");
	                }
	                reader.Close();
	                dbcmd.Dispose();
	                result(words);
	            } catch (Exception e) {
	                Debug.LogError("Exception reading from DB: " + e.Message);
	            }
	            dbLock = false;
	            loading = false;
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
	            } else if(currBatch != batchOffset) { //Jumped multiple batches. Get a whole new dataset
                    if(!loading)
                        cleanup = data;
                    loading = true;
                    GetWords((currBatch - 1) * batchSize, batchSize * 3, words => {
                        data = words;
	                    batchOffset = currBatch - 1;
	                });
	            }
	        } else if (batchOffset > 0 && -dataOffset < (batchOffset + 1) * batchSize) {
	            if (currBatch == batchOffset) { //Just one batch, fetch only the next one
                    GetWords((batchOffset - 1) * batchSize, batchSize, words => {
	                    Array.Copy(data, 0, data, batchSize, batchSize * 2);
	                    Array.Copy(words, 0, data, 0, batchSize);
	                    batchOffset--;
	                });
	            } else if (currBatch != batchOffset) { //Jumped multiple batches. Get a whole new dataset
                    if (!loading)
                        cleanup = data;
                    loading = true;
	                if (currBatch < 1)
	                    currBatch = 1;
                    GetWords((currBatch - 1) * batchSize, batchSize * 3, words => {
                        data = words;
	                    batchOffset = currBatch - 1;
	                });
	            }
	        }
	        if (cleanup != null) {
                //Clean up all existing gameobjects
                foreach (DictionaryListItemData item in cleanup) {
                    if (item.item != null) {
                        RecycleItem(item.template, item.item);
                        item.item = null;
                    }
                }
	            cleanup = null;
	        }

	        if (scrolling) {
	            scrollDelta = (scrollOffset - lastScrollOffset) / Time.deltaTime;
	            lastScrollOffset = scrollOffset;
	            if (scrollDelta > maxMomentum)
	                scrollDelta = maxMomentum;
                if (scrollDelta < -maxMomentum)
                    scrollDelta = -maxMomentum;
            } else {
	            scrollOffset += scrollDelta * Time.deltaTime;
	            if (scrollDelta > 0) {
	                scrollDelta -= scrollDamping * Time.deltaTime;
	                if (scrollDelta < 0) {
	                    scrollDelta = 0;
	                }
	            } else if (scrollDelta < 0) {
	                scrollDelta += scrollDamping * Time.deltaTime;
	                if (scrollDelta > 0) {
	                    scrollDelta = 0;
	                }
	            }
	        }
	        if (dataOffset >= dataLength) {
	            scrollReturn = scrollOffset;
	        }
        }
        
	    public void OnStartScrolling() {
	        scrolling = true;
	    }

        public void OnStopScrolling() {
            scrolling = false;
            if (scrollOffset > 0) {
                scrollOffset = 0;
                scrollDelta = 0;
            }
            if (scrollReturn < float.MaxValue) {
                scrollOffset = scrollReturn;
                scrollReturn = float.MaxValue;
                scrollDelta = 0;
            }
        }

        protected override void UpdateItems() {
            if (data == null || data.Length == 0 || loading) {
                loadingIndicator.SetActive(true);
                return;
            }
            for (int i = 0; i < data.Length; i++) {
                if (i + dataOffset + batchOffset * batchSize < -1) {        //Checking against -1 lets the first element overflow
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