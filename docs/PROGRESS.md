# PROGRESS.md

세션별 진행 로그.

---

## 2026-04-15 — 세션 초기화

### 목표
- CLAUDE.md의 세션 초기화 절차 수행
- `docs/` 디렉토리 및 파생 문서 10종 생성
- Phase 1 착수 준비

### 완료
- [x] `docs/TASKS.md` 생성 — 전체 Task 목록 + Phase별 분류
- [x] `docs/PHASE_1.md` ~ `docs/PHASE_5.md` 생성 — Phase 세부 작업 명세
- [x] `docs/ARCHITECTURE.md` 생성 — 디렉토리 구조 + 클래스 책임 + ML-Agents 상속 구조
- [x] `docs/REWARDS.md` 생성 — 직군별 보상 함수 수식
- [x] `docs/OBSERVATIONS.md` 생성 — Commander/Unit Observation Space 인덱스 맵
- [x] `docs/PROGRESS.md` 생성 (현재 파일)

### 미완료 / 다음 세션
- [ ] Phase 1 착수 — TASK-101 Grid 기반 필드 구현부터 시작
- [ ] 기존 `Assets/Scripts/RaySensorAgent.cs`와 Phase 1 설계의 관계 정리 필요 (학습용 튜토리얼 자산인지 확인)

### 메모
- 작업 브랜치를 `AutoChessLike`로 확정 (CLAUDE.md 업데이트 완료). 앞으로 모든 Phase 작업은 이 브랜치에서 진행.
- 워킹 트리에 수정/미추적 파일 다수 존재 (SampleScene, TutorialInfo, Action Minis Demo 등)
