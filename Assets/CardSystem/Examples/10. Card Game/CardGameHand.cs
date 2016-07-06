using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Random = System.Random;

namespace CardSystem {
	public class CardGameHand : ListViewController<CardData, Card> {
        public float radius = 0.25f;
        public float interpolate = 15f;
	    public float stackOffset = 0.01f;
	    public int handSize = 5;
        public CardGameList controller;

	    private float cardDegrees, cardsOffset;

	    private new Vector3 itemSize {
	        get { return controller.itemSize; }
	    }

	    protected override void Setup() {
            data = new CardData[handSize];
	        for (int i = 0; i < handSize; i++) {
	            data[i] = controller.DrawCard();
	            data[i].item.transform.parent = transform;
	        }
	    }

	    protected override void ComputeConditions() {
            cardDegrees = Mathf.Atan((itemSize.x + padding) / radius) * Mathf.Rad2Deg;
	        cardsOffset = cardDegrees * (data.Length - 1) * 0.5f;
	    }

	    protected override void UpdateItems() {
            DebugDrawCircle(radius, 24, transform.position, transform.rotation);
            DebugDrawCircle(radius + itemSize.z, 24, transform.position, transform.rotation);
            for (int i = 0; i < data.Length; i++) {
                Positioning(data[i].item.transform, i);
            }
        }

	    protected override void Positioning(Transform t, int offset) {
	        Quaternion sliceRotation = Quaternion.AngleAxis(90 - cardsOffset + cardDegrees * offset, Vector3.up);
            t.localPosition = Vector3.Lerp(t.localPosition, 
                sliceRotation * (Vector3.left * radius) 
                + Vector3.up * stackOffset * offset, interpolate * Time.deltaTime);
            t.localRotation = Quaternion.Lerp(t.localRotation, sliceRotation * Quaternion.AngleAxis(90, Vector3.up), interpolate * Time.deltaTime);
        }

        public static void DebugDrawCircle(float radius, int slices, Vector3 center) {
            DebugDrawCircle(radius, slices, center, Quaternion.identity);
        }

        public static void DebugDrawCircle(float radius, int slices, Vector3 center, Quaternion orientation) {
            for (var i = 0; i < slices; i++) {
                Debug.DrawLine(
                    center + orientation * Quaternion.AngleAxis(((float)360 * (i)) / slices, Vector3.up) * Vector3.forward * radius,
                    center + orientation * Quaternion.AngleAxis(((float)360 * (i + 1)) / slices, Vector3.up) * Vector3.forward * radius);
            }
        }

	    public void DrawCard(Card item) {
	        if (data.Length < handSize) {
                List<CardData> newData = new List<CardData>(data);
                newData.Add(item.data);
	            data = newData.ToArray();
	            controller.RemoveCardFromDeck(item.data);
	            item.transform.parent = transform;
	        } else {
                //TODO: Message to user
	            Debug.Log("Can't draw card, hand is full!");
	        }
	    }

	    public void ReturnCard(Card item) {
            if (data.Contains(item.data)) { 
                List<CardData> newData = new List<CardData>(data);
                newData.Remove(item.data);
                data = newData.ToArray();
                controller.AddCardToDeck(item.data);
            } else {
                //TODO: Message to user
                Debug.Log("Something went wrong... This card is not in your hand");
            }
        }
	}
}