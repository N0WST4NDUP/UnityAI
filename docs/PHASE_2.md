# Phase 2 — Unit Agent 학습

## 목표
Phase 1의 Grid 전투 씬 위에 ML-Agents를 붙여 직군별 공유 Policy로 Self-play 학습을 가동한다. Commander는 아직 연결하지 않는다.

## 선행 조건 (의존 Phase)
- Phase 1 완료 (TASK-101 ~ TASK-104)

## 세부 Task 목록

### TASK-201: 직군별 Observation 설계
- 구현 위치: `Assets/Scripts/Agents/UnitAgent.cs` (기본 클래스) + 직군별 서브클래스
- 입력: 자기 상태 + 주변 n×n 셀 스냅샷 + 시너지 플래그
- 출력: `VectorSensor` / `GridSensor` observation
- 완료 조건:
  - `docs/OBSERVATIONS.md` 인덱스 맵과 일치
  - 최대 유닛 수 기준 고정 크기 + 0 패딩 + 존재 flag
  - 직군별 observation 차이 문서화

### TASK-202: 직군별 보상 함수 구현
- 구현 위치: `Assets/Scripts/Agents/Rewards/` (TankReward, DealerReward, MageReward)
- 완료 조건:
  - `docs/REWARDS.md` 수식과 일치
  - 승리 공통 보상 / 패배 공통 패널티 적용
  - 직군별 보상(어그로량/킬/범위 적중)이 독립 축적

### TASK-203: Self-play 기본 전투 학습
- 구현 위치: `config/ppo/unit_agent.yaml` + Python `mlagents-learn` 런처
- 완료 조건:
  - 2팀 대칭 환경에서 학습 수렴 확인
  - Tensorboard 상 승률 ~50% 수렴
  - ONNX export 성공

### TASK-204: 시너지 파라미터 연결
- 구현 위치: `Assets/Scripts/Synergies/SynergyFlags.cs` → UnitAgent observation
- 완료 조건:
  - 공격적/수비적 시너지 flag가 observation에 반영
  - reward shaping 분기 (전진/유지 가중치 변화)

## 주의사항
- **CUDA 환경 ONNX export 이슈**: `model_serialization.py`에 `dynamo=False` 패치가 필요할 수 있다 (CLAUDE.md 참조).
- Observation 크기는 학습 시작 후 변경하면 재학습이 강제되므로, TASK-201에서 확정.
- Self-play 시 Commander가 없으므로 유닛이 제자리에서 진동할 수 있음 → 이동 인센티브 소폭 부여 허용.
