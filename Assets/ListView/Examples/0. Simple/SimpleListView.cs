using UnityEngine;

namespace ListView
{
    public class SimpleListView : MonoBehaviour
    {
        [SerializeField]
        GameObject m_Prefab;

        [SerializeField]
        int m_DataOffset;

        [SerializeField]
        float m_ItemHeight = 1;

        [SerializeField]
        int m_Range = 5;

        [SerializeField]
        string[] m_Data;

        [SerializeField]
        GUISkin m_Skin;

        TextMesh[] m_Items;

        void Start()
        {
            m_Items = new TextMesh[m_Range];
            for (var i = 0; i < m_Range; i++)
            {
                m_Items[i] = Instantiate(m_Prefab).GetComponent<TextMesh>();
                m_Items[i].transform.position = transform.position + Vector3.down * i * m_ItemHeight;
                m_Items[i].transform.parent = transform;
            }

            UpdateList();
        }

        void UpdateList()
        {
            for (var i = 0; i < m_Range; i++)
            {
                var dataIdx = i + m_DataOffset;
                if (dataIdx >= 0 && dataIdx < m_Data.Length)
                {
                    m_Items[i].text = m_Data[dataIdx];
                }
                else
                {
                    m_Items[i].text = "";
                }
            }
        }

        void OnGUI()
        {
            GUI.skin = m_Skin;
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            GUILayout.Label("This is an overly simplistic m_List view. Click the buttons below to scroll, or modify Data Offset in the inspector");
            if (GUILayout.Button("Scroll Next"))
            {
                ScrollNext();
            }

            if (GUILayout.Button("Scroll Prev"))
            {
                ScrollPrev();
            }

            GUILayout.EndArea();
        }

        void ScrollNext()
        {
            m_DataOffset++;
            UpdateList();
        }

        void ScrollPrev()
        {
            m_DataOffset--;
            UpdateList();
        }
    }
}
