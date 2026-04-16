# Phase 1 — 전투 씬 기초

## 목표
준비 페이즈(Grid 배치) → 전투 페이즈(NavMesh 자유 이동) 전환이 동작하고, 유닛이 자유롭게 이동하며 기본 전투가 가능한 최소 전투 씬을 구축한다. ML-Agents 연결 전 단계이며, 이후 Phase 2 학습의 환경이 된다.

## 선행 조건 (의존 Phase)
- 없음 (프로젝트 시작점)

## 기존 완료 작업 (재설계 전 완료, 수정 필요)
- `GridField.cs`, `GridCell.cs` — Grid 기반 셀 관리 (유지, CellState.Danger 제거 필요)
- `Structure.cs` — 내구도 관리 (유지, NavMeshObstacle 연결 필요)
- `StructurePlacer.cs` — 준비 페이즈 배치 (유지)
- `StructureDamageTester.cs` — 테스트용 (유지)

## 세부 Task 목록

### TASK-105: 기존 코드 재설계 적용
- 구현 위치: `Assets/Scripts/Grid/GridCell.cs`, `Assets/Scripts/Structures/Structure.cs`
- 작업 내용:
  - `CellState.Danger` 제거 (전투 중 위험 영역은 월드 좌표 AoE로 처리)
  - `Structure.cs`에 NavMeshObstacle 연동 추가 (Die() 시 Obstacle 비활성화)
- 완료 조건:
  - CellState enum: Empty(0), Structure(1), Occupied(2) 세 가지만 존재
  - Structure 파괴 시 NavMeshObstacle 비활성화 확인

### TASK-106: 유닛 기본 구현
- 구현 위치: `Assets/Scripts/Units/Unit.cs`
- 입력: HP, 팀 ID, 직군
- 출력: 데미지 처리, 사망 시 제거
- 완료 조건:
  - TakeDamage / Die 동작
  - 준비 페이즈에서 Grid 셀에 배치 가능
  - 전투 시작 시 World 좌표로 전환

### TASK-107: NavMesh 기반 유닛 이동
- 구현 위치: `Assets/Scripts/Units/UnitMovement.cs`
- 입력: 목표 위치 (World 좌표)
- 출력: NavMeshAgent를 통한 이동
- 완료 조건:
  - NavMeshAgent.SetDestination()으로 자유 이동
  - 구조물(NavMeshObstacle)을 자동 회피
  - 구조물 파괴 후 해당 경로로 이동 가능 확인

### TASK-108: 준비 → 전투 페이즈 전환 시스템
- 구현 위치: `Assets/Scripts/Phase/PhaseManager.cs`, `Assets/Scripts/Phase/NavMeshBaker.cs`
- 작업 내용:
  - 준비 페이즈: Grid 활성, 유닛/구조물 배치
  - 전투 시작 버튼: NavMeshSurface.BuildNavMesh() → 유닛 NavMeshAgent 활성화
  - 전투 페이즈: Grid 비활성, 자유 이동
- 완료 조건:
  - 준비 페이즈에서 유닛/구조물 배치 후 "전투 시작" → NavMesh Bake 성공
  - 유닛이 NavMesh 위에서 자유 이동
  - 구조물이 NavMeshObstacle로 동작 (유닛이 회피)

### TASK-109: 기본 공격 / 스킬 시스템 (직군별)
- 구현 위치: `Assets/Scripts/Combat/` (Attack, Skill, DamageSystem 등)
- 입력: 공격자 Unit, 타겟 Unit 또는 위치
- 출력: 피해 적용, 사망 처리
- 완료 조건:
  - 탱커 / 딜러 / 마법사 3직군 기본 공격 동작
  - 마법사 범위 스킬은 월드 좌표 AoE
  - 사망 시 유닛 제거

### TASK-110: 구조물 파괴 → NavMesh 갱신 연동
- 구현 위치: `Assets/Scripts/Structures/Structure.cs`
- 작업 내용:
  - 구조물 파괴 시 NavMeshObstacle 제거
  - NavMesh 자동 복구 확인 (Carve 기능)
  - 유닛이 파괴된 구조물 자리를 통과할 수 있는지 확인
- 완료 조건:
  - 구조물 파괴 → 해당 위치 NavMesh 복구 → 유닛 통과 가능

## 주의사항
- Phase 1에서는 ML-Agents 학습 코드를 섞지 않는다.
- NavMesh Bake는 전투 시작 시 1회만. 전투 중 지형 변경은 Phase 4.
- 유닛 이동은 NavMeshAgent에 목표 좌표를 주는 방식. Agent(AI)가 목표를 결정하는 것은 Phase 2.
- 현재는 테스트 목적으로 마우스 클릭 등 임시 입력으로 유닛 이동/공격 테스트.
