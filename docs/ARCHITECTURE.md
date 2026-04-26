# ARCHITECTURE.md

Unity 컴포넌트 / C# 클래스 책임 / ML-Agents 클래스 상속 구조.

> **2026-04-20 Feudal HRL 재설계 반영** — Team/Squad/Command 추상화 신설, UnitAttack 수동화, DI 방식 전환 계획 포함.

---

## 1. 디렉토리 구조

### 현재 구현 상태 (Phase 1 완료)

```
Assets/Scripts/
├── Fields/                         ← 준비 페이즈 전용
│   ├── GridField.cs                # 2D 셀 배열 + World↔Grid 변환
│   └── GridCell.cs                 # CellState enum (Empty/Structure/Occupied)
├── Interfaces/
│   ├── IMovable.cs                 # MoveTo(Vector3)
│   ├── IDamageable.cs              # OnDamaged(float)
│   └── IGroupOwned.cs              # GroupId
├── Units/
│   ├── UnitBase/
│   │   ├── Unit.cs                 # abstract: 정체성 + 복귀 로직
│   │   └── Human/HumanSoldier.cs   # Unit 구체 클래스
│   ├── UnitEnum.cs                 # Tribe, UnitType
│   ├── UnitHealth.cs               # IDamageable, HP 관리
│   ├── UnitMovement.cs             # IMovable, NavMeshAgent 래핑
│   ├── UnitAttack.cs               # ⚠️ 현재 자동 타겟팅 — (B)안에서 수동화 예정
│   ├── UnitAnimator.cs
│   ├── UnitPlacer.cs
│   └── StatsSO/
│       ├── UnitStatsSO.cs          # abstract ScriptableObject
│       └── Melee/MeleeStatsSO.cs   # Strategy: ExecuteAttack
├── Structures/
│   ├── Structure.cs                # 내구도 + NavMeshObstacle + IDamageable
│   └── StructurePlacer.cs
├── Phase/
│   ├── PhaseManager.cs             # 준비↔전투 전환, 이벤트 4종
│   └── NavMeshBaker.cs             # NavMeshSurface.BuildNavMesh() 래퍼
├── Placement/
│   └── PlacementController.cs      # 배치 모드 전환 (Unit/Structure)
├── Registry/
│   └── RegistryManager.cs          # GroupId별 유닛 리스트 (최대 8그룹)
├── Agents/
│   └── SoldierAgent.cs             # ⚠️ 관측 4차원만, 재설계 대상
├── Testing/                        ← Phase 2 완료 후 일괄 삭제
│   ├── BattleTestInput.cs
│   ├── StructureDamageTester.cs
│   └── GroupTest.cs                # static groupId — Team 시스템으로 대체
└── TrainManager.cs                 # ML-Agents 학습 통계 UI
```

### 목표 구조 (Phase 2~3 신설)

```
Assets/Scripts/
├── (위 구조 유지 — 단 Testing/ 제거, Agents/·Registry/ 확장)
├── Teams/                          # 신규 — 팀/진영 시스템
│   ├── Team.cs                     # 소유 유닛·분대·골드·시너지
│   └── TeamRegistry.cs             # 모든 Team 참조
├── Squads/                         # 신규 — 분대 시스템 ⭐
│   ├── Squad.cs                    # SquadId, 소속 Unit[], 현재 SquadCommand
│   ├── SquadRegistry.cs            # 모든 Squad 참조 (Team별)
│   └── SquadFormation.cs           # 준비 페이즈 편성 UI/로직
├── Commands/                       # 신규 — 명령 파이프라인 ⭐
│   ├── SquadCommand.cs             # 3필드(+@) 데이터 클래스
│   ├── TargetPriority.cs           # enum
│   ├── StructureHandling.cs        # enum
│   ├── CommandBroker.cs            # 2초 tick 발행, Squad에 push
│   └── ScriptedCommander.cs        # Phase 2 Stage 2 학습용 규칙 기반 커맨더
├── Agents/
│   ├── SoldierAgent.cs             # 재설계 — Q10 관측, 행동, 보상
│   └── CommanderAgent.cs           # Phase 3 — 완전 정보 관측, 분대 명령 액션
├── Combat/                         # 결과물 (선택적 분리)
│   └── (UnitAttack은 Units/ 유지, 타겟 관리 로직만 분리 고려)
├── Episode/                        # 신규 — 에피소드 경계
│   └── EpisodeManager.cs           # 승/패 판정, MLAgents EndEpisode 트리거
├── Synergy/                        # 신규 — Phase 2 중후반
│   └── SynergyFlags.cs
├── Terrain/                        # Phase 4
│   └── TerrainEditor.cs
└── Items/                          # Phase 4
    ├── Barricade.cs
    └── Bombardment.cs
```

---

## 2. 셀 상태 enum (준비 페이즈 전용)

```csharp
public enum CellState : byte {
    Empty     = 0,   // 배치 가능
    Structure = 1,   // 구조물 점유
    Occupied  = 2,   // 유닛 점유
}
```

> 기존의 `Danger = 3`은 제거. 전투 중 위험 영역은 Grid가 아닌 월드 좌표 AoE로 처리.

---

## 3. 클래스 책임 요약

### 3-1. 유지 — 현재 구현

| 클래스 | 책임 | 의존 방식 | (B)안 영향 |
|--------|------|----------|-----------|
| `GridField` | 준비 페이즈 셀 상태 관리, 좌표 변환 | — | 변경 없음 |
| `PhaseManager` | 준비↔전투 전환, 이벤트 4종 발행 | SerializeField: NavMeshBaker | 스페이스바 → UI 교체 (Phase 6) |
| `NavMeshBaker` | NavMeshSurface.BuildNavMesh() 래퍼 | SerializeField: NavMeshSurface | 변경 없음 |
| `PlacementController` | Unit/Structure 배치 모드 | — | 숫자키 → UI (Phase 6) |
| `Unit` (abstract) | 정체성 + 복귀 위치 | FindGameObjectWithTag → DI 전환 필요 | `SquadId` 필드 추가 |
| `UnitHealth` | HP 관리, Die | Init(Unit) 주입 | 변경 없음 (Q3 일치) |
| `UnitMovement` | NavMeshAgent 래핑, Phase 이벤트 구독 | Init(Unit) 주입 | Agent가 MoveTo 호출 |
| `UnitAnimator` | 애니메이션 파라미터 제어 | GetComponent | 변경 없음 |
| `HumanSoldier` | Unit 구체 클래스, 컴포넌트 조립 | GetComponent | `SoldierAgent` 추가 조립 |
| `UnitPlacer` | 준비 페이즈 유닛 배치 | SerializeField | GroupId 획득 방식 교체 (Team 시스템) |
| `Structure` | 내구도 + NavMeshObstacle | GetComponent | DI 전환 필요 |
| `StructurePlacer` | 준비 페이즈 구조물 배치 | SerializeField | GroupId 획득 방식 교체 |
| `UnitStatsSO`/`MeleeStatsSO` | 공격 스탯 + 실행 Strategy | ScriptableObject | 변경 없음 |
| `RegistryManager` | GroupId별 유닛 리스트 | — | Squad 단위 확장, 디버그 키 제거 |
| `TrainManager` | 학습 통계 UI | Singleton | 변경 없음 |

### 3-2. 수정 — (B)안 반영

| 클래스 | 수정 방향 | 비고 |
|---|---|---|
| `Unit` | `SquadId` 필드 + 프로퍼티 추가, `Init(groupId, squadId, position)` 확장 | 준비 페이즈에서 커맨더가 편성 시 세팅 |
| `UnitAttack` | **타겟 자동 선택 제거** → 외부(Agent)가 설정한 타겟에만 공격 | 가장 중요한 리팩토링 ⭐ |
| `UnitEnum` | `UnitType`(종, 예: Archer) + `UnitRole`(직군, 예: Tank/Dealer/Mage) 분리 | 공유 Policy는 Role 기준 |
| `SoldierAgent` | `UnitAgent`로 이름 바꾸거나 Role별 분리 고려. 관측(자기+명령+분대동료), 행동, 보상 전면 재설계 | OBSERVATIONS.md / REWARDS.md 참조 |
| `RegistryManager` | SquadRegistry 기능 흡수 또는 분리. Update 디버그 키 제거 | — |
| `Unit`/`Structure` Awake | `FindGameObjectWithTag` → DI 생성자 주입으로 전환 | 병렬 학습 환경 안전성 |

### 3-3. 신규 — Phase 2~3 추가

| 클래스 | 책임 | Phase |
|---|---|---|
| `Team` | 팀 식별, 소유 유닛·분대·골드·시너지 보유 | 2 |
| `TeamRegistry` | 모든 Team 참조, GroupTest 대체 | 2 |
| `Squad` | SquadId, 소속 `Unit[]`, `CurrentCommand` | 2 |
| `SquadRegistry` | Team별 분대 조회 | 2 |
| `SquadFormation` | 준비 페이즈 편성 UI + 커맨더 편성 API | 2 (수동 편성) / 3 (Agent) |
| `SquadCommand` | 3필드(+@) 데이터 클래스 | 2 |
| `TargetPriority` enum | 탱커/딜러/마법사/구조물/전체무시 등 | 2 |
| `StructureHandling` enum | 부수기/우회/무시 | 2 |
| `CommandBroker` | 2초 tick에 커맨더로부터 명령 수집 후 분대에 push | 2 |
| `ScriptedCommander` | 규칙 기반 커맨더 (Phase 2 Stage 2용) | 2 |
| `CommanderAgent` | RL 커맨더 (Phase 3) | 3 |
| `EpisodeManager` | 한 팀 전멸 / 시간 초과 판정, EndEpisode 트리거 | 2 |
| `SynergyFlags` | 시너지 플래그 관측/보상 연결 | 2 중후반 |

### 3-4. 제거 / 대체

| 대상 | 시점 | 대체 |
|---|---|---|
| `GroupTest` (static int) | Team 시스템 도입 직후 | `Team.GroupId` |
| `BattleTestInput` | Phase 2 완료 | Agent 이동 |
| `StructureDamageTester` | Phase 2 완료 | Agent 공격 |
| `RegistryManager.Update` 디버그 키 | Team 시스템 도입 직후 | — |
| `PhaseManager.Update` 스페이스바 | Phase 6 UI 도입 시 | UI 버튼 |
| `PlacementController.Update` 숫자키 | Phase 6 UI 도입 시 | UI 토글 |

---

## 4. Phase 이벤트 구독 구조

```
PhaseManager
├── OnPreparationStart ← Unit.ReturnTo(), Structure.ReturnTo()
├── OnPreparationEnd   ← (현재 미구독)
├── OnBattleStart      ← UnitMovement.EnableMovement(), UnitAttack.Enable(),
│                       Unit.Register(), CommandBroker.StartTicking(),
│                       EpisodeManager.Begin()
└── OnBattleEnd        ← UnitMovement.DisableMovement(), UnitAttack.Disable(),
                        Unit.Unregister(), CommandBroker.Stop()
```

- 각 컴포넌트가 독립 구독 (분산 구독 원칙)
- 핸들러는 모두 idempotent — 호출 순서 의존 없음

---

## 5. Unit 컴포넌트 계층

```
GameObject (HumanSoldier 프리팹)
├── HumanSoldier : Unit          — 정체성(Tribe/Role/Class/Rank) + SquadId + 복귀 위치
├── UnitHealth   : IDamageable   — HP 관리, Die() → SetActive(false)
├── UnitMovement : IMovable      — NavMeshAgent 래핑
├── UnitAttack                   — (B)안에서 수동화: Agent가 CurrentTarget 설정
├── UnitAnimator                 — 애니메이션 파라미터
├── SoldierAgent : Agent         — Phase 2 신규. 관측·행동·보상
└── NavMeshAgent                 — Unity 네이티브
```

---

## 6. ML-Agents 상속 구조

```
Unity.MLAgents.Agent
├── SoldierAgent  (BehaviorName = "UnitTank" | "UnitDealer" | "UnitMage")
│     직군별 공유 Policy. BehaviorName으로 구분.
└── CommanderAgent (BehaviorName = "Commander")
      단일 Policy. 팀당 1개 Agent.
```

- 직군별 Agent는 **같은 BehaviorName 공유** → 공유 Policy
- 커리큘럼 학습 Stage별 Agent 상태:
  - Stage 1: 둘 다 Heuristic (스크립트)
  - Stage 2: `SoldierAgent` 학습 / `ScriptedCommander` 사용
  - Stage 3: `CommanderAgent` 학습 / `SoldierAgent` ONNX freeze

---

## 7. 데이터 흐름 (전투 1틱)

```
[2초 tick]
Commander (Agent 또는 Scripted)
    ↓ CollectObservations (완전 정보)
    ↓ OnActionReceived → SquadCommand[N] 생성
CommandBroker
    ↓ 각 Squad.CurrentCommand = 새 명령

[매 물리 스텝]
Unit (SoldierAgent)
    ↓ CollectObservations (자기 + CurrentCommand + 분대동료 + 지역)
    ↓ OnActionReceived → 이동/타겟/스킬
UnitMovement.MoveTo(target)
UnitAttack.SetTarget(target) → 쿨타임마다 Damage 적용
UnitHealth.OnDamaged / Die

[수행도 평가]
Unit: 명령 수행 여부로 매 스텝 reward (수행도 80% + micro 20%)
Commander: 2초 tick마다 또는 에피소드 종료 시 reward (sparse+최소 dense)

[에피소드 경계]
EpisodeManager: 한 팀 전멸 또는 시간 초과 → 양 Agent EndEpisode()
```

---

## 8. 듀얼 페이즈 전환 흐름

```
[준비 페이즈]
Grid 활성 / NavMesh 비활성
유닛: Grid 셀에 고정
구조물: Grid 셀에 배치
지형: 높낮이 편집 가능
분대: 커맨더가 편성 (또는 수동 편성)

        ↓ PhaseManager.StartBattle()

[전환]
1. 지형/구조물 확정
2. NavMeshSurface.BuildNavMesh()
3. 유닛 Grid 좌표 → World 좌표
4. NavMeshAgent 활성화
5. 구조물 NavMeshObstacle(Carve) 활성화
6. 분대 편성 확정 → Squad 인스턴스 생성
7. CommandBroker 시작 (첫 명령 즉시 발행)

        ↓

[전투 페이즈]
Grid 비활성 / NavMesh 활성
유닛: 명령 범위 내 자율 이동
구조물: NavMeshObstacle (파괴 시 제거)
CommandBroker: 2초마다 새 명령
EpisodeManager: 승/패 감시
```

---

## 9. 주요 구조적 주의사항

### 9-1. UnitAttack의 타겟 선택 분리
- **현재**: `UnitAttack.Update()`가 `OverlapSphere`로 가까운 적 자동 공격
- **(B)안**: 타겟 선택은 `SoldierAgent`의 책임. `UnitAttack`은 "`CurrentTarget`이 지정되어 있으면 쿨타임마다 때림"만 담당
- 이렇게 해야 명령의 `priority_filter`가 의미를 가짐

### 9-2. DI 전환 필요성
- `FindGameObjectWithTag("Manager")` 호출은 ML-Agents **병렬 환경**(env 여러 개 동시 실행)에서 잘못된 매니저를 잡을 위험
- Phase 2 착수 전 `Unit.Init(PhaseManager, RegistryManager, ...)` 방식으로 전환 필수

### 9-3. Observation 크기 변경 주의
- Agent observation 크기 변경 시 기존 ONNX 폐기 → 재학습 필요
- 빈 슬롯은 0 패딩 + 존재 flag로 처리 → 유닛 수 변동에 강건

### 9-4. 커맨더 정책 분리 여부 (미확정)
- 준비 페이즈 액션(분대 편성) + 전투 페이즈 액션(명령 발행)이 구조적으로 다름
- **1 Policy (2 head)** vs **2 Policy** — Phase 3 착수 시 결정
