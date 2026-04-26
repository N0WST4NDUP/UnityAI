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
- [ ] 기존 `Assets/Scripts/RaySensorAgent.cs`와 Phase 1 설계의 관계 정리 필요 (학습용 튜토리얼 자산인지 확인)

### 메모
- 작업 브랜치를 `AutoChessLike`로 확정 (CLAUDE.md 업데이트 완료). 앞으로 모든 Phase 작업은 이 브랜치에서 진행.
- 워킹 트리에 수정/미추적 파일 다수 존재 (SampleScene, TutorialInfo, Action Minis Demo 등)

---

## 2026-04-15 (계속) — TASK-101 Grid 기반 필드 구현

### 목표
- Phase 1 첫 Task인 TASK-101 완료
- 학습 중심 (개념→이유→코드) 진행으로 사용자가 Unity/C# 기초 개념을 체득

### 완료
- [x] `Assets/Scripts/Grid/GridCell.cs` — `CellState : byte` enum 정의 (Empty/Structure/Occupied/Danger), namespace `UnityAI.Grid`
- [x] `Assets/Scripts/Grid/GridField.cs` — MonoBehaviour
  - Inspector 필드: `_width`, `_height`, `_cellSize` (기본 16x16x1)
  - 읽기 전용 프로퍼티: `Width`, `Height`, `CellSize`
  - Cell State API: `GetCellState` (경계 밖 → `Structure` 반환, 방어적 기본값) / `SetCellState` (경계 밖 → 조용히 무시 + `#if UNITY_EDITOR` LogWarning)
  - 좌표 변환: `WorldToGrid` (XZ 평면, `Mathf.FloorToInt`) / `GridToWorld` (셀 중심 `+0.5` 보정)
  - Gizmos: `OnDrawGizmos` — 에디트 모드는 `DrawEmptyFrame` (와이어), Play 중은 상태별 색 큐브 (Structure=갈색, Occupied=파랑, Danger=빨강)
- [x] `Assets/Scripts/Grid/GridTester.cs` — `[SerializeField] GridField` 참조 기반 1회성 검증 스크립트
- [x] `Assets/Scenes/Grid_Sandbox.unity` — Phase 1 전용 테스트 씬
- [x] TASK-101 완료 조건 4종 검증: Get/Set 동작 / Gizmos 시각화 / 런타임 셀 변화 실시간 반영 / 좌표 변환 왕복

### 학습 포인트 (사용자)
- C# enum + `: byte` 타입 고정의 의미 (GridSensor 채널 효율 + 계약 명시)
- MonoBehaviour 생명주기: `Awake` 가 필드 초기화/Inspector 주입 **이후**에 호출되는 이유 (`new CellState[_width, _height]` 는 반드시 Awake)
- `[SerializeField] private` + `public readonly property (=> _field)` 조합의 의미 (진실의 원천 하나)
- `Mathf.FloorToInt` vs `(int)` 캐스팅의 음수 동작 차이 (경계 밖 흡수 규칙을 위해 Floor 필수)
- 셀 중심 반환 (`+0.5f`) 이 경계 판정 부동소수점 오차에 둔감한 이유
- 방어적 기본값 패턴 (`GetCellState` 경계 밖 → `Structure`): 호출자가 경계 체크를 재작성하지 않도록 API가 흡수
- Gizmos 의 에디트 모드 호출 + `_cells == null` 분기 필요성
- `OnDrawGizmos` vs `OnDrawGizmosSelected` 차이
- Inspector 드래그 참조 할당이 `FindObjectOfType` / `GameObject.Find` 보다 나은 이유 (명시성·성능·디버깅)

### 세션 중 잡은 버그
- `WorldToGrid` 초안에서 `world.y` 를 사용 → XZ 평면 규칙과 불일치. `world.z` 로 수정. Phase 2 학습에서 전원 y=0 으로 판정되는 대형 버그를 사전 차단.

### 미완료 / 다음 세션
- [ ] TASK-102 구조물 배치 로직 — `StructurePlacer`, `Structure` 스크립트 신규 작성
- [ ] `GridField.transform.position` 을 좌표 변환에 반영할지 결정 (현재는 world 원점 고정). Phase 3 전에 논의 필요.
- [ ] 기존 `TrainManager.cs` 와 Phase 1 씬 연동 여부 (Phase 2 시점에 재검토)

---

## 2026-04-16 — TASK-102 구조물 배치 로직

### 목표
- Phase 1 두 번째 Task인 TASK-102 완료
- Structure / StructurePlacer / StructureDamageTester 3개 스크립트 작성

### 완료
- [x] `Assets/Scripts/Structures/Structure.cs` — MonoBehaviour
  - `_maxDurability` / `_currDurability` (int, Inspector + Runtime)
  - `Init(GridField, Vector2Int)` — Instantiate 후 런타임 초기화 패턴
  - `TakeDamage(int)` — 캡슐화된 데미지 API, 음수 방어 포함
  - `private Die()` — 셀 상태 Empty 복귀 + Destroy
- [x] `Assets/Scripts/Structures/StructurePlacer.cs` — MonoBehaviour
  - Raycast → WorldToGrid → GetCellState 검증 → Instantiate 파이프라인
  - `_isPreparationPhase` bool로 준비 페이즈 게이팅
  - Guard clause 패턴 적용
- [x] `Assets/Scripts/Structures/StructureDamageTester.cs` — 우클릭 데미지 테스트용
- [x] GridField에 `Vector2Int` 오버로드 추가 (사용자 자발적 리팩터링)
- [x] Grid_Sandbox 씬에 Ground Plane + StructurePlacer + Structure Prefab 설정
- [x] TASK-102 완료 조건 4종 검증: 준비페이즈 한정 배치 / 중복 배치 방지 / 파괴 시 셀 복귀 / 파괴 후 재생성

### 학습 포인트 (사용자)
- MonoBehaviour Init() 패턴: 생성자 대신 public Init()로 런타임 값 주입
- 캡슐화와 불변 규칙: TakeDamage()가 유일한 HP 변경 통로 → Die() 규칙 보장
- `private Die()` vs `public Die()`: 내부 로직은 private으로 → 외부에서 규칙 우회 방지
- Raycast에 물리 Collider 필수라는 점
- Camera.main 필드 초기화 불가 → Awake()에서 캐싱
- Instantiate 반환값 vs 프리팹 원본 구분
- YAGNI 원칙: IDamageable은 실제 중복 발생 시(TASK-104) 추출
- int vs float HP: 이산적 내구도에는 int가 부동소수점 오차 없이 안전
- TryGetComponent와 null safety

### Weak Points (다음 세션 복습 권장)

1. **Camera.main 필드 초기화**
   - `private Camera _camera = Camera.main;`으로 작성 → Awake로 수정
   - 복습 질문: "왜 필드 초기화 시점에 Camera.main이 null인가?"

2. **Instantiate 반환값**
   - 프리팹 원본에 Init()을 호출하는 실수 발생 → 반환값에 호출로 수정
   - 복습 질문: "Instantiate(prefab)과 prefab은 같은 오브젝트인가?"

3. **TryGetComponent null safety**
   - TryGetComponent 결과를 체크하지 않고 바로 사용 → NullReferenceException 가능
   - 복습 질문: "바닥을 우클릭하면 왜 에러가 나는가?"

### 미완료 / 다음 세션
- [ ] ~~TASK-103 유닛 Grid 이동 구현~~ → 재설계로 폐기, TASK-107 (NavMesh 이동)으로 대체
- [ ] StructureDamageTester의 TryGetComponent guard clause 수정

---

## 2026-04-16 (계속) — 프로젝트 재설계

### 목표
- 게임 비전 재정의: Grid 셀 이동 → NavMesh 자유 이동 (미니어처 워게임 컨셉)
- CLAUDE.md 마스터 문서 + 전체 파생 문서 재작성

### 배경
- 사용자의 원래 비전: 준비 페이즈에서 Grid 보드에 미니어처 배치 → 전투 시작 시 미니어처가 살아서 자유롭게 싸우는 소규모 전쟁 시뮬레이션
- 기존 설계(Grid 셀 단위 이동)는 이 비전과 맞지 않음
- NavMesh 기반 자유 이동으로 전면 재설계

### 핵심 설계 변경

| 항목 | 기존 | 변경 |
|------|------|------|
| 전투 이동 | Grid A*/BFS (셀 단위) | NavMesh + NavMeshAgent (자유 이동) |
| 구조물 | Grid 셀 상태만 | NavMeshObstacle(Carve) + Collider + 내구도 |
| 전투 중 Grid | 셀 점유 계속 갱신 | 사용 안 함 (준비 페이즈 전용) |
| CellState | Empty/Structure/Occupied/Danger | Empty/Structure/Occupied (Danger 제거) |
| 바리케이드 | Grid 셀에 설치 | 자유 좌표 + 방향 회전 + NavMeshObstacle |
| 포탄 | Grid 위험 셀 + 틱 | 월드 좌표 AoE (MOBA/RTS 스타일) |
| NavMesh Bake | 없음 | 전투 시작 시 1회 (NavMeshSurface.BuildNavMesh) |
| 지형 편집 | 없음 | 준비 페이즈에서 수성 측 높낮이 조절 |
| AI 관측 | GridSensor (셀=유닛) | VectorSensor + RaySensor (연속 좌표 기반) |
| 벽 부수기/우회 | 해당 없음 (Grid 이동) | Agent가 학습으로 판단 |

### 완료
- [x] CLAUDE.md 마스터 문서 전면 재작성
- [x] ARCHITECTURE.md 재작성 (디렉토리 구조, 클래스 책임, 듀얼 페이즈 전환 흐름)
- [x] PHASE_1.md 재작성 (TASK-105~110, NavMesh 기반)
- [x] PHASE_2.md 재작성 (VectorSensor+RaySensor, 구조물 부수기 학습)
- [x] PHASE_3.md 재작성 (Commander 관측 비교 실험 명시)
- [x] PHASE_4.md 재작성 (바리케이드/포탄 자유 좌표, 지형 편집, 구조물 확장)
- [x] PHASE_5.md 재작성
- [x] TASKS.md 재작성 (TASK 번호 체계 갱신)
- [x] OBSERVATIONS.md 재작성 (연속 좌표 기반, 비교 실험 계획)
- [x] REWARDS.md 업데이트 (구조물 관련 보상 추가)

### 기존 코드 영향
- `GridField.cs` / `GridCell.cs` — 유지 (준비 페이즈 전용으로 역할 축소, CellState.Danger 제거 필요)
- `Structure.cs` — 유지 + NavMeshObstacle 연동 추가 필요
- `StructurePlacer.cs` — 유지 (준비 페이즈 배치 로직은 그대로)
- `StructureDamageTester.cs` — 유지 (테스트용)

### 미완료 / 다음 작업
- [ ] TASK-105: 기존 코드에 재설계 적용 (CellState.Danger 제거, Structure NavMeshObstacle 연동)
- [ ] TASK-106~110: Phase 1 나머지 Task 순차 진행

### Weak Points (다음 세션 복습 권장)

이번 세션에서 사용자가 처음 마주했거나 놓쳤던 포인트. 다음 세션 시작 시 가볍게 복습하면 좋다.

1. **MonoBehaviour 생명주기 타이밍**
   - 필드 초기화 구문은 **Inspector 주입 이전**에 실행됨 → `new CellState[_width, _height]` 를 필드 자리에서 초기화하면 Inspector 값이 반영 안 됨
   - `Awake()` 는 Inspector 주입 **이후**에 호출되므로 배열 할당은 여기서
   - 복습 질문: "왜 `private CellState[,] _cells = new CellState[_width, _height];` 는 컴파일·런타임 모두 문제가 되는가?"

2. **auto-property vs `[SerializeField] + expression-bodied property`**
   - `public int Width { get; private set; } = 16;` 은 **별도 백킹 필드**가 생겨 값이 두 곳에 저장됨
   - `[SerializeField] private int _width; public int Width => _width;` 는 값이 하나. 진실의 원천 하나.
   - 복습 질문: "같은 값이 두 곳에 저장되면 왜 위험한가?" (Step 0 NavMesh/Grid 분열 이야기와 같은 원리)

3. **좌표축 규율 — XZ 평면에서 `world.y` 는 절대 쓰지 않는다**
   - 세션 중 `WorldToGrid` 초안에서 `world.y` 를 사용해 버그 발생
   - Unity: Y=높이, Z=세로. Grid 의 y 인덱스는 Unity 의 Z축과 대응
   - 복습 질문: "`GridToWorld(new Vector2Int(3, 5))` 의 반환값에서 Y 성분은 왜 0 이어야 하는가?"

4. **방어적 기본값 패턴**
   - `GetCellState` 경계 밖 → `Structure` 반환 / `SetCellState` 경계 밖 → 조용히 무시
   - "호출자가 경계 체크를 재작성하지 않도록 API 가 흡수" 라는 사고방식이 처음이라 낯설 수 있음
   - 복습 질문: "경계 밖에서 `Empty` 를 반환하면 무슨 문제가 생기는가? 예외 던지기 대신 `Structure` 를 선택한 이유는?"

5. **Gizmos 가이드 읽기 누락**
   - Step 4 실습에서 `OnDrawGizmos` 본문이 비어 있는 채로 저장됨 (제공 가이드의 구조를 건너뜀)
   - 구조 가이드를 받을 때 **빈칸만 채우는 게 아니라 전체 구조를 따라 쳐야** 함. 다음 세션부터 가이드를 한 번 훑은 뒤 타이핑 시작 권장.

6. **개념 지연 설명이 혼란을 준 경우**
   - "스레드 안전성" 을 미리 언급해 혼란만 줌. 이번 Phase 에서 불필요한 개념이므로 머릿속에서 일단 삭제해도 OK.
   - 교훈: 어시스턴트가 맥락 없이 흘린 개념은 잡지 않아도 된다. 필요해질 때 다시 꺼내진다.

---

## 2026-04-17 — TASK-105 재설계 적용 + 구조물 라이프사이클 리팩터

### 목표
- `CellState.Danger` 제거 (재설계 반영: 전투 중 위험 영역은 월드 좌표 AoE 로)
- `Structure` ↔ `NavMeshObstacle` 연동 (파괴 시 NavMesh Carve 자동 복구)
- **(세션 중 발생한 설계 전환)** 라운드 복구를 고려한 구조물 라이프사이클 재설계

### 완료
- [x] `CellState` enum: `Danger` 제거 → Empty / Structure / Occupied 3종 확정
- [x] `GridField.DrawCell`: `case CellState.Danger` 블록 제거
- [x] `Structure.cs` 리팩터
  - `[RequireComponent(typeof(NavMeshObstacle))]` 어트리뷰트 추가
  - `using UnityEngine.AI;` 추가
  - `_obstacle` 필드 + `Awake()` 에서 `GetComponent<NavMeshObstacle>()` 캐싱
  - `Die()`: `Destroy(gameObject)` → `gameObject.SetActive(false)` 로 변경
  - `Die()`: Grid 셀 상태 변경 코드 제거 (RoundManager 책임으로 이동)
  - `_gridField` 필드 제거 + `Init(GridField, Vector2Int)` → `Init(Vector2Int)` 로 파라미터 축소
- [x] `StructurePlacer.cs`: `Init(_grid, gridPos)` → `Init(gridPos)` 호출부 동기화
- [x] Structure 프리팹: NavMeshObstacle 컴포넌트 설정 (Shape=Box, Size 맞춤, **Carve=ON**, Carve Only Stationary 기본값)
- [x] TASK-105 완료 조건 검증: CellState 3종 / 파괴 시 GameObject 비활성 (Obstacle 효과 자동 해제 경로) / Grid 셀 상태 유지(복구 대비)

### 설계 결정 (향후 영향 있는 판단)

1. **파괴 처리: `Destroy` → `SetActive(false)` 전환**
   - 이유: 라운드 종료 시 성벽 복구가 게임의 전략적 깊이 핵심 ("실패 → 약간의 수정" 루프). 오토체스+워게임 컨셉에서 구조물 연속성이 **선택 아닌 필수**라고 판단.
   - 부가 효과: 풀링에 유리 (GC 압박 ↓)
   - 다만 풀링 매니저 / Reset 메서드 등 인프라 구축은 **RoundManager 도입 시점**까지 연기 (YAGNI: 복구 타이밍/정책이 아직 미정)

2. **책임 소재 이동: Grid 상태 관리 → Structure 에서 분리**
   - `Structure.Die()` 는 "**보이지 않게 된다**" 까지만 담당. Grid 조작 책임 이관.
   - 원칙: Grid 상태 = **준비 페이즈의 의도**. 전투 중 Die() 가 Empty 로 바꿔도 아무도 안 읽음 + 라운드 복구 시 RoundManager 가 다시 Structure 로 되돌리는 왕복 발생 → 제거.
   - 플레이어가 복구를 포기한 경우의 Empty 전환은 **라운드 경계에서 RoundManager 가** 플레이어 선택에 기반해 수행.

3. **NavMeshObstacle 참조 방식: `GetComponent` + `[RequireComponent]`**
   - 관계 유형: **계약(Contract)** — Structure 는 NavMeshObstacle 없이 기능 불가
   - 비교 대상: `GridField` 는 **설정(Configuration)** — 씬 안 여러 후보 중 디자이너 선택 → `[SerializeField]` + Inspector 드래그
   - `[RequireComponent]` 는 컴포넌트 존재를 에디터 수준에서 강제 (실수로 제거 불가 + 자동 추가)

### 학습 포인트 (사용자)

- **NavMesh 파이프라인 전체 그림**
  - NavMesh = "걸을 수 있는 바닥의 미리 구운 지도" (Bake 1회)
  - NavMeshAgent = 그 위에서 목적지까지 자동 경로 탐색 + 재계산
  - NavMeshObstacle = NavMesh 위 동적 장애물
- **NavMeshObstacle 의 Carve 옵션 차이**
  - Carve = OFF: 회피 동작만 영향. NavMesh 는 그대로 (싸지만 벽에는 부적합)
  - Carve = ON: NavMesh 에 실제로 구멍 뚫음 + 비활성 시 자동 복구 (비싸지만 파괴형 구조물 필수)
- **`GameObject.SetActive(bool)` vs `Component.enabled`**
  - SetActive = GameObject + 자식 + 모든 컴포넌트 on/off (enabled 필드는 건드리지 않음)
  - .enabled = 그 컴포넌트 하나만 on/off (플래그 자체 변경)
  - 함정: 둘 다 쓰면 복구 시 `.enabled=false` 잔존 → Obstacle 이 살아나지 않음
- **`[RequireComponent]` 어트리뷰트** — 동일 GameObject 필수 컴포넌트 명시. 자동 추가 + 제거 방지
- **참조 방식 선택 원칙: 계약 vs 설정**
  - 계약 (같은 GO + 필수) → `GetComponent` + `[RequireComponent]`
  - 설정 (다른 GO 또는 선택) → `[SerializeField]` + Inspector 드래그
- **책임의 레이어 분리 (SoC)** — 각 클래스는 자기 레이어만 담당. 상위 매니저가 레이어 간 조율
- **YAGNI 판단 경계** — "답을 찍는 것" vs "디자인이 확정된 것". 설계 질문에 구체적 답이 가능하면 YAGNI 대상 아님 → 설계 반영

### Weak Points (다음 세션 복습 권장)

1. **`SetActive` vs `.enabled` 타이밍 함정**
   - `_obstacle.enabled = false` + `gameObject.SetActive(false)` 를 둘 다 쓰면 `.enabled` 가 `false` 로 고정됨 → `SetActive(true)` 복구 시 Obstacle 이 비활성 상태 → Carve 기능 부활 안 함
   - 복습 질문: "SetActive 와 .enabled 는 각각 어느 레이어를 건드리는가? 풀링 복구 경로에서 `.enabled` 명시 조작을 피해야 하는 이유는?"

2. **책임의 레이어 침범 (Structure vs RoundManager)**
   - 파괴 로직이 자연스레 "Grid 정리까지 내가 한다" 로 흐르기 쉬움 → 이게 레이어 침범
   - 복습 질문: "`Die()` 가 Grid 를 안 건드리는 게 왜 더 깨끗한가? RoundManager 가 아직 없는 지금도 그렇게 두는 이유는?"

3. **계약 vs 설정 — 참조 방식 기준**
   - 복습 질문: "`NavMeshObstacle` 은 `GetComponent`, `GridField` 는 `[SerializeField]` 인 이유는? 판단 기준 한 줄로 정리해보기"

4. **NavMeshObstacle Carve 가 '비싼' 이유**
   - NavMesh 위에 구멍을 뚫고 메꿀 때마다 주변 메시 재계산 비용 발생 → 구조물 수가 많거나 자주 켜지고 꺼지면 프레임 드랍 가능
   - 복습 질문: "Carve=ON 의 비용은 어느 시점에 발생하는가? '많이 켜고 끄기' 가 왜 문제가 될 수 있나?"

### 미완료 / 다음 세션
- [ ] TASK-106 유닛 기본 구현 — 다음 작업
- [ ] RoundManager 도입 시점(Phase 미정)의 복구 로직 설계 — TASKS.md `장기 TODO` 에 기록 완료

### 메모
- Structure 프리팹 루트에 NavMeshObstacle 자동 추가는 `[RequireComponent]` 덕분에 Unity 가 수행 (수동 Add Component 불필요)
- `Carve Only Stationary` 는 기본값 유지 — 구조물은 항상 정지 상태라 적합
- 전투 페이즈 NavMesh 실동작 검증(벽 회피/파괴 후 돌파) 은 **TASK-107/108** 단계에서 (`NavMeshSurface` + `NavMeshAgent` 도입 후). 이번 세션에서는 "파괴 시 GameObject 비활성" 까지만 관찰 가능
- `StructureDamageTester.TryGetComponent` guard clause 수정은 여전히 미처리 — 다음 Tester 손대는 세션에 같이 정리 권장

---

## 2026-04-17 (계속) — TASK-106 유닛 기본 구현 + 네임스페이스 제거

### 목표
- TASK-106: `UnitBase` + `UnitPlacer` 구현 (HP / 팀 ID / 직군, 준비 페이즈 Grid 배치)
- (세션 후반 추가) 모든 스크립트에서 namespace 제거

### 완료
- [x] `Assets/Scripts/Units/UnitEnum.cs` — `Tribe(Human/Orc)`, `UnitType(Soldier/Archer/Tank)` 열거형
- [x] `Assets/Scripts/Interfaces/IDamageable.cs` — `OnDamaged(float damage)` 단일 메서드 (TASK-102 에서 YAGNI 로 미뤄뒀던 인터페이스 추출 시점)
- [x] `Assets/Scripts/Units/Unit.cs` — MonoBehaviour, IDamageable
  - `[RequireComponent(typeof(NavMeshAgent))]` — 전투 이동 계약 (TASK-107 대비)
  - Inspector: `_tribe`, `_type`, `_maxHealth = 100f`
  - Runtime: `_groupId` (Init 주입), `_currentHealth`, `_startPosition`
  - Properties: `GroupId / Tribe / Type / MaxHealth / CurrentHealth / IsDead / StartPosition`
  - `OnEnable()` — 활성화 시 `_currentHealth = _maxHealth` 리셋 (라운드 복구 재활용 대비)
  - `Init(Vector2Int, int)` — grid 위치 + 그룹 ID 저장
  - `OnDamaged(float)` + `private Die()` → `gameObject.SetActive(false)` (Structure 와 동일 패턴)
  - 세션 중 `abstract` 제거 + `UnitBase` → `Unit` 리네이밍 (컴포지션 채택 결정 이후)
- [x] `Assets/Scripts/Units/UnitPlacer.cs` — `StructurePlacer` 패턴 복사 + 필요한 지점만 수정
  - `CellState.Occupied` 로 셀 상태 세팅 (Structure 는 `Structure`)
  - `Init(gridPos, groupId)` 호출 (groupId 는 임시 하드코딩 `0`)
- [x] TASK-106 완료 조건 검증: Play 모드에서 Unit 배치 → Grid Gizmo 파란색(Occupied) 반영 확인
- [x] (세션 후반) 모든 스크립트(`GridCell.cs`, `GridField.cs`, `Structure.cs`, `StructurePlacer.cs`, `StructureDamageTester.cs`) 에서 `namespace UnityAI.*` 제거
- [x] (세션 후반) `Structure.cs` / `StructurePlacer.cs` 의 `using UnityAI.Grid;` 제거
- [x] (세션 후반) `Structure.cs` 의 미사용 `_obstacle` 필드 + `Awake()` 제거 (IDE 진단 `Private member '_obstacle' can be removed` 반영)

### 설계 결정 (향후 영향)

1. **컴포지션 > 상속 (공격/스킬 로직 처리 방향)**
   - `UnitBase` 의 `abstract` 제거, concrete 클래스로 확정
   - Soldier/Archer/Tank 서브클래스 생성 안 함
   - TASK-109 에서 `AttackComponent`, `SkillComponent` 등 **별도 컴포넌트를 각 프리팹에 부착**하는 방식
   - 이유: Unity 컴포넌트 철학 + 조합 유연성 + 깊은 상속 계층 회피

2. **정체성 분류 축: SerializeField vs Init 인자 분리**
   - `Tribe` / `UnitType` → **프리팹별 고정 정체성** → SerializeField
   - `GroupId` (팀 ID) → **매치별 동적 소속** → Init 인자
   - 기준: "여러 인스턴스가 공유할 값인가" (프리팹) vs "인스턴스마다 다를 수 있나" (런타임 주입)

3. **IDamageable 인터페이스 추출 (YAGNI 해제 시점)**
   - TASK-102 결정("중복 발생 시 추출")의 번복
   - 현재 `Unit` 만 구현. `Structure` 의 `TakeDamage(int)` → `OnDamaged(float)` 일관화는 TASK-110 에서

4. **namespace 제거 (현 규모 대비 과한 계층)**
   - 나중에 **대분류/소분류**로 의미있는 경계가 생기는 시점에 재도입
   - Unity 는 namespace 강제하지 않음 → 작은 프로젝트에선 글로벌 네임스페이스가 오히려 검색·네비 용이

### 학습 포인트 (사용자)

- **MonoBehaviour 이벤트 실행 순서 vs 외부 Init 호출 순서**
  - Instantiate → Awake → OnEnable → Start → ... **이후** 외부 코드가 `Init(...)` 호출
  - OnEnable 에서 Init 로 주입될 값(예: `_startPosition`) 에 의존하면 **첫 실행 시 기본값(0)** 으로 동작 → 은밀한 버그
  - 이번 세션엔 본인이 직접 연표를 추적해서 발견 (OnEnable 의 `transform.position = new(_startPosition.x, 0f, _startPosition.y)` 함정)

- **Placer 가 월드 좌표를 책임진다 (SoC 원칙 일관)**
  - `Instantiate(prefab, worldPos, rotation)` 에서 spawner 가 위치 지정
  - Unit/Structure 본체는 좌표 변환을 몰라도 됨 → GridField 참조를 본체에 안 둬도 됨
  - TASK-105 에서 `Structure._gridField` 제거한 것과 같은 논리

- **SerializeField vs Init 인자 — 선택 기준의 언어화**
  - "프리팹마다 같은 값" → SerializeField
  - "매 인스턴스/매치마다 다른 값" → Init 주입
  - 이 기준을 두 번째 클래스(Unit) 에 적용하며 체득

- **abstract 유지 판단 = "서브클래스의 행동 차이를 어떻게 표현하느냐"**
  - 상속으로 표현 → abstract 유지
  - 컴포지션으로 표현 → abstract 불필요
  - "지금 abstract 가 뭘 해주고 있나?" 라는 회고 질문 유효

- **읽기 전용 프로퍼티의 필요성**
  - `[SerializeField] private` 만으론 Inspector 에서만 보이고 **런타임 외부 코드가 못 읽음**
  - 시너지 매니저 / UI / 디버그 등 외부 시스템이 읽어야 하면 `public X => _x;` 프로퍼티 필수
  - "설정"(Inspector) 과 "조회"(런타임) 는 별개 경로라는 감각

- **GameObject.SetActive vs Component.enabled 복습 (TASK-105 연장)**
  - `_obstacle` 필드를 아예 제거한 배경. `[RequireComponent]` + `SetActive` 만으로 충분 — C# 레퍼런스 유지할 이유가 없음

### Weak Points (다음 세션 복습 권장)

1. **SerializeField 초기값 미설정 = 0 함정**
   - `_maxHealth` 에 `= 100f` 기본값이 없으면 Inspector 세팅을 깜빡한 프리팹은 **첫 프레임에 즉시 사망** (HP 0)
   - 복습 질문: "C# value type 의 기본값이 0 인 것과, SerializeField 미설정 함정의 관계를 설명해보기. `_maxHealth` 와 `_maxDurability` 둘 다 기본값을 주는 것이 왜 방어적인가?"

2. **외부 Init 은 OnEnable **뒤** 에 실행된다**
   - 이벤트 실행 순서(Instantiate → Awake → OnEnable → Start → 외부 Init) 를 **제대로 떠올려** 야 OnEnable 이 Init 값에 의존하는 설계가 위험함을 인식
   - 복습 질문: "`var u = Instantiate(prefab); u.Init(pos, 0);` 이 실행되면 전체 이벤트 실행 순서는? 각 시점의 `_startPosition` 값은?"

3. **GroupId 하드코딩 `0` — 드러난 기술 부채**
   - `TryPlaceUnit(0)` 의 `0` 은 테스트용. 실제론 **"지금 배치 중인 플레이어"** 가 공급해야 함 (Phase 6 플레이어 시스템)
   - 복습 질문: "현 구조에서 'Player 1 이 자기 팀 유닛만 배치' 를 어떻게 강제할 수 있는가? UnitPlacer 에 PlayerId 를 SerializeField 로 둘지, 아니면 상위 PlacementController 가 내려줄지 — 각각의 장단점?"

### 미완료 / 다음 세션
- [ ] TASK-107 NavMesh 기반 유닛 이동 — 다음 작업
- [ ] Unit.cs 내 `//` 빈 주석 정리 (사소)
- [ ] `Structure` 에도 `IDamageable` 구현 적용 (TASK-110 에 포함 예상)
- [ ] `UnitPlacer` + `StructurePlacer` 좌클릭 충돌 — TASK-108 PhaseManager 가 해결
- [ ] `StructureDamageTester.TryGetComponent` guard clause (지속 미처리)

### 메모
- 검증 범위: Unit 단일 배치 + Grid 상태 파란색 확인까지. 이동/전투는 TASK-107, TASK-109.
- `UnitBase → Unit` 리네이밍은 세션 중 수용 (파일·클래스·UnitPlacer 참조 모두 갱신됨). `abstract` 제거와 맞물려 자연스러운 선택.
- `Assets/Scripts/Grid/` → `Assets/Scripts/Fields/` 폴더 리네이밍 (사용자 직접 수행). "필드 시스템" 이라는 CLAUDE.md 용어와 정렬.
- 테스트 시 StructurePlacer 와 UnitPlacer 중 하나만 활성화하거나 `_isPreparationPhase` 토글로 번갈아 테스트하는 워크어라운드 사용
- IDE 진단이 `_obstacle` 미사용을 잡아준 덕에 Structure 도 추가 정리됨 — 정적 분석 신호를 놓치지 말 것

---

## 2026-04-18 — TASK-108 + TASK-107 (페이즈 전환 + NavMesh 이동)

### 목표
- TASK-108: 준비↔전투 페이즈 전환 시스템 (PhaseManager + NavMesh Bake)
- TASK-107: NavMeshAgent 기반 유닛 자유 이동 + 구조물 회피

### 완료 — TASK-108
- [x] `Assets/Scripts/Phase/NavMeshBaker.cs` — `NavMeshSurface.BuildNavMesh()` 래퍼
- [x] `Assets/Scripts/Phase/PhaseManager.cs`
  - `Phase` enum (Preparation / Battle)
  - 이벤트 4종: `OnPreparationStart` / `OnPreparationEnd` / `OnBattleStart` / `OnBattleEnd` (세션 후반 세분화)
  - `StartBattle()` / `StartPreparation()` 메서드 (스페이스바 트리거 임시 입력)
  - 전환 시퀀스: Bake → state 변경 → 이벤트 발행 (순서 중요)
- [x] `Assets/Scripts/Placement/PlacementController.cs` — 1/2 키로 Unit/Structure 배치 모드 전환 (좌클릭 충돌 해결)
- [x] `UnitPlacer` + `StructurePlacer`에 phase guard + mode guard 연동 (기존 `_isPreparationPhase` bool 제거)
- [x] 씬 세팅: Manager GameObject(PhaseManager + NavMeshBaker), NavMeshSurface on Ground (Collect=All), 프리팹 NavMeshAgent 체크박스 OFF 저장
- [x] TASK-108 완료 조건 검증: 배치→스페이스→Bake 로그, 구조물 영역이 NavMesh에서 제외됨, 유닛이 NavMesh 위로 스냅

### 완료 — TASK-107
- [x] `Assets/Scripts/Units/UnitMovement.cs`
  - `Awake`: `GetComponent<NavMeshAgent>()` 캐싱 + 태그 기반 `PhaseManager` 찾기 + 초기 `DisableMovement()`
  - 구독: `OnBattleStart` → `EnableMovement` / `OnBattleEnd` → `DisableMovement`
  - 해제: `OnDestroy`에서 짝 해제
  - `OnDisable`: `DisableMovement()` (기능은 중복이지만 의도 드러내기 코드)
  - `MoveTo(Vector3)`: `_agent.enabled` 가드 → `SetDestination`
  - `DisableMovement`: `isOnNavMesh` 가드 → `ResetPath()` → `enabled = false` (순서 중요)
- [x] `Assets/Scripts/Testing/BattleTestInput.cs` — 우클릭 → `FindObjectsByType<UnitMovement>` → `MoveTo(hit.point)`
- [x] `Assets/Scripts/Testing/` 폴더 신설 + `StructureDamageTester.cs` 이동 (Unity Editor drag-and-drop로 meta 보존)
- [x] Unit 프리팹: NavMeshAgent 컴포넌트 체크박스 OFF로 저장 (Instantiate 시점 네이티브 초기화 경고 차단)
- [x] TASK-107 완료 조건 검증: 기본 이동, 구조물 회피 (우회 경로), NavMesh 상 정상 주행

### 설계 결정 (향후 영향)

1. **분산 구독 (컴포넌트별) vs 중앙화 구독 — 분산 선택**
   - 각 컴포넌트(UnitMovement, 향후 UnitAgent 등)가 독립적으로 Phase 이벤트 구독
   - 이유: SRP + `UnitMovement` 재사용성 (Unit 없는 "이동하는 구조물" 같은 것도 재활용 가능)
   - 구독 비용은 무시 가능 (C# delegate 매우 가벼움)
   - Phase 2에서 `UnitAgent`도 동일 패턴: 자기 `enabled`를 직접 토글

2. **사망 처리 — 별도 OnDeath 이벤트 없이 `SetActive(false)` + `OnDisable` 활용**
   - `SetActive(false)` → 모든 컴포넌트의 `OnDisable` 자동 호출
   - 각 컴포넌트가 자기 정리 책임만 감당 (예: `UnitMovement.OnDisable` → `DisableMovement()`)
   - Unity 생명주기를 "암묵적 브로드캐스트"로 재활용 → 이벤트 중복 설계 회피

3. **이벤트 구독 모델 — Start/OnDestroy 기반 (OnEnable/OnDisable 아님)**
   - 원칙: "유닛이 필드에 체류하는 동안 = Phase 이벤트 구독 유지"
   - SetActive 토글(생존↔사망)은 필드 체류의 하위 상태 → 구독과 무관
   - 필드 이탈(판매/합성/가방 복귀) = Destroy 트리거 → `OnDestroy`에서 해제
   - 풀링 기반 구조에서 흔한 `OnEnable/OnDisable` 구독 패턴은 이 프로젝트엔 부적합

4. **`Unit._startPosition`: `Vector2Int(gridPos) → Vector3(worldPos)`**
   - 전투 중 "시작 위치로 복귀"에만 쓰이므로 월드 좌표 직접 저장이 단순
   - `UnitPlacer`가 `_grid.GridToWorld(gridPos)` 계산 후 `Init(worldPos, groupId)`로 전달
   - Placer가 좌표 변환을 전담(SoC): `Unit`/`UnitMovement`는 `GridField`를 몰라도 됨

5. **PhaseManager 이벤트 세분화 — 4개**
   - `OnPreparationStart` / `OnPreparationEnd` / `OnBattleStart` / `OnBattleEnd`
   - 구독자가 어느 전환 타이밍에 반응할지 선택 가능 (확장성)
   - 세션 중반에 2개 → 4개로 세분화

6. **Dummy-Real 스왑 vs 컴포넌트 토글 — 토글 선택**
   - 준비 페이즈에서 ML-Agent 추론/NavMeshAgent 경고를 피하기 위한 설계 후보 중
   - Dummy 스왑은 상태 이관 부담 + 프리팹 2종 → 오버엔지니어링
   - `NavMeshAgent.enabled`, `Agent.enabled` 등 컴포넌트 단위 on/off로 충분
   - GameObject 1개로 정체성/상태 일관성 유지

7. **프리팹 레벨에서 NavMeshAgent 초기 disabled 저장 (불가피)**
   - Unity 네이티브 초기화가 C# `Awake`보다 먼저 실행됨
   - `Awake`에서 `_agent.enabled = false`는 **이미 늦음** → 경고 발생
   - 해결: 프리팹 저장 시 체크박스 OFF. `Awake`의 disable은 방어 코드로 유지

8. **NavMeshAgent 참조: `GetComponent` + `[RequireComponent]` (계약 관계)**
   - `Unit`에 이미 `[RequireComponent(NavMeshAgent)]` → 같은 GameObject에 존재 보장
   - `UnitMovement`는 `GetComponent`로 Awake 시 캐싱
   - TASK-105의 "계약 vs 설정" 원칙 연장

### 학습 포인트 (사용자)

- **NavMesh 파이프라인 전체 실전 흐름**
  - 준비 페이즈엔 NavMesh 없음 → 전투 시작 시 `NavMeshSurface.BuildNavMesh()` 1회 → NavMeshAgent enable
  - 이미 배치된 NavMeshObstacle(Carve=ON)은 Bake 시 자동으로 해당 영역 제외
  - NavMesh Bake는 비용이 큼 → 매 프레임 구우면 성능 이슈 → 전환 시점에만

- **C# event 패턴의 실전 활용**
  - `public event Action OnBattleStart;` → `OnBattleStart?.Invoke();`
  - `+=` 구독 / `-=` 해제, `null` 조건 연산자 `?.` 로 구독자 0명일 때 NRE 회피
  - "구독한 모든 곳에서 반드시 해제" 원칙 (MissingReferenceException 방지)

- **Unity 생명주기 — 네이티브 초기화 < Awake**
  - NavMeshAgent 같은 네이티브 컴포넌트는 Instantiate 시점에 C++ 레벨 초기화
  - C# `Awake`는 그 뒤 → `Awake`에서 `enabled = false` 걸어도 이미 경고 발생
  - 방어책은 **프리팹 레벨**에서: 체크박스 OFF로 저장

- **"의도 드러내기 코드(intent-revealing code)"의 가치**
  - `OnDisable`의 `DisableMovement()` 호출은 Unity 자동 동작과 중복
  - 그럼에도 유지: 읽는 사람에게 의도 명시 + 확장 지점 제공
  - "보이는 중복" vs "숨은 암묵적 의존" — 전자가 유지보수에 유리할 수 있음

- **SRP 3-layer: Unit / UnitMovement / (미래) UnitAgent**
  - `Unit` = 정체성 (HP, groupId, 스탯)
  - `UnitMovement` = 이동 실행 메커니즘 (NavMeshAgent 래핑)
  - `UnitAgent` = (Phase 2) ML-Agents 의사결정. `Agent`는 `Unit`을 "대체"가 아닌 **추가**되는 컴포넌트
  - `UnitMovement`는 "누가 시키는지 모름" → 테스트 입력, ML-Agent, 스크립트 AI 모두 재사용

- **NavMeshAgent API 선택지**
  - `SetDestination(Vector3)`: 경로 탐색 + 이동 (정상 이동 명령)
  - `Warp(Vector3)`: 순간이동 (NavMesh 위로 스냅 + 에이전트 상태 리셋)
  - `transform.position` 직접 대입: agent 활성 상태면 스냅 부작용 가능 → 피함
  - `ResetPath()`: 현재 경로 취소 (단, agent가 NavMesh 위일 때만 유효)

- **`isOnNavMesh` 가드의 필요성**
  - `ResetPath()`, `Warp()` 등 일부 API는 agent가 NavMesh 위에 있을 때만 유효
  - 첫 Awake 시점(Bake 전) 또는 disabled 상태에선 NavMesh 위에 없음 → 이 가드로 방어

- **메서드 가드 위치 판단 기준**
  - "상태를 **바꾸는** 메서드" (예: `EnableMovement`): 무조건 실행 (가드 없음)
  - "상태에 **의존하는** 메서드" (예: `MoveTo`): 현재 상태 체크 후 조용히 무시

### Weak Points (다음 세션 복습 권장)

1. **EnableMovement / DisableMovement — 가드 위치 혼동**
   - 세션 중 `EnableMovement`에 `if (!_agent.enabled) return;`를 넣어 **활성화 자체가 막히는** 버그
   - 논리 역전: "이미 꺼져있으면 켜는 걸 막는" 형태가 됨
   - 복습 질문: "상태를 바꾸는 메서드와 상태에 의존하는 메서드는 가드가 왜 다르게 들어가야 하는가? `MoveTo`와 `EnableMovement`의 가드 정책이 반대인 이유는?"

2. **NavMeshAgent 초기화 타이밍 — 프리팹 레벨 저장의 필요성**
   - `Failed to create agent because there is no valid NavMesh` 경고 발생 → C# `Awake`로는 못 막음
   - 프리팹의 체크박스 상태가 곧 Instantiate 직후의 초기 상태
   - 복습 질문: "`Instantiate` → `Awake` → `OnEnable` 실행 순서에서 NavMeshAgent의 네이티브 초기화는 어느 시점인가? `Awake`의 `_agent.enabled = false`가 늦는 이유는?"

3. **구독/해제 짝 맞추기 원칙 — 어느 생명주기 쌍을 쓸 것인가**
   - Start/OnDestroy: 객체 생존 전체 기간 구독 (필드 체류 모델)
   - OnEnable/OnDisable: SetActive 사이클마다 재구독 (풀링 모델)
   - 잘못 섞으면 이벤트 누수 또는 MissingReferenceException
   - 복습 질문: "이 프로젝트에서 Start/OnDestroy 모델을 택한 이유는? 어떤 다른 프로젝트 구조에서는 OnEnable/OnDisable이 더 적합한가?"

4. **이벤트 순서 의존을 피하는 설계 (idempotent handler)**
   - 같은 이벤트(`OnBattleEnd`)에 여러 컴포넌트 구독 시 호출 순서는 delegate 내부 순서에 의존 (불안정)
   - 각 핸들러는 서로의 상태에 의존하지 않도록 자체 가드 (예: `WarpTo`가 `enabled` + `isOnNavMesh` 체크)
   - 복습 질문: "`Unit.ReturnToStart`와 `UnitMovement.DisableMovement`가 둘 다 `OnBattleEnd`를 구독할 때, 어느 쪽이 먼저 불리는가? 이 순서에 의존하지 않으려면 `UnitMovement.WarpTo`를 어떻게 설계해야 하는가?"

### 미완료 / 다음 세션
- [ ] `Unit.ReturnToStart` 구현 — `OnBattleEnd` 구독 → `_movement.WarpTo(_startPosition)`
- [ ] `UnitMovement.WarpTo(Vector3)` 추가 — `agent.enabled && isOnNavMesh` 가드, 아니면 `transform.position` 대체
- [ ] 사망 유닛 부활 로직 (`OnBattleEnd` 또는 `OnPreparationStart` → `SetActive(true)` + HP 리셋) — 책임자 결정 필요 (Unit 자체? RoundManager?)
- [ ] TASK-109 기본 공격/스킬 시스템 (탱커/딜러/마법사 3직군)
- [ ] TASK-110 구조물 파괴 → NavMesh 갱신 연동 검증 (TASK-107 3번째 완료 조건 포함)
- [ ] `StructureDamageTester.TryGetComponent` guard clause (3세션째 미처리)

### 메모
- `UnitPlacer.Init` 시그니처 변경: `Init(Vector2Int, int)` → `Init(Vector3, int)` (worldPos 직접 전달)
- `BattleTestInput.FindObjectsByType`는 테스트용. 실제 구현은 유닛 레지스트리 또는 명령 브로드캐스트로 교체 예상
- `PhaseManager.Update`의 스페이스바 트리거는 임시: `StartPreparation()` → `StartBattle()` 연쇄 호출로 Prep↔Battle 토글 (실전은 Phase 6 UI 버튼)
- 컴파일 에러 수정 과정 중 `MoveTo` 가드가 사라졌다가 재추가되는 혼선 발생 — Weak Point 1번 원인
- Unity Editor에서 폴더 이동 시 반드시 Editor 창에서 drag-and-drop (meta/GUID 보존)

---

## 2026-04-19 — Phase 1 연장 설계 재논의 (구현 보류)

### 목표
- TASK-108 종료 후 밀린 연장 작업(`ReturnToStart` / `WarpTo`, 사망 유닛 부활) 재개
- TASK-109 공격 코어까지 커버 시도 (세션 범위 Step 1~3)

### 결과
- **코드 구현은 전면 보류**. 세션 중반에 책임 재분할 필요성 발견 → 다음 세션에 **설계 확정 단계부터 재개**.
- 문서(TASKS.md, PROGRESS.md, 플랜 파일) 만 정리하고 종료.

### 도달한 설계 결정 (다음 세션 시작점)

1. **`Unit.OnBattleEnd` 구독 → `OnPreparationStart` 로 단순화**
   - 근거: `PhaseManager.StartPreparation` 은 두 이벤트를 `OnBattleEnd → OnPreparationStart` 순으로 발행. 후자 시점엔 agent disabled 가 시맨틱으로 보장.
   - 이벤트 의미론: `OnBattleEnd` = 정리 구간 / `OnPreparationStart` = 초기화 구간
   - 2026-04-18 Weak Point 4번 (순서 의존) 은 "이벤트 시맨틱 계약" 으로 해소

2. **`UnitMovement.WarpTo` 폐기 (YAGNI)**
   - 독립 사용처가 현재 0. `Unit.ReturnToStart` 위임 한 곳뿐이었음 → speculative abstraction
   - 필요해지면 그때 추가 (점멸 스킬, 텔레포트 아이템 등)

3. **`OnEnable` 의 암묵적 HP 리셋 제거**
   - 시점이 "활성화" 라는 물리적 사건에 묶여 암묵적이었음
   - 명시적 라운드 리셋 메서드로 통합 (이름 미확정) — HP / (미래) 상태이상 / 쿨다운 등을 한 곳에 모음

4. **책임 재분할 방향 (미확정, 다음 세션에서 확정)**
   - `Unit` → `UnitBody` 리네이밍 후보 (체력/정체성만)
   - `PreparationPosition` 을 `UnitMovement` 로 이동 (이동 영역에 속하는 상태)
   - `BoardRegistry` 는 `SetActive(true)` 만 담당하는 얇은 역할 (이름 재검토 필요 — `UnitLifecycle`, `FieldRoster` 등)
   - 각 컴포넌트가 Phase 이벤트 **독립 구독** (분산 구독 원칙 극단)
   - 핸들러는 모두 idempotent → 순서 무관

### 미확정 (다음 세션 결정 사항)
- [ ] `Unit` → `UnitBody` 이름 확정 여부
- [ ] `UnitBody` 의 라운드 리셋 메서드 이름 (HP + 미래 확장 항목)
- [ ] `UnitMovement` 의 위치 복귀 메서드 이름
- [ ] `BoardRegistry` 의 적합한 이름 (`UnitLifecycle`? `FieldRoster`? 유지?)
- [ ] `UnitPlacer` 의 초기화 라인 재작성 (두 컴포넌트 각각 `Init` 호출 패턴)
- [ ] 첫 배치 시 초기 상태 세팅 경로 (`OnEnable` 제거 대응)
- [ ] 프리팹 `MonoScript` 참조 갱신 필요 여부 확인 (Unit → UnitBody 리네이밍 시)

### 학습 포인트 (사용자)

- **이벤트 4개 세분화의 의미**
  - 각 이벤트마다 **시맨틱 계약**이 있음. `OnBattleEnd`=정리 구간, `OnPreparationStart`=초기화 구간
  - 시맨틱을 따르면 순서 의존은 자연스럽게 해소
  - "어느 이벤트에 걸 것인가" 가 "호출 순서 가드를 어떻게 넣을 것인가" 보다 상위 설계

- **`SetActive(false)` 상태의 C# event delegate 호출**
  - delegate invocation 은 GameObject 활성도와 무관하게 실행
  - Unity 런타임이 막는 건 Update/OnEnable/OnTriggerEnter 등 **생명주기 콜백** 만
  - 결과: SetActive(false) 된 UnitMovement 의 `DisableMovement` 도 호출됨 (idempotent 라 문제 없음)
  - 2026-04-18 Weak Point 3번 (구독/해제 Start/OnDestroy 쌍) 의 심화

- **YAGNI 판단의 날카로움**
  - `WarpTo` 는 독립 사용처 0 → speculative abstraction
  - 판단 기준: "지금 호출자가 있는가" / "없다면 추측만으로 일반화하지 말 것"

- **책임 분할의 깊이 — 속성 vs 맥락**
  - `_startPosition` 은 Unit 의 **몸체 속성** 이 아니라 **필드 레이아웃 맥락**
  - 기준: "이 정보가 해당 객체의 본질 속성인가" vs "외부에서 주어지는 맥락인가"
  - Phase 6 TFT 벤치 드래그 시나리오에서 드러나는 문제: 유닛은 그대로인데 배치 위치가 라운드마다 달라짐 → 위치를 Unit 이 소유하면 Registry 와 진실의 원천이 두 곳

- **분산 구독 원칙의 극단**
  - 각 컴포넌트가 자기 영역 이벤트를 독립 구독 + 자기 상태 소유
  - 모든 핸들러가 idempotent 하면 순서 무관 + 컴포넌트 간 직접 의존 최소화

### Weak Points (다음 세션 복습 권장)

1. **Event delegate 호출 vs Unity 생명주기 콜백 분리**
   - SetActive(false) 된 GameObject 의 컴포넌트가 event 구독하고 있으면 그 메서드는 **여전히 호출됨**
   - 복습 질문: "SetActive(false) 상태의 UnitMovement 에 OnBattleEnd 가 호출되면 DisableMovement 가 실행되는가? Update 는 어떤가? 둘의 차이를 C#/Unity 의 어느 계층에서 구분되는지 설명해보기."

2. **이벤트 시맨틱 계약 — 순서 의존을 피하는 상위 도구**
   - `OnBattleEnd` 와 `OnPreparationStart` 사이의 시맨틱 계약을 이해하면 핸들러 위치가 자연스럽게 정해짐
   - 복습 질문: "`OnBattleEnd` 에 `ReturnToStart` 를 걸면 왜 '순서 의존' 문제가 발생하는가? `OnPreparationStart` 로 옮기면 왜 그 문제가 소멸하는가?"

3. **소유자 선택 기준 — 속성 vs 맥락**
   - 복습 질문: "`_startPosition` 이 Unit 의 몸체 속성이 아니라고 본 이유는? Phase 6 의 TFT 벤치 시나리오에서 어떤 모순이 드러나는가?"

4. **YAGNI vs 미래 확장 대비 — 판단 기준**
   - 복습 질문: "`WarpTo` 는 지금 왜 YAGNI 대상인가? 언제쯤 추가해야 하는 API 인가? 그 사이에는 어떤 더 단순한 방법으로 대체했나?"

### 미완료 / 다음 세션
- [ ] **설계 확정 단계부터 재개** — 위 "미확정" 항목 체크리스트 해결
- [ ] 확정 후 구현 (Unit 분리 or 유지, UnitMovement 확장, BoardRegistry 신규, UnitPlacer 수정)
- [ ] TASK-109 공격 코어 abstract (설계 확정 뒤)
- [ ] TASK-110 (`StructureDamageTester.TryGetComponent` guard + `Structure.IDamageable` 일관화 포함) — 여전히 밀림

### 메모
- 이번 세션은 **코드 변경 0**. 문서(TASKS.md, PROGRESS.md, 플랜 파일) 만 업데이트.
- 플랜 파일(`misty-waddling-quasar.md`) 의 원안은 세션 중 재검토로 **실질 폐기**. 다음 세션 시작 시 새 플랜 작성 권장 (또는 기존 파일에 "설계 재논의 중" 표시 후 덮어쓰기).
- Weak Point 에 `OnEnable` 제거 관련 **첫 배치 초기화 경로** 이슈가 실질 포함되어 있음 — 다음 세션에서 꼭 짚을 것.

---

## 2026-04-19 (2차) — Claude 복귀 코드 리뷰 + 문서 최신화

### 목표
- Claude 없이 구현된 코드 전체 리뷰
- 잘한점 / 잘못된점 / 수정 방향 피드백
- 문서 최신화 + 다음 전략 수립

### 사용자 독립 구현 내용 (Claude 없이 진행)

- [x] `Assets/Scripts/Interfaces/IMovable.cs` — `MoveTo(Vector3)` 단일 메서드
- [x] `Assets/Scripts/Units/UnitHealth.cs` — `IDamageable` 구현, `Init(Unit)` 주입 패턴
- [x] `Assets/Scripts/Units/UnitMovement.cs` — `IMovable` 구현, `Init(Unit)` 주입, Phase 이벤트 독립 구독
- [x] `Assets/Scripts/Units/UnitAnimator.cs` — 애니메이션 전담 컴포넌트
- [x] `Assets/Scripts/Units/Human/HumanSoldier.cs` — `Unit` 구체 클래스, 컴포넌트 조립
- [x] `Unit.cs` 리팩터 — abstract로 변경, 정체성+복귀만 담당
- [x] `Assets/Scripts/Phase/PhaseManager.cs` — 이벤트 4종, StartBattle/StartPreparation
- [x] `Assets/Scripts/Phase/NavMeshBaker.cs` — BuildNavMesh 래퍼
- [x] `Assets/Scripts/Placement/PlacementController.cs` — 배치 모드 전환
- [x] `Assets/Scripts/Testing/BattleTestInput.cs` — 전투 테스트 입력

### 코드 리뷰 결과

**✅ 잘한 부분:**
- SRP 철저: UnitHealth / UnitMovement / UnitAnimator 각 책임 명확
- IMovable, IDamageable 인터페이스 도입 (ISP 준수)
- Init(Unit) 주입 패턴 일관 적용 (계약 기반 초기화)
- PhaseManager 이벤트 독립 구독 — 컴포넌트별 자율 관리
- OnDestroy 이벤트 해제 누락 없음 (Unit.cs, UnitMovement.cs)
- RequireComponent 어트리뷰트 적절히 사용
- TODO 주석으로 의도적 미완성 명시

**❌ 개선 필요 (우선순위 순):**
- `UnitPlacer` ↔ `StructurePlacer` 로직 거의 동일 (DRY 위반) → TASK-109/110 후 리팩토링
- `Unit.cs:24` — `FindGameObjectWithTag` DIP 위반 → SerializeField 교체 권장
- `UnitMovement.cs:18` OnDestroy에서 `_unit` null 가능성 → `_unit?.PhaseManager` 가드 권장

**설계 확정:**
- Unit 클래스명: Unit 유지 / PreparationPosition: Unit 소유 유지
- Phase 구독: 컴포넌트 독립 구독 확정 (이미 구현됨)
- BoardRegistry: TASK-111 신규 착수 예정

### 완료
- [x] TASKS.md 최신화 (설계 확정 반영, TASK-111 신규 추가)
- [x] PROGRESS.md 세션 기록
- [x] ARCHITECTURE.md 실제 구현 구조로 갱신
- [x] PHASE_1.md 미완료 Task 명세 보완

### 다음 세션 착수 순서
1. TASK-111 BoardRegistry 설계 + 구현
2. TASK-109 기본 공격 시스템
3. TASK-110 구조물 파괴 + NavMesh 갱신 검증

### 메모
- 코드 변경 없음. 리뷰 + 문서 작업만.

---

## 2026-04-19 (3차) — TASK-109 기본 공격 시스템 + TASK-110 완료

### 목표
- TASK-109: 기본 공격/스킬 시스템 구현
- TASK-110: 구조물 파괴 + NavMesh 갱신 연동 검증

### 완료
- [x] `Assets/Scripts/Units/Attack/UnitStatsSO.cs` — abstract SO, `ExecuteAttack(Transform, int, Collider[])` 추상 메서드
- [x] `Assets/Scripts/Units/StatsSO/Melee/MeleeStatsSO.cs` — 단일 타겟 근접 공격 (가장 가까운 적 1명)
- [x] `Assets/Scripts/Units/UnitAttack.cs` — OverlapSphere 탐색 + 쿨타임 + Phase 이벤트 구독
- [x] `Assets/Scripts/Interfaces/IGroupOwned.cs` — `int GroupId { get; }` 인터페이스
- [x] `Unit.cs` — `IGroupOwned` 구현
- [x] `Structure.cs` — `IGroupOwned` + `IDamageable` 구현, `GroupId` 추가, 준비 페이즈 자동 복구 (`ReturnTo()`), `TakeDamage(int)` 제거
- [x] `HumanSoldier.cs` — `UnitAttack` 연결
- [x] `StructurePlacer.cs` — `Init(int groupId, Vector3 position)` 호출부 업데이트

### 설계 결정
- **Strategy 패턴 적용**: `UnitStatsSO` (abstract) → `MeleeStatsSO` / `AoEStatsSO` (예정)
- **프리팹 = 유닛 정의**: HumanTank vs OrcTank는 다른 SO를 연결한 다른 프리팹
- **IGroupOwned로 팀 구분**: LayerMask + GroupId 이중 필터
- **Structure도 준비 페이즈 복구**: `OnPreparationStart → ReturnTo()` (Unit 패턴 동일)

### 검증 결과
- ✅ 유닛 자동 공격 동작
- ✅ 구조물 파괴 동작
- ✅ 파괴된 구조물 자리 NavMesh 복구 → 유닛 통과
- ✅ 준비 페이즈 전환 시 구조물/유닛 모두 복구

### 학습 포인트 (사용자)
- **Strategy + ScriptableObject 조합**: SO가 데이터이자 알고리즘 (OCP 달성)
- **프리팹이 곧 유닛 정의**: 종족×직군 조합 폭발을 프리팹+SO 조합으로 해결
- **가장 가까운 것 찾기 패턴**: `float.MaxValue`로 시작, 루프에서 갱신
- **IGroupOwned**: 유닛과 구조물 모두 GroupId를 가져야 하는 이유 (팀 구분)

### Weak Points (다음 세션 복습 권장)
1. **MeleeStatsSO가 처음에 전체 타겟을 때린 이유** — foreach로 모두 때리는 것 vs 최솟값 찾기 패턴 구분
2. **GroupId 하드코딩 `0`**: UnitPlacer의 groupId=0은 기술 부채 — Phase 6 팀 시스템에서 해결 예정

### 미완료 / 다음 세션
- Phase 2 착수: TASK-201 직군별 Observation 설계
- Placer 코드 중복 리팩토링 (낮은 우선순위)

---

## 2026-04-19 (4차) — Phase 2 시작: ML-Agents 기초 + TASK-201

### 목표
- ML-Agents Agent 클래스 개념 학습
- Observation 공간 설계 (이론)
- `UnitAgent.cs` 기초 뼈대 작성 (CollectObservations 자기 상태 관측)

### 환경 확인
- Python mlagents: 설치 완료

### 완료
- [x] ML-Agents Agent 클래스 개념 학습 (MonoBehaviour 상속, CollectObservations/OnActionReceived/Heuristic)
- [x] Observation 설계 이론 (자기 상태 + 주변 유닛 + 0패딩 고정 크기 방식)
- [x] `Assets/Scripts/Agents/UnitAgent.cs` — Agent 상속, CollectObservations 자기 상태 (HP, 위치, 팀ID)
- [x] `Assets/Scripts/Registry/RegistryManager.cs` — 팀별 유닛 목록 관리
- [x] `Unit.cs` — OnBattleStart/OnBattleEnd 이벤트로 Register/Unregister 연결
- [x] `UnitHealth.cs` — Die() 시 Unregister() 호출

### 미완료
- [ ] UnitAgent 주변 적 유닛 관측 (적 슬롯 × 10, 존재flag + HP + 위치)
- [ ] 구조물 관측
- [ ] RaySensor 설정
- [ ] Commander 명령 슬롯 예약

### 다음 세션
- Phase 2 전체 흐름(Observation→Action→Reward→학습)을 한 번에 그린 뒤 재시작
- 조각 구현 전에 "유기적으로 어떻게 동작하는가" 큰 그림 먼저 확립

### Weak Points (다음 세션 복습 권장)
1. Observation / Action / Reward 세 요소가 학습 루프에서 어떻게 연결되는지 전체 흐름
2. Commander가 추가됐을 때 Unit Observation이 어떻게 바뀌는지

---

## 2026-04-20 — Feudal HRL 전면 재설계

### 목표
- 커맨더/유닛 학습 종속성 문제 논의 → 완전 독립 학습 구조로 재설계
- 설계 Q&A로 10개 핵심 결정사항 확정
- 현재 코드 전수 검토 + (B)안 적합성 평가
- 파생 문서 전면 갱신

### 핵심 의사결정 (Q1~Q10)

| 질문 | 결정 |
|---|---|
| Q1 명령 단위 | (b) 분대 단위 |
| Q2 갱신 주기 | (a) 2초 고정 tick (학습 안정 후 이벤트 하이브리드 확장 가능) |
| Q3 유닛 순종 | (ii) HP 0 외 안전밸브 없음 |
| Q4 분대 구성 | (ii) 준비 페이즈 편성 + 전투 중 고정 |
| Q5 명령 필드 | 3필드 시작 (target_position + priority_filter + structure_handling), 확장 6+필드 문서화 |
| Q6 학습 순서 | (C) 커리큘럼 (Scripted → RL 유닛 → RL 커맨더) |
| Q7 유닛 보상 | (ii) 수행도 80% + micro 20%, **승/패 보상 금지** |
| Q8 커맨더 보상 | (다) Sparse + 최소 dense |
| Q9 커맨더 관측 | (i) 완전 정보 시작 (시야 제한은 Phase 3+ 연구) |
| Q10 유닛 관측 | (iii) 지역 + 현재 명령 + 분대 동료 (적 전체는 안 봄) |

### 코드 전수 검토 결과

**유지 가능 (Phase 1 기반, (B)안과 궁합 OK)**:
- PhaseManager, NavMeshBaker, GridField/GridCell, Structure, UnitHealth, UnitMovement, UnitAnimator, UnitStatsSO/MeleeStatsSO, PlacementController, UnitPlacer/StructurePlacer, RegistryManager 뼈대, TrainManager

**수정 필요**:
- `Unit.cs`: SquadId 필드 추가
- `UnitAttack.cs`: 자동 타겟팅 제거, 수동화 (Agent가 SetTarget) ⭐
- `UnitEnum.cs`: UnitType(종) vs UnitRole(직군) 분리
- `SoldierAgent.cs`: 관측/행동/보상 전면 재설계
- `RegistryManager.cs`: 디버그 키 제거, Squad 단위 확장
- `Unit`/`Structure` Awake: FindGameObjectWithTag → DI 전환

**신규 추가**:
- Team 시스템 (Team, TeamRegistry) — GroupTest 대체
- Squad 시스템 (Squad, SquadRegistry, SquadFormation)
- Command 시스템 (SquadCommand, TargetPriority, StructureHandling, CommandBroker, ScriptedCommander)
- CommanderAgent (Phase 3)
- EpisodeManager
- SynergyFlags (Phase 2 중후반)

**제거 대상**:
- `GroupTest` 정적 변수 (Team 도입 직후)
- `BattleTestInput`, `StructureDamageTester` (Phase 2 종료 시)
- `RegistryManager.Update` 디버그 키 블록

### 완료
- [x] CLAUDE.md — AI 아키텍처 section 전면 재작성, 핵심 설계 결정 테이블 재구성
- [x] ARCHITECTURE.md — 디렉토리 목표 구조, 클래스 책임(유지/수정/신규/제거), 데이터 흐름, 주의사항
- [x] REWARDS.md — 유닛/커맨더 보상 완전 분리, 독립성 마지노선 명시, 스테이지별 보상 적용
- [x] OBSERVATIONS.md — 유닛 관측(자기+명령+분대동료+지역) 64차원 초안, 커맨더 완전 정보 ~222차원 초안
- [x] PHASE_2.md — 커리큘럼 Stage 1~2 Task 재편 (TASK-201~206 + TASK-210~218 인프라)
- [x] PHASE_3.md — 커리큘럼 Stage 3 Task 재편 (TASK-310~317)
- [x] TASKS.md — 재설계 결정사항 기록, 구/신 Task 매핑, 제거 목록 명시
- [x] PROGRESS.md — 본 세션 로그

### 완료 (재검토 추가분)

- [x] PHASE_1.md — 완료 상태로 재정리, TASK-109/110 완료 처리, (B)안 재설계로 인한 후속 수정 영향(TASK-210/211/212/217) 명시
- [x] PHASE_4.md — 바리케이드/포탄의 관측 슬롯 반영, 지형 편집을 커맨더 준비 정책(TASK-313)과 연계, structure_handling 확장 가능성 명시
- [x] PHASE_5.md — 관리 대상 Policy 테이블 (직군별 + 커맨더), 전략 다양성을 명령 어휘 활용도로 재정의, Alternating Fine-tune을 TASK-504로 추가

### 미완료 / 다음 세션

- [ ] Phase 2 착수 — TASK-210 (DI 전환)부터 시작 권장

### 메모

- 이번 세션은 코드 변경 없이 설계·문서만 갱신.
- 기존 `SoldierAgent.cs` / `RegistryManager.cs`는 일단 두고 Phase 2 TASK-211/217에서 재설계.
- 코드 커밋은 별도 요청 시에만.

### Weak Points (다음 세션 복습 권장)

1. **Feudal HRL 개념** — 왜 유닛에 승/패 보상을 주면 종속성이 생기는가
2. **명령 3필드의 의미** — target_position + priority_filter + structure_handling으로 표현 가능한 전략 (공성/우회/수비/돌파)
3. **커리큘럼 3단계** — 왜 alternating보다 순차 freeze가 독립성에 유리한가
4. **UnitAttack 수동화의 이유** — 자동 타겟팅이 명령의 priority_filter를 무력화하는 구조적 문제
5. **커맨더 관측 "완전 정보"의 함의** — 연출(안개 전쟁)과 학습 안정성 트레이드오프

---

## 2026-04-26 — Phase 2 착수: Weak Points 복습 + TASK-210 (DI 전환)

### 목표
- 지난 재설계(2026-04-20) Weak Points 4개 복습 (1주일 공백 보충)
- TASK-210 착수: Unit/Structure의 `FindGameObjectWithTag` 제거 → DI 전환
- DI 개념 학습 (왜 Find 호출이 ML-Agents 병렬 학습에서 위험한가)

### 완료 — Weak Points 복습
- [x] **#1 Feudal HRL 종속성** — 사용자 답변 거의 정확. 보강: 종속성의 본질 = "유닛 정책이 특정 커맨더에 맞춰져 다른 커맨더와 조합 불가능"
- [x] **#2 명령 3필드** — 처음엔 모름. `target_position + priority_filter + structure_handling`로 4가지 전략(공성/돌파/수비/우회) 표현 가능. NavMesh 의문(우회 방향 통제) 토론 → "NavMesh = 실행기 / Agent+커맨더 = 의사결정기, 우회 방향은 시간차 명령 시퀀스로 통제"
- [x] **#3 커리큘럼 vs Alternating** — 사용자 답 정확("가중치 꼬임, 걸음마 비유"). 보강: OOD 문제 + Stage 3의 학습 신호 오염. PHASE_2.md 160라인의 "Scripted 커맨더가 명령 어휘 전영역 커버해야 함"이 직결
- [x] **#4 UnitAttack 수동화** — 사용자 답 완벽. "사슬의 끊어진 고리" 개념으로 정리: Agent 출력이 환경에 영향 없으면 gradient 무의미

### 완료 — TASK-210 DI 전환

**설계 결정 (사용자 발의)**:
- RegistryManager 의존성을 **지금** Unit에서 분리 (옵션 B 채택)
- 이유: 현재 누구도 GroupedUnits를 읽지 않음 → 사실상 죽은 코드. TASK-211 (Team 시스템)에서 역할을 명확히 잡으며 재도입이 깔끔
- YAGNI 정신 + 재설계 폭이 큰 영역에 미리 결합 만들지 않기

**수정 파일 6개**:
- [x] `Unit.cs` — Awake 통째 삭제, RegistryManager 필드/Register/Unregister 제거, `Init(int, Vector3, PhaseManager)` 시그니처 + Init 안에서 이벤트 구독, OnDestroy null 가드
- [x] `UnitHealth.cs` — `_unit.Unregister()` 호출만 제거 (`_unit` 필드와 `Init(Unit)` 시그니처는 일관성 위해 유지 — 사용자 결정)
- [x] `HumanSoldier.cs` — `base.Awake();` 한 줄 삭제
- [x] `Structure.cs` — Awake 통째 삭제, `Init(int, Vector3, PhaseManager)` 시그니처 + Init 안에서 이벤트 구독, OnDestroy null 가드
- [x] `UnitPlacer.cs` — `soldier.Init(GroupTest.groupId, worldPos, _phaseManager)` 호출
- [x] `StructurePlacer.cs` — `structure.Init(GroupTest.groupId, worldPos, _phaseManager)` 호출

**검증 (Unity 플레이 테스트)**:
- ✅ 컴파일 통과
- ✅ 유닛/구조물 배치 정상
- ✅ 전투 시 NavMesh 이동 + 자동 공격
- ✅ 구조물 파괴 → NavMesh 복구 → 유닛 통과
- ✅ 준비 페이즈 복귀 시 ReturnTo 정상 (Init 안 이벤트 구독이 정상 동작 증명)

### 학습 포인트 (사용자)
- **DI의 본질**: 의존성을 "찾는" 게 아니라 "받는" — 환경 격리가 코드 구조로 강제됨
- **Find의 진짜 위험은 성능이 아니라 비결정성**: ML-Agents 병렬 환경에서 16개 매니저 중 어느 것에 연결될지 보장 없음 → 환경 간 신호 오염 → **에러 없이 학습이 망가지는** 최악의 디버깅 상황
- **YAGNI 적용**: 리팩토링 중에 안 쓰이는 의존성을 발견하면 미래 사용처가 모호하더라도 일단 분리. 진짜 필요할 때 명확한 역할로 재도입.
- **이벤트 구독을 Awake → Init으로 옮긴 연쇄 효과**: Find의 매력 = "Awake에서 끝남". DI 전환 후 "Instantiate 직후 → Init 호출 전" 구간이 미완성 상태가 됨 → OnDestroy null 가드 필요

### Weak Points (다음 세션 복습 권장)
1. **이벤트 구독을 Init 안으로 옮긴 이유** — 왜 Awake 시점엔 매니저가 없는가 (Instantiate → Awake → Init 호출 순서)
2. **OnDestroy null 가드의 의미** — Init이 호출되기 전에 파괴되는 비정상 케이스 방어
3. **YAGNI 판단의 실용 기준** — RegistryManager는 "지금 안 쓰이는데 곧 쓰일 예정". 이런 회색지대에서 "지금 분리"를 택한 이유 (재설계 폭이 큰 영역에선 미리 결합 만들지 않기)

### 미완료 / 다음 세션
- [ ] TASK-211: Team 시스템 구축 (`Team`, `TeamRegistry`) — `GroupTest` 정적 변수 대체
- TASK-211에서 RegistryManager 역할이 Team으로 흡수될지, 별도로 살아남을지 결정 필요

### 메모
- 코드 변경 6개 파일, 모두 사용자가 직접 타이핑 (가이드만 제공)
- 커밋은 별도 요청 시에만 (현재 미커밋)

