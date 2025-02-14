using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform cameraTransform; // 카메라를 따라가게 설정
    public float cameraHeight = 10f; // 카메라 높이
    public float cameraDistance = 6f; // 캐릭터와의 거리
    public float cameraAngle = 45f; // 카메라 각도

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // 우클릭 감지
        {
            MoveToMousePosition();
        }

        if (cameraTransform != null)
        {
            UpdateCameraPosition();
        }
    }

    void MoveToMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            agent.SetDestination(hit.point); // 클릭한 위치로 이동
        }
    }

    void UpdateCameraPosition()
    {
        Vector3 cameraOffset = new Vector3(0, cameraHeight, -cameraDistance);
        Quaternion rotation = Quaternion.Euler(cameraAngle, 0, 0);
        cameraTransform.position = transform.position + rotation * cameraOffset;
        cameraTransform.LookAt(transform.position);
    }
}