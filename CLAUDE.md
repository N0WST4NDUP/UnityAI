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

TFT / 오토체스 스타일의 자동 전투 게임. 아이템 시스템 대신 **공성/수성 구조물 시스템**과 **전투 중 랜덤 아이템**으로 전투 다양성을 부여한다.

### 게임 플로우

```
[준비 페이즈]
├── 공성 측: 상대 필드 레이아웃 확인 후 유닛 배치
├── 수성 측: 자기 필드에 구조물(성/벽) 건설 + 유닛 배치
└── 양측: 시너지 구성 + 골드 관리

[전투 페이즈]
├── Agent 자동 전투 시작 (유저는 관망)
├── 유저: 전투 중 랜덤 아이템 자유 사용 가능 (제약 조건 있음)
└── 승패 판정

[라운드 종료]
├── 공성/수성 역할 교대 (매 라운드)
└── 서버 공유 Policy 가중치 업데이트

[판 종료]
└── 8인 배틀로얄 방식으로 최후 생존자 결정
```

---

## AI 아키텍처

### Hierarchical RL 구조

```
Commander Agent (1개, 팀 전체 지휘)
    ↓ 복합 명령 출력
Unit Agents (직군별 공유 Policy)
    ↓ 실행
전투 환경 (Grid 기반 필드)
```

### Commander Agent

| 항목 | 내용 |
|------|------|
| 역할 | 전장 관측 → 상대 패턴 분석 → 전략 명령 |
| 관측 | GridSensor (유닛 위치 + 구조물 + 지형 + 위험 셀) + VectorSensor (역할 플래그 + 상대 패턴 히스토리) |
| 출력 명령 | 총공세 / 후퇴 / 특정 유닛 포지션 조정 / 특정 유닛 타겟 포커싱 (복합) |
| 학습 목표 | 전략 다양성 (B) |

**설계 주의사항**:
- GridSensor 기반 관측 → 유닛 수 변화에 무관하게 설계 가능
- 공성/수성 역할 플래그를 VectorSensor에 포함 → 역할별 전략 자동 전환

### Unit Agents

직군별 공유 Policy 방식. 같은 직군 유닛은 하나의 Policy를 공유한다.

| 직군 | 역할 | 보상 특이사항 |
|------|------|--------------|
| 탱커 (방패병 등) | 어그로 유인 + 구조물 방어 | 어그로량 + 탱킹 피해량 보상 |
| 딜러 / 암살자 | 주요 타겟 제거 + 구조물 파괴 | 적 핵심 유닛 처치 보상 |
| 마법사 | 범위 스킬 누킹 | 스킬 적중 + 범위 피해량 보상 |

**공통 보상**:
- 승리 시 공통 보상
- 패배 시 공통 패널티

**유닛 수 변화 대응**:
- Observation은 최대 유닛 수 기준 고정
- 빈 슬롯은 0 패딩 + 유닛 존재 여부 flag 포함

### 시너지 시스템

시너지 = Agent 행동 성향 하이퍼파라미터.
유저가 시너지를 구성하면 해당 시너지 플래그가 Agent의 observation 또는 reward shaping에 반영된다.

예시:
- `공격적 시너지` → 전진 행동 가중치 증가, 후퇴 패널티 증가
- `수비적 시너지` → 포지션 유지 보상 증가, 어그로 유지 보상 증가

---

## 필드 시스템

### Grid 기반 설계

NavMesh 미사용. 모든 이동과 지형은 Grid 셀 상태로 관리한다.

```
셀 상태값:
- 0: 빈 공간 (이동 가능)
- 1: 구조물 (이동 불가, 내구도 있음)
- 2: 유닛 점유 (이동 불가)
- 3: 위험 셀 (포탄 낙하 예고, 잔여 틱 포함)
```

### 구조물 시스템

- 준비 페이즈에서 유저가 Grid 셀에 구조물 배치
- 구조물 = 내구도를 가진 셀 (파괴 가능)
- 파괴 시 해당 셀 상태 0으로 변경 → Agent 실시간 지형 변화 인식

### 아이템 시스템 (전투 중 유저 개입)

| 아이템 | 효과 | 제약 조건 |
|--------|------|-----------|
| 바리케이트 | Grid 셀에 임시 벽 생성 | 아군 필드 절반에만 설치 가능 / 유닛 점유 셀 불가 |
| 포탄 | 범위 피해 | 낙하 예고 존 n초 노출 후 낙하 / Agent가 위험 셀로 인식하고 회피 가능 |
| (추후 확장 가능) | | |

**Agent 관점**:
- 아이템 = 예측 불가능한 환경 변화
- 전략 다양성(B) + 극적 연출(D) 목표에 직접 기여

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

### Phase 1 — Grid 전투 씬 기초
- [ ] Grid 기반 필드 구현 (셀 상태 관리)
- [ ] 구조물 배치 로직 (준비 페이즈)
- [ ] 유닛 Grid 기반 이동 구현 (NavMesh 미사용)
- [ ] 기본 공격 / 스킬 시스템 (직군별)

### Phase 2 — Unit Agent 학습
- [ ] 직군별 Observation 설계 (VectorSensor + 주변 셀)
- [ ] 직군별 보상 함수 구현
- [ ] Self-play 기본 전투 학습
- [ ] 시너지 파라미터 연결

### Phase 3 — Commander Agent 연결
- [ ] Commander GridSensor 설계 (전장 전체 관측)
- [ ] 공성/수성 역할 플래그 VectorSensor 연결
- [ ] Commander → Unit 명령 파이프라인 구현
- [ ] Hierarchical RL 학습 루프 구성

### Phase 4 — 아이템 시스템
- [ ] 바리케이트 셀 검증 로직 (설치 제약)
- [ ] 포탄 위험 셀 + 잔여 틱 observation 추가
- [ ] 아이템 사용 UI (유저 개입 포인트)

### Phase 5 — 발전 구조
- [ ] 판 종료 후 가중치 업데이트 파이프라인
- [ ] ONNX 추론 Unity 로드 연결
- [ ] 전략 다양성 / 극적 연출 보상 튜닝

### Phase 6 — 게임 외형 (선택)
- [ ] 8인 배틀로얄 라운드 진행 로직
- [ ] 골드 / 상점 시스템
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

Commander / Unit Agent별 VectorSensor 인덱스 맵과 GridSensor 설정값을 명세한다.

---

## 핵심 설계 결정 사항 (변경 시 이 문서 업데이트 필요)

| 결정 항목 | 결정 내용 | 변경 가능성 |
|-----------|-----------|-------------|
| 이동 시스템 | Grid 기반, NavMesh 미사용 | 낮음 |
| Observation 크기 | 최대 유닛 수 기준 고정 + 패딩 | 낮음 |
| Commander 관측 | GridSensor 기반 | 낮음 |
| Unit Policy | 직군별 공유 | 중간 |
| 가중치 업데이트 | 판 종료 시 단순 덮어쓰기 (초기) | 높음 |
| 공성/수성 교대 | 매 라운드 교대 | 중간 |
| 아이템 사용 | 전투 중 유저 자유 사용 (제약 있음) | 중간 |

---

## 참고 스택

- Unity ML-Agents: `com.unity.ml-agents`
- Python: `mlagents` 패키지
- 추론: ONNX (Unity Sentis or ML-Agents 내장)
- 버전 관리: Git (`AutoChessLike` 브랜치)
- 개발 환경: Windows 11, Intel Arc GPU
