# Phase 1 — Grid 전투 씬 기초

## 목표
NavMesh 없이 Grid 기반 필드 위에서 유닛이 이동하고 기본 전투가 가능한 최소 전투 씬을 구축한다. ML-Agents 연결 전 단계이며, 이후 Phase 2 학습의 환경이 된다.

## 선행 조건 (의존 Phase)
- 없음 (프로젝트 시작점)

## 세부 Task 목록

### TASK-101: Grid 기반 필드 구현
- 구현 위치: `Assets/Scripts/Grid/GridField.cs`, `Assets/Scripts/Grid/GridCell.cs`
- 입력: 필드 크기 (예: 16x16), 셀 단위 크기
- 출력: 2D 배열 상태 (0=빈/1=구조물/2=유닛/3=위험), World ↔ Grid 좌표 변환 API
- 완료 조건:
  - `GridField.GetCellState(x, y)`, `SetCellState(x, y, state)` 동작
  - Unity Gizmos로 에디터에서 셀 상태 시각화
  - 씬 런타임에서 셀 점유 변화를 실시간 반영

### TASK-102: 구조물 배치 로직 (준비 페이즈)
- 구현 위치: `Assets/Scripts/Structures/StructurePlacer.cs`, `Assets/Scripts/Structures/Structure.cs`
- 입력: 유저 클릭 (임시 UI), 구조물 프리팹
- 출력: 해당 셀 상태를 1로 변경 + 내구도 컴포넌트 부착
- 완료 조건:
  - 준비 페이즈에서만 배치 가능
  - 유닛 점유 셀(2) 또는 이미 구조물 존재 셀(1)에 설치 불가
  - 파괴 시 셀 상태 0으로 복귀

### TASK-103: 유닛 Grid 기반 이동 구현
- 구현 위치: `Assets/Scripts/Units/UnitMover.cs`, `Assets/Scripts/Units/Unit.cs`
- 입력: 목표 셀 좌표
- 출력: 셀 단위 A* 또는 BFS 경로 + 보간 이동
- 완료 조건:
  - NavMesh 컴포넌트 일절 미사용
  - 이동 중 셀 점유(2) 갱신 (출발 셀 0, 도착 셀 2)
  - 구조물(1)과 타 유닛(2)을 장애물로 회피

### TASK-104: 기본 공격 / 스킬 시스템 (직군별)
- 구현 위치: `Assets/Scripts/Combat/` (Attack, Skill, DamageSystem 등)
- 입력: 공격자 Unit, 타겟 Unit 또는 셀
- 출력: 피해 적용, 사망 처리, 셀 점유 해제
- 완료 조건:
  - 탱커 / 딜러 / 마법사 3직군 기본 공격 동작
  - 마법사 범위 스킬은 Grid 셀 단위 AoE
  - 사망 시 유닛 풀에서 제거 + 셀 상태 0 복귀

## 주의사항
- **이동 시스템은 절대 NavMesh를 사용하지 않는다.** Phase 3 Commander의 GridSensor 관측 일관성이 깨진다.
- 셀 상태 enum은 `docs/ARCHITECTURE.md`의 값과 동기화 유지.
- 유닛 수는 이후 최대치 고정이므로, 풀(Pool) 구조로 설계 권장.
- Phase 1에서는 학습 코드를 절대 섞지 않는다. ML-Agents는 Phase 2에서 도입.
