---
title: 취약 개념 및 보완 기록
last_updated: 2026-04-12
---

# 취약 개념 및 보완 기록

세션 중 이해가 부족하거나 반복적으로 혼동한 개념을 기록합니다.
다음 세션 시작 시 이 파일을 먼저 검토하여 복습 여부를 확인합니다.

---

## 기록 방법

```
### [Phase X] 개념명
- **발견일:** YYYY-MM-DD
- **상황:** 어떤 맥락에서 막혔는지
- **증상:** 무엇을 헷갈렸는지
- **보완 설명:** 해결된 설명
- **상태:** 미해결 / 부분이해 / 해결
```

---

## Phase 1: State Pattern 기초

### [Phase 1] OnExit의 역할
- **발견일:** 2026-04-12
- **상황:** IState의 3개 메서드 역할을 설명하는 중
- **증상:** OnEnter(초기화), OnUpdate(실행)는 직관적으로 이해했지만, OnExit가 구체적으로 왜 필요한지 몰랐음
- **보완 설명:** 상태 전환 시 이전 상태가 켜둔 것들(콜라이더, 애니메이션 등)을 정리해야 하는 책임. "뒷정리"
- **상태:** 해결

### [Phase 1] 상태 간 의존성 혼동
- **발견일:** 2026-04-12
- **상황:** State가 StateMachine을 캐싱하는 구조를 보고
- **증상:** "상태끼리 독립적이어야 하는데 StateMachine을 통해 서로 의존하는 것 아닌가?" 혼동
- **보완 설명:** State는 StateMachine만 알고, 다른 State의 존재는 모름. StateMachine에 대한 의존은 불가피하며 올바른 설계. "상태끼리의 독립성"과 "StateMachine에 대한 의존"은 다른 개념
- **상태:** 해결

---

## Phase 2: 적 NPC AI

### [Phase 2] 후위 증가 연산자 혼동
- **발견일:** 2026-04-15
- **상황:** `NextWaypoint()`에서 `_patrolIndex = _patrolIndex++ % _waypoints.Length` 작성
- **증상:** `++`를 다른 연산과 섞어 쓰면 후위 증가라 반환 후 증가가 일어나 원래 값으로 덮어써짐
- **보완 설명:** `_patrolIndex = (_patrolIndex + 1) % _waypoints.Length`처럼 명시적으로 +1. `++`는 단독으로만 사용
- **상태:** 해결

### [Phase 2] remainingDistance vs Vector3.Distance
- **발견일:** 2026-04-15
- **상황:** ChaseState에서 감지 범위 이탈 판정에 `remainingDistance` 사용
- **증상:** `remainingDistance`는 NavMesh 경로 거리(우회 포함), 감지는 시야 개념이라 직선 거리가 맞음
- **보완 설명:** 이동 도착 판정 → `remainingDistance`, 감지/공격 범위 판정 → `Vector3.Distance`
- **상태:** 해결

### [Phase 2] pathPending 미고려
- **발견일:** 2026-04-15
- **상황:** PatrolState에서 `SetDestination` 직후 `remainingDistance` 체크
- **증상:** 경로 계산 중(`pathPending=true`)일 때 `remainingDistance`가 0을 반환해 즉시 도착 판정 발생 가능
- **보완 설명:** `!_enemy.Agent.pathPending &&` 조건 추가로 경로 계산 완료 후에만 체크
- **상태:** 해결

---

## Phase 3: 보스 몬스터 AI

_아직 학습 시작 전 - 기록 없음_

---

## Phase 4: ML-Agents 기초

_아직 학습 시작 전 - 기록 없음_

---

## Phase 5: 하이브리드 에이전트

_아직 학습 시작 전 - 기록 없음_

---

## 반복 혼동 패턴

_(3회 이상 같은 개념에서 막히면 여기 별도 기록)_

_아직 없음_
