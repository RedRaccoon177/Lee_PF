using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform cameraTransform; // ī�޶� ���󰡰� ����
    public float cameraHeight = 10f; // ī�޶� ����
    public float cameraDistance = 6f; // ĳ���Ϳ��� �Ÿ�
    public float cameraAngle = 45f; // ī�޶� ����

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // ��Ŭ�� ����
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
            agent.SetDestination(hit.point); // Ŭ���� ��ġ�� �̵�
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