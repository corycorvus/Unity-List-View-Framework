using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Random = System.Random;

namespace CardSystem {
	public class CardList : ListViewController<CardData, Card> {
	    public string defaultTemplate = "Card";
	    public float interpolate = 15f;
	    public float recycleDuration = 0.3f;
	    public Transform leftDeck, rightDeck;

	    private float scrollReturn = float.MaxValue;
	    private float itemHeight;

        protected override void Setup() {
			base.Setup();

            List<CardData> dataList = new List<CardData>(52);
            for (int i = 0; i < 4; i++) {
                for (int j = 1; j < 14; j++) {
                    CardData card = new CardData();
                    switch (j) {
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
            Random rnd = new Random();
            data = dataList.OrderBy(x => rnd.Next()).ToArray();
        }

	    void OnDrawGizmos() {
	        Gizmos.DrawWireCube(transform.position, new Vector3(range, itemSize.y, itemSize.z));
	    }

	    public void OnStopScrolling() {
	        if (scrollOffset > itemSize.x) {
	            scrollOffset = itemSize.x * 0.5f;
	        }
	        if (scrollReturn < float.MaxValue) {
	            scrollOffset = scrollReturn;
	            scrollReturn = float.MaxValue;
	        }
	    }

        protected override void UpdateItems() {
            for (int i = 0; i < data.Length; i++) {
                if (i + dataOffset < 0) {
                    ExtremeLeft(data[i]);
                } else if (i + dataOffset > numItems - 1) {
                    ExtremeRight(data[i]);
                } else {
                    ListMiddle(data[i], i);
                }
            }
        }

        protected override void ExtremeLeft(CardData data) {
            RecycleItemAnimated(data, leftDeck);
        }
        protected override void ExtremeRight(CardData data) {
            RecycleItemAnimated(data, rightDeck);
        }
        protected override void ListMiddle(CardData data, int offset) {
            if (data.item == null) {
                data.item = GetItem(data);
                if (offset + dataOffset > numItems / 2) {
                    data.item.transform.position = rightDeck.transform.position;
                    data.item.transform.rotation = rightDeck.transform.rotation;
                } else {
                    data.item.transform.position = leftDeck.transform.position;
                    data.item.transform.rotation = leftDeck.transform.rotation;
                }
            }
            Positioning(data.item.transform, offset);
        }

        protected override Card GetItem(CardData data) {
            if (data == null) {
                Debug.LogWarning("Tried to get item with null data");
                return null;
            }
            if (!_templates.ContainsKey(data.template)) {
                Debug.LogWarning("Cannot get item, template " + data.template + " doesn't exist");
                return null;
            }
            Card item = null;
            if (_templates[data.template].pool.Count > 0) {
                item = (Card)_templates[data.template].pool[0];
                _templates[data.template].pool.RemoveAt(0);

                item.gameObject.SetActive(true);
                item.Setup(data);
            } else {
                item = Instantiate(_templates[data.template].prefab).GetComponent<Card>();
                item.transform.parent = transform;
                item.Setup(data);
            }
            return item;
        }

	    void RecycleItemAnimated(CardData data, Transform destination) {
	        if (data.item == null)
	            return;
	        MonoBehaviour item = data.item;
	        data.item = null;
	        StartCoroutine(RecycleAnimation(item, data.template, destination, recycleDuration));
	    }
        IEnumerator RecycleAnimation(MonoBehaviour card, string template, Transform destination, float speed) {
            float start = Time.time;
            Quaternion startRot = card.transform.rotation;
            Vector3 startPos = card.transform.position;
            while (Time.time - start < speed) {
                card.transform.rotation = Quaternion.Lerp(startRot, destination.rotation, (Time.time - start) / speed);
                card.transform.position = Vector3.Lerp(startPos, destination.position, (Time.time - start) / speed);
                yield return null;
            }
            card.transform.rotation = destination.rotation;
            card.transform.position = destination.position;
            RecycleItem(template, card);
        }
        protected override void Positioning(Transform t, int offset) {
            t.position = Vector3.Lerp(t.position, leftSide + (offset * _itemSize.x + scrollOffset) * Vector3.right, interpolate * Time.deltaTime);
            t.rotation = Quaternion.Lerp(t.rotation, Quaternion.identity, interpolate * Time.deltaTime);
        }

    }
}