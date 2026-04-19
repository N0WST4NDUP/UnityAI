# Phase 1 — 전투 씬 기초

## 목표
준비 페이즈(Grid 배치) → 전투 페이즈(NavMesh 자유 이동) 전환이 동작하고, 유닛이 자유롭게 이동하며 기본 전투가 가능한 최소 전투 씬을 구축한다. ML-Agents 연결 전 단계이며, 이후 Phase 2 학습의 환경이 된다.

## 선행 조건 (의존 Phase)
- 없음 (프로젝트 시작점)

## 완료된 Task

### ✅ TASK-105: 기존 코드 재설계 적용 (2026-04-17)
- `CellState.Danger` 제거 → Empty / Structure / Occupied 3종 확정 (`Assets/Scripts/Fields/GridCell.cs`)
- `Structure.cs`에 `[RequireComponent(typeof(NavMeshObstacle))]` 추가
  - NavMeshObstacle과의 연동은 코드 참조 없음 — `Die()` 에서 `SetActive(false)` 시 Unity가 자동으로 모든 컴포넌트 비활성화 (Carve 해제 → NavMesh 자동 복구)
  - `_obstacle` 필드를 명시적으로 두지 않는 이유: `SetActive`만으로 충분, 불필요한 참조 제거

### ✅ TASK-106: 유닛 기본 구현 (2026-04-17 → 2026-04-19 재설계)
구현된 파일:
- `Unit.cs` (abstract) — 정체성(Tribe/UnitType/Rank) + `_preparationPosition` + `Return()`
  - `OnPreparationStart` 구독 → `Return()`: `SetActive(true)` + 위치 복원
- `UnitHealth.cs` — `IDamageable` 구현, HP/Die 관리, `Init(Unit)` 주입
- `UnitMovement.cs` — `IMovable` 구현, NavMeshAgent 래핑, `Init(Unit)` 주입
  - `OnBattleStart` → `EnableMovement()`, `OnBattleEnd` → `DisableMovement()`
- `UnitAnimator.cs` — `PlayMoveAnimation(float speed)` 전담
- `UnitEnum.cs` — `Tribe`, `UnitType` enum
- `Human/HumanSoldier.cs` — `Unit` 구체 클래스: Awake에서 컴포넌트 캐싱, Start에서 Init 호출

### ✅ TASK-107: NavMesh 기반 유닛 이동 (2026-04-18)
- `UnitMovement.MoveTo(Vector3)` → `NavMeshAgent.SetDestination()`
- `enableMovement/DisableMovement()` Phase 이벤트로 제어
- 구조물 회피 경로 탐색 동작 확인

### ✅ TASK-108: 준비 → 전투 페이즈 전환 시스템 (2026-04-18)
- `PhaseManager.cs`: 이벤트 4종 (OnPreparationEnd/Start, OnBattleStart/End), Space 토글 (Phase 6에서 UI 교체)
- `NavMeshBaker.cs`: `NavMeshSurface.BuildNavMesh()` 래퍼
- `PlacementController.cs`: 1/2 키로 배치 모드 전환 (Phase 6에서 UI 교체)

## 진행 중인 Task

### 🔲 TASK-109: 기본 공격/스킬 시스템
- 구현 위치: `Assets/Scripts/Units/UnitAttack.cs` (신규), `IAttackable` 인터페이스
- 설계 방향 (SOLID):
  - `IAttackable` 인터페이스: `Attack()` 또는 타겟 설정 메서드
  - `UnitAttack.cs`: 타겟 탐색(`OverlapSphere`) → `IDamageable.OnDamaged()` 호출 + 쿨타임
  - 공격력/사거리는 ScriptableObject (OCP: 직군별 다른 값, 코드 변경 없이 데이터로 분기)
- 완료 조건:
  - 근처 적 유닛 자동 타겟 선택
  - 사거리 내 접근 → `IDamageable.OnDamaged()` 호출
  - 쿨타임 적용, 타겟 사망 시 새 타겟 선택

### 🔲 TASK-110: 구조물 파괴 → NavMesh 갱신 연동 검증
- 구현 위치: `Assets/Scripts/Structures/Structure.cs`, `Testing/StructureDamageTester.cs`
- 작업 내용:
  - `StructureDamageTester.TryGetComponent` guard clause 추가 (3세션 미처리)
  - `Structure`에 `IDamageable` 인터페이스 적용 (float 타입 일관화, `TakeDamage(int)` → `OnDamaged(float)`)
  - 파괴 후 NavMesh 복구 + 유닛 통과 실제 검증
- 완료 조건:
  - 구조물 `SetActive(false)` → NavMeshObstacle Carve 해제 → 해당 경로 유닛 통과 가능

## 주의사항
- Phase 1에서는 ML-Agents 학습 코드를 섞지 않는다.
- NavMesh Bake는 전투 시작 시 1회만. 전투 중 지형 변경은 Phase 4.
- 유닛 이동은 NavMeshAgent에 목표 좌표를 주는 방식. Agent(AI)가 목표를 결정하는 것은 Phase 2.
- 현재는 테스트 목적으로 마우스 클릭 등 임시 입력으로 유닛 이동/공격 테스트.
