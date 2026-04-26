# TASKS.md

> 전체 작업 목록 + 진행 상태. Phase별 세부 명세는 `PHASE_N.md` 참조.
> **2026-04-20 Feudal HRL 재설계 반영** — Phase 2/3 Task 전면 재편.

## 진행 중
- (없음 — TASK-211 완료, TASK-212 착수 대기)

## 확정된 설계 결정 (2026-04-20 Feudal HRL 재설계)
- ✅ AI 구조: Feudal HRL (커맨더·유닛 완전 독립 학습)
- ✅ 명령 단위: 분대(Squad) 단위, 준비 페이즈 편성 + 전투 중 고정
- ✅ 명령 필드: 3필드(target_position + priority_filter + structure_handling) 시작, 우선순위 순 확장
- ✅ 명령 갱신: 2초 고정 tick
- ✅ 유닛 순종: HP 0 외 안전밸브 없음
- ✅ 유닛 보상: 수행도 80% + micro 20%, **승/패 신호 금지**
- ✅ 커맨더 보상: Sparse(승/패) + 최소 dense(손실비·구조물·교착)
- ✅ 커맨더 관측: 완전 정보 (시야 제한은 Phase 3+ 연구 과제)
- ✅ 유닛 관측: 자기 + CurrentCommand + 분대 동료 + 지역 (적 전체는 안 봄)
- ✅ 학습 순서: 커리큘럼 3단계 (Scripted → RL 유닛 → RL 커맨더)

## 확정된 설계 결정 (이전 세션, 2026-04-19)
- ✅ `Unit` 클래스명 유지 — 이미 구현됨
- ✅ `_preparationPosition` 은 `Unit` 소유
- ✅ 컴포넌트 독립 구독 채택
- ✅ `UnitHealth` 별도 컴포넌트 분리
- ✅ HumanSoldier 구체 클래스 도입

---

## Phase 2 — 인프라 + 유닛 학습 (Stage 1~2)

### Stage 1: 인프라 구축 (RL 없이 파이프라인 검증)

- [x] **TASK-210**: DI 전환 — Unit/Structure의 FindGameObjectWithTag 제거 — 2026-04-26
- [x] **TASK-211**: Team 시스템 구축 (`Team`, `TeamRegistry`) — 2026-04-26
  - Team = POCO (`Assets/Scripts/Teams/Team.cs`), TeamRegistry = MonoBehaviour
  - RegistryManager 흡수 결정 → 폐기 (TASK-291 자동 해결)
  - 디렉토리 정리: `Placement/` → `Controller/` rename
- [ ] **TASK-212**: Squad 시스템 구축 (`Squad`, `SquadRegistry`, `Unit.SquadId`) — 담당: System
- [ ] **TASK-213**: SquadCommand 데이터 구조 + enum (`TargetPriority`, `StructureHandling`) — 담당: System
- [ ] **TASK-214**: CommandBroker — 2초 tick 발행 — 담당: System
- [ ] **TASK-215**: ScriptedCommander — 규칙 기반 커맨더 — 담당: System
- [ ] **TASK-216**: EpisodeManager — 승/패 판정 — 담당: System
- [ ] **TASK-217**: UnitAttack 수동화 ⭐ (타겟 외부 주입) — 담당: System
- [ ] **TASK-218**: UnitEnum 분리 — UnitType(종) vs UnitRole(직군) — 담당: System

### Stage 2: 유닛 RL 학습

- [ ] **TASK-201**: 유닛 Observation 구현 (OBSERVATIONS.md §1) — 담당: Unit
- [ ] **TASK-202**: 유닛 행동 공간 구현 (이동/타겟/스킬) — 담당: Unit
- [ ] **TASK-203**: 유닛 보상 구현 (REWARDS.md §1) — 담당: Unit
- [ ] **TASK-204**: Self-play 기본 전투 학습 — 담당: Unit
- [ ] **TASK-205**: 시너지 파라미터 연결 — 담당: Unit
- [ ] **TASK-206**: 구조물 부수기 vs 우회 학습 — 담당: Unit

### 제거 대상 (Phase 2 중/종료 시)

- [x] **TASK-290**: `GroupTest` 정적 변수 제거 — 2026-04-26 (PlacementController.ActiveGroupId로 흡수)
- [x] **TASK-291**: `RegistryManager.Update` 디버그 키 제거 — 2026-04-26 (RegistryManager 통째 삭제로 자동 해결)
- [ ] **TASK-292**: `BattleTestInput`·`StructureDamageTester`·`Testing/` 디렉토리 제거 (Phase 2 종료 시)

---

## Phase 3 — 커맨더 학습 (Stage 3)

- [ ] **TASK-310**: 유닛 Policy Freeze (ONNX export) — 담당: System
- [ ] **TASK-311**: CommanderAgent 관측 구현 (완전 정보) — 담당: Commander
- [ ] **TASK-312**: CommanderAgent 액션 구현 (분대별 명령 발행) — 담당: Commander
- [ ] **TASK-313**: 분대 편성 정책 (준비 페이즈, Policy 분리 여부 결정) — 담당: Commander
- [ ] **TASK-314**: Commander 보상 구현 (REWARDS.md §2) — 담당: Commander
- [ ] **TASK-315**: 공성/수성 역할 플래그 — 담당: Commander
- [ ] **TASK-316**: GridSensor 오버레이 비교 실험 — 담당: Commander
- [ ] **TASK-317**: 학습 루프 구성 + Scripted 대비 승률 검증 — 담당: Commander

---

## Phase 4 — 아이템 + 구조물 확장 (기존 유지)

- [ ] TASK-401: 바리케이드 시스템 — 담당: System
- [ ] TASK-402: 포탄 투하 시스템 — 담당: System
- [ ] TASK-403: 아이템 사용 UI — 담당: System
- [ ] TASK-404: 지형 편집 시스템 — 담당: System
- [ ] TASK-405: 구조물 종류 확장 — 담당: System

## Phase 5 — 발전 구조

- [ ] TASK-501: 판 종료 가중치 업데이트 파이프라인 — 담당: System
- [ ] TASK-502: ONNX 추론 Unity 로드 — 담당: System
- [ ] TASK-503: 전략 다양성 / 극적 연출 보상 튜닝 — 담당: Commander

---

## 완료

- [x] TASK-210: DI 전환 — Unit/Structure의 FindGameObjectWithTag 제거 (Phase 2 Stage 1) — 2026-04-26
  - PhaseManager만 DI 주입, RegistryManager 의존성은 일시 제거 (TASK-211 Team 시스템에서 재등장)
  - Unit/Structure: Awake 통째 삭제 + Init 시그니처 확장 + 이벤트 구독을 Init 안으로
  - UnitHealth: `_unit.Unregister()` 제거 (Register/Unregister 메서드 자체가 Unit에서 사라짐)
- [x] TASK-110: 구조물 파괴→NavMesh 갱신 연동 (Phase 1) — 2026-04-19
- [x] TASK-109: 기본 공격/스킬 시스템 (Phase 1) — 2026-04-19 (UnitStatsSO+MeleeStatsSO Strategy, UnitAttack)
- [x] TASK-108: 준비→전투 페이즈 전환 시스템 (Phase 1) — 2026-04-18
- [x] TASK-107: NavMesh 기반 유닛 이동 (Phase 1) — 2026-04-18
- [x] TASK-106: 유닛 기본 구현 (Phase 1) — 2026-04-19 (Unit abstract + HumanSoldier + UnitHealth/UnitMovement/UnitAnimator/UnitAttack)
- [x] TASK-105: 기존 코드 재설계 적용 (Phase 1) — 2026-04-17
- [x] TASK-102: 구조물 배치 로직 (Phase 1) — 2026-04-16
- [x] TASK-101: Grid 기반 필드 구현 (Phase 1) — 2026-04-15

---

## 블로킹 / 의존

- Phase 2 Stage 2 (TASK-201~206)는 Stage 1 인프라(TASK-210~218) 완료 후 시작
- Phase 3는 Phase 2 완료 (유닛 ONNX 확보) 후 시작
- Phase 4는 Phase 3 이후
- Phase 5는 Phase 3 완료 후

## 제거된 Task (2026-04-20 재설계로 무효화)

- ~~TASK-111 BoardRegistry~~ — 분대/팀 시스템으로 대체, 사실상 불필요
- ~~TASK-301 Commander 관측 방식 비교 (기존)~~ — TASK-316으로 재정의
- ~~TASK-302 공성/수성 역할 플래그 (기존)~~ — TASK-315로 이관
- ~~TASK-303 Commander → Unit 명령 파이프라인 (기존)~~ — TASK-213/214로 재정의 (Phase 2로 이관)
- ~~TASK-304 Hierarchical RL 학습 루프 (기존)~~ — TASK-317로 재정의

---

## 장기 TODO (Phase 6 — 오토체스 메타 시스템)

- 카드 풀 테이블 (코스트별 장수) + `ShopManager`
- `PlayerBoard.Bench` (대기 유닛) + `PlayerBoard.Field` (전투 참여 유닛) 3계층 분리
- 8인 배틀로얄 라운드 진행 로직
- 골드 / 상점 시스템
- 레벨 → 유닛 수 제한 시스템
- 전체 UI / UX

## 장기 TODO (미래 과제)

- 안개 전쟁 (커맨더 시야 제한) — Phase 3+ 연구
- 전투 중 분대 재편성 — Phase 4+ 확장
- 명령 필드 확장 (자세·대형·퇴각 조건) — 학습 부진 시 우선순위 순
- Alternating fine-tune (커맨더/유닛 교대 학습) — Phase 5 이후
