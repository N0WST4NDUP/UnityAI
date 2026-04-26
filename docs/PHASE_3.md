# Phase 3 — Commander Agent 학습 (커리큘럼 Stage 3)

> **2026-04-20 Feudal HRL 재설계 반영** — 유닛 Policy를 **freeze** 한 상태에서 커맨더를 독립 학습.

## 목표

1. 학습된 유닛 Policy를 ONNX로 freeze
2. CommanderAgent 구현 (완전 정보 관측 + 분대별 명령 발행)
3. 분대 편성 정책 (준비 페이즈)과 명령 발행 정책 (전투 페이즈)의 구조 확정
4. 커맨더 단독 RL로 고유 전략 학습 (유닛 종속성 없이)

## 선행 조건 (의존 Phase)

- Phase 2 완료 (TASK-210~218 인프라 + TASK-201~206 유닛 학습)
- Stage 2 결과 ONNX 모델 확보 (직군별)

---

## 세부 Task 목록

### TASK-310: 유닛 Policy Freeze
- 구현 위치: Python `mlagents` → `--inference` 또는 ONNX export
- 작업:
  - Stage 2에서 학습된 직군별 ONNX 저장
  - Unity에서 BehaviorType을 `InferenceOnly`로 고정
- 완료 조건:
  - 유닛 Agent가 학습 중 가중치 업데이트 없이 추론만 수행
  - TensorBoard에 유닛 reward 기록이 남지 않아야 함

### TASK-311: CommanderAgent 구현 — 관측
- 구현 위치: `Assets/Scripts/Agents/CommanderAgent.cs`
- 기반: OBSERVATIONS.md §2 (완전 정보 + 분대 현재 명령)
- 완료 조건:
  - 전장 메타, 아군/적군 유닛, 구조물, 분대 명령 관측 수집
  - 관측 크기 ~222차원 (초안)

### TASK-312: CommanderAgent 구현 — 액션 (전투)
- 구현 위치: `CommanderAgent.OnActionReceived`
- 액션: 분대 수 × 3필드 (target_position + priority_filter + structure_handling)
- `CommandBroker`에 액션을 push, Broker가 분대에 배포
- 완료 조건:
  - 2초 tick마다 모든 분대에 새 명령 발행
  - Scripted 커맨더를 완전히 대체

### TASK-313: 분대 편성 정책 (준비 페이즈)
- 구현 위치: `CommanderAgent` 또는 별도 `SquadFormationAgent`
- 액션: 보유 유닛을 N개 분대에 할당 (discrete)
- **설계 결정 필요**: 전투 정책과 1개로 묶을지 2개로 분리할지
  - 옵션 A: 단일 Agent, 2 head (prep head + battle head)
  - 옵션 B: 2개 Agent (PrepCommander + BattleCommander)
- 완료 조건:
  - 준비 페이즈에 분대 편성 액션 발행
  - 편성이 전투 성과에 유의미한 영향을 주는지 검증

### TASK-314: Commander 보상 구현 (REWARDS.md §2 기반)
- 구현 위치: `Assets/Scripts/Agents/Rewards/CommanderRewardEvaluator.cs`
- Sparse: 승/패/무승부
- 최소 Dense: 유닛 손실·처치·구조물 파괴/상실·교착
- 완료 조건:
  - 에피소드 종료 시 sparse 보상
  - 이벤트 기반 dense 보상
  - 유닛 보상과 독립 (공유 항목 없음)

### TASK-315: 공성/수성 역할 플래그
- 구현 위치: Commander observation + SquadCommand 필드
- 라운드마다 역할 교대 → 커맨더가 역할별 전략 학습
- 완료 조건:
  - 역할 flag가 관측에 포함
  - 수성 시 방어 전략, 공성 시 돌파 전략 경향 관찰

### TASK-316: GridSensor 오버레이 비교 실험
- 구현 위치: `CommanderAgent` 관측 2가지 버전
- 실험:
  - **방안 A**: VectorSensor 단독 (TASK-311 기본)
  - **방안 B**: VectorSensor + GridSensor 오버레이 (공간 인식 보강)
- 완료 조건:
  - 두 방식 학습 곡선 비교
  - 결과를 OBSERVATIONS.md §2-3에 기록 후 최종 방식 확정

### TASK-317: 학습 루프 구성
- 구현 위치: Python `mlagents` config + Unity 학습 씬
- 설정:
  - Commander BehaviorName 단일
  - 유닛 ONNX freeze
  - Self-play (양 팀 같은 Commander policy)
- 완료 조건:
  - 커맨더가 다양한 전략(우회·공성·수비) 발화
  - Scripted 커맨더 대비 승률 우위 확인

---

## 주의사항

- **유닛 Policy는 반드시 freeze**. 공동 학습으로 되돌아가면 종속성 재등장.
- Commander tick 주기(2초)는 Phase 2의 `CommandBroker`와 일치해야 함.
- 분대 수·구조는 Phase 2에서 고정된 상태여야 학습 가능.
- 커맨더 관측 방식(VectorSensor vs Grid 오버레이)은 **사전 가정 금지**, 반드시 실험 후 확정.
- Hierarchical RL의 보상 전파: **Commander=느린 시그널, Unit=빠른 시그널**. 이 둘이 섞이지 않게 주의.
- 분대 편성 정책은 Phase 3 후반에 확장해도 됨 — 초기엔 수동/규칙 편성으로 시작해도 OK.
