using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveIconArrowEffect : MonoBehaviour
{
    [Header("아이콘 화살표 위아래 이동 속도")] 
    public float _floatSpeed = 1f;
    [Header("아이콘 화살표 떠오르는 높이")]
    public float _floatAmplitude = 0.5f;

    private Vector3 _startPos;
    public LayerMask _targetLayer;

    public PlayerController _controller;

    void Start()
    {
        _startPos = transform.position;
    }

    void Update()
    {
        if(_controller._IsPlayerContactMoveIcon == false)
        {
            // 떠오르는 효과
            float newY = _startPos.y + Mathf.Sin(Time.time * _floatSpeed) * _floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(0, -99, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _targetLayer) != 0)
        {
            _controller._IsPlayerContactMoveIcon = true;
            transform.position = new Vector3(0, -99, 0);
        }
    }
}
