# ARCHITECTURE.md

Unity 컴포넌트 / C# 클래스 책임 / ML-Agents 클래스 상속 구조.

## 디렉토리 구조

### 현재 구현 상태 (Phase 1 진행 중)

```
Assets/Scripts/
├── Fields/                         ← 준비 페이즈 전용
│   ├── GridField.cs                # 2D 셀 배열 + World↔Grid 변환
│   └── GridCell.cs                 # CellState enum (Empty/Structure/Occupied)
├── Interfaces/
│   ├── IMovable.cs                 # MoveTo(Vector3)
│   └── IDamageable.cs              # OnDamaged(float)
├── Units/
│   ├── Unit.cs                     # abstract: 정체성(Tribe/UnitType/Rank) + 복귀 로직
│   ├── UnitEnum.cs                 # Tribe, UnitType enum
│   ├── UnitHealth.cs               # IDamageable 구현, HP 관리
│   ├── UnitMovement.cs             # IMovable 구현, NavMeshAgent 래핑
│   ├── UnitAnimator.cs             # 애니메이션 파라미터 제어
│   ├── UnitPlacer.cs               # 준비 페이즈 유닛 배치
│   └── Human/
│       └── HumanSoldier.cs         # Unit 구체 클래스 (컴포넌트 조립)
├── Structures/
│   ├── Structure.cs                # 내구도 + NavMeshObstacle
│   └── StructurePlacer.cs          # 준비 페이즈 구조물 배치
├── Phase/
│   ├── PhaseManager.cs             # 준비↔전투 전환, 이벤트 4종 발행
│   └── NavMeshBaker.cs             # NavMeshSurface.BuildNavMesh() 래퍼
├── Placement/
│   └── PlacementController.cs      # 배치 모드(Unit/Structure) 전환
├── Testing/                        ← 테스트 전용, Phase 6에서 제거
│   ├── BattleTestInput.cs          # 우클릭 → 전체 유닛 이동 명령
│   └── StructureDamageTester.cs    # 우클릭 → 구조물 데미지
└── TrainManager.cs                 # ML-Agents 학습 통계 (Phase 2 본격화)
```

### 목표 구조 (Phase 2+ 추가 예정)

```
Assets/Scripts/
├── (위 현재 구조 유지)
├── Units/
│   └── BoardRegistry.cs            # TASK-111: 배치 유닛 목록 + 부활 관리
├── Combat/                         # TASK-109
│   └── UnitAttack.cs               # IAttackable 구현, 타겟 탐색 + 쿨타임
├── Agents/                         # Phase 2
│   ├── UnitAgent.cs
│   └── CommanderAgent.cs
├── Terrain/                        # Phase 4
│   └── TerrainEditor.cs
└── Items/                          # Phase 4
    ├── Barricade.cs
    └── Bombardment.cs
```

## 셀 상태 enum (준비 페이즈 전용)

```csharp
public enum CellState : byte {
    Empty     = 0,   // 배치 가능
    Structure = 1,   // 구조물 점유 (배치 불가)
    Occupied  = 2,   // 유닛 점유 (배치 불가)
}
```

> 기존의 `Danger = 3`은 제거. 전투 중 위험 영역은 Grid가 아닌 월드 좌표 AoE로 처리.

## 클래스 책임 요약

### 현재 구현된 클래스

| 클래스 | 책임 | 의존 방식 |
|--------|------|----------|
| `GridField` | 준비 페이즈 셀 상태 관리, 좌표 변환 | — |
| `PhaseManager` | 준비↔전투 전환, 이벤트 4종 발행 | SerializeField: NavMeshBaker |
| `NavMeshBaker` | NavMeshSurface.BuildNavMesh() 래퍼 | SerializeField: NavMeshSurface |
| `PlacementController` | 배치 모드(Unit/Structure) 전환 | — |
| `Unit` (abstract) | 정체성(Tribe/Type/Rank) + 복귀 위치 + Return() | FindGameObjectWithTag → 개선 필요 |
| `UnitHealth` | IDamageable 구현, HP/Die 관리 | Init(Unit) 주입 |
| `UnitMovement` | IMovable 구현, NavMeshAgent 래핑, Phase 이벤트 구독 | Init(Unit) 주입, GetComponent: NavMeshAgent |
| `UnitAnimator` | 애니메이션 파라미터 제어 | GetComponent: Animator |
| `HumanSoldier` | Unit 구체 클래스, 컴포넌트 조립 | GetComponent: UnitHealth/UnitMovement/UnitAnimator |
| `UnitPlacer` | 준비 페이즈 유닛 배치, Grid 상태 갱신 | SerializeField: PhaseManager/PlacementController/GridField |
| `Structure` | 내구도 + SetActive 파괴 처리 | GetComponent: NavMeshObstacle |
| `StructurePlacer` | 준비 페이즈 구조물 배치 | SerializeField: PhaseManager/PlacementController/GridField |

### 예정 클래스 (Phase 1 나머지)

| 클래스 | 책임 | 비고 |
|--------|------|------|
| `BoardRegistry` | 배치 유닛 목록 관리, 라운드 복구 시 SetActive(true) | TASK-111 |
| `UnitAttack` | IAttackable 구현, 타겟 탐색 + 쿨타임 + IDamageable 호출 | TASK-109 |

## Phase 이벤트 구독 구조

```
PhaseManager
├── OnPreparationEnd   ← (현재 미구독)
├── OnBattleStart      ← UnitMovement.EnableMovement()
├── OnBattleEnd        ← UnitMovement.DisableMovement()
└── OnPreparationStart ← Unit.Return()
```

- 각 컴포넌트가 독립 구독 (분산 구독 원칙)
- 핸들러는 모두 idempotent — 호출 순서 의존 없음

## Unit 컴포넌트 계층

```
GameObject (HumanSoldier 프리팹)
├── HumanSoldier  : Unit (abstract)  — 정체성(Tribe/Type/Rank) + 복귀 위치 + Return()
├── UnitHealth    : IDamageable       — HP 관리, Die() → SetActive(false)
├── UnitMovement  : IMovable          — NavMeshAgent 래핑, Phase 이벤트 구독
├── UnitAnimator                      — 애니메이션 파라미터 제어
└── NavMeshAgent                      — Unity 네이티브 컴포넌트 (계약)
```

## ML-Agents 상속 구조 (Phase 2 예정)

```
Unity.MLAgents.Agent
└── UnitAgent        — 직군별 공유 Policy
CommanderAgent       — 별도 BehaviorName
```

- 직군별 Agent는 **같은 BehaviorName 공유** → 공유 Policy 방식.
- CommanderAgent는 별도 BehaviorName.

## 데이터 흐름 (전투 1틱)

```
전장 상태 스냅샷 (유닛 위치 + 구조물 + 지형)
    ↓
CommanderAgent.CollectObservations (전장 전체 관측)
    ↓
Commander action (전략 명령)
    ↓ CommandBus
UnitAgent.CollectObservations (자기 상태 + 주변 인식 + 명령 flag + 시너지)
    ↓
Unit action (이동 방향 / 공격 대상 / 스킬 사용 / 구조물 공격)
    ↓
NavMeshAgent 이동 실행 + 전투 처리
    ↓
보상 계산 + 상태 갱신
```

## 듀얼 페이즈 전환 흐름

```
[준비 페이즈]
Grid 활성 / NavMesh 비활성
유닛: Grid 셀에 고정
구조물: Grid 셀에 배치
지형: 높낮이 편집 가능

        ↓ PhaseManager.StartCombat()

[전환]
1. 지형/구조물 확정
2. NavMeshSurface.BuildNavMesh()
3. 유닛 Grid 좌표 → World 좌표
4. NavMeshAgent 활성화
5. 구조물 NavMeshObstacle(Carve) 활성화

        ↓

[전투 페이즈]
Grid 비활성 / NavMesh 활성
유닛: 자유 이동 (NavMeshAgent)
구조물: NavMeshObstacle (파괴 시 제거 → NavMesh 복구)
지형: 변경 불가
```

## 변경 규칙

- 셀 상태 enum 값 수정 시 기존 코드 전수 검토.
- Agent observation 크기 변경 시 기존 ONNX 폐기 → 재학습 필요.
- NavMesh 관련 설정 변경 시 Bake 타이밍 재검증 필요.
