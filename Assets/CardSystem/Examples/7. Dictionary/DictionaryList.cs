using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;

namespace CardSystem {
	public class DictionaryList : ListViewController<DictionaryListItemData, DictionaryListItem> {
	    public string databasePath = "/CardSystem/Examples/7. Dictionary/wordnet30.db";
        public int batchSize = 10;
	    public string defaultTemplate = "DictionaryItem";

	    private DictionaryListItemData[] lastBatch, nextBatch;
	    IDbConnection dbconn;

        protected override void Setup() {
			base.Setup();

            string conn = "URI=file:" + Application.dataPath + databasePath;
            
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open(); //Open connection to the database.

            data = GetBatch(0, batchSize);
            nextBatch = GetBatch(batchSize, batchSize);
        }

	    void OnDestroy() {
            dbconn.Close();
            dbconn = null;
        }

	    DictionaryListItemData[] GetBatch(int offset, int size) {
	        DictionaryListItemData[] batch = new DictionaryListItemData[size];
            IDbCommand dbcmd = dbconn.CreateCommand();
            string sqlQuery = string.Format("SELECT lemma, definition FROM word as W JOIN sense as S on W.wordid=S.wordid JOIN synset as Y on S.synsetid=Y.synsetid ORDER BY W.wordid limit {0} OFFSET {1}", size, offset);
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
	        int count = 0;
            while (reader.Read()) {
                string lemma = reader.GetString(0);
                string definition = reader.GetString(1);

                //Debug.Log("word= " + lemma + "  def =" + definition);
                batch[count] = new DictionaryListItemData();
                batch[count].template = defaultTemplate;
                batch[count].word = lemma;

                //Wrap definition
                string[] words = definition.Split(' ');
                int charCount = 0;
                foreach (string word in words) {
                    charCount += word.Length + 1;
                    if (charCount > 40) { //Guesstimate
                        batch[count].definition += "\n";
                        charCount = 0;
                    }
                    batch[count].definition += word + " ";
                }
                count++;
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
	        return batch;
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

            if (-dataOffset > batchSize) {
                data = GetBatch(batchSize, batchSize);
                scrollOffset = 0;
            }
        }
        protected override void Positioning(Transform t, int offset) {
            t.position = leftSide + (offset * _itemSize.y + scrollOffset) * Vector3.down;
        }
    }
}