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
