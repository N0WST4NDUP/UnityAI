# Phase 5 — 발전 구조

## 목표
판 종료 시 학습된 Policy를 서버에 반영하고, ONNX 추론을 Unity에 로드하여 포트폴리오 수준의 발전 파이프라인을 완성한다.

## 선행 조건 (의존 Phase)
- Phase 3 완료 (Commander + Unit 학습 가능한 상태)
- Phase 4는 병렬 가능

## 세부 Task 목록

### TASK-501: 판 종료 후 가중치 업데이트 파이프라인
- 구현 위치: `Assets/Scripts/Training/WeightUpdater.cs`, Python 측 병합 스크립트
- 완료 조건:
  - 판 종료 시 로컬 체크포인트 → 중앙 저장소 단순 덮어쓰기
  - Catastrophic Forgetting 모니터링 로그 출력
  - (필요 시) EWC 도입 훅 확보

### TASK-502: ONNX 추론 Unity 로드
- 구현 위치: `Assets/Scripts/Agents/PolicyLoader.cs`
- 완료 조건:
  - 학습된 ONNX 파일을 런타임에 교체 가능
  - Unity Sentis 또는 ML-Agents 내장 추론 경로 선택
  - 추론 FPS 측정 (Intel Arc GPU 기준)

### TASK-503: 전략 다양성 / 극적 연출 보상 튜닝
- 완료 조건:
  - 동일 상황에서 Commander가 복수 전략을 보이는지 평가
  - 지루한 대치 상황 패널티 / 극적 역전 보너스 튜닝
  - Tensorboard metric 추가

## 주의사항
- 가중치 덮어쓰기는 단순 시작이지만, 판 수가 쌓이면 Catastrophic Forgetting 위험. 지표 모니터링 필수.
- ONNX export 시 CUDA 환경 `dynamo=False` 패치 이슈 재확인.
- 포트폴리오 목표이므로 서버 분리는 하지 않는다 (로컬 파일 기반).
