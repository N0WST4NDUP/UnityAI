# CLAUDE.md — AutoChess Combat AI Project Master Document

> **Claude Code 지침**: 이 파일을 먼저 읽고, 하단 `## 세션 초기화 절차`를 따라 파생 문서를 생성한 뒤 작업을 시작할 것. 모든 세션은 이 문서와 파생 문서들을 공유 컨텍스트로 사용한다.

---

## 프로젝트 개요

| 항목 | 내용 |
|------|------|
| 프로젝트명 | AutoChess Combat AI |
| 엔진 | Unity 6000.3.11f1 |
| AI 프레임워크 | ML-Agents (Python + ONNX) |
| 개발 환경 | Windows 11, Intel Arc GPU, Intel AI Boost NPU |
| 개발 목표 | 포트폴리오 + 소규모 친구들과 플레이 가능한 수준 |
| 작업 브랜치 | `AutoChessLike` (UnityAI repo) |

---

## 게임 컨셉

**미니어처 워게임 + 오토체스** 하이브리드. 준비 페이즈에서 Grid 보드 위에 미니어처를 배치하면, 전투가 시작되면 미니어처가 살아나서 자유롭게 싸우는 소규모 전쟁 시뮬레이션.

- **준비 페이즈**: Grid 기반 배치 (미니어처를 보드에 놓는 느낌)
- **전투 페이즈**: NavMesh 기반 자유 이동 (미니어처가 살아서 실제로 싸우는 느낌)
- **TFT/오토체스 룰**: 레벨에 따른 유닛 수 제한 + 시너지 시스템 + 골드/상점
- **공성/수성**: 아이템 시스템 대신 **구조물 시스템 + 지형 편집 + 전투 중 아이템**으로 다양성 부여

### 게임 플로우

```
[준비 페이즈]
├── 수성 측: 자기 진영(맵 절반)에 지형 높낮이 조절 + 구조물(성벽/문) 건설 + 유닛 배치
├── 공성 측: 상대 필드 레이아웃 확인 후 유닛 배치
├── 양측: 시너지 구성 + 골드 관리
└── 배치 완료 → NavMesh Bake (한 번)

[전투 페이즈]
├── 미니어처 활성화 → NavMesh 위에서 자유 이동 + 자동 전투
├── Agent: 이동/공격/스킬/구조물 파괴 등 자율 판단
├── 유저: 전투 중 아이템 사용 (바리케이드 설치, 포탄 투하 등)
├── 구조물 파괴 → NavMeshObstacle 제거 → NavMesh 실시간 갱신
└── 승패 판정

[라운드 종료]
├── 공성/수성 역할 교대 (매 라운드)
└── 서버 공유 Policy 가중치 업데이트

[판 종료]
└── 8인 배틀로얄 방식으로 최후 생존자 결정
```

---

## 듀얼 페이즈 시스템

이 프로젝트의 핵심 구조. 준비와 전투에서 완전히 다른 시스템이 작동한다.

### 준비 페이즈 — Grid 기반

| 항목 | 설명 |
|------|------|
| 배치 시스템 | Grid 셀 단위로 유닛/구조물 배치 |
| 지형 편집 | 자기 진영 맵 절반의 높낮이 조절 (일정 수준까지) |
| 구조물 건설 | 성벽, 문, 탑 등 Grid 셀에 배치 |
| Grid 역할 | 배치 위치 관리, 겹침 방지, 시각적 가이드 |

### 전투 페이즈 — NavMesh 기반 자유 이동

| 항목 | 설명 |
|------|------|
| 이동 | NavMeshAgent 기반 연속 좌표 이동 |
| 구조물 | NavMeshObstacle (Carve=true), 파괴 시 실시간 NavMesh 갱신 |
| NavMesh Bake | 전투 시작 시 NavMeshSurface.BuildNavMesh() 1회 실행 |
| Grid 역할 | **사용 안 함** — 전투 중 유닛은 Grid에서 해방 |

### 전투 시작 전환 흐름

```
준비 완료 버튼
    ↓
1. 지형/구조물 확정
2. NavMeshSurface.BuildNavMesh()  ← 지형+구조물 반영 Bake
3. 구조물에 NavMeshObstacle(Carve=true) 활성화
4. 유닛 Grid 위치 → World 좌표 변환 → NavMeshAgent 활성화
5. 전투 시작
```

---

## AI 아키텍처

### Hierarchical RL 구조

```
Commander Agent (1개, 팀 전체 지휘)
    ↓ 전략 명령 출력
Unit Agents (직군별 공유 Policy)
    ↓ 자율 행동 실행
전투 환경 (NavMesh 기반 자유 이동)
```

### Commander Agent

| 항목 | 내용 |
|------|------|
| 역할 | 전장 관측 → 상대 패턴 분석 → 전략 명령 |
| 관측 | VectorSensor (모든 유닛 위치/상태, 구조물 상태, 역할 플래그) 또는 GridSensor (유닛 위치를 Grid에 투영한 오버레이) — Phase 3에서 확정 |
| 출력 명령 | 총공세 / 후퇴 / 특정 유닛 타겟 포커싱 / 포지션 조정 (복합) |
| 학습 목표 | 전략 다양성 |

**설계 주의사항**:
- 유닛 수가 16~20으로 소규모 → VectorSensor만으로 충분할 수 있음
- GridSensor 오버레이 방식도 가능: 유닛의 연속 좌표를 Grid에 투영하여 공간 인식
- Phase 3에서 두 방식을 비교 실험 후 확정

### Unit Agents

직군별 공유 Policy 방식. 같은 직군 유닛은 하나의 Policy를 공유한다.

| 직군 | 역할 | 보상 특이사항 |
|------|------|--------------|
| 탱커 (방패병 등) | 어그로 유인 + 구조물 방어 | 어그로량 + 탱킹 피해량 보상 |
| 딜러 / 암살자 | 주요 타겟 제거 + 구조물 파괴 | 적 핵심 유닛 처치 보상 |
| 마법사 | 범위 스킬 누킹 | 스킬 적중 + 범위 피해량 보상 |

**Agent 행동 공간 (Continuous)**:
- 이동 방향 (2D 벡터)
- 공격/스킬 대상 선택
- **구조물 부술지 vs 우회할지 판단** ← NavMesh가 결정하지 않음, Agent가 학습

**유닛 수 변화 대응**:
- Observation은 최대 유닛 수 기준 고정
- 빈 슬롯은 0 패딩 + 유닛 존재 여부 flag 포함

**공통 보상**:
- 승리 시 공통 보상
- 패배 시 공통 패널티

### 시너지 시스템

시너지 = Agent 행동 성향 하이퍼파라미터.
유저가 시너지를 구성하면 해당 시너지 플래그가 Agent의 observation 또는 reward shaping에 반영된다.

예시:
- `공격적 시너지` → 전진 행동 가중치 증가, 후퇴 패널티 증가
- `수비적 시너지` → 포지션 유지 보상 증가, 어그로 유지 보상 증가

---

## 필드 시스템

### 준비 페이즈: Grid 기반

유닛 배치와 구조물 건설을 위한 Grid. 전투 시작 후에는 사용하지 않는다.

```
셀 상태값 (준비 페이즈 전용):
- 0: 빈 공간 (배치 가능)
- 1: 구조물 (배치 불가)
- 2: 유닛 점유 (배치 불가)
```

**지형 편집**:
- 수성 측은 자기 진영(맵 절반)의 높낮이를 조절 가능
- 일정 수준까지만 (타이쿤류 느낌)
- 전투 시작 시 NavMesh Bake에 반영

### 전투 페이즈: NavMesh 기반

| 요소 | 처리 방식 |
|------|-----------|
| 유닛 이동 | NavMeshAgent.SetDestination() |
| 구조물 | NavMeshObstacle (Carve=true), 내구도 보유 |
| 구조물 파괴 | NavMeshObstacle 제거 → NavMesh 자동 복구 → 유닛 통과 가능 |
| 지형 (높낮이) | NavMesh Bake 시 반영, 전투 중 변경 불가 |
| 벽 부수기 vs 우회 | Agent가 학습으로 판단 (NavMesh는 실행만 담당) |

### 구조물 시스템

- 준비 페이즈에서 유저가 Grid 셀에 구조물 배치
- 구조물 = NavMeshObstacle + Collider + 내구도
- 파괴 시 NavMeshObstacle 제거 → NavMesh 실시간 갱신 → Agent가 지형 변화 인식
- **종류 확장 예정**: 성벽, 문, 탑, 계단 등 (Phase 4+)

### 아이템 시스템 (전투 중 유저 개입)

| 아이템 | 효과 | 방식 |
|--------|------|------|
| 바리케이드 | 임시 장애물 생성 | 자유 좌표 + 방향 회전, NavMeshObstacle(Carve) |
| 포탄 | 범위 피해 (AoE) | 자유 좌표 타겟팅, 낙하 예고 시각 표시 후 폭발 (MOBA/RTS 스타일) |
| (추후 확장 가능) | | |

**Agent 관점**:
- 바리케이드 = 실시간 지형 변화 → NavMesh 갱신 인식 → 경로 재계산
- 포탄 = 위험 영역 인식 → 회피 또는 무시 판단
- 전략 다양성 + 극적 연출 목표에 직접 기여

---

## 유닛 규모

| 항목 | 수치 |
|------|------|
| 한 팀 유닛 수 | 8~10 (레벨에 따라 증가) |
| 양 팀 합계 | 최대 16~20 |
| 유닛 단위 | 개별 유닛 (소대 아님) |

---

## 서버 공유 Policy

| 항목 | 내용 |
|------|------|
| 공유 단위 | Commander Policy 1개 + 직군별 Unit Policy |
| 업데이트 타이밍 | 판 종료 시 가중치 업데이트 |
| 배포 방식 | ONNX 추론 (Unity에서 로드) |
| 현재 목표 | 포트폴리오 수준 (실서버 분리 없음) |

**Catastrophic Forgetting 대응**:
- 판 수가 쌓일수록 과거 전략 망각 위험 존재
- 초기엔 단순 가중치 덮어쓰기로 시작
- 문제 발생 시 EWC (Elastic Weight Consolidation) 도입 검토

---

## 구현 로드맵 (Phase)

### Phase 1 — 전투 씬 기초
- [ ] Grid 기반 준비 페이즈 필드 (셀 상태 관리) — 완료
- [ ] 구조물 배치 로직 (준비 페이즈) — 완료, NavMeshObstacle 연결 필요
- [ ] 유닛 NavMesh 기반 자유 이동 구현
- [ ] 준비 → 전투 페이즈 전환 시스템 (NavMesh Bake 포함)
- [ ] 기본 공격 / 스킬 시스템 (직군별)
- [ ] 구조물 파괴 → NavMesh 갱신 연동

### Phase 2 — Unit Agent 학습
- [ ] 직군별 Observation 설계 (VectorSensor + RaySensor)
- [ ] 직군별 보상 함수 구현
- [ ] Self-play 기본 전투 학습
- [ ] 시너지 파라미터 연결
- [ ] 구조물 부수기 vs 우회 행동 학습

### Phase 3 — Commander Agent 연결
- [ ] Commander 관측 방식 확정 (VectorSensor vs GridSensor 오버레이 비교 실험)
- [ ] 공성/수성 역할 플래그 연결
- [ ] Commander → Unit 명령 파이프라인 구현
- [ ] Hierarchical RL 학습 루프 구성

### Phase 4 — 아이템 + 구조물 확장
- [ ] 바리케이드: 자유 좌표 배치 + NavMeshObstacle
- [ ] 포탄: AoE 범위 타겟팅 + 낙하 예고 + 폭발
- [ ] 아이템 사용 UI (유저 개입 포인트)
- [ ] 지형 편집 시스템 (준비 페이즈, 높낮이 조절)
- [ ] 구조물 종류 확장 (문, 탑, 계단 등)

### Phase 5 — 발전 구조
- [ ] 판 종료 후 가중치 업데이트 파이프라인
- [ ] ONNX 추론 Unity 로드 연결
- [ ] 전략 다양성 / 극적 연출 보상 튜닝

### Phase 6 — 게임 외형 (선택)
- [ ] 8인 배틀로얄 라운드 진행 로직
- [ ] 골드 / 상점 시스템
- [ ] 레벨 → 유닛 수 제한 시스템
- [ ] UI / UX

---

## 세션 초기화 절차

> **Claude Code는 새 세션 시작 시 반드시 아래 절차를 따를 것.**

### 1. 파생 문서 확인

`docs/` 디렉토리에 아래 파일들이 존재하는지 확인한다.
없으면 즉시 생성한다.

```
docs/
├── TASKS.md          # 전체 Task 목록 + 진행 상태
├── PHASE_1.md        # Phase 1 세부 작업 명세
├── PHASE_2.md        # Phase 2 세부 작업 명세
├── PHASE_3.md        # Phase 3 세부 작업 명세
├── PHASE_4.md        # Phase 4 세부 작업 명세
├── PHASE_5.md        # Phase 5 세부 작업 명세
├── ARCHITECTURE.md   # 클래스 구조 + 컴포넌트 설계 상세
├── REWARDS.md        # 직군별 보상 함수 상세 설계
├── OBSERVATIONS.md   # Agent별 Observation Space 상세 설계
└── PROGRESS.md       # 세션별 진행 로그 (날짜 + 완료 항목)
```

### 2. PROGRESS.md 업데이트

세션 시작 시 날짜와 목표를 기록한다.
세션 종료 시 완료 항목과 미완료 항목을 기록한다.

### 3. TASKS.md 기준으로 작업 선택

TASKS.md의 우선순위와 의존 관계를 확인하고 다음 작업을 선택한다.

---

## 파생 문서 생성 가이드

### TASKS.md 생성 규칙

```markdown
# TASKS.md

## 진행 중
- [ ] TASK-XXX: 작업명 (Phase N) — 담당: Unit/Commander/System

## 완료
- [x] TASK-XXX: 작업명

## 블로킹
- TASK-XXX는 TASK-YYY 완료 후 시작 가능
```

### PHASE_N.md 생성 규칙

각 Phase 문서는 아래 구조로 작성한다:

```markdown
# Phase N — 단계명

## 목표
## 선행 조건 (의존 Phase)
## 세부 Task 목록
### TASK-N01: 작업명
- 구현 위치 (Unity Scene / Script / Python)
- 입력 / 출력
- 완료 조건
## 주의사항
```

### ARCHITECTURE.md 생성 규칙

Unity 컴포넌트 구조, C# 클래스 책임, ML-Agents Agent 클래스 상속 구조를 포함한다.

### REWARDS.md 생성 규칙

직군별 보상 함수를 수식과 함께 명세한다. 공통 보상 / 직군별 보상 / 페널티 분리.

### OBSERVATIONS.md 생성 규칙

Commander / Unit Agent별 Observation 설계를 명세한다. Phase 3에서 VectorSensor vs GridSensor 오버레이 비교 실험 결과를 반영.

---

## 핵심 설계 결정 사항 (변경 시 이 문서 업데이트 필요)

| 결정 항목 | 결정 내용 | 변경 가능성 |
|-----------|-----------|-------------|
| 준비 페이즈 | Grid 기반 배치 (셀 단위) | 낮음 |
| 전투 이동 | NavMesh + NavMeshAgent (자유 이동) | 낮음 |
| 구조물 | NavMeshObstacle(Carve) + 내구도 | 낮음 |
| NavMesh Bake | 전투 시작 시 1회 (NavMeshSurface.BuildNavMesh) | 낮음 |
| 벽 부수기/우회 | Agent 학습으로 판단 (NavMesh는 실행만) | 낮음 |
| 바리케이드 | 자유 좌표 + 방향 회전 + NavMeshObstacle | 중간 |
| 포탄 | 자유 좌표 AoE (MOBA/RTS 스타일) | 중간 |
| Commander 관측 | Phase 3에서 확정 (VectorSensor vs GridSensor 비교) | 높음 |
| Unit 관측 | VectorSensor + RaySensor (Phase 2에서 확정) | 높음 |
| Unit Policy | 직군별 공유 | 중간 |
| 가중치 업데이트 | 판 종료 시 단순 덮어쓰기 (초기) | 높음 |
| 공성/수성 교대 | 매 라운드 교대 | 중간 |
| 유닛 규모 | 팀당 8~10, 합계 16~20 | 중간 |
| 지형 편집 | 수성 측 준비 페이즈에서만 (높낮이 제한적) | 중간 |

---

## 참고 스택

- Unity ML-Agents: `com.unity.ml-agents`
- Unity AI Navigation: NavMesh + NavMeshAgent + NavMeshObstacle + NavMeshSurface
- Python: `mlagents` 패키지
- 추론: ONNX (Unity Sentis or ML-Agents 내장)
- 버전 관리: Git (`AutoChessLike` 브랜치)
- 개발 환경: Windows 11, Intel Arc GPU
