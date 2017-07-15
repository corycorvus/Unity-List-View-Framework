using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace ListView
{
    class CardGameList : ListViewController<CardData, Card, int>
    {
        public string defaultTemplate = "Card";
        public float interpolate = 15f;
        public float recycleDuration = 0.3f;
        public int dealMax = 5;
        public Transform deck;

        Vector3 m_StartPos;


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
            Shuffle(dataList);
        }

        void Shuffle(List<CardData> dataList)
        {
            var rnd = new Random();
            data = dataList.OrderBy(x => rnd.Next()).ToList();
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position + Vector3.left * (itemSize.x * dealMax - listHeight) * 0.5f,
                new Vector3(listHeight, itemSize.y, itemSize.z));
        }

        protected override void UpdateItems()
        {
            m_StartPos = transform.position + Vector3.left * itemSize.x * dealMax * 0.5f;
            var doneSettling = true;
            var offset = 0f;
            for (var i = 0; i < data.Count; i++)
            {
                var datum = data[i];
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

            if (m_Settling && doneSettling)
                EndSettling();
        }

        void ExtremeLeft(CardData data)
        {
            RecycleItemAnimated(data, deck);
        }

        void ExtremeRight(CardData data)
        {
            RecycleItemAnimated(data, deck);
        }

        void ListMiddle(CardData data, int order, float offset, ref bool doneSettling)
        {
            Card card;
            var index = data.index;
            if (!m_ListItems.TryGetValue(index, out card))
            {
                card = GetItem(data);
                card.transform.position = deck.transform.position;
                card.transform.rotation = deck.transform.rotation;
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
            t.position = Vector3.Lerp(t.position, m_StartPos + (offset * itemSize.x + scrollOffset) * Vector3.right,
                interpolate * Time.deltaTime);
            t.rotation = Quaternion.Lerp(t.rotation, Quaternion.identity, interpolate * Time.deltaTime);
        }

        void RecycleCard(CardData data)
        {
            RecycleItemAnimated(data, deck);
        }

        public CardData DrawCard()
        {
            if (data.Count == 0)
            {
                Debug.Log("Out of Cards");
                return null;
            }

            var cardData = data.Last();
            data.Remove(cardData);

            var index = cardData.index;
            Card card;
            if (!m_ListItems.TryGetValue(index, out card))
            {
                card = GetItem(cardData);
                m_ListItems[index] = card;
            }

            return cardData;
        }

        public void RemoveCardFromDeck(CardData cardData)
        {
            data.Remove(cardData);
        }

        public void AddCardToDeck(CardData cardData)
        {
            data.Add(cardData);

            // TODO: Remove if unnecessary
            //cardData.item.transform.parent = transform;
            RecycleCard(cardData);
        }

        public void Deal()
        {
            //range += itemSize.x;
            if (listHeight >= itemSize.x * (dealMax + 1))
            {
                scrollOffset -= itemSize.x * dealMax;
                //range = 0;
            }
            if (-scrollOffset >= (data.Count - dealMax) * itemSize.x)
            {
                //reshuffle
                Shuffle(new List<CardData>(data));
                scrollOffset = itemSize.x * 0.5f;
            }
        }
    }
}