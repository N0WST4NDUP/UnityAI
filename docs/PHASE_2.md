# Phase 2 — Unit Agent 학습 (커리큘럼 Stage 1~2)

> **2026-04-20 Feudal HRL 재설계 반영** — 커리큘럼 3단계 중 Stage 1~2에 해당. Stage 3는 Phase 3에서 진행.

## 목표

1. 팀/분대/명령 인프라 구축 (Team, Squad, SquadCommand, CommandBroker)
2. Scripted 커맨더로 분대별 명령 파이프라인 동작 확인 (Stage 1)
3. Scripted 커맨더 아래에서 **Unit Agent(직군별 공유 Policy)**가 명령 수행도 + micro 품질을 학습 (Stage 2)

## 선행 조건 (의존 Phase)
- Phase 1 완료 (전투 씬 기초, NavMesh 이동/공격, 구조물 연동)
- DI 전환 (Unit/Structure의 `FindGameObjectWithTag` 제거)

## 커리큘럼 스테이지

| Stage | 커맨더 | 유닛 | 목적 |
|---|---|---|---|
| 1 | Scripted | Scripted (Heuristic 또는 BT) | 파이프라인 검증 |
| 2 | Scripted | **RL 학습 중** | 유닛 정책 학습 (Phase 2 중심) |
| 3 | **RL 학습 중** | ONNX freeze | Phase 3 |

---

## 세부 Task 목록

### 인프라 구축 (Stage 1 준비)

#### TASK-210: DI 전환 — Unit/Structure의 FindGameObjectWithTag 제거
- 구현 위치: `Unit.cs`, `Structure.cs`, 관련 `Init()` 호출부
- 변경: 매니저 참조는 생성자/Init 주입으로 전환. 병렬 학습 환경 안전 확보.
- 완료 조건: `Awake`에서 Find 호출 제거, 모든 의존성 명시적 주입.

#### TASK-211: Team 시스템 구축
- 구현 위치: `Assets/Scripts/Teams/Team.cs`, `TeamRegistry.cs`
- 책임: 팀 식별, 소유 유닛·분대·골드·시너지. `GroupTest` 정적 변수 대체.
- 완료 조건:
  - `UnitPlacer`·`StructurePlacer`가 Team에서 GroupId 획득
  - `GroupTest` 참조 전부 제거

#### TASK-212: Squad 시스템 구축
- 구현 위치: `Assets/Scripts/Squads/Squad.cs`, `SquadRegistry.cs`, `Unit.cs` 수정
- 책임: 분대 정의, 유닛 ↔ 분대 매핑, 현재 명령 보관
- 변경: `Unit`에 `SquadId` 필드 추가, `Init(groupId, squadId, position)` 확장
- 완료 조건:
  - 준비 페이즈에서 커맨더(수동 입력 OK)가 분대 편성 가능
  - Squad가 자기 유닛 목록 유지

#### TASK-213: SquadCommand 데이터 구조
- 구현 위치: `Assets/Scripts/Commands/SquadCommand.cs` + enum 3종
- 필드: `target_position`, `priority_filter`, `structure_handling` (3필드 시작)
- 완료 조건: 구조체 정의 + 테스트 케이스 (인코딩·직렬화)

#### TASK-214: CommandBroker — 2초 tick 발행
- 구현 위치: `Assets/Scripts/Commands/CommandBroker.cs`
- 책임: 2초마다 커맨더(ScriptedCommander 또는 CommanderAgent)에서 명령 수집 → 분대에 push
- 완료 조건:
  - Battle 시작 시 tick 개시, Battle 종료 시 정지
  - Scripted 커맨더 연결 시 실제로 Squad.CurrentCommand가 갱신됨

#### TASK-215: ScriptedCommander — 규칙 기반 커맨더
- 구현 위치: `Assets/Scripts/Commands/ScriptedCommander.cs`
- 규칙(초안):
  - 탱커 분대 → 적 최전방 영역 이동 + 탱커 우선순위 + 구조물 부수기
  - 딜러 분대 → 탱커 후방 배치 + 딜러 우선순위 + 구조물 무시
  - 마법사 분대 → 원거리 유지 + 전체 대상
- 완료 조건: Stage 1 데모가 동작 (Heuristic 유닛으로도 명령 수행)

#### TASK-216: EpisodeManager — 승/패 판정
- 구현 위치: `Assets/Scripts/Episode/EpisodeManager.cs`
- 판정: 한 팀 전멸 / 시간 초과 (180초 등)
- 완료 조건: Battle 종료 시 자동으로 승/패 결과 발행

#### TASK-217: UnitAttack 수동화 ⭐
- 구현 위치: `Assets/Scripts/Units/UnitAttack.cs`
- 변경: `OverlapSphere` 자동 타겟 제거 → `SetTarget(IDamageable)` 외부 주입
- 쿨타임·범위 체크는 유지, 타겟 선택은 Agent 책임으로 이관
- 완료 조건: Agent가 `SetTarget` 호출 시에만 공격 발동

#### TASK-218: UnitEnum 분리 — Class vs Role
- 구현 위치: `Assets/Scripts/Units/UnitEnum.cs`
- 변경: `UnitType`(종, 예: Archer/Soldier) + `UnitRole`(직군, Tank/Dealer/Mage) 분리
- 공유 Policy는 `UnitRole` 기준
- 완료 조건: 유닛 프리팹의 Role/Type이 독립 설정 가능

---

### RL 유닛 학습 (Stage 2)

#### TASK-201: 유닛 Observation 구현 (OBSERVATIONS.md §1 기반)
- 구현 위치: `Assets/Scripts/Agents/SoldierAgent.cs` (또는 Role별 분리)
- 관측 구성:
  - 자기 상태, CurrentCommand, 분대 동료, 지역 적/구조물, 시너지 flag
- VectorSensor ~64차원 + RaySensor 16 ray
- 완료 조건:
  - 유닛이 자기 분대의 명령을 관측으로 받음
  - 관측 크기가 유닛 수 변동과 무관 (0 패딩 + flag)

#### TASK-202: 유닛 행동 공간 구현
- 구현 위치: `SoldierAgent.OnActionReceived`
- 액션:
  - 이동 방향 (continuous 2D, NavMeshAgent에 전달)
  - 타겟 선택 (discrete, 주변 후보 중 1)
  - 스킬 사용 여부 (discrete, 직군별)
- 완료 조건: Agent 액션 → `UnitMovement.MoveTo` + `UnitAttack.SetTarget` 연결

#### TASK-203: 유닛 보상 구현 (REWARDS.md §1 기반)
- 구현 위치: `Assets/Scripts/Agents/Rewards/UnitRewardEvaluator.cs`
- 수행도 80% (타겟 매치·영역 근접/수렴·구조물 처리·명령 위반)
- Micro 20% (공격 성공·피격 회피·스킬 적중·사망·오폭)
- **승/패 보상 절대 금지 확인**
- 완료 조건:
  - 직군별 차별 계수 적용
  - 매 step 유닛에게 보상 발행

#### TASK-204: Self-play 기본 전투 학습
- 구현 위치: Python `mlagents` 설정 + Unity 학습 씬
- 설정:
  - 두 팀 모두 같은 ScriptedCommander 사용
  - 유닛 Policy는 직군별 BehaviorName 공유
- 완료 조건:
  - Unit Agent가 명령을 수행하는 기본 행동 학습
  - TensorBoard로 수행도 리워드 곡선 상승 확인

#### TASK-205: 시너지 파라미터 연결
- 구현 위치: `Assets/Scripts/Synergy/SynergyFlags.cs` + Agent Observation/Reward
- 완료 조건:
  - 시너지 flag가 Agent observation에 포함 (60-63 인덱스)
  - 시너지 → 보상 계수 조정 (REWARDS.md §3)
  - 시너지 변경 시 행동 경향 변화 확인

#### TASK-206: 구조물 부수기 vs 우회 학습
- 구현 위치: 유닛 행동 + 보상
- 기반: `structure_handling` 필드 (Break/Detour/Ignore)
- 완료 조건:
  - 명령이 `Break`면 구조물 공격 보상 획득
  - 명령이 `Detour`면 구조물 회피 보상 획득
  - 두 보상이 상호 배타적으로 동작

---

## 제거 대상 Task (Phase 2 착수 전~종료 시)

| 대상 | 시점 | 대체 |
|---|---|---|
| `GroupTest` 전역 변수 | TASK-211 완료 직후 | Team.GroupId |
| `RegistryManager.Update` 디버그 키 (Alpha3) | TASK-211 완료 직후 | — |
| `BattleTestInput` | Phase 2 종료 시 | Agent 이동 |
| `StructureDamageTester` | Phase 2 종료 시 | Agent 공격 |
| `Testing/` 디렉토리 전체 | Phase 2 종료 시 | — |

---

## 주의사항

- Observation 설계 시 시너지 flag 슬롯 미리 예약 (초기 0 패딩).
- RaySensor 설정(각도·거리·태그)은 실험으로 조정. 변경 시 OBSERVATIONS.md 업데이트.
- `UnitAttack` 수동화는 Agent 학습 선결 조건 ⭐. 이거 없으면 명령이 무의미.
- **유닛 보상에 승/패 신호 절대 포함 금지** (독립성 마지노선).
- Scripted 커맨더는 단순해도 되지만 **명령 필드를 모두 쓰도록** 설계 — 유닛이 모든 명령 종류를 관측/수행하게 만들어야 Stage 3에서 RL 커맨더가 활용 가능.
