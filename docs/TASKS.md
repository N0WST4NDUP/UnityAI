# TASKS.md

> 전체 작업 목록 + 진행 상태. Phase별 세부 명세는 `PHASE_N.md` 참조.

## 진행 중
- [ ] TASK-101: Grid 기반 필드 구현 (Phase 1) — 담당: System
- [ ] TASK-102: 구조물 배치 로직 (Phase 1) — 담당: System
- [ ] TASK-103: 유닛 Grid 이동 구현 (Phase 1) — 담당: Unit
- [ ] TASK-104: 직군별 기본 공격/스킬 시스템 (Phase 1) — 담당: Unit

## 예정 (Phase 2)
- [ ] TASK-201: 직군별 Observation 설계 — 담당: Unit
- [ ] TASK-202: 직군별 보상 함수 구현 — 담당: Unit
- [ ] TASK-203: Self-play 기본 전투 학습 — 담당: Unit
- [ ] TASK-204: 시너지 파라미터 연결 — 담당: Unit

## 예정 (Phase 3)
- [ ] TASK-301: Commander GridSensor 설계 — 담당: Commander
- [ ] TASK-302: 공성/수성 역할 플래그 VectorSensor — 담당: Commander
- [ ] TASK-303: Commander → Unit 명령 파이프라인 — 담당: Commander
- [ ] TASK-304: Hierarchical RL 학습 루프 — 담당: Commander

## 예정 (Phase 4)
- [ ] TASK-401: 바리케이트 셀 검증 로직 — 담당: System
- [ ] TASK-402: 포탄 위험 셀 + 잔여 틱 observation — 담당: System
- [ ] TASK-403: 아이템 사용 UI — 담당: System

## 예정 (Phase 5)
- [ ] TASK-501: 판 종료 가중치 업데이트 파이프라인 — 담당: System
- [ ] TASK-502: ONNX 추론 Unity 로드 — 담당: System
- [ ] TASK-503: 전략 다양성 / 극적 연출 보상 튜닝 — 담당: Commander

## 완료
_(없음)_

## 블로킹 / 의존
- Phase 2는 Phase 1 (TASK-101~104) 완료 후 시작
- Phase 3는 Phase 2 (TASK-201~204) 완료 후 시작
- Phase 4는 Phase 3 이후 병렬 가능
- Phase 5는 Phase 3 완료 후 시작
