# Phase 3 — Commander Agent 연결

## 목표
전장 전체를 관측하는 Commander Agent를 구현하고, Unit Agent에 전략 명령을 내리는 Hierarchical RL 구조를 완성한다.

## 선행 조건 (의존 Phase)
- Phase 2 완료 (Unit Agent 기본 전투 학습)

## 세부 Task 목록

### TASK-301: Commander 관측 방식 비교 실험
- 구현 위치: `Assets/Scripts/Agents/CommanderAgent.cs`
- 실험 내용:
  - **방안 A**: VectorSensor — 모든 유닛 위치/상태/HP를 벡터로 직접 입력
  - **방안 B**: GridSensor 오버레이 — 유닛의 연속 좌표를 Grid에 투영하여 공간 인식
- 완료 조건:
  - 두 방식 모두 구현 + 학습 실험
  - 성능/학습 속도 비교 후 최종 방식 확정
  - 결과를 OBSERVATIONS.md에 기록

### TASK-302: 공성/수성 역할 플래그 연결
- 구현 위치: Commander + Unit observation
- 완료 조건:
  - 역할 플래그가 관측에 포함
  - 역할별 전략 경향 학습 확인

### TASK-303: Commander → Unit 명령 파이프라인
- 구현 위치: `Assets/Scripts/Agents/CommandBus.cs`
- 명령 종류: 총공세 / 후퇴 / 타겟 포커싱 / 포지션 조정
- 완료 조건:
  - Commander action → CommandBus → Unit Agent observation에 명령 flag 반영
  - Unit Agent가 명령에 따라 행동 변화 확인

### TASK-304: Hierarchical RL 학습 루프 구성
- 구현 위치: Python `mlagents` 설정 + Unity 학습 씬
- 완료 조건:
  - Commander + Unit 동시 학습 동작
  - Commander 명령이 전투 결과에 유의미한 영향
  - TensorBoard로 학습 곡선 확인

## 주의사항
- Commander 관측 방식은 **반드시 비교 실험 후 확정**. 사전 가정으로 결정하지 않는다.
- Commander action frequency는 Unit보다 낮을 수 있음 (N틱마다 1회). 실험으로 결정.
- Hierarchical RL 보상 전파: Commander=게임 결과 기반, Unit=행동 기반. 타임스케일 차이에 유의.
