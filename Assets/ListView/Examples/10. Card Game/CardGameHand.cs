using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ListView
{
    class CardGameHand : ListViewController<CardData, Card, int>
    {
        [SerializeField]
        float m_Radius = 0.25f;

        [SerializeField]
        float m_Interpolate = 15f;

        [SerializeField]
        float m_StackOffset = 0.01f;

        [SerializeField]
        int m_HandSize = 5;

        [SerializeField]
        float m_IndicatorTime = 0.5f;

        [SerializeField]
        CardGameList m_Controller;

        [SerializeField]
        Renderer m_Indicator;

        float m_CardDegrees, m_CardsOffset;

        new Vector3 itemSize { get { return m_Controller.itemSize; } }

        protected override void Setup()
        {
            base.Setup();
            data = new List<CardData>(m_HandSize);
            for (var i = 0; i < m_HandSize; i++)
            {
                CardData cardData;
                var card = m_Controller.DrawCard(out cardData);
                card.transform.parent = transform;
                m_ListItems[cardData.index] = card;
                data.Add(cardData);
            }
        }

        protected override void ComputeConditions()
        {
            m_CardDegrees = Mathf.Atan((itemSize.x + m_Padding) / m_Radius) * Mathf.Rad2Deg;
            m_CardsOffset = m_CardDegrees * (data.Count - 1) * 0.5f;
        }

        protected override void UpdateItems()
        {
            DebugDrawCircle(m_Radius - itemSize.z * 0.5f, 24, transform.position, transform.rotation);
            DebugDrawCircle(m_Radius + itemSize.z * 0.5f, 24, transform.position, transform.rotation);
            var doneSettling = true;
            for (var i = 0; i < data.Count; i++)
            {
                UpdateVisibleItem(data[i], i, i, ref doneSettling);
            }
        }

        protected override void UpdateItem(Transform t, int offset, float f, ref bool doneSettling)
        {
            var sliceRotation = Quaternion.AngleAxis(90 - m_CardsOffset + m_CardDegrees * offset, Vector3.up);
            t.localPosition = Vector3.Lerp(t.localPosition,
                sliceRotation * (Vector3.left * m_Radius)
                + Vector3.up * m_StackOffset * offset, m_Interpolate * Time.deltaTime);
            t.localRotation = Quaternion.Lerp(t.localRotation, sliceRotation * Quaternion.AngleAxis(90, Vector3.down),
                m_Interpolate * Time.deltaTime);
        }

        public static void DebugDrawCircle(float radius, int slices, Vector3 center)
        {
            DebugDrawCircle(radius, slices, center, Quaternion.identity);
        }

        public static void DebugDrawCircle(float radius, int slices, Vector3 center, Quaternion orientation)
        {
            for (var i = 0; i < slices; i++)
            {
                Debug.DrawLine(
                    center + orientation * Quaternion.AngleAxis((float) 360 * i / slices, Vector3.up) *
                    Vector3.forward * radius,
                    center + orientation * Quaternion.AngleAxis((float) 360 * (i + 1) / slices, Vector3.up) *
                    Vector3.forward * radius);
            }
        }

        public void DrawCard(Card item)
        {
            if (data.Count < m_HandSize)
            {
                var cardData = item.data;
                data.Add(cardData);
                m_ListItems[cardData.index] = item;
                m_Controller.RemoveCardFromDeck(cardData);
                item.transform.parent = transform;
            }
            else
            {
                Indicate();
                Debug.Log("Can't draw card, hand is full!");
            }
        }

        public void ReturnCard(Card item)
        {
            var cardData = item.data;
            if (data.Contains(cardData))
            {
                data.Remove(cardData);
                var card = m_ListItems[cardData.index];
                m_ListItems.Remove(cardData.index);
                m_Controller.AddCardToDeck(card);
            }
            else
            {
                Indicate();
                Debug.Log("Something went wrong... This card is not in your hand");
            }
        }

        void Indicate()
        {
            StartCoroutine(DoIndicate());
        }

        IEnumerator DoIndicate()
        {
            m_Indicator.enabled = true;
            yield return new WaitForSeconds(m_IndicatorTime);
            m_Indicator.enabled = false;
        }
    }
}