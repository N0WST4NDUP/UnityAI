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
