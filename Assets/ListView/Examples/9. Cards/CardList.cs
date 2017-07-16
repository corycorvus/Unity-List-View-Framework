using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

//Images sourced from http://web.stanford.edu/~jlewis8/cs148/pokerscene/

namespace ListView
{
    class CardList : ListViewController<CardData, Card, int>
    {
        [SerializeField]
        string m_DefaultTemplate = "Card";

        [SerializeField]
        float m_RecycleDuration = 0.3f;

        [SerializeField]
        bool m_AutoScroll;

        [SerializeField]
        Transform m_LeftDeck;

        [SerializeField]
        Transform m_RightDeck;

        [SerializeField]
        float m_Range;

        protected override float listHeight
        {
            get { return m_Data.Count * itemSize.x; }
        }

        void Awake()
        {
            size = m_Range * Vector3.right;
        }

        protected override void Setup()
        {
            base.Setup();

            var dataList = new List<CardData>(52);
            for (var i = 0; i < 4; i++)
            {
                for (var j = 1; j < 14; j++)
                {
                    var card = new CardData();
                    switch (j)
                    {
                        case 1:
                            card.value = "A";
                            break;
                        case 11:
                            card.value = "J";
                            break;
                        case 12:
                            card.value = "Q";
                            break;
                        case 13:
                            card.value = "K";
                            break;
                        default:
                            card.value = j + "";
                            break;
                    }
                    card.suit = (Card.Suit) i;
                    card.template = m_DefaultTemplate;
                    card.idx = i * 14 + j;
                    dataList.Add(card);
                }
            }
            var rnd = new Random();
            data = dataList.OrderBy(x => rnd.Next()).ToList();
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(m_Range, itemSize.y, itemSize.z));
        }

        protected override void ComputeConditions()
        {
            base.ComputeConditions();
            m_StartPosition = (m_Extents.x - itemSize.x * 0.5f) * Vector3.left;
        }

        protected override void UpdateItems()
        {
            if (m_AutoScroll)
            {
                scrollOffset -= scrollSpeed * Time.deltaTime;
                if (-scrollOffset > listHeight || scrollOffset >= 0)
                    scrollSpeed *= -1;
            }

            var doneSettling = true;
            var offset = 0f;
            for (var i = 0; i < m_Data.Count; i++)
            {
                var datum = m_Data[i];
                var localOffset = offset + scrollOffset;
                if (localOffset + itemSize.x < 0)
                {
                    ExtremeLeft(datum);
                }
                else if (localOffset > m_Size.x)
                {
                    //End the m_List one item early
                    ExtremeRight(datum);
                }
                else
                {
                    ListMiddle(datum, i, localOffset, ref doneSettling);
                }

                offset += itemSize.x;
            }
            m_LastScrollOffset = scrollOffset;

            if (m_Settling && doneSettling)
                EndSettling();
        }

        void ExtremeLeft(CardData data)
        {
            RecycleItemAnimated(data, m_LeftDeck);
        }

        void ExtremeRight(CardData data)
        {
            RecycleItemAnimated(data, m_RightDeck);
        }

        void ListMiddle(CardData data, int order, float offset, ref bool doneSettling)
        {
            Card card;
            var index = data.index;
            if (!m_ListItems.TryGetValue(index, out card))
            {
                card = GetItem(data);
                m_ListItems[index] = card;

                if (scrollOffset - m_LastScrollOffset < 0)
                {
                    card.transform.position = m_RightDeck.transform.position;
                    card.transform.rotation = m_RightDeck.transform.rotation;
                }
                else
                {
                    card.transform.position = m_LeftDeck.transform.position;
                    card.transform.rotation = m_LeftDeck.transform.rotation;
                }
                StartSettling();
            }

            UpdateItem(card.transform, order, offset, ref doneSettling);
        }

        void RecycleItemAnimated(CardData data, Transform destination)
        {
            var dataIndex = data.index;
            Card card;
            if (!m_ListItems.TryGetValue(dataIndex, out card))
                return;

            StartCoroutine(RecycleAnimation(card, data.template, destination, m_RecycleDuration));
            m_ListItems.Remove(dataIndex);
        }

        IEnumerator RecycleAnimation(Card card, string template, Transform destination, float speed)
        {
            var start = Time.time;
            var startRot = card.transform.rotation;
            var startPos = card.transform.position;
            while (Time.time - start < speed)
            {
                card.transform.rotation = Quaternion.Lerp(startRot, destination.rotation, (Time.time - start) / speed);
                card.transform.position = Vector3.Lerp(startPos, destination.position, (Time.time - start) / speed);
                yield return null;
            }
            card.transform.rotation = destination.rotation;
            card.transform.position = destination.position;

            RecycleItem(template, card);
        }

        protected override void UpdateItem(Transform t, int order, float offset, ref bool doneSettling)
        {
            var targetPosition = m_StartPosition + offset * Vector3.right;
            var targetRotation = Quaternion.identity;
            UpdateItemTransform(t, order, targetPosition, targetRotation, false, ref doneSettling);
        }

    }
}