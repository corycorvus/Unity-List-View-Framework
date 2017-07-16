using UnityEngine;

class MouseWiggle : MonoBehaviour
{
    [SerializeField]
    float m_Speed = -2f;

    [SerializeField]
    Transform m_Pivot;

    void Update()
    {
        transform.RotateAround(m_Pivot.position, Vector3.up, Input.GetAxis("Mouse X") * Time.deltaTime * m_Speed);
    }
}
