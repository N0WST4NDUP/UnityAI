# Phase 2 — Unit Agent 학습

## 목표
Phase 1에서 구축한 전투 환경(NavMesh 자유 이동 + 기본 전투) 위에서 Unit Agent가 자유 이동 + 기본 전투를 학습한다. 직군별 보상 함수를 설계하고 Self-play로 기본 전투 행동을 습득시킨다.

## 선행 조건 (의존 Phase)
- Phase 1 완료 (전투 씬 기초: NavMesh 이동 + 공격 + 페이즈 전환)

## 세부 Task 목록

### TASK-201: 직군별 Observation 설계
- 구현 위치: `Assets/Scripts/Agents/UnitAgent.cs` 및 직군별 Agent
- 관측 구성 (초안, 실험 후 조정):
  - VectorSensor: 자기 상태 (HP, 위치, 직군, 팀) + 주변 유닛 정보 + 구조물 거리
  - RaySensor: 주변 부채꼴 스캔 (적/아군/구조물 태그 구분)
- 완료 조건:
  - 3직군 Agent가 CollectObservations에서 관측 수집 동작
  - 관측 크기가 고정 (유닛 수 변화에 무관)

### TASK-202: 직군별 보상 함수 구현
- 구현 위치: `Assets/Scripts/Agents/Rewards/`
- 보상 설계는 `docs/REWARDS.md` 참조
- 완료 조건:
  - 직군별 차별화된 보상 동작 확인
  - 공통 승리/패배 보상 동작

### TASK-203: Self-play 기본 전투 학습
- 구현 위치: Python `mlagents` 설정 + Unity 학습 씬
- 완료 조건:
  - Unit Agent가 적을 향해 이동 + 공격하는 기본 행동 학습
  - 탱커: 전방 진출 / 딜러: 타겟 추적 / 마법사: 거리 유지 + 스킬
  - TensorBoard로 학습 곡선 확인

### TASK-204: 시너지 파라미터 연결
- 구현 위치: `Assets/Scripts/Synergies/SynergyFlags.cs`, Agent Observation
- 완료 조건:
  - 시너지 플래그가 Agent observation에 포함
  - 시너지에 따른 행동 경향 변화 확인

### TASK-205: 구조물 부수기 vs 우회 행동 학습
- 구현 위치: Agent 행동 공간 + 보상 함수
- 완료 조건:
  - Agent가 벽 앞에서 부수기/우회 중 상황에 맞는 선택을 학습
  - 보상 튜닝으로 한쪽만 선택하지 않도록 조정

## 주의사항
- Observation 설계 시 Phase 3 Commander 명령 flag 슬롯을 미리 예약 (0 패딩).
- RaySensor 설정(각도, 거리, 태그)은 실험으로 조정. 변경 시 OBSERVATIONS.md 업데이트.
- 구조물 부수기 행동은 NavMeshAgent.SetDestination이 아닌 직접 이동으로 구현해야 함 (NavMesh는 구조물을 피하므로).
