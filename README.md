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
스타크래프트식 총관리자 구조로, Commander가 유닛들을 관찰하고 이동 명령을 내려 목표를 달성한다.

- Commander가 GridSensor로 맵 전체를 관찰
- GoalZone 안으로 N마리의 유닛을 이동시키면 성공
- 제한 시간 10초 초과 시 실패

## Observation

| 센서 | 내용 |
|------|------|
| `GridSensor` | 맵 전체 공간 감지 |
| `VectorSensor` | 목표 유닛 수 |

## Action (MultiDiscrete)

| Branch | 의미 | 범위 |
|--------|------|------|
| 0 | 유닛 선택 | 0 ~ 9 |
| 1 | 명령 타입 | 0 = 이동 (확장 가능) |
| 2 | 목표 위치 | GoalZone 중심 or 그리드 인덱스 |

## Reward

| 조건 | 보상 |
|------|------|
| 매 스텝, GoalZone 안 유닛 1마리당 | `+0.01` |
| 에피소드 성공 (N마리 달성) | `+1.0` |
| 시간 초과 | `-0.5` |


## Training Condition

- 에피소드 제한 시간: 10초
- 유닛 및 GoalZone에 **Collider** 필수 (GridSensor 감지 조건)
- 유닛 태그: `Unit`
- GoalZone 태그: `GoalZone`
- Commander Agent에 `GridSensorComponent` + `VectorSensorComponent` 부착

## Training
[]

## Result
[]