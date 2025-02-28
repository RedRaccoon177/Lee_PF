using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    Vector3 _playerPosition;
    Vector3 _destination;
    NavMeshAgent _agent;

    Vector3 _skillQRange = new Vector3(4f, 2f, 4f); // 박스 크기
    Vector3 _skillQCenter = new Vector3(0f, 1.5f, 1f); // 박스 위치 오프셋

    LayerMask _layerMask;

    Animator _animator;

    void Start()
    {
        _layerMask = LayerMask.GetMask("Player");
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();

        if (ShooterController._instance != null)
        {
            ShooterController._instance.OnShooterPositionChanged += UpdatePlayerPosition;
        }

        StartCoroutine(MoveToTarget());
        StartCoroutine(AttackPlayer());
    }

    void OnDisable()
    {
        if (ShooterController._instance != null)
        {
            ShooterController._instance.OnShooterPositionChanged -= UpdatePlayerPosition;
        }
    }
    void OnDrawGizmos()
    {
        // 몬스터의 평타 공격 범위
        Gizmos.color = Color.red;
        // 기즈모가 그려질 위치 (플레이어 기준 항상 앞으로!)
        Vector3 _playerSkillQPositionGizmo = transform.position
                                + transform.right * _skillQCenter.x  // 좌우 이동
                                + transform.up * _skillQCenter.y     // 높이 이동
                                + transform.forward * _skillQCenter.z; // 항상 플레이어 앞을 기준

        // 회전 적용 (항상 플레이어 회전 유지)
        Gizmos.matrix = Matrix4x4.TRS(_playerSkillQPositionGizmo, transform.rotation, _skillQRange);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    void UpdatePlayerPosition(Vector3 newPosition)
    {
        _playerPosition = newPosition;
    }

    IEnumerator MoveToTarget()
    {
        while (true)
        {
            if (_playerPosition != null && _destination != _playerPosition)
            {
                _agent.SetDestination(_playerPosition);
                _destination = _playerPosition;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator AttackPlayer()
    {
        while (true)
        {
            // Gizmos와 동일한 위치 적용!
            Vector3 worldCenter = transform.TransformPoint(_skillQCenter);

            // 동일한 크기, 회전 적용!
            Collider[] _playerCol = Physics.OverlapBox(
                worldCenter,         // 월드 좌표 기준 중심
                _skillQRange / 2,    // 박스 크기
                transform.rotation,  // 회전값 적용
                _layerMask           // 감지할 레이어
            );

            foreach (Collider col in _playerCol)
            {
                ShooterController player = col.GetComponent<ShooterController>();
                if (player != null)
                {
                    _animator.SetTrigger("IsAttack");
                    yield return new WaitForSeconds(0.7f);
                    StartCoroutine(DamagePlayer());
                }
                else
                {
                    Debug.LogWarning("ShooterController가 없는 Collider 감지됨: " + col.gameObject.name);
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator DamagePlayer()
    {
        // Gizmos와 동일한 위치 적용!
        Vector3 worldCenter = transform.TransformPoint(_skillQCenter);

        // 동일한 크기, 회전 적용!
        Collider[] _playerCol = Physics.OverlapBox(
            worldCenter,         // 월드 좌표 기준 중심
            _skillQRange / 2,    // 박스 크기
            transform.rotation,  // 회전값 적용
            _layerMask           // 감지할 레이어
        );

        foreach (Collider col in _playerCol)
        {
            ShooterController player = col.GetComponent<ShooterController>();
            if (player != null)
            {
                player.TakeDamage(10);
            }
            else
            {
                Debug.LogWarning("ShooterController가 없는 Collider 감지됨: " + col.gameObject.name);
            }
        }

        yield break;
    }

    public void ResetEnemy()
    {
        _layerMask = LayerMask.GetMask("Player");
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();

        if (ShooterController._instance != null)
        {
            ShooterController._instance.OnShooterPositionChanged += UpdatePlayerPosition;
        }

        StartCoroutine(MoveToTarget());
        StartCoroutine(AttackPlayer());
    }
}
