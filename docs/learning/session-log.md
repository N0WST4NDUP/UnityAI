---
title: 세션 로그
last_updated: 2026-04-12
---

# 세션 로그


각 학습 세션의 진행 내용, 결정 사항, 다음 세션 시작점을 기록합니다.

---

## 세션 로그 형식

```
## Session YYYY-MM-DD

**시작 Phase/단계:**
**종료 Phase/단계:**

**진행 내용:**
- 

**결정 사항:**
- 

**다음 세션 시작점:**
> 

**특이사항:**
-
```

---

## Session 2026-04-12 (2회차)

**시작 Phase/단계:** Phase 1 — IState 인터페이스 설계

**종료 Phase/단계:** Phase 1 — 색깔 전환 테스트 동작 확인

**진행 내용:**
- Enum 방식의 문제점 직접 도출 (Update 비대화, 상태 추가 시 기존 코드 수정)
- State Pattern 핵심 개념 이해: 상태를 객체로 만들어 행동을 분리
- IState 인터페이스 작성 (OnEnter/OnUpdate/OnExit)
- StateMachine 순수 C# 클래스로 구현 (Tick/ChangeState)
- IdleState, ActiveState 작성 (스페이스바로 색깔 전환)
- TestAgent에서 StateMachine 소유 및 초기 상태 설정
- Unity 씬에서 동작 확인 완료

**결정 사항:**
- StateMachine은 MonoBehaviour 아닌 순수 C# 클래스로 유지
- 각 State는 생성자에서 필요한 것만 주입받는 방식

**다음 세션 시작점:**
> Phase 1 완료. Phase 2 (적 NPC AI) 시작 — NavMesh 개념 설명 및 EnemyAgent 설계부터

**특이사항:**
- 없음

---

## Session 2026-04-12

**시작 Phase/단계:** 학습 환경 세팅

**종료 Phase/단계:** CLAUDE.md 및 학습 문서 구조 완성

**진행 내용:**
- 학습 방향 설정: State Pattern 기초 → 적 NPC → 보스 AI → ML-Agents → 하이브리드 통합
- CLAUDE.md 작성 (세션 간 컨텍스트 유지용)
- curriculum.md, progress.md, weak-points.md, session-log.md 생성

**결정 사항:**
- 학습은 Phase 1부터 순서대로 진행
- 각 Phase는 별도 Unity 씬으로 분리
- 이해 부족 시 weak-points.md에 즉시 기록

**다음 세션 시작점:**
> Phase 1 시작: IState 인터페이스와 StateMachine 클래스 설계부터 시작

**특이사항:**
- Unity ML-Agents 4.0.2 이미 설치되어 있음 (Phase 4에서 별도 설치 불필요)
- 브랜치: StatePatternAgent
