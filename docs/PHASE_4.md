# Phase 4 — 아이템 + 구조물 확장

## 목표
전투 중 유저 개입 시스템(바리케이드, 포탄)과 준비 페이즈 지형 편집, 구조물 종류 확장을 구현한다.

## 선행 조건 (의존 Phase)
- Phase 3 완료 (Hierarchical RL 기본 동작)
- Phase 1의 NavMesh 시스템이 안정적으로 동작

## 세부 Task 목록

### TASK-401: 바리케이드 시스템
- 구현 위치: `Assets/Scripts/Items/Barricade.cs`
- 방식: 자유 좌표 + 방향 회전 + NavMeshObstacle(Carve=true)
- 완료 조건:
  - 전투 중 클릭+드래그로 바리케이드 배치
  - NavMesh 실시간 갱신 → 유닛 경로 재계산
  - Agent가 바리케이드를 인식하고 대응

### TASK-402: 포탄 투하 시스템
- 구현 위치: `Assets/Scripts/Items/Bombardment.cs`
- 방식: 자유 좌표 AoE, 낙하 예고 시각 표시 → N초 후 폭발
- 완료 조건:
  - 낙하 예고 원형 표시 + 폭발 데미지
  - Agent가 예고 영역을 인식하고 회피 가능

### TASK-403: 아이템 사용 UI
- 구현 위치: `Assets/Scripts/UI/ItemPanel.cs`
- 완료 조건: 전투 중 아이템 선택 + 사용 UI 동작

### TASK-404: 지형 편집 시스템
- 구현 위치: `Assets/Scripts/Terrain/TerrainEditor.cs`
- 방식: 준비 페이즈에서 수성 측 진영 높낮이 조절 (타이쿤류)
- 완료 조건:
  - 마우스로 지형 높낮이 조절 (범위 제한)
  - 전투 시작 시 NavMesh Bake에 반영

### TASK-405: 구조물 종류 확장
- 확장 종류: 문(Gate), 탑(Tower), 계단(Stairs), 사다리(Ladder) 등
- 완료 조건:
  - 각 구조물 고유 특성 동작
  - NavMesh와 올바른 상호작용

## 주의사항
- 바리케이드/포탄은 Agent observation에 반영 필요. Phase 2~3 observation 확장 슬롯 활용.
- 지형 편집은 준비 페이즈에서만 가능. 전투 중 변경 불가.
