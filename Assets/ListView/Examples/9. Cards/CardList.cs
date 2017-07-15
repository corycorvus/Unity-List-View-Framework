using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

//Images sourced from http://web.stanford.edu/~jlewis8/cs148/pokerscene/

namespace ListView
{
    class CardList : ListViewController<CardData, Card, int>
    {
        public string defaultTemplate = "Card";
        public float interpolate = 15f;
        public float recycleDuration = 0.3f;
        public bool autoScroll;
        public float scrollSpeed = 1;
        public Transform leftDeck, rightDeck;

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
                    card.template = defaultTemplate;
                    dataList.Add(card);
                }
            }
            var rnd = new Random();
            data = dataList.OrderBy(x => rnd.Next()).ToList();
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(listHeight, itemSize.y, itemSize.z));
        }

        public void OnStopScrolling()
        {
            if (scrollOffset > itemSize.x)
            { //Let us over-scroll one whole card
                scrollOffset = itemSize.x * 0.5f;
            }
            if (m_ScrollReturn < float.MaxValue)
            {
                scrollOffset = m_ScrollReturn;
                m_ScrollReturn = float.MaxValue;
            }
        }

        protected override void ComputeConditions()
        {
            base.ComputeConditions();
            m_StartPosition = (m_Extents.z - itemSize.z * 0.5f) * Vector3.left;
        }

        protected override void UpdateItems()
        {
            if (autoScroll)
            {
                scrollOffset -= scrollSpeed * Time.deltaTime;
                if (-scrollOffset > listHeight || scrollOffset >= 0)
                    scrollSpeed *= -1;
            }

            var doneSettling = true;
            var offset = 0f;
            var order = 0;
            for (var i = 0; i < m_Data.Count; i++)
            {
                var datum = m_Data[i];
                if (offset + scrollOffset + itemSize.z < 0)
                {
                    ExtremeLeft(datum);
                }
                else if (offset + scrollOffset > m_Size.z)
                {
                    //End the m_List one item early
                    ExtremeRight(datum);
                }
                else
                {
                    ListMiddle(datum, i, offset, ref doneSettling);
                }
            }
            m_LastScrollOffset = scrollOffset;

            if (m_Settling && doneSettling)
                EndSettling();
        }

        void ExtremeLeft(CardData data)
        {
            RecycleItemAnimated(data, leftDeck);
        }

        void ExtremeRight(CardData data)
        {
            RecycleItemAnimated(data, rightDeck);
        }

        void ListMiddle(CardData data, int order, float offset, ref bool doneSettling)
        {
            var index = data.index;
            Card card;
            if (!m_ListItems.TryGetValue(index, out card))
            {
                card = GetItem(data);
                if (scrollOffset - m_LastScrollOffset < 0)
                {
                    card.transform.position = rightDeck.transform.position;
                    card.transform.rotation = rightDeck.transform.rotation;
                } else
                {
                    card.transform.position = leftDeck.transform.position;
                    card.transform.rotation = leftDeck.transform.rotation;
                }

                m_ListItems[index] = card;
            }
            UpdateVisibleItem(data, order, offset, ref doneSettling);
        }

        void RecycleItemAnimated(CardData data, Transform destination)
        {
            Card card;
            if (m_ListItems.TryGetValue(data.index, out card))
                StartCoroutine(RecycleAnimation(card, data.template, destination, recycleDuration));
        }

        IEnumerator RecycleAnimation(Card card, string template, Transform destination, float speed)
        {
            float start = Time.time;
            Quaternion startRot = card.transform.rotation;
            Vector3 startPos = card.transform.position;
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
            t.position = Vector3.Lerp(t.position, m_StartPosition + (offset * itemSize.x + scrollOffset) * Vector3.right, interpolate * Time.deltaTime);
            t.rotation = Quaternion.Lerp(t.rotation, Quaternion.identity, interpolate * Time.deltaTime);
        }

    }
}