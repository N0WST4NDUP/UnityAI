# Phase 5 — 발전 구조

## 목표
서버 공유 Policy 업데이트 파이프라인, ONNX 추론 로드, 보상 튜닝을 통해 전략 다양성과 극적 연출을 향상시킨다.

## 선행 조건 (의존 Phase)
- Phase 3 완료 (Hierarchical RL 기본 동작)

## 세부 Task 목록

### TASK-501: 판 종료 가중치 업데이트 파이프라인
- 구현 위치: `Assets/Scripts/Training/WeightUpdater.cs`
- 초기: 단순 덮어쓰기. Catastrophic Forgetting 모니터링.

### TASK-502: ONNX 추론 Unity 로드
- 구현 위치: `Assets/Scripts/Training/PolicyLoader.cs`
- 학습된 ONNX 모델을 Unity에서 로드하여 추론 모드 전투 실행.

### TASK-503: 전략 다양성 / 극적 연출 보상 튜닝
- "정문 돌파 팀" vs "우회 팀" 같은 전략 분화 유도.
- 변경 시 REWARDS.md 업데이트.

## 주의사항
- Catastrophic Forgetting 심각 시 EWC 도입 검토.
- 보상 튜닝은 TensorBoard 기반. 변경 히스토리 기록.
