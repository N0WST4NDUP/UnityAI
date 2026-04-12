---
title: StatePatternAgent 학습 커리큘럼
last_updated: 2026-04-12
---

# StatePatternAgent 학습 커리큘럼

Unity + State Pattern + ML-Agents를 단계별로 통합 학습하는 로드맵입니다.
학습 난이도는 낮은 순서로 배치되어 있으며, 각 Phase는 이전 Phase를 기반으로 확장됩니다.

---

## 전체 구조

```
Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5
 FSM 기초   적 NPC    보스 AI   ML-Agents  하이브리드
```

---

## Phase 1: State Pattern 기초 (FSM 설계)

**목표:** State Pattern이 무엇인지 이해하고 Unity에서 C# 인터페이스로 구현한다.

**학습 개념:**
- FSM(Finite State Machine)이란 무엇인가
- State Pattern vs switch-case 비교
- IState 인터페이스 설계 (`OnEnter`, `OnUpdate`, `OnExit`)
- StateMachine 클래스 설계 (상태 등록, 전환, 실행)

**실습 결과물:**
- `Scripts/StateMachine/IState.cs`
- `Scripts/StateMachine/StateMachine.cs`
- 동작 확인용 최소 씬 (큐브 하나가 색깔 변하며 상태 전환)

**완료 기준:**
- IState 인터페이스를 직접 설명할 수 있다
- StateMachine이 상태를 어떻게 관리하는지 설명할 수 있다

---

## Phase 2: 적 NPC AI (Enemy Agent)

**목표:** State Pattern으로 실제로 움직이는 적 NPC를 만든다.

**상태 구조:**
```
Idle ──────→ Patrol
  ↑              ↓ (플레이어 감지)
  └── Lost ←── Chase ──→ Attack
                              ↓ (체력 0)
                            Dead
```

**학습 개념:**
- NavMeshAgent 연동 (이동)
- Physics.OverlapSphere / Raycast (감지)
- 각 State 클래스 분리 설계
- 상태 전환 조건 (Transition) 구현

**실습 결과물:**
- `Scripts/Enemy/States/` 하위 5개 상태 클래스
- `Scripts/Enemy/EnemyAgent.cs` (StateMachine을 소유하는 컴포넌트)
- 플레이어가 움직이면 적이 반응하는 씬

**완료 기준:**
- 각 상태 전환이 의도대로 작동한다
- 상태 클래스 추가/제거가 EnemyAgent.cs 수정 없이 가능하다

---

## Phase 3: 보스 몬스터 AI (Hierarchical FSM)

**목표:** 페이즈 시스템이 있는 보스를 통해 계층적 FSM을 이해한다.

**상태 구조 (2-레이어):**
```
[상위 FSM] Phase1 (100~60%) → Phase2 (60~30%) → Phase3 (30~0%)

[하위 FSM - Phase 내부]
Idle → Attack → SpecialAttack → Stagger → (repeat)
```

**학습 개념:**
- 계층적 FSM (Hierarchical FSM) 설계
- 상위/하위 StateMachine 분리
- 페이즈 전환 조건 (체력 임계값)
- 각 페이즈별 다른 행동 패턴

**실습 결과물:**
- `Scripts/Boss/BossAgent.cs`
- `Scripts/Boss/Phases/` (Phase1State, Phase2State, Phase3State)
- `Scripts/Boss/Actions/` (Attack, SpecialAttack, Stagger 등)

**완료 기준:**
- 보스 체력이 줄면 자동으로 페이즈가 전환된다
- 각 페이즈에서 다른 공격 패턴이 실행된다

---

## Phase 4: ML-Agents 기초

**목표:** ML-Agents의 핵심 개념을 이해하고 간단한 학습 에이전트를 만든다.

**학습 개념:**
- ML-Agents 아키텍처 (Agent, Brain, Environment)
- Observation 수집 (CollectObservations)
- Action 처리 (OnActionReceived)
- Reward 설계 (AddReward, SetReward)
- Heuristic 모드로 직접 조작 테스트
- 학습 실행 (mlagents-learn)

**실습 결과물:**
- 간단한 Ball Rolling 에이전트 or 커스텀 탐색 에이전트
- `Scripts/MLAgents/SimpleAgent.cs`
- 학습 config yaml 파일

**완료 기준:**
- Agent가 스스로 목표 지점에 도달하도록 학습시킬 수 있다
- Observation / Action / Reward 각각의 역할을 설명할 수 있다

---

## Phase 5: 하이브리드 에이전트 (State Pattern + ML-Agents)

**목표:** State Pattern으로 고수준 전략을 관리하고, 각 상태에서 ML-Agents 정책을 실행한다.

**아키텍처:**
```
[StateMachine - 고수준 전략]
    ExploreState   → ML-Agents 탐색 정책 실행
    CombatState    → ML-Agents 전투 정책 실행  
    RetreatState   → 룰 기반 도주 로직 (ML 없이)
    
[ML-Agents - 저수준 행동]
    각 State가 독립적인 Brain 또는 공유 Brain 사용
```

**학습 개념:**
- State별 독립 정책 vs 공유 정책 트레이드오프
- StateMachine 전환 조건을 Observation으로 활용
- 룰 기반 + 학습 기반 혼합 설계

**실습 결과물:**
- `Scripts/Hybrid/HybridAgent.cs`
- `Scripts/Hybrid/States/` (ExploreState, CombatState, RetreatState)
- 멀티 에이전트 씬 (적 + 하이브리드 에이전트 대결)

**완료 기준:**
- 상황에 따라 State가 전환되며 ML 정책이 교체된다
- Phase 2 Enemy와 Phase 5 Hybrid가 대결하는 씬이 동작한다

---

## 참고 사항

- 각 Phase는 별도 씬(Scene)으로 분리하는 것을 권장
- Scripts 폴더 구조는 `docs/learning/progress.md`에서 관리
- 이해가 부족한 개념은 `docs/learning/weak-points.md`에 기록
