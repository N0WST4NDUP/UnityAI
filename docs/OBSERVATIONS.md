# OBSERVATIONS.md

Commander / Unit Agent별 VectorSensor 인덱스 맵 + GridSensor 채널 설정.

---

## UnitAgent (공통 베이스)

### VectorSensor (고정 크기)

| Index | 이름 | 크기 | 범위 | 설명 |
|-------|------|------|------|------|
| 0 | self_hp_norm | 1 | [0,1] | 현재 HP / 최대 HP |
| 1 | self_pos_x | 1 | [0,1] | Grid X / W |
| 2 | self_pos_y | 1 | [0,1] | Grid Y / H |
| 3 | role_flag | 3 | one-hot | 탱커/딜러/마법사 |
| 6 | team_flag | 1 | {0,1} | 0=아군 / 1=적군 관점(self-play) |
| 7 | attacker_flag | 1 | {0,1} | 이번 라운드 공성 측 |
| 8 | defender_flag | 1 | {0,1} | 수성 측 |
| 9 | synergy_offensive | 1 | {0,1} | 공격적 시너지 |
| 10 | synergy_defensive | 1 | {0,1} | 수비적 시너지 |
| 11 | cmd_assault | 1 | {0,1} | Commander 명령: 총공세 |
| 12 | cmd_retreat | 1 | {0,1} | 후퇴 |
| 13 | cmd_reposition | 2 | [-1,1]² | 포지션 조정 (dx,dy) |
| 15 | cmd_target_focus | 1 | [0,1] | 타겟 포커싱 우선도 |
| 16 | cooldowns | 3 | [0,1] | 스킬별 쿨다운 |
| 19 | nearest_ally_dist | 1 | [0,1] | 최근접 아군 거리 |
| 20 | nearest_enemy_dist | 1 | [0,1] | 최근접 적 거리 |

합계: **21 floats** (직군별 서브클래스가 확장 가능)

### GridSensor (주변 n×n, 자기 중심)

| 채널 | 의미 |
|------|------|
| 0 | 빈 공간 (1=empty) |
| 1 | 구조물 |
| 2 | 아군 유닛 점유 |
| 3 | 적군 유닛 점유 |
| 4 | 위험 셀 (포탄 예고) |
| 5 | 위험 셀 잔여 틱 / max_tick |

크기: **7×7 (자기 중심), 6채널**

---

## TankAgent / DealerAgent / MageAgent 확장

### TankAgent 추가 VectorSensor
| 이름 | 설명 |
|------|------|
| current_aggro | 어그로 누적량 / max |

### DealerAgent 추가 VectorSensor
| 이름 | 설명 |
|------|------|
| target_priority_vec | 가장 위협적인 적 3명의 상대 좌표 + HP |

### MageAgent 추가 VectorSensor
| 이름 | 설명 |
|------|------|
| best_aoe_center | 최적 AoE 중심 후보 (dx,dy) |
| aoe_cluster_size | 후보 범위 내 적 수 |

---

## CommanderAgent

### GridSensor (전장 전체)

| 설정 | 값 |
|------|----|
| Grid 크기 | 16 × 16 (필드 크기와 동일) |
| Cell Scale | 1 |
| 채널 수 | 7 |

| 채널 | 의미 |
|------|------|
| 0 | 아군 유닛 존재 |
| 1 | 아군 유닛 HP 비율 |
| 2 | 적군 유닛 존재 |
| 3 | 적군 유닛 HP 비율 |
| 4 | 구조물 (내구도 정규화) |
| 5 | 위험 셀 존재 |
| 6 | 위험 셀 잔여 틱 / max |

### VectorSensor

| Index | 이름 | 크기 | 설명 |
|-------|------|------|------|
| 0 | attacker_flag | 1 | 공성 측 여부 |
| 1 | defender_flag | 1 | 수성 측 여부 |
| 2 | synergy_vec | 4 | 현재 시너지 one-hot (확장 여지) |
| 6 | alive_count_ally | 1 | 아군 생존 수 / max |
| 7 | alive_count_enemy | 1 | 적군 생존 수 / max |
| 8 | round_progress | 1 | 라운드 경과 시간 비율 |
| 9 | enemy_pattern_history | 8 | 직전 라운드 행동 경향 임베딩 |

합계: **17 floats**

### Action Space (Commander)

| Branch | 크기 | 의미 |
|--------|------|------|
| 0 | 3 | 전면 전략 (총공세 / 유지 / 후퇴) |
| 1 | N_units+1 | 포커싱 타겟 유닛 인덱스 (없음 포함) |
| 2 | 4 | 포지션 조정 방향 (N/S/E/W/none) |

Multi-discrete 복합 명령 → CommandBus가 분기.

---

## 주의사항
- **Unit VectorSensor에 Commander 명령 slot이 포함되어 있으므로 Phase 2에선 zero로 고정 후 Phase 3 연결 시 활성화.** 크기 자체는 Phase 2부터 확보해 둔다 (재학습 방지).
- GridSensor 채널 수는 Phase 4 위험 셀 추가 시 이미 반영되어 있다.
- 유닛 수 상한 `N_units`는 한 팀 최대 8 기준 (`2팀=16`). 변경 시 Commander action branch 크기 재설정.
