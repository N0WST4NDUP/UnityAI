# Phase 1 — 전투 씬 기초 (완료)

## 목표
준비 페이즈(Grid 배치) → 전투 페이즈(NavMesh 자유 이동) 전환이 동작하고, 유닛이 자유롭게 이동하며 기본 전투가 가능한 최소 전투 씬을 구축한다. ML-Agents 연결 전 단계이며, 이후 Phase 2 학습의 환경이 된다.

## 선행 조건 (의존 Phase)
- 없음 (프로젝트 시작점)

## 상태

**✅ 완료 (2026-04-19)**. Phase 2 착수 대기 중.

> **2026-04-20 Feudal HRL 재설계 영향**: Phase 1은 완료 상태를 유지. 다만 Phase 2의 인프라 Task들이 Phase 1 산출물의 일부를 개조한다. 특히:
> - [TASK-217](PHASE_2.md) — `UnitAttack`의 자동 타겟팅 로직(TASK-109 산출물)을 **수동화**로 재작업.
> - [TASK-210](PHASE_2.md) — `Unit.Awake`/`Structure.Awake`의 `FindGameObjectWithTag` 호출을 **DI 주입**으로 전환.
> - [TASK-211/212](PHASE_2.md) — `GroupTest` 전역변수·`RegistryManager` 디버그 블록 제거.

## 완료된 Task

### ✅ TASK-101: Grid 기반 필드 구현 (2026-04-15)
- `Assets/Scripts/Fields/GridCell.cs` — `CellState : byte` (Empty/Structure/Occupied)
- `Assets/Scripts/Fields/GridField.cs` — 2D 셀 배열 + 월드/그리드 좌표 변환 + Gizmos

### ✅ TASK-102: 구조물 배치 로직 (2026-04-16)
- `Assets/Scripts/Structures/Structure.cs` — 내구도 + NavMeshObstacle
- `Assets/Scripts/Structures/StructurePlacer.cs` — 준비 페이즈 배치

### ✅ TASK-105: 기존 코드 재설계 적용 (2026-04-17)
- `CellState.Danger` 제거 → Empty / Structure / Occupied 3종 확정
- `Structure`에 `[RequireComponent(typeof(NavMeshObstacle))]`
- `Die()` → `SetActive(false)` 만으로 Carve 해제 → NavMesh 자동 복구

### ✅ TASK-106: 유닛 기본 구현 (2026-04-17 → 재설계 2026-04-19)
구현된 파일:
- `Unit.cs` (abstract) — 정체성(Tribe/UnitType/Rank) + `_preparationPosition` + `Return()`
- `UnitHealth.cs` — `IDamageable` 구현, HP/Die 관리
- `UnitMovement.cs` — `IMovable` 구현, NavMeshAgent 래핑
- `UnitAnimator.cs` — `PlayMoveAnimation`/`PlayDieAnimation`
- `UnitEnum.cs` — `Tribe`, `UnitType`
- `Human/HumanSoldier.cs` — Unit 구체 클래스

### ✅ TASK-107: NavMesh 기반 유닛 이동 (2026-04-18)
- `UnitMovement.MoveTo(Vector3)` → `NavMeshAgent.SetDestination()`
- Phase 이벤트로 `EnableMovement`/`DisableMovement`
- 구조물 회피 경로 탐색 동작 확인

### ✅ TASK-108: 준비 → 전투 페이즈 전환 (2026-04-18)
- `PhaseManager` 이벤트 4종, Space 토글
- `NavMeshBaker.BuildNow()` — NavMeshSurface.BuildNavMesh() 래퍼
- `PlacementController` 1/2 키 전환

### ✅ TASK-109: 기본 공격 시스템 (2026-04-19)
- `UnitStatsSO` (abstract ScriptableObject) + `MeleeStatsSO` (Strategy: `ExecuteAttack`)
- `UnitAttack.cs` — `OverlapSphere` 타겟 탐색 + 쿨타임
- `IGroupOwned` 인터페이스로 아군/적군 구분
- ⚠️ **(B)안에서 재작업 예정**: 자동 타겟 탐색 → 수동 타겟 주입(TASK-217)

### ✅ TASK-110: 구조물 파괴 → NavMesh 갱신 (2026-04-19)
- `Structure`에 `IDamageable` 적용 (float 타입 통일)
- 파괴 시 `SetActive(false)` → Carve 해제 → NavMesh 자동 복구
- 준비 페이즈 복구 로직 (`OnPreparationStart` 구독)

## 주의사항 (역사적 기록)

- Phase 1에서는 ML-Agents 학습 코드 포함하지 않음. Phase 2에서 시작.
- NavMesh Bake는 전투 시작 시 1회만. 전투 중 지형 변경은 Phase 4에서.
- 유닛 이동은 NavMeshAgent에 목표 좌표 주는 방식. AI 결정은 Phase 2에서.
- Phase 1 종료 시점의 테스트 도구(`BattleTestInput`, `StructureDamageTester`, `GroupTest`)는 Phase 2 종료 시 일괄 제거.
