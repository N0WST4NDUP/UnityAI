# UnityAI

## Branches
- `main`
- `RaySensorTutorial`
- \***`CommanderAgent`**

## Installed Packages

| Package | Version | Publisher |
|---------|---------|-----------|
| ML-Agents | 4.0.2 | Unity Technologies |
| Assistant | 2.4.0-preview.1 | Unity Technologies |

## Summary

ML-Agents 기반 탑다운 RTS 스타일 Commander 에이전트 구현.
Commander가 유닛들의 상태를 관찰하고 GoalZone으로 이동 명령을 내려 목표를 달성한다.

- Commander가 VectorSensor로 유닛별 거리·속도를 관찰
- 에피소드마다 GoalZone 위치·크기·필요 인원 수가 랜덤으로 변경
- GoalZone 안으로 N마리의 유닛을 이동시키면 성공
- 제한 시간 30초 초과 시 실패

## Architecture

| 스크립트 | 역할 |
|----------|------|
| `CommanderAgent` | ML-Agents 에이전트. 관찰 수집·행동 실행·보상 계산 |
| `UnitManager` | 유닛 풀 관리, ETA 기준 정렬 후 명령 대상 선정 |
| `Unit` | Rigidbody 기반 이동, 타겟 직선 추적 + Slerp 회전 |
| `GoalZone` | Trigger Collider로 유닛 진입 감지, 에피소드마다 랜덤 초기화 |
| `TrainManager` | 실시간 학습 통계 UI (에피소드 수, 성공률, 평균 보상 등) |

## Observation

### GridSensor (Inspector 컴포넌트)

| 항목 | 값 |
|------|----|
| 그리드 크기 | 49 × 49 × 1 |
| 셀 크기 | 1 × 0.01 × 1 (월드 단위) |
| 감지 태그 | `Unit`, `GoalZone` |
| Agent 회전 따라가기 | 비활성 (절대 좌표계) |

맵 전체를 격자로 관찰해 유닛과 GoalZone의 위치를 파악한다.

### VectorSensor (CollectObservations 코드)

| 관찰값 | 정규화 방법 | 크기 |
|--------|-------------|------|
| 목표 인원 수 | `UnitsNeeded / MaxCapability` | 1 |
| 유닛별 GoalZone까지 거리 | `dist / 72f` (맵 대각선) | MaxCapability (10) |
| 유닛별 이동 속도 | `MoveSpeed / 10f` | MaxCapability (10) |

총 벡터 관찰 크기: **21** (없는 유닛 슬롯은 0 패딩)

## Action (Discrete, Branch 1개)

| Branch | 의미 | 범위 |
|--------|------|------|
| 0 | 몇 명에게 명령할지 | 0 ~ `UnitsNeeded - 1` |

- ETA(도착 예상 시간) 가장 짧은 N명에게만 GoalZone 이동 명령
- GoalZone 안에 이미 있는 유닛은 대상에서 제외

## Reward

| 조건 | 보상 |
|------|------|
| 매 스텝, GoalZone 안 유닛 비율 | `UnitsInside / Capability × 0.001` |
| 에피소드 성공 (N마리 달성) | `+1.0` |
| 시간 초과 (30초) | `-0.5` |

## Training Condition

- 에피소드 제한 시간: **30초**
- GoalZone 필요 인원: 에피소드마다 `Random.Range(1, Capability + 1)` 랜덤
- GoalZone 크기: 에피소드마다 `0.5 ~ 1.0` 랜덤
- 유닛 속도: 생성 시 `[MaxSpeed/2, MaxSpeed]` 범위 랜덤 배정
- 유닛 태그: `Unit` / GoalZone 태그: `GoalZone`