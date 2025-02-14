using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRPGPerspective : MonoBehaviour
{
    public Transform cameraTransform; // ī�޶� ���󰡰� ����
    public float cameraHeight = 17f; // ī�޶� ����
    public float cameraDistance = 6f; // ĳ���Ϳ��� �Ÿ�
    public float cameraAngle = 45f; // ī�޶� ����

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraTransform != null)
        {
            UpdateCameraPosition();
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
