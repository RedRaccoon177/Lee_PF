using UnityEngine;

public class OverlapBoxExample : MonoBehaviour
{
    public Vector3 boxSize = new Vector3(2f, 2f, 2f); // 박스 크기
    public Vector3 boxOffset = Vector3.zero; // 박스 위치 오프셋
    public LayerMask targetLayer; // 감지할 레이어

    void Update()
    {
        Vector3 boxCenter = transform.position + boxOffset; // 박스의 중심

        // OverlapBox를 사용하여 박스 내에 존재하는 모든 Collider 가져오기
        Collider[] hitColliders = Physics.OverlapBox(boxCenter, boxSize / 2, Quaternion.identity, targetLayer);

        foreach (Collider col in hitColliders)
        {
            Debug.Log($"감지된 오브젝트: {col.gameObject.name}");
        }
    }

    // 박스가 어디에 있는지 확인하기 위해 기즈모 표시
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(transform.position + boxOffset, Quaternion.identity, boxSize);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
