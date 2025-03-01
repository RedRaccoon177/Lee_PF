using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [SerializeField] GameObject monsterPrefab;  //몬스터 프리펩

    [SerializeField] ParticleSystem muzzleParticle; // 총구 화염 프리팹
    [SerializeField] int defaultPoolSize = 10; // 기본 풀 크기
    
    [SerializeField] ParticleSystem bulletParticle; // 탄알 궤적 프리팹
    //[SerializeField] float trailDuration = 0.05f; // 탄알 궤적 이동 시간

    [SerializeField] ParticleSystem hitParticle;    //탄알 막힌 곳

    //[SerializeField] int PoolSize = 10; // 기본 풀 크기

    float _spawnTime = 2f;

    ObjectPool<ParticleSystem> muzzleFlashPool; //총구 화염 풀
    ObjectPool<ParticleSystem> bulletTrailPool; //총 궤적 풀
    ObjectPool<ParticleSystem> hitPointPool;    //총알 자국 풀
    ObjectPool<GameObject> monsterPool;         //몬스터 풀

    [SerializeField] private List<Transform> spawnPoints; // 지정된 몬스터 스폰 위치 목록

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        muzzleFlashPool = new ObjectPool<ParticleSystem>
            (
                createFunc: () => Instantiate(muzzleParticle, transform), // 새로운 파티클 생성

                //풀에서 오브젝트를 꺼낼 때 실행
                actionOnGet: (particle) =>
                {
                    particle.gameObject.SetActive(true);
                    particle.Play();
                },

                actionOnRelease: (particle) =>
                {
                    particle.gameObject.SetActive(false);
                },

                actionOnDestroy: (particle) => Destroy(particle.gameObject),

                collectionCheck: false,

                defaultCapacity: defaultPoolSize,

                maxSize: defaultPoolSize * 2
            );

        bulletTrailPool = new ObjectPool<ParticleSystem>
            (
                createFunc: () => Instantiate(bulletParticle, transform),
                actionOnGet: (particle) =>
                {
                    particle.gameObject.SetActive(true);
                    particle.Play();
                },

                actionOnRelease: (particle) =>
                {
                    particle.gameObject.SetActive(false);
                },

                actionOnDestroy: (particle) => Destroy(particle.gameObject),

                collectionCheck: false,

                defaultCapacity: defaultPoolSize,

                maxSize: defaultPoolSize * 2
            );

        hitPointPool = new ObjectPool<ParticleSystem>
            (
                createFunc: () => Instantiate(hitParticle, transform), // 새로운 파티클 생성

                //풀에서 오브젝트를 꺼낼 때 실행
                actionOnGet: (particle) =>
                {
                    particle.gameObject.SetActive(true);
                    particle.Play();
                },

                actionOnRelease: (particle) =>
                {
                    particle.gameObject.SetActive(false);
                },

                actionOnDestroy: (particle) => Destroy(particle.gameObject),

                collectionCheck: false,

                defaultCapacity: defaultPoolSize,

                maxSize: defaultPoolSize * 2
            );

        monsterPool = new ObjectPool<GameObject>
            (
            createFunc: () => Instantiate(monsterPrefab, transform), // 몬스터 프리팹 생성

            // 풀에서 몬스터를 꺼낼 때 실행 (활성화 & 초기화)
            actionOnGet: (monster) =>
            {
                monster.SetActive(true);
                monster.transform.position = GetSpawnPosition(); // 지정된 위치에서 스폰
                monster.GetComponent<EnemyController>().ResetEnemy(); // 초기화 함수 호출
            },

            // 풀에 반환할 때 실행 (비활성화)
            actionOnRelease: (monster) =>
            {
                monster.SetActive(false);
            },

            // 제거할 때 실행 (메모리 정리)
            actionOnDestroy: (monster) => Destroy(monster),

            collectionCheck: false,

            defaultCapacity: 100, // 기본 풀 크기

            maxSize: 200 // 최대 풀 크기
        );

        // 몬스터 소환 코루틴 시작 (한 번만 실행)
        StartCoroutine(SpawnMonsters());
    }

    Vector3 GetSpawnPosition()
    {
        if (spawnPoints.Count == 0)
        {
            return transform.position; // 기본 위치 반환 (에러 방지)
        }

        return spawnPoints[Random.Range(0, spawnPoints.Count)].position;
    }
    IEnumerator SpawnMonsters()
    {
        while (true)
        {
            GameObject monster = monsterPool.Get(); // 풀에서 몬스터 꺼내기
            monster.transform.position = GetSpawnPosition();
            yield return new WaitForSeconds(_spawnTime); // 2초마다 스폰
        }
    }


    public void MonsterRelease(GameObject monster)
    {
        monsterPool.Release(monster);
    }


    public void SpawnMuzzleFlash(Vector3 position, Quaternion rotation)
    {
        ParticleSystem flash = muzzleFlashPool.Get();
        flash.transform.SetPositionAndRotation(position, rotation);

        // 파티클 재사용을 위해 자동 반환
        StartCoroutine(ReleaseAfterDuration(flash));
    }
    IEnumerator ReleaseAfterDuration(ParticleSystem flash)
    {
        yield return new WaitForSeconds(flash.main.duration);
        muzzleFlashPool.Release(flash);
    }


    public void SpawnBulletTrail(Vector3 start, Vector3 target, float speed)
    {
        // 총알을 특정 방향으로 발사
        Vector3 direction = (target - start).normalized; // 방향 계산
        ParticleSystem trail = bulletTrailPool.Get();
        trail.transform.position = start;
        StartCoroutine(MoveTrail(trail, start, direction, speed));
    }
    IEnumerator MoveTrail(ParticleSystem trail, Vector3 start, Vector3 direction, float speed)
    {
        Vector3 currentPos = start;

        while (true) // 무한 루프 (충돌할 때까지 계속 이동)
        {
            Vector3 nextPos = currentPos + direction * speed * Time.deltaTime; // 일정 속도로 이동

            //  Raycast로 충돌 감지 (현재 위치 → 이동할 방향으로 검사)
            if (Physics.Raycast(currentPos, direction, out RaycastHit hit, speed * Time.deltaTime))
            {
                Vector3 hitRo = hit.normal;
                Quaternion rotation = Quaternion.LookRotation(hitRo);

                //  충돌 발생 시 Trail을 충돌한 위치에 고정
                trail.transform.position = hit.point;

                ParticleSystem hitPoint = hitPointPool.Get();
                hitPoint.transform.SetPositionAndRotation(hit.point, rotation);
                StartCoroutine(hitPointRelease(hitPoint));

                if (hit.collider.CompareTag("Enemy"))
                {
                    hit.collider.gameObject.GetComponent<EnemyHealth>().TakeDamage(10);
                }
                break; // 이동 중지
            }
            //  충돌 없으면 계속 이동
            trail.transform.position = nextPos;
            currentPos = nextPos;

            yield return null; // 다음 프레임까지 대기
        }

        // Trail이 충돌한 이후에도 잠시 남도록 유지
        yield return new WaitForSeconds(trail.main.duration);
        bulletTrailPool.Release(trail); // 풀에 반환
    }
    IEnumerator hitPointRelease(ParticleSystem hitPoint)
    {
        yield return new WaitForSeconds(hitPoint.main.duration);
        hitPointPool.Release(hitPoint);
    }

}
