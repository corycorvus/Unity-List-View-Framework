using UnityEngine;

namespace ListView
{
    public abstract class ListViewItemData<TIndex>
    {
        public virtual TIndex index { get; protected set; }

        public string template
        {
            get { return m_Template; }
            set { m_Template = value; }
        }

        [SerializeField]
        string m_Template;
    }

    [System.Serializable]
    public class ListViewItemInspectorData : ListViewItemData<int> { }
}
