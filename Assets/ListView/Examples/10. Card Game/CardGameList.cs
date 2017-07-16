using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace ListView
{
    class CardGameList : ListViewController<CardData, Card, int>
    {
        public string defaultTemplate = "Card";
        public float interpolate = 15f;
        public float recycleDuration = 0.3f;
        public int dealMax = 5;

        [FormerlySerializedAs("deck")]
        [SerializeField]
        public Transform m_Deck;

        [SerializeField]
        float m_Range;

        protected override void Setup()
        {
            base.Setup();

            data = new List<CardData>(52);
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
                    card.idx = i * 14 + j;
                    data.Add(card);
                }
            }
            Shuffle();

            m_Range = 0;
            m_ScrollOffset = itemSize.x * 0.5f;
        }

        void Shuffle()
        {
            var rnd = new Random();
            data = data.OrderBy(x => rnd.Next()).ToList();
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + Vector3.left * (itemSize.x * dealMax - m_Range) * 0.5f,
                new Vector3(m_Range, itemSize.y, itemSize.z));
        }

        protected override void UpdateItems()
        {
            m_StartPosition = Vector3.left * itemSize.x * dealMax * 0.5f;
            size = m_Range * Vector3.right;
            var doneSettling = true;
            var offset = 0f;
            for (var i = 0; i < data.Count; i++)
            {
                var datum = data[i];
                var localOffset = offset + scrollOffset;
                if (localOffset < 0)
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

            if (m_Settling && doneSettling)
                EndSettling();
        }

        public override void OnScrollEnded()
        {
        }

        void ExtremeLeft(CardData data)
        {
            RecycleItemAnimated(data, m_Deck);
        }

        void ExtremeRight(CardData data)
        {
            RecycleItemAnimated(data, m_Deck);
        }

        void ListMiddle(CardData data, int order, float offset, ref bool doneSettling)
        {
            Card card;
            var index = data.index;
            if (!m_ListItems.TryGetValue(index, out card))
            {
                card = GetItem(data);
                card.transform.position = m_Deck.transform.position;
                card.transform.rotation = m_Deck.transform.rotation;
                m_ListItems[index] = card;
            }

            UpdateItem(card.transform, order, offset, ref doneSettling);
        }

        protected override Card GetItem(CardData data)
        {
            if (data == null)
            {
                Debug.LogWarning("Tried to get item with null data");
                return null;
            }

            if (!m_TemplateDictionary.ContainsKey(data.template))
            {
                Debug.LogWarning("Cannot get item, template " + data.template + " doesn't exist");
                return null;
            }

            Card item;
            if (m_TemplateDictionary[data.template].pool.Count > 0)
            {
                item = m_TemplateDictionary[data.template].pool[0];
                m_TemplateDictionary[data.template].pool.RemoveAt(0);

                item.gameObject.SetActive(true);
                item.GetComponent<BoxCollider>().enabled = true;
                item.Setup(data);
            }
            else
            {
                item = Instantiate(m_TemplateDictionary[data.template].prefab).GetComponent<Card>();
                item.transform.parent = transform;
                item.Setup(data);
            }

            return item;
        }

        void RecycleItemAnimated(CardData data, Transform destination)
        {
            Card card;
            var index = data.index;
            if (!m_ListItems.TryGetValue(index, out card))
                return;

            m_ListItems.Remove(index);

            card.GetComponent<BoxCollider>().enabled = false; //Disable collider so we can't click the card during the animation
            StartCoroutine(RecycleAnimation(card, data.template, destination, recycleDuration));
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

        void RecycleCard(CardData data)
        {
            RecycleItemAnimated(data, m_Deck);
        }

        public Card DrawCard(out CardData cardData)
        {
            if (data.Count == 0)
            {
                Debug.Log("Out of Cards");
                cardData = null;
                return null;
            }

            cardData = data.Last();
            data.Remove(cardData);

            var index = cardData.index;
            Card card;
            if (m_ListItems.TryGetValue(index, out card))
                m_ListItems.Remove(index);
            else
                card = GetItem(cardData);

            return card;
        }

        public void RemoveCardFromDeck(CardData cardData)
        {
            data.Remove(cardData);
            m_ListItems.Remove(cardData.index);
            if (m_Range > 0)
                m_Range -= itemSize.x;
        }

        public void AddCardToDeck(Card card)
        {
            var cardData = card.data;
            data.Add(cardData);

            m_ListItems[cardData.index] = card;
            card.transform.parent = transform;
            RecycleCard(cardData);
        }

        public void Deal()
        {
            m_Range += itemSize.x;
            if (m_Range >= itemSize.x * (dealMax + 1))
            {
                scrollOffset -= itemSize.x * dealMax;
                m_Range = 0;
            }
            if (-scrollOffset >= (data.Count - dealMax) * itemSize.x)
            {
                //reshuffle
                Shuffle();
                scrollOffset = itemSize.x * 0.5f;
            }
        }
    }
}