using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRPGPerspective : MonoBehaviour
{
    public Transform cameraTransform; // 카메라를 따라가게 설정
    public float cameraHeight = 17f; // 카메라 높이
    public float cameraDistance = 6f; // 캐릭터와의 거리
    public float cameraAngle = 45f; // 카메라 각도

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
