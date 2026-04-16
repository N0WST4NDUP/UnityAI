# ARCHITECTURE.md

Unity 컴포넌트 / C# 클래스 책임 / ML-Agents 클래스 상속 구조.

## 디렉토리 구조 (목표)

```
Assets/Scripts/
├── Grid/
│   ├── GridField.cs           # 2D 셀 배열 + World↔Grid 변환 (준비 페이즈 전용)
│   └── GridCell.cs            # 셀 상태 enum
├── Units/
│   ├── Unit.cs                # 유닛 베이스 (HP, 직군, 소속 팀)
│   ├── UnitMovement.cs        # NavMeshAgent 기반 이동
│   └── UnitPool.cs            # 최대 유닛 수 고정 풀
├── Combat/
│   ├── AttackSystem.cs
│   ├── SkillSystem.cs
│   └── DamageSystem.cs
├── Structures/
│   ├── Structure.cs           # 내구도 + NavMeshObstacle
│   └── StructurePlacer.cs     # 준비 페이즈 배치
├── Terrain/
│   └── TerrainEditor.cs       # 준비 페이즈 지형 높낮이 편집
├── Phase/
│   ├── PhaseManager.cs        # 준비↔전투 페이즈 전환 관리
│   └── NavMeshBaker.cs        # 전투 시작 시 NavMesh Bake
├── Items/
│   ├── Barricade.cs           # 자유 좌표 + NavMeshObstacle(Carve)
│   └── Bombardment.cs         # AoE 범위 타겟팅 + 낙하 예고
├── Agents/
│   ├── UnitAgent.cs           # : Agent (ML-Agents)
│   ├── TankAgent.cs           # : UnitAgent
│   ├── DealerAgent.cs         # : UnitAgent
│   ├── MageAgent.cs           # : UnitAgent
│   ├── CommanderAgent.cs      # : Agent
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

## 셀 상태 enum (준비 페이즈 전용)

```csharp
public enum CellState : byte {
    Empty     = 0,   // 배치 가능
    Structure = 1,   // 구조물 점유 (배치 불가)
    Occupied  = 2,   // 유닛 점유 (배치 불가)
}
```

> 기존의 `Danger = 3`은 제거. 전투 중 위험 영역은 Grid가 아닌 월드 좌표 AoE로 처리.

## 클래스 책임 요약

| 클래스 | 책임 | 의존 |
|--------|------|------|
| `GridField` | 준비 페이즈 셀 상태 관리, 좌표 변환 | - |
| `PhaseManager` | 준비↔전투 페이즈 전환, NavMesh Bake 트리거 | NavMeshBaker, GridField |
| `NavMeshBaker` | 전투 시작 시 NavMeshSurface.BuildNavMesh() | NavMeshSurface |
| `Unit` | HP/팀/직군 보유 | - |
| `UnitMovement` | NavMeshAgent 기반 이동 실행 | NavMeshAgent, Unit |
| `Structure` | 내구도 + NavMeshObstacle 관리, 파괴 시 Obstacle 제거 | NavMeshObstacle |
| `StructurePlacer` | 준비 페이즈 설치 검증 | GridField |
| `TerrainEditor` | 준비 페이즈 지형 높낮이 편집 | - |
| `Barricade` | 전투 중 임시 장애물, NavMeshObstacle(Carve) | NavMeshObstacle |
| `Bombardment` | 전투 중 AoE 범위 피해, 낙하 예고 시각 표시 | - |
| `UnitAgent` | 관측/행동/보상 루프 | Unit, UnitMovement |
| `CommanderAgent` | 전장 관측, 복합 명령 출력 | CommandBus |
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
전장 상태 스냅샷 (유닛 위치 + 구조물 + 지형)
    ↓
CommanderAgent.CollectObservations (전장 전체 관측)
    ↓
Commander action (전략 명령)
    ↓ CommandBus
UnitAgent.CollectObservations (자기 상태 + 주변 인식 + 명령 flag + 시너지)
    ↓
Unit action (이동 방향 / 공격 대상 / 스킬 사용 / 구조물 공격)
    ↓
NavMeshAgent 이동 실행 + 전투 처리
    ↓
보상 계산 + 상태 갱신
```

## 듀얼 페이즈 전환 흐름

```
[준비 페이즈]
Grid 활성 / NavMesh 비활성
유닛: Grid 셀에 고정
구조물: Grid 셀에 배치
지형: 높낮이 편집 가능

        ↓ PhaseManager.StartCombat()

[전환]
1. 지형/구조물 확정
2. NavMeshSurface.BuildNavMesh()
3. 유닛 Grid 좌표 → World 좌표
4. NavMeshAgent 활성화
5. 구조물 NavMeshObstacle(Carve) 활성화

        ↓

[전투 페이즈]
Grid 비활성 / NavMesh 활성
유닛: 자유 이동 (NavMeshAgent)
구조물: NavMeshObstacle (파괴 시 제거 → NavMesh 복구)
지형: 변경 불가
```

## 변경 규칙

- 셀 상태 enum 값 수정 시 기존 코드 전수 검토.
- Agent observation 크기 변경 시 기존 ONNX 폐기 → 재학습 필요.
- NavMesh 관련 설정 변경 시 Bake 타이밍 재검증 필요.
