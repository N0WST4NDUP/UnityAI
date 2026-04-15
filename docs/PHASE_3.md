# Phase 3 — Commander Agent 연결

## 목표
Commander Agent를 도입하여 Unit Agent 위에 Hierarchical RL 구조를 완성한다. 전장 전체를 GridSensor로 관측하고 복합 명령을 Unit에 내려보낸다.

## 선행 조건 (의존 Phase)
- Phase 2 완료 (Unit Policy 수렴 확인)

## 세부 Task 목록

### TASK-301: Commander GridSensor 설계
- 구현 위치: `Assets/Scripts/Agents/CommanderAgent.cs`
- 입력: 전장 전체 Grid 상태 스냅샷
- 출력: Commander 전용 GridSensor
- 완료 조건:
  - 유닛 수 변화에 관계없이 관측 크기 불변
  - 구조물/위험 셀 채널 분리

### TASK-302: 공성/수성 역할 플래그 VectorSensor
- 완료 조건:
  - `isAttacker`, `isDefender` flag가 VectorSensor에 추가
  - 라운드 교대 시 flag 자동 반전

### TASK-303: Commander → Unit 명령 파이프라인
- 구현 위치: `Assets/Scripts/Agents/CommandBus.cs`
- 명령 종류: 총공세 / 후퇴 / 포지션 조정 / 타겟 포커싱 (복합 가능)
- 완료 조건:
  - Commander의 discrete action이 Unit observation에 브로드캐스트
  - 복합 명령 (예: 후퇴 + 특정 타겟 포커싱) 동시 적용 가능

### TASK-304: Hierarchical RL 학습 루프
- 완료 조건:
  - Commander와 Unit이 동시에 학습 (또는 교대 frozen)
  - `config/ppo/commander_agent.yaml` 분리
  - 학습 수렴 확인

## 주의사항
- Commander 명령이 Unit observation에 추가되면 TASK-201의 observation 크기가 바뀐다 → **재학습 필요**.
- Commander 학습률은 Unit보다 낮게 설정 (상위 정책 안정성).
- 공성/수성 플래그가 observation에 없으면 Commander가 역할 구분을 못 한다. 누락 주의.
