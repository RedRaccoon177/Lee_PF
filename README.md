<!-- ===== HEADER ===== -->
<h1 align="center">🔫 Lee_PF 🔫</h1>
<p align="center">
  Unity 3D 기반 <b>쿼터뷰 슈터 프로토타입</b><br/>
  핵심: <b>Blend Tree 기반 8방향(Walk/Run) 로코모션</b> + <b>상체 레이어(사격/재장전) 분리</b>
</p>

<br>

<!-- 링크 버튼 영역 -->
<p align="center">
  <a href="https://youtu.be/EnrMzb5rDoI?si=G0Y91NyAXXJGylte">
    <img src="https://img.shields.io/badge/플레이%20영상-YouTube-red?logo=youtube&logoColor=white" />
  </a>
   <a href="https://www.canva.com/design/DAGusJR6Rj8/oqtCCGhOprGTfJjlf6Ingw/edit?ui=eyJEIjp7IlQiOnsiQSI6IlBCTHZQNW5EQnAxTERHZzkifX19">
    <img src="https://img.shields.io/badge/Portfolio%20Canva-핵심%20기술%20Canva-blue" />
  </a>
  <a href="https://www.canva.com/design/DAGgqVwY2FE/4-tWwp8yW8CPqFZ3U3rpaA/view?utm_content=DAGgqVwY2FE&utm_campaign=designshare&utm_medium=link2&utm_source=uniquelinks&utlId=ha49ef4cc8c">
    <img src="https://img.shields.io/badge/발표용%20Canva-발표용%20Canva-blue" />
  </a>
</p>

<br>

<!-- ===== SCREENSHOTS (2 images) ===== -->
<table align="center">
  <tr>
    <td width="50%">
      <img src="https://github.com/user-attachments/assets/e0392ba1-f5e4-41b5-9017-1c77a8e9b342" alt="Lee_PF 1" width="100%"/>
    </td>
    <td width="50%">
      <img src="https://github.com/user-attachments/assets/8641533d-ccc3-49fe-8938-0f5a0ffa5a9e" alt="Lee_PF 2" width="100%"/>
    </td>
  </tr>
</table>

<br>

## 📌 프로젝트 정보
- 개발 인원: **1명**
- 제작 기간: **2025.02.19 ~ 2025.03.03 (9일)**
- 장르: **쿼터뷰 슈터 / 전투 프로토타입**
- 엔진: **Unity 3D (2022.3.21f1)**
- 본 README는 포트폴리오 용도로 **핵심 기술 + 구현 시스템(코드)** 중심으로 정리했습니다.

<br>

---

<br>

## 📚 목차
- [🎯 게임 소개](#game-intro)
- [🧠 핵심 기술](#key-tech)
  - [1) 8방향 Blend Tree 로코모션(Walk/Run)](#anim-8dir)
  - [2) 상체 레이어 분리(사격/재장전)](#upperbody)
  - [3) Animator 파라미터 스무딩(방향 보간)](#anim-smoothing)
  - [4) 커맨드 패턴 기반 전투 입력 분리](#command-pattern)
  - [5) Object Pool 기반 VFX/Enemy 풀링](#object-pooling)
  - [6) Input System 이벤트 기반 입력 처리](#input-system)
  - [7) Death 전환(Any State)](#death)
  - [8) Animator 파라미터 요약](#param-summary)
- [✅ 구현 시스템 (코드 기준)](#what-i-built)
  - [플레이어](#player-system)
  - [전투](#combat-system)
  - [적](#enemy-system)
  - [풀링](#pool-system)
  - [UI/게임 흐름](#ui-flow)
  - [사운드](#sound-system)
  - [카메라](#camera-system)
  - [씬 로딩](#scene-system)
- [🛠️ 기술 스택](#tech-stack)
- [👨‍💻 개발자 소개](#developer)

<br>

---

<br>

<a name="game-intro"></a>
## 🎯 게임 소개
Lee_PF는 플레이어가 **이동/조준/사격/재장전**을 수행하고, 적이 **추적/공격/피격/사망**하는 기본 전투 루프를 갖춘 **쿼터뷰 슈터 프로토타입**입니다.  
조작감에 직접 영향을 주는 요소(이동 로코모션, 전투 동작, 입력 처리, 전투 VFX 성능)를 중심으로 시스템을 구성했습니다.

<br>

---

<br>

<a name="key-tech"></a>
## 🧠 핵심 기술

<a name="anim-8dir"></a>
### 1) 8방향 Blend Tree 로코모션(Walk/Run)
플레이어 로코모션은 `MoveSpeed`(속도)와 `DirectionX/Y`(방향)를 분리해, 속도 변화와 방향 변화가 서로 충돌하지 않도록 구성했습니다.  
`MoveBlendTree` 내부에서 Walk/Run을 분리한 뒤, 각 상태에서 8방향을 2D Blend Tree로 보간하도록 설계했습니다.

- WalkBlend / RunBlend 타입: **2D Simple Directional**
- 사용 파라미터: `DirectionX`, `DirectionY`

8방향 Threshold 좌표는 아래처럼 구성했습니다.
- Forward: (0, 1)
- Forward Right: (0.7, 0.7)
- Right: (1, 0)
- Backward Right: (0.7, -0.7)
- Backward: (0, -1)
- Backward Left: (-0.7, -0.7)
- Left: (-1, 0)
- Forward Left: (-0.7, 0.7)

이 구조 덕분에 대각 이동에서도 클립이 자연스럽게 섞여 이어지고, Walk에서 Run으로 전환될 때도 같은 방향 축을 공유해서 이동 감각이 안정적으로 유지됩니다.

<br>

<a name="upperbody"></a>
### 2) 상체 레이어 분리(사격/재장전)
로코모션(하체)과 전투 동작(상체)을 분리하기 위해 `UpBody Layer`를 별도로 구성했습니다.  
이동 로코모션은 유지한 채, 사격/재장전 동작만 상체 레이어에서 처리하도록 설계했습니다.

- UpBody Layer 상태 흐름:
  - `Entry -> MoveBlendTree`
  - `MoveBlendTree <-> Firing`
  - `MoveBlendTree <-> Reloading`

전투 도중에도 하체 로코모션이 끊기지 않고, 전투 동작이 끝나면 MoveBlendTree로 자연스럽게 복귀하도록 구성했습니다.

<br>

<a name="anim-smoothing"></a>
### 3) Animator 파라미터 스무딩(방향 보간)
8방향 블렌딩은 입력이 급격하게 변하면 애니메이션이 덜컥거릴 수 있어서, 방향 파라미터는 보간으로 안정화했습니다.  
`ShooterAnimationManager.cs`에서 `DirectionX/Y`를 `Mathf.Lerp` 기반으로 부드럽게 따라가게 만들어 방향 전환이 매끄럽게 이어지도록 구성했습니다.

<br>

<a name="command-pattern"></a>
### 4) 커맨드 패턴 기반 전투 입력 분리
전투 입력은 입력 처리와 실제 실행 로직이 섞이기 쉬워서, 커맨드 패턴으로 역할을 분리했습니다.

- `ShooterAllCommand.cs`
  - `ShooterICommand` 인터페이스(`Execute()`)
  - `ShooterShootCommand` / `ShooterReloadCommand`

사격은 “입력 감지”와 “사격 실행(이펙트/트레일/애니메이션 트리거)”을 분리해 호출하는 형태로 구성했습니다.  
이 구조는 무기/공격 방식이 늘어나도 입력 처리 코드가 비대해지지 않도록 확장 방향을 잡아줍니다.

<br>

<a name="object-pooling"></a>
### 5) Object Pool 기반 VFX/Enemy 풀링
전투에서 자주 생성되는 VFX(총구 화염, 탄도 트레일, 피격/사망 파티클)와 Enemy는 풀링으로 재사용하도록 구성했습니다.

- `ObjectPoolManager.cs`
  - Enemy 풀 + 파티클 풀을 구성해 스폰/반환 루틴을 통일

Instantiate/Destroy 반복을 줄여 전투 상황에서도 프레임이 흔들리지 않도록 설계했습니다.

<br>

<a name="input-system"></a>
### 6) Input System 이벤트 기반 입력 처리
Unity Input System을 사용해 입력을 이벤트 콜백으로 분리했습니다.

- `ShooterController.cs`
  - `PlayerInput`, `InputAction` 기반
  - 콜백: `OnMove`, `OnRun`, `OnShoot`, `OnReload`

조준은 `Ray + Plane.Raycast`로 바닥 평면 교차점을 구해 타겟을 계산하는 방식으로 구성했습니다.

<br>

<a name="death"></a>
### 7) Death 전환(Any State)
사망은 특정 상태에서만 진입하도록 제한하지 않고, 어떤 상황에서도 즉시 전환될 수 있게 `Any State -> Death` 전환으로 구성했습니다.  
이동 중이든, 사격/재장전 중이든 체력이 0이 되는 순간 사망 상태로 확실히 들어가도록 해 전투 흐름이 어색하게 남지 않게 했습니다.

<br>

<a name="param-summary"></a>
### 8) Animator 파라미터 요약
- Float: `MoveSpeed`, `DirectionX`, `DirectionY`
- Bool: `IsIdle`, `IsReload`
- Trigger: `IsShooting`, `IsSkillQ`, `IsDeath`

<br>

---

<br>

<a name="what-i-built"></a>
## ✅ 구현 시스템 (코드 기준)

<a name="player-system"></a>
### 1) 플레이어
- `ShooterController.cs`: 이동/달리기/사격/재장전 입력 처리, 조준 타겟 계산
- `ShooterAnimationManager.cs`: MoveSpeed/DirectionX/Y 갱신 및 방향 보간
- `ShooterHealth.cs`: 플레이어 체력/피격/사망 처리

<br>

<a name="combat-system"></a>
### 2) 전투
- `ShooterAllCommand.cs`: 커맨드 패턴 기반 사격/재장전 실행 분리
- `ObjectPoolManager.cs`: 사격 VFX(총구 화염/트레일/피격) 풀링 및 스폰

<br>

<a name="enemy-system"></a>
### 3) 적
- `EnemyController.cs`: 적 행동 제어(추적/공격 등)
- `EnemyHealth.cs`: 적 피격/사망 처리 및 연동

<br>

<a name="pool-system"></a>
### 4) 풀링
- `ObjectPoolManager.cs`: Enemy/VFX 풀 관리 및 반환 처리

<br>

<a name="ui-flow"></a>
### 5) UI/게임 흐름
- `GameManager.cs`: 게임 진행/상태 관리
- `HealthBarManager.cs`: 체력 UI 갱신
- `UIButtonSound.cs`: UI 상호작용 사운드

<br>

<a name="sound-system"></a>
### 6) 사운드
- `SoundManager.cs`: 효과음 관리(사격/피격/버튼 등)

<br>

<a name="camera-system"></a>
### 7) 카메라
- `CameraRPGPerspective.cs`: 쿼터뷰 카메라 시점 유지 및 타겟 추적

<br>

<a name="scene-system"></a>
### 8) 씬 로딩
- `SceneManager.cs`: 씬 전환/로드 흐름 제어

<br>

---

<br>

<a name="tech-stack"></a>
## 🛠️ 기술 스택
- 엔진: Unity 3D (2022.3.21f1)
- 언어: C#
- 입력: Unity Input System (PlayerInput, InputAction)
- 애니메이션: Animator Controller, Blend Tree(2D Simple Directional), Layer(UpBody), Any State Transition
- 설계 패턴: 커맨드 패턴(Command Pattern)
- 최적화: Object Pool(Unity ObjectPool) 기반 풀링

<br>

---

<br>

<a name="developer"></a>
## 👨‍💻 개발자 소개
- GitHub: [https://github.com/RedRaccoon177]
- Tistory: [https://wearelast99.tistory.com/]
- YouTube: [유튜브 채널](https://www.youtube.com/@%EC%9D%B4%EC%9C%A0-z9c)
- Canva 포트폴리오: [포트폴리오](https://www.canva.com/design/DAGusJR6Rj8/BOtICI6F1raShPyHHewjxg/view?utm_content=DAGusJR6Rj8&utm_campaign=designshare&utm_medium=link2&utm_source=uniquelinks&utlId=h691958bd9a)
- Canva 이력서: [이력서](https://www.canva.com/design/DAGj7YKBoc8/YPk_CLe8B1taKTE-nneUJA/view?utm_content=DAGj7YKBoc8&utm_campaign=designshare&utm_medium=link2&utm_source=uniquelinks&utlId=ha914d97458)
