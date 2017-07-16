using System.Linq;
using UnityEngine;

namespace ListView
{
    class CubeList : ListViewController<CubeItemData, CubeItem, int>
    {
        [SerializeField]
        float m_Range;

        [SerializeField]
        CubeItemData[] m_CubeData;

        void Awake()
        {
            size = Vector3.forward * m_Range;

            for (var i = 0; i < m_CubeData.Length; i++)
            {
                m_CubeData[i].idx = i;
            }

            data = m_CubeData.ToList();
        }

        protected override void Setup()
        {
            base.Setup();
            for (var i = 0; i < data.Count; i++)
            {
                data[i].text = i + "";
            }
        }
    }
}
