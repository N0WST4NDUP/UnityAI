---
title: 학습 진행 상황
last_updated: 2026-04-15
---

# 학습 진행 상황

## 현재 Phase

| Phase | 주제 | 상태 |
|-------|------|------|
| Phase 1 | State Pattern 기초 (FSM 설계) | 완료 |
| Phase 2 | 적 NPC AI (Enemy Agent) | 완료 |
| Phase 3 | 보스 몬스터 AI (Hierarchical FSM) | 미시작 |
| Phase 4 | ML-Agents 기초 | 미시작 |
| Phase 5 | 하이브리드 에이전트 | 미시작 |

---

## Phase 1: State Pattern 기초

**상태:** 완료

**완료 항목:**
- [x] IState 인터페이스 작성 (`Scripts/StateMachine/IState.cs`)
- [x] StateMachine 클래스 작성 (`Scripts/StateMachine/StateMachine.cs`)
- [x] 색깔 전환 테스트 동작 확인 (TestAgent + IdleState + ActiveState)
- [x] 동작 확인 씬 생성 (Scene: `Phase1_FSMBasics`)

**메모:**
- StateMachine은 순수 C# 클래스로 구현 (MonoBehaviour 아님)
- TestAgent가 StateMachine을 소유하고 Tick() 호출
- 각 State는 생성자에서 StateMachine과 Renderer를 받음

---

## Phase 2: 적 NPC AI

**상태:** 완료

**완료 항목:**
- [x] NavMeshAgent 베이크 및 기본 이동 확인 (NavMeshSurface 컴포넌트 방식)
- [x] `Scripts/Enemy/EnemyAgent.cs` 작성
- [x] EnemyIdleState 구현
- [x] EnemyPatrolState 구현
- [x] EnemyChaseState 구현
- [x] EnemyAttackState 구현
- [x] EnemyDeadState 구현
- [x] 상태 전환 로직 연결
- [x] 씬에서 동작 확인 (Scene: `Phase2_EnemyAI`)

**메모:**
- Unity 6에서 NavMesh 베이크는 NavMeshSurface 컴포넌트로 처리 (Bake 탭 없음)
- 상태 클래스명 컨벤션: `Enemy` 접두사 사용 (예: EnemyIdleState)
- EnemyAgent를 생성자에 통째로 넘기는 방식 채택 (프로퍼티로 필요한 값 노출)
- 웨이포인트 인덱스는 EnemyAgent가 관리, `CurrentWaypoint` 프로퍼티 + `NextWaypoint()` 메서드
- 웨이포인트 도착 후 Idle을 거쳐 다음 순찰 (자연스러운 대기 효과)
- 오브젝트 풀링 구조: `OnEnable()`에서 HP 리셋 + Idle 상태 복귀
- 상태별 색깔: Idle=파랑, Patrol=초록, Chase=노랑, Attack=빨강, Dead=회색
- `pathPending` 체크로 첫 프레임 도착 오판 방지
- TakeDamage에 `#if UNITY_EDITOR` 조건부 컴파일로 LogWarning 처리

---

## Phase 3: 보스 몬스터 AI

**상태:** 미시작

**완료 항목:**
- [ ] `Scripts/Boss/BossAgent.cs` 작성
- [ ] Phase1State 구현
- [ ] Phase2State 구현
- [ ] Phase3State 구현
- [ ] 하위 FSM (Action 상태들) 구현
- [ ] 체력 임계값 전환 확인
- [ ] 씬에서 동작 확인 (Scene: `Phase3_BossAI`)

**메모:**
_없음_

---

## Phase 4: ML-Agents 기초

**상태:** 미시작

**완료 항목:**
- [ ] ML-Agents 패키지 확인 (설치됨: 4.0.2)
- [ ] `Scripts/MLAgents/SimpleAgent.cs` 작성
- [ ] Observation 설계
- [ ] Action 설계
- [ ] Reward 설계
- [ ] Heuristic 모드 테스트
- [ ] 학습 config yaml 작성
- [ ] 학습 실행 및 결과 확인

**메모:**
_없음_

---

## Phase 5: 하이브리드 에이전트

**상태:** 미시작

**완료 항목:**
- [ ] `Scripts/Hybrid/HybridAgent.cs` 작성
- [ ] ExploreState 구현 (ML-Agents 정책 연동)
- [ ] CombatState 구현 (ML-Agents 정책 연동)
- [ ] RetreatState 구현 (룰 기반)
- [ ] 멀티 에이전트 씬 구성
- [ ] Enemy vs Hybrid 대결 씬 동작 확인

**메모:**
_없음_

---

## 생성된 파일 트리

```
Assets/
└── Scripts/
    ├── StateMachine/       ← Phase 1
    │   ├── IState.cs
    │   └── StateMachine.cs
    ├── Enemy/              ← Phase 2
    │   ├── EnemyAgent.cs
    │   └── States/
    │       ├── EnemyIdleState.cs   ✅ 완료
    │       ├── EnemyPatrolState.cs
    │       ├── EnemyChaseState.cs
    │       ├── EnemyAttackState.cs
    │       └── EnemyDeadState.cs
    ├── Boss/               ← Phase 3
    │   ├── BossAgent.cs
    │   ├── Phases/
    │   │   ├── Phase1State.cs
    │   │   ├── Phase2State.cs
    │   │   └── Phase3State.cs
    │   └── Actions/
    │       ├── AttackAction.cs
    │       ├── SpecialAttackAction.cs
    │       └── StaggerAction.cs
    ├── MLAgents/           ← Phase 4
    │   └── SimpleAgent.cs
    └── Hybrid/             ← Phase 5
        ├── HybridAgent.cs
        └── States/
            ├── ExploreState.cs
            ├── CombatState.cs
            └── RetreatState.cs
```
