# ARCHITECTURE.md

Unity 컴포넌트 / C# 클래스 책임 / ML-Agents 클래스 상속 구조.

## 디렉토리 구조 (목표)

```
Assets/Scripts/
├── Grid/
│   ├── GridField.cs           # 2D 셀 배열 + World↔Grid 변환
│   └── GridCell.cs            # 셀 상태 enum + 내구도
├── Units/
│   ├── Unit.cs                # 유닛 베이스 (HP, 직군, 소속 팀)
│   ├── UnitMover.cs           # Grid 기반 이동
│   └── UnitPool.cs            # 최대 유닛 수 고정 풀
├── Combat/
│   ├── AttackSystem.cs
│   ├── SkillSystem.cs
│   └── DamageSystem.cs
├── Structures/
│   ├── Structure.cs           # 내구도 보유 셀
│   └── StructurePlacer.cs     # 준비 페이즈 배치
├── Items/
│   ├── Barricade.cs
│   └── Cannon.cs              # 위험 셀 + 잔여 틱
├── Agents/
│   ├── UnitAgent.cs           # : Agent (ML-Agents)
│   ├── TankAgent.cs           # : UnitAgent
│   ├── DealerAgent.cs         # : UnitAgent
│   ├── MageAgent.cs           # : UnitAgent
│   ├── CommanderAgent.cs      # : Agent (GridSensor)
│   ├── CommandBus.cs          # Commander → Unit 명령 브로드캐스트
│   └── Rewards/               # 직군별 reward 함수
├── Synergies/
│   └── SynergyFlags.cs
├── Training/
│   ├── WeightUpdater.cs
│   └── PolicyLoader.cs        # ONNX 로드
└── UI/
    └── ItemPanel.cs
```

## 셀 상태 enum

```csharp
public enum CellState : byte {
    Empty     = 0,
    Structure = 1,
    Occupied  = 2,
    Danger    = 3,
}
```

## 클래스 책임 요약

| 클래스 | 책임 | 의존 |
|--------|------|------|
| `GridField` | 전체 셀 상태 관리, 좌표 변환 | - |
| `Unit` | HP/팀/직군 보유, 셀 점유 등록 | GridField |
| `UnitMover` | BFS/A* 경로 탐색, 셀 점유 갱신 | GridField, Unit |
| `Structure` | 내구도, 파괴 시 셀 상태 0 복귀 | GridField |
| `StructurePlacer` | 준비 페이즈 설치 검증 | GridField |
| `UnitAgent` | 관측/행동/보상 루프 | Unit, SynergyFlags |
| `CommanderAgent` | 전장 관측, 복합 명령 출력 | GridField, CommandBus |
| `CommandBus` | Commander 명령 → 해당 Unit observation 추가 | UnitAgent |
| `PolicyLoader` | 런타임 ONNX 교체 | - |

## ML-Agents 상속 구조

```
Unity.MLAgents.Agent
├── UnitAgent (abstract)
│   ├── TankAgent
│   ├── DealerAgent
│   └── MageAgent
└── CommanderAgent
```

- 직군별 Agent는 **같은 BehaviorName 공유** → 공유 Policy 방식.
- CommanderAgent는 별도 BehaviorName.

## 데이터 흐름 (전투 1틱)

```
GridField 스냅샷
    ↓
CommanderAgent.CollectObservations (GridSensor)
    ↓
Commander action (복합 명령)
    ↓ CommandBus
UnitAgent.CollectObservations (자기 상태 + 주변 셀 + 명령 flag + 시너지)
    ↓
Unit action (이동/공격/스킬)
    ↓
GridField 상태 갱신 + Reward 계산
```

## 변경 규칙
- 셀 상태 enum 값 수정 시 `docs/OBSERVATIONS.md` 채널 맵 동기화 필수.
- Agent observation 크기 변경 시 기존 ONNX 폐기 → 재학습 필요.
