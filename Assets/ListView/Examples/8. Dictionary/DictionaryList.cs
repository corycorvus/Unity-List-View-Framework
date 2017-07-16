using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using UnityEngine;

//Borrows from http://answers.unity3d.com/questions/743400/database-sqlite-setup-for-unity.html
//Dictionary from https://wordnet.princeton.edu/

namespace ListView
{
    public class DictionaryList : ListViewController<DictionaryListItemData, DictionaryListItem, KeyValuePair<int, int>>
    {
        public const string editorDatabasePath = "ListView/Examples/8. Dictionary/wordnet30.db";
        public const string databasePath = "wordnet30.db";

        [SerializeField]
        int m_BatchSize = 15;

        [SerializeField]
        string m_DefaultTemplate = "DictionaryItem";

        [SerializeField]
        GameObject m_LoadingIndicator;

        [SerializeField]
        int m_MaxWordCharacters = 30; //Wrap word after 30 characters

        [SerializeField]
        int m_DefinitionCharacterWrap = 40; //Wrap definition after 40 characters

        [SerializeField]
        int m_MaxDefinitionLines = 4; //Max 4 lines per definition

        [SerializeField]
        float m_Range = 12f;

        delegate void WordsResult(List<DictionaryListItemData> words);

        protected override float listHeight { get { return m_DataLength; } }

        volatile bool m_DBLock;

        List<DictionaryListItemData> m_Cleanup;
        int m_DataLength; //Total number of items in the data set
        int m_BatchOffset; //Number of batches we are offset
        bool m_Loading;

        IDbConnection m_DBConnection;

        void Awake()
        {
            size = Vector3.forward * m_Range;
        }

        protected override void Setup()
        {
            base.Setup();

#if UNITY_EDITOR
            string conn = "URI=file:" + Path.Combine(Application.dataPath, editorDatabasePath);
#else
            string conn = "URI=file:" + Path.Combine(Application.dataPath, databasePath);
#endif

            m_DBConnection = new SqliteConnection(conn);
            m_DBConnection.Open(); //Open connection to the database.

            if (m_MaxWordCharacters < 4)
            {
                Debug.LogError("Max word length must be > 3");
            }

            try
            {
                var dbcmd = m_DBConnection.CreateCommand();
                var sqlQuery = "SELECT COUNT(lemma) FROM word as W JOIN sense as S on W.wordid=S.wordid JOIN synset as Y on S.synsetid=Y.synsetid";
                dbcmd.CommandText = sqlQuery;
                var reader = dbcmd.ExecuteReader();
                while (reader.Read())
                {
                    m_DataLength = reader.GetInt32(0);
                }
                reader.Close();
                dbcmd.Dispose();
            } catch
            {
                Debug.LogError("DB error, couldn't get total data length");
            }

            data = null;
            //Start off with some data
            GetWords(0, m_BatchSize * 3, words => { data = words; });
        }

        void OnDestroy()
        {
            m_DBConnection.Close();
            m_DBConnection = null;
        }

        void GetWords(int offset, int range, WordsResult result)
        {
            if (m_DBLock)
            {
                return;
            }
            if (result == null)
            {
                Debug.LogError("Called GetWords without a result callback");
                return;
            }
            m_DBLock = true;
            //Not sure what the current deal is with threads. Hopefully this is OK?
            new Thread(() =>
            {
                try
                {
                    var words = new List<DictionaryListItemData>(range);
                    var dbcmd = m_DBConnection.CreateCommand();
                    var sqlQuery = string.Format("SELECT W.wordid, Y.synsetid, lemma, definition FROM word as W JOIN sense as S on W.wordid=S.wordid JOIN synset as Y on S.synsetid=Y.synsetid ORDER BY W.wordid limit {0} OFFSET {1}", range, offset);
                    dbcmd.CommandText = sqlQuery;
                    var reader = dbcmd.ExecuteReader();
                    var count = 0;
                    while (reader.Read())
                    {
                        var wordid = reader.GetInt32(0);
                        var synsetid = reader.GetInt32(1);
                        var id = new KeyValuePair<int, int>(wordid, synsetid);
                        var lemma = reader.GetString(2);
                        var definition = reader.GetString(3);
                        var word = new DictionaryListItemData { idx = id, template = m_DefaultTemplate};
                        words.Add(word);

                        //truncate word if necessary
                        if (lemma.Length > m_MaxWordCharacters)
                        {
                            lemma = lemma.Substring(0, m_MaxWordCharacters - 3) + "...";
                        }
                        words[count].word = lemma;

                        //Wrap definition
                        var wrds = definition.Split(' ');
                        var charCount = 0;
                        var lineCount = 0;
                        foreach (var wrd in wrds)
                        {
                            charCount += wrd.Length + 1;
                            if (charCount > m_DefinitionCharacterWrap)
                            { //Guesstimate
                                if (++lineCount >= m_MaxDefinitionLines)
                                {
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
                    if (count < m_BatchSize)
                    {
                        Debug.LogWarning("reached end");
                    }
                    reader.Close();
                    dbcmd.Dispose();
                    result(words);
                } catch (Exception e)
                {
                    Debug.LogError("Exception reading from DB: " + e.Message);
                }
                m_DBLock = false;
                m_Loading = false;
            }).Start();
        }

        protected override void ComputeConditions()
        {
            base.ComputeConditions();
            m_StartPosition = m_Extents - itemSize * 0.5f;

            var dataOffset = (int)(-scrollOffset / itemSize.z);

            var currBatch = dataOffset / m_BatchSize;
            if (dataOffset > (m_BatchOffset + 2) * m_BatchSize)
            {
                //Check how many batches we jumped
                if (currBatch == m_BatchOffset + 2) // Just one batch, fetch only the previous one
                {
                    GetWords((m_BatchOffset + 3) * m_BatchSize, m_BatchSize, words =>
                    {
                        data.RemoveRange(0, m_BatchSize);
                        data.AddRange(words);
                        m_BatchOffset++;
                    });
                }
                else if (currBatch != m_BatchOffset) // Jumped multiple batches. Get a whole new dataset
                {
                    if (!m_Loading)
                        m_Cleanup = data;

                    m_Loading = true;
                    GetWords((currBatch - 1) * m_BatchSize, m_BatchSize * 3, words =>
                    {
                        data = words;
                        m_BatchOffset = currBatch - 1;
                    });
                }
            }
            else if (m_BatchOffset > 0 && dataOffset < (m_BatchOffset + 1) * m_BatchSize)
            {
                if (currBatch == m_BatchOffset) // Just one batch, fetch only the next one
                {
                    GetWords((m_BatchOffset - 1) * m_BatchSize, m_BatchSize, words =>
                    {
                        data.RemoveRange(m_BatchSize * 2, m_BatchSize);
                        words.AddRange(data);
                        m_Data = words;
                        m_BatchOffset--;
                    });
                }
                else if (currBatch != m_BatchOffset) // Jumped multiple batches. Get a whole new dataset
                {
                    if (!m_Loading)
                        m_Cleanup = data;

                    m_Loading = true;
                    if (currBatch < 1)
                        currBatch = 1;
                    GetWords((currBatch - 1) * m_BatchSize, m_BatchSize * 3, words =>
                    {
                        data = words;
                        m_BatchOffset = currBatch - 1;
                    });
                }
            }

            if (m_Cleanup != null)
            {
                //Clean up all visible items
                foreach (var data in m_Cleanup)
                {
                    if (data == null)
                        continue;

                    var index = data.index;
                    DictionaryListItem item;
                    if (m_ListItems.TryGetValue(index, out item))
                        Recycle(index);
                }

                m_Cleanup = null;
            }
        }

        public void OnStartScrolling()
        {
            m_Scrolling = true;
        }

        public void OnStopScrolling()
        {
            m_Scrolling = false;
            if (scrollOffset > 0)
            {
                scrollOffset = 0;
                m_ScrollDelta = 0;
            }
            if (m_ScrollReturn < float.MaxValue)
            {
                scrollOffset = m_ScrollReturn;
                m_ScrollReturn = float.MaxValue;
                m_ScrollDelta = 0;
            }
        }

        protected override void UpdateItems()
        {
            if (data == null || data.Count == 0 || m_Loading)
            {
                m_LoadingIndicator.SetActive(true);
                return;
            }

            base.UpdateItems();

            m_LoadingIndicator.SetActive(false);
        }
    }
}