using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;

namespace CardSystem {
	public class DictionaryList : ListViewController<DictionaryListItemData, DictionaryListItem> {
	    public string dataFile;
	    public GameObject[] models;

        readonly Dictionary<string, AdvancedList.ModelPool> _models = new Dictionary<string, AdvancedList.ModelPool>();
        protected override void Setup() {
			//base.Setup();

            string conn = "URI=file:" + Application.dataPath + "/CardSystem/Examples/7. Dictionary/wordnet30.db"; //Path to database.
            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open(); //Open connection to the database.
            IDbCommand dbcmd = dbconn.CreateCommand();
            string sqlQuery = "SELECT lemma, definition FROM word as W JOIN sense as S on W.wordid=S.wordid JOIN synset as Y on S.synsetid=Y.synsetid ORDER BY W.wordid limit 100 OFFSET 0";
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read()) {
                string lemma = reader.GetString(0);
                string definition = reader.GetString(1);

                Debug.Log("word= " + lemma + "  def =" + definition);
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
            return;

        TextAsset text = Resources.Load<TextAsset>(dataFile);
		    if (text) {
		        JSONObject obj = new JSONObject(text.text);
                data = new DictionaryListItemData[obj.Count];
                for (int i = 0; i < data.Length; i++) {
                    data[i] = new DictionaryListItemData();
                    data[i].FromJSON(obj[i], this);
                }
		    } else data = new DictionaryListItemData[0];
            
        }

        protected override void UpdateItems() {
            int count = 0;
            //UpdateRecursively(data, ref count);
        }

	    void UpdateRecursively(DictionaryListItemData[] data, ref int count) {
	        foreach (DictionaryListItemData item in data) {
	            if (count + dataOffset < 0) {
	                ExtremeLeft(item);
	            } else if (count + dataOffset > numItems) {
	                ExtremeRight(item);
	            } else {
	                ListMiddle(item, count + dataOffset);
	            }
	            count++;
	            if (item.children != null) {
	                if (item.expanded) {
	                    UpdateRecursively(item.children, ref count);
	                } else {
	                    RecycleChildren(item);
	                }
	            }
	        }
	    }

	    void RecycleChildren(DictionaryListItemData data) {
            foreach (DictionaryListItemData child in data.children) {
                RecycleItem(child.template, child.item);
                child.item = null;
                if(child.children != null)
                    RecycleChildren(child);
            }
        }
        
        public GameObject GetModel(string name) {
            if (!_models.ContainsKey(name)) {
                Debug.LogWarning("Cannot get model, " + name + " doesn't exist");
                return null;
            }
            GameObject model = null;
            if (_models[name].pool.Count > 0) {
                model = _models[name].pool[0];
                _models[name].pool.RemoveAt(0);

                model.gameObject.SetActive(true);
            } else {
                model = Instantiate(_models[name].prefab);
                model.transform.parent = transform;
            }
	        return model;
	    }

        public class ModelPool {
            public readonly GameObject prefab;
            public readonly List<GameObject> pool = new List<GameObject>();

            public ModelPool(GameObject prefab) {
                if (prefab == null)
                    Debug.LogError("Template prefab cannot be null");
                this.prefab = prefab;
            }
        }
    }
}