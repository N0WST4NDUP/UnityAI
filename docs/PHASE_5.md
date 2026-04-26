# Phase 5 — 발전 구조

> **2026-04-20 Feudal HRL 재설계 반영** — ONNX는 유닛(직군별) + 커맨더 모두 관리. Alternating fine-tune이 장기 옵션으로 추가.

## 목표

서버 공유 Policy 업데이트 파이프라인, ONNX 추론 로드, 보상 튜닝을 통해 전략 다양성과 극적 연출을 향상시킨다.

## 선행 조건 (의존 Phase)
- Phase 3 완료 (커맨더 RL 학습 완료)
- Phase 4 아이템·지형 시스템 (선택)

## 관리 대상 Policy

| Policy | 소스 | 업데이트 주기 |
|---|---|---|
| Tank Policy (공유) | Phase 2 산출 | 판 종료 시 |
| Dealer Policy (공유) | Phase 2 산출 | 판 종료 시 |
| Mage Policy (공유) | Phase 2 산출 | 판 종료 시 |
| Commander Policy | Phase 3 산출 | 판 종료 시 |

> 직군 추가 시 ONNX 파일도 추가됨. `PolicyLoader`가 직군별 매핑.

## 세부 Task 목록

### TASK-501: 판 종료 가중치 업데이트 파이프라인
- 구현 위치: `Assets/Scripts/Training/WeightUpdater.cs`
- 초기: **단순 덮어쓰기** (직군별 + 커맨더 각각)
- 모니터링: Catastrophic Forgetting (과거 전략 망각)
- 완료 조건:
  - 판 종료 시 모든 Policy ONNX 저장
  - 다음 판 시작 시 저장된 ONNX 로드

### TASK-502: ONNX 추론 Unity 로드
- 구현 위치: `Assets/Scripts/Training/PolicyLoader.cs`
- 기능:
  - BehaviorName → ONNX 파일 매핑
  - `ModelAsset` 런타임 교체 (Agent.SetModel)
- 완료 조건:
  - 학습 모드(`Default`) ↔ 추론 모드(`InferenceOnly`) 전환
  - 각 Agent가 자기 BehaviorName에 맞는 ONNX 로드

### TASK-503: 전략 다양성 / 극적 연출 보상 튜닝

새 설계 하에서의 "전략 다양성"은 **커맨더의 명령 어휘 활용도**로 측정.

- 지표:
  - 명령 priority_filter 분포 (편중 없이 다양하게 쓰는가)
  - structure_handling 분포 (Break/Detour/Ignore 모두 사용하는가)
  - target_position 분포 (맵 전역 활용하는가)
- 튜닝 대상:
  - 커맨더 dense reward의 교착 패널티
  - 공성/수성 역할 flag에 따른 reward shaping
- 완료 조건:
  - "정문 돌파" 판 vs "우회" 판 같은 상이한 전개 관찰
  - REWARDS.md §2 계수 튜닝 히스토리 기록

### TASK-504: Alternating Fine-tune (선택)
- 구현 위치: Python mlagents config (step-based freeze/unfreeze)
- 배경: 커리큘럼 3단계 후 상호 적응 추가 학습. **승/패 보상 유닛 투입 금지 원칙 유지**.
- 설정:
  - Commander N step 학습 → freeze
  - 유닛 M step 학습 → freeze
  - 반복
- 완료 조건:
  - 두 policy가 서로 적응하면서도 종속성 발생 없이 수렴
  - 단독 학습 대비 승률 개선 확인

## 주의사항

- **Catastrophic Forgetting** 심각 시 EWC (Elastic Weight Consolidation) 도입 검토.
  - 증상: 최근 판만 잘하고 이전 판 상대에 약해짐
  - 감지: 과거 체크포인트 대비 승률 롤링 평가
- 보상 튜닝은 **TensorBoard 기반**. 계수 변경 히스토리 REWARDS.md에 기록.
- Alternating fine-tune은 **선택 사항**. 독립성이 흔들린다는 징후가 있으면 보류.
- ONNX 파일 버전 관리 필요 (직군 추가·관측 크기 변경 시 구 ONNX 폐기).
