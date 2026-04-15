# Phase 4 — 아이템 시스템

## 목표
전투 중 유저 개입 포인트인 아이템(바리케이트, 포탄)을 Grid에 연결하고 Agent가 이를 환경 변화로 인식하도록 한다.

## 선행 조건 (의존 Phase)
- Phase 3 완료 (Commander가 위험 셀을 관측할 수 있어야 회피 학습 가능)

## 세부 Task 목록

### TASK-401: 바리케이트 셀 검증 로직
- 구현 위치: `Assets/Scripts/Items/Barricade.cs`
- 제약: 아군 필드 절반에만 설치 가능 / 유닛 점유 셀 불가
- 완료 조건:
  - 검증 실패 시 설치 취소 + 유저 피드백
  - 설치 시 해당 셀 상태 1로 변경, 임시 내구도 부여

### TASK-402: 포탄 위험 셀 + 잔여 틱 observation
- 구현 위치: `Assets/Scripts/Items/Cannon.cs` + Grid 상태 확장
- 완료 조건:
  - 포탄 낙하 n초 전부터 해당 셀 상태 3 (위험)
  - 잔여 틱 수가 observation 채널로 노출
  - 낙하 시 범위 피해 + 셀 상태 0 복귀

### TASK-403: 아이템 사용 UI
- 구현 위치: `Assets/Scripts/UI/ItemPanel.cs`
- 완료 조건:
  - 전투 중 유저가 아이템 선택 → 셀 클릭으로 사용
  - 쿨다운 / 소지 제한 UI 표시

## 주의사항
- 위험 셀 상태(3)가 observation 채널에 추가되면 Commander/Unit observation 크기가 또 바뀐다 → Phase 3 완료 후 **일괄 적용 권장**.
- 바리케이트 내구도는 구조물과 동일 시스템 재사용.
- 포탄의 잔여 틱은 0~n 정수로 정규화 후 전달.
