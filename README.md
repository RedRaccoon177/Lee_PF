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

<!-- ===== SCREENSHOTS (2x2) ===== -->
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

## 프로젝트 정보
- 개발 인원: **1명**
- 제작 기간: **2025.02.19 ~ 2025.03.03 (9일)**
- 장르: **쿼터뷰 슈터 / 전투 프로토타입**
- 엔진: **Unity 3D (2022.3.21f1)**
- 본 README는 포트폴리오 용도로 **애니메이션 시스템(Animator/BlendTree/Layer) 중심**으로 정리했습니다.

<br>

---

<br>

## 목차
- [게임 소개](#game-intro)
- [핵심 기술](#key-tech)
  - [8방향 Blend Tree 로코모션(Walk/Run)](#anim-8dir)
  - [상체 레이어 분리(사격/재장전)](#upperbody)
  - [Death 처리(Any State 진입)](#death)
  - [파라미터 설계 요약](#param-summary)
- [구현 시스템 (코드 기준)](#what-i-built)
  - [플레이어(입력/이동/조준/애니메이션)](#player-system)
  - [전투(사격/재장전/피격)](#combat-system)
  - [적 AI/체력/사망](#enemy-system)
  - [오브젝트 풀링(VFX/Enemy)](#pool-system)
  - [UI/게임 흐름](#ui-flow)
  - [사운드](#sound-system)
  - [카메라](#camera-system)
  - [씬 로딩](#scene-system)
- [기술 스택](#tech-stack)
- [개발자 소개](#developer)

<br>

---

<br>

<a name="game-intro"></a>
## 🎯 게임 소개
Lee_PF는 플레이어가 이동/조준/사격/재장전을 수행하고, 적이 추적/공격하는 기본 전투 루프를 갖춘 **쿼터뷰 슈터 프로토타입**입니다.

이 프로젝트에서 가장 강조하는 포인트는,
**"조작감에 직접 연결되는 애니메이션 품질"**을 목표로 한  
**8방향 이동(걷기/달리기) Blend Tree 설계 + 상체 레이어 분리 구조**입니다.

<br>

---

<br>

<a name="key-tech"></a>
## 🧠 핵심 기술

<a name="anim-8dir"></a>
### 1) Blend Tree 기반 8방향 로코모션 (Walk/Run)
플레이어 로코모션은 `MoveSpeed(속도)`와 `DirectionX/Y(방향)`를 분리해,
**속도 전환(Idle/Walk/Run)**과 **방향 전환(8방향)**이 서로 꼬이지 않도록 구성했습니다.

#### (1) 로코모션 트리 구조
- `MoveBlendTree` 내부에서 **WalkBlend / RunBlend를 분리**해 구성
- Walk/Run 모두 동일한 좌표 체계로 8방향을 구성하여,
  **걷기 -> 달리기 전환 중에도 방향이 깨지지 않도록** 설계

#### (2) WalkBlend / RunBlend Blend Type (팩트)
- `WalkBlend` : **2D Simple Directional**
- `RunBlend` : **2D Simple Directional**
- 파라미터: `DirectionX`, `DirectionY`

#### (3) 8방향 Threshold 매핑 (팩트)
WalkBlend / RunBlend 모두 다음 좌표로 8방향 클립을 배치했습니다.

- Forward: (0, 1)
- Forward Right: (0.7, 0.7)
- Right: (1, 0)
- Backward Right: (0.7, -0.7)
- Backward: (0, -1)
- Backward Left: (-0.7, -0.7)
- Left: (-1, 0)
- Forward Left: (-0.7, 0.7)

#### (4) 설계 의도(포트폴리오 포인트)
- **방향 전환이 "클립 스왑"이 아니라 "보간"으로 자연스럽게 이어지도록**
  - 2D Blend Tree에서 입력 벡터가 이동할수록 주변 클립이 가중치로 섞여, 급격한 끊김을 줄임
- **Walk/Run을 같은 입력 축(DirectionX/Y)로 유지**
  - 속도만 달라지고 방향 축은 동일하므로, 이동 조작감이 일관됨
- **8방향 클립 구성(대각 포함)으로 조작성 강화**
  - 쿼터뷰에서 체감이 큰 "대각 입력"을 끊김 없이 처리

<br>

<a name="upperbody"></a>
### 2) 상체 레이어 분리 (사격/재장전이 이동을 깨지 않게)
로코모션(하체)과 전투 동작(상체)을 분리하기 위해,
`Base Layer`와 별도의 `UpBody Layer`를 구성했습니다.

- `UpBody Layer` 상태 구성(팩트)
  - `Entry -> MoveBlendTree`
  - `MoveBlendTree <-> Firing`
  - `MoveBlendTree <-> Reloading`

#### (1) 왜 레이어를 분리했는가?
- 이동 중 사격/재장전을 구현할 때,
  로코모션 전체를 Firing/Reloading으로 갈아끼우면 **이동이 끊기거나 전환이 부자연스러워질 가능성**이 큼
- 상체 레이어로 분리하면,
  **하체 로코모션은 유지**하면서 **상체만 전투 동작으로 전환** 가능

#### (2) 전환 구조의 장점
- 이동 상태를 유지하면서 사격/재장전을 수행하므로,
  **전투 중 조작감(입력 반응성)이 유지**됨
- Firing/Reloading이 끝나면 MoveBlendTree로 복귀하는 왕복 구조라,
  상태 폭발을 줄이고 유지보수를 단순화함

<br>

<a name="death"></a>
### 3) Death 처리 (Any State 진입)
사망은 특정 상태에서만 진입하는 방식이 아니라,
**Any State에서 바로 Death로 진입**하도록 구성했습니다.

- `Base Layer`에서 `Any State -> Knife_St_Death_B` 전환(팩트)

#### 의도
- 전투 중 어떤 타이밍(이동/사격/재장전/기타)에서도
  **사망 전환이 누락되지 않도록** 보장하는 구조

<br>

<a name="param-summary"></a>
### 4) 파라미터 설계 요약 (팩트)
Animator 파라미터는 다음과 같이 구성했습니다.

- Float
  - `MoveSpeed`
  - `DirectionX`
  - `DirectionY`
- Bool
  - `IsIdle`
  - `IsReload`
- Trigger
  - `IsShooting`
  - `IsSkillQ`
  - `IsDeath`

<br>

---

<br>

<a name="what-i-built"></a>
## ✅ 구현 시스템 (코드 기준)
아래는 프로젝트에서 사용한 스크립트를 **역할 단위로 정리**한 목록입니다.

<a name="player-system"></a>
### 1) 플레이어(입력/이동/조준/애니메이션)
- `ShooterController.cs`
  - 플레이어 입력/이동/조준 및 전투 입력 트리거 처리
- `ShooterAnimationManager.cs`
  - Animator 파라미터 갱신(로코모션/전투 상태 연동)
- `ShooterHealth.cs`
  - 플레이어 체력/피격/사망 처리

<br>

<a name="combat-system"></a>
### 2) 전투(사격/재장전/피격)
- `ShooterAllCommand.cs`
  - 사격/재장전 등 행동을 커맨드 형태로 분리해 호출하는 구조
- `ObjectPoolManager.cs`
  - 총구 화염/탄도 트레일/피격 파티클 등 전투 VFX를 풀링/재사용

<br>

<a name="enemy-system"></a>
### 3) 적 AI/체력/사망
- `EnemyController.cs`
  - 적 이동/추적/공격 등 행동 제어
- `EnemyHealth.cs`
  - 적 피격/사망 처리 및 점수/이펙트 연동

<br>

<a name="pool-system"></a>
### 4) 오브젝트 풀링(VFX/Enemy)
- `ObjectPoolManager.cs`
  - Enemy 및 전투 이펙트(Particle/Trail 등) 재사용 풀링
  - 전투 중 Instantiate/Destroy 비용을 줄이기 위한 구조

<br>

<a name="ui-flow"></a>
### 5) UI/게임 흐름
- `GameManager.cs`
  - 게임 진행(점수/시간/상태) 및 게임 오버 등 흐름 관리
- `HealthBarManager.cs`
  - 체력바 UI 갱신/표시 관련 관리
- `UIButtonSound.cs`
  - UI 상호작용에 대한 버튼 사운드 처리

<br>

<a name="sound-system"></a>
### 6) 사운드
- `SoundManager.cs`
  - 사격/피격/버튼 등 효과음 관리

<br>

<a name="camera-system"></a>
### 7) 카메라
- `CameraRPGPerspective.cs`
  - 쿼터뷰/탑뷰 형태의 카메라 시점 유지 및 타겟 추적

<br>

<a name="scene-system"></a>
### 8) 씬 로딩
- `SceneManager.cs`
  - 씬 전환/로드 관련 래퍼(프로젝트 내 씬 흐름 제어)

<br>

---

<br>

<a name="tech-stack"></a>
## 🛠️ 기술 스택
- 엔진: Unity 3D (2022.3.21f1)
- 언어: C#
- 애니메이션: Animator Controller, Blend Tree (2D Simple Directional), Layer 분리(UpBody), Any State Transition
- (프로젝트 구성에 따라 추가 가능): Object Pooling, NavMesh/AI, UI(TMP 등)

<br>

---

<br>

<a name="developer"></a>
## 👨‍💻 개발자 소개
- GitHub: [https://github.com/RedRaccoon177]
- Tistory: [https://wearelast99.tistory.com/]
- YouTube: [유튜브 채널](https://www.youtube.com/@%EC%9D%B4%EC%9C%A0-z9c)
- Canva 포트폴리오: [포트폴리오](https://www.canva.com/design/DAGusJR6Rj8/BOtICI6F1raShPyHHewjxg/view?utm_content=DAGusJR6Rj8&utm_campaign=designshare&utm_medium=link2&utm_source=uniquelinks&utlId=h691958bd9a)
- Canva 이력서: [이력서](https://www.canva.com/design/DAGusJR6Rj8/YPk_CLe8B1taKTE-nneUJA/view?utm_content=DAGj7YKBoc8&utm_campaign=designshare&utm_medium=link2&utm_source=uniquelinks&utlId=ha914d97458)

