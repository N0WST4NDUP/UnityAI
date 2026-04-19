# TASKS.md

> 전체 작업 목록 + 진행 상태. Phase별 세부 명세는 `PHASE_N.md` 참조.

## 진행 중
- [ ] TASK-201: 직군별 Observation 설계 (Phase 2) — 담당: Unit
- [ ] TASK-111: BoardRegistry 구현 여부 검토 — 필요성 재평가 (Phase 미정)

## 확정된 설계 결정 (2026-04-19 재논의 → 확정)
- ✅ `Unit` 클래스명 유지 (`UnitBody` 불채택) — 이미 구현됨
- ✅ `_preparationPosition` 은 `Unit` 소유 유지 — 이미 구현됨 (Unit.cs:10)
- ✅ 컴포넌트 독립 구독 채택 — 이미 구현됨 (Unit→OnPreparationStart, UnitMovement→OnBattleStart/End)
- ✅ `OnEnable` 암묵적 HP 리셋 제거 → `UnitHealth` 별도 컴포넌트로 분리됨
- ✅ `UnitMovement.WarpTo` YAGNI — 채택 안 함
- ✅ HumanSoldier 구체 클래스 도입 (Unit abstract, HumanSoldier : Unit)
- ⏳ `BoardRegistry` 필요성 재평가: Unit.Return()이 OnPreparationStart로 이미 처리 중 → 추가 목록 관리가 필요해질 시점에 도입 (TASK-111, Phase 미정)

## 예정 (Phase 2)
- [ ] TASK-201: 직군별 Observation 설계 — 담당: Unit
- [ ] TASK-202: 직군별 보상 함수 구현 — 담당: Unit
- [ ] TASK-203: Self-play 기본 전투 학습 — 담당: Unit
- [ ] TASK-204: 시너지 파라미터 연결 — 담당: Unit
- [ ] TASK-205: 구조물 부수기 vs 우회 행동 학습 — 담당: Unit

## 예정 (Phase 3)
- [ ] TASK-301: Commander 관측 방식 비교 실험 — 담당: Commander
- [ ] TASK-302: 공성/수성 역할 플래그 연결 — 담당: Commander
- [ ] TASK-303: Commander → Unit 명령 파이프라인 — 담당: Commander
- [ ] TASK-304: Hierarchical RL 학습 루프 — 담당: Commander

## 예정 (Phase 4)
- [ ] TASK-401: 바리케이드 시스템 — 담당: System
- [ ] TASK-402: 포탄 투하 시스템 — 담당: System
- [ ] TASK-403: 아이템 사용 UI — 담당: System
- [ ] TASK-404: 지형 편집 시스템 — 담당: System
- [ ] TASK-405: 구조물 종류 확장 — 담당: System

## 예정 (Phase 5)
- [ ] TASK-501: 판 종료 가중치 업데이트 파이프라인 — 담당: System
- [ ] TASK-502: ONNX 추론 Unity 로드 — 담당: System
- [ ] TASK-503: 전략 다양성 / 극적 연출 보상 튜닝 — 담당: Commander

## 완료
- [x] TASK-110: 구조물 파괴→NavMesh 갱신 연동 (Phase 1) — 2026-04-19 (Structure IDamageable 통일, 준비 페이즈 복구, GroupId 추가)
- [x] TASK-109: 기본 공격/스킬 시스템 (Phase 1) — 2026-04-19 (UnitStatsSO+MeleeStatsSO Strategy, UnitAttack, IGroupOwned)
- [x] TASK-108: 준비→전투 페이즈 전환 시스템 (Phase 1) — 2026-04-18
- [x] TASK-107: NavMesh 기반 유닛 이동 (Phase 1) — 2026-04-18
- [x] TASK-106: 유닛 기본 구현 (Phase 1) — 2026-04-17 → 2026-04-19 재설계 (Unit abstract + HumanSoldier + UnitHealth/UnitMovement/UnitAnimator/UnitAttack)
- [x] TASK-105: 기존 코드 재설계 적용 (Phase 1) — 2026-04-17
- [x] TASK-102: 구조물 배치 로직 (Phase 1) — 2026-04-16
- [x] TASK-101: Grid 기반 필드 구현 (Phase 1) — 2026-04-15

## 블로킹 / 의존
- Phase 2는 Phase 1 (TASK-109~110) 완료 후 시작
- Phase 3는 Phase 2 완료 후 시작
- Phase 4는 Phase 3 이후 (아이템은 Agent observation 반영 필요)
- Phase 5는 Phase 3 완료 후 시작

## 장기 TODO (Phase 미정, RoundManager 도입 시점에 검토)
- 구조물 라운드 복구 로직: `Structure.Init()` 재호출 또는 `Reset()` 메서드 설계
  - 풀링 매니저 필요성 재평가 (현재 규모에선 단순 `SetActive(true)` 순회로 충분할 수 있음)
  - Grid 재동기화는 RoundManager 책임 (구조물은 자기 레이어만)
  - 플레이어 선택(복구 / 포기 / 수리 비용) 정책 확정 필요

## 장기 TODO (Phase 6 — 오토체스 메타 시스템)
- 카드 풀 테이블 (코스트별 장수: 1코스트 40장, 2코스트 30장 등) + `ShopManager`
- `PlayerBoard.Bench` (대기 유닛) + `PlayerBoard.Field` (전투 참여 유닛) 3계층 분리
- 현 `BoardRegistry` 는 Phase 6 에서 `PlayerBoard.Field` 로 승격 이관 예정
