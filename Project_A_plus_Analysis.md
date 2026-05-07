> 2. Analysis

<div align="center">

# PROJECT A+

(이미지: Project A+ 대표 커버 이미지 필요 - 저해상도 픽셀 아트 스타일의 2D 플랫포머 캐릭터가 대학 캠퍼스 또는 강의실 배경에서 ‘A+’를 향해 나아가는 장면)

**22313535 류재민**  
**reel7722@gmail.com**

</div>

\pagebreak

[ Revision history ]

| Revision date | Version # | Description | Author |
| :--- | :--- | :--- | :--- |
| 2026/03/26 | 0.01 | First Write Documentation | 류재민 |
| 2026/03/27 | 0.02 | Second Write & Fix | 류재민 |
| 2026/05/07 | 0.10 | First Analysis document | 류재민 |
|  |  |  |  |

\pagebreak

= Contents =

1. Introduction ...........................................................................................

2. Use case analysis ................................................................................

3. Domain analysis ...................................................................................

4. User Interface prototype .......................................................................

5. Glossary ...............................................................................................

6. References ...........................................................................................

\pagebreak

# 1. Introduction

## 1. Summary

최근의 게임 산업은 하이엔드급 하드웨어를 기반으로 한 화려한 3D 그래픽과 방대한 서사 구조를 지향하는 경향이 뚜렷하다. 그러나 이러한 흐름은 때로 게임 본연의 가치인 즉각적인 조작의 즐거움과 직관적인 성취감을 약화시키는 요소로 작용하기도 한다. 이에 본 프로젝트는 복잡한 시스템보다 명확한 성장 체계와 경쾌한 조작감을 중심으로 한 2D 액션 플랫포머 게임 **Project A+**를 제작하고자 한다.

**Project A+**는 대학생이 학기 중 경험하는 학점 취득 과정, 과제 부담, 시험 스트레스 등을 게임의 전투 및 성장 시스템으로 치환한 게임이다. ‘수업’은 스테이지, ‘과제’는 엘리트 몬스터, ‘중간고사와 기말고사’는 보스 캐릭터로 구성된다. 또한 ‘공부량’은 공격력, ‘멘탈’은 체력, ‘최종 성적’은 점수 및 랭크로 표현하여 대학 생활의 심리적 압박을 게임적 목표와 보상 구조로 재해석한다.

시각적으로는 복잡한 렌더링 대신 저해상도 픽셀 아트, 즉 도트 그래픽을 사용한다. 이를 통해 직관적인 화면 구성과 빠른 피드백을 제공하며, 물리 엔진 기반의 이동과 점프를 통해 플랫포머 장르 특유의 경쾌한 플레이 경험을 구현한다.

## 2. Introduce “Project A+”

이번에 제작할 게임 **Project A+**는 Unity 엔진과 Windows 플랫폼을 기반으로 하는 2D 액션 플랫포머 게임이다. 플레이어는 한 학기의 흐름을 따라 총 10개의 스테이지를 진행하며, 각 스테이지에서 적대적 오브젝트를 처치하고 경험치와 아이템을 획득한다. 축적된 성장 자원은 능력치 포인트로 전환되어 플레이어의 ‘공부량’, ‘멘탈’, 이동 효율, 아이템 활용 능력 등을 강화하는 데 사용된다.

게임의 핵심 루프는 다음과 같다.

1. 학기 시작 버튼을 통해 게임에 진입한다.
2. 플레이어 캐릭터를 조작하여 수업 스테이지를 진행한다.
3. 과제 몬스터와 시험 보스 캐릭터를 처치한다.
4. 경험치와 아이템을 획득한다.
5. 능력치를 분배하여 캐릭터를 성장시킨다.
6. 10개의 스테이지 점수를 종합하여 최종 A+ 획득을 목표로 한다.

이러한 구조를 통해 현실의 학업 부담을 단순한 스트레스가 아니라 극복 가능한 게임적 도전으로 전환하는 것이 본 프로젝트의 핵심 방향이다.

## 3. Goal

이번 Analysis 보고서에서는 **Project A+**가 어떤 방식으로 진행되고 동작하는지 설명하기 위해 Use case analysis, Domain analysis, User Interface prototype을 중심으로 분석한다. 해당 보고서를 읽고 나면 플레이어가 게임을 시작하고, 캐릭터를 조작하고, 능력치를 분배하고, 아이템 및 경험치를 획득하는 흐름이 시스템 내부에서 어떻게 처리되는지 이해할 수 있다.

본 프로젝트의 구체적인 목표는 다음과 같다.

- 총 10개의 스테이지를 설계하고 각 스테이지 클리어 점수를 종합하여 최종 A+를 획득하는 성취 시스템을 완성한다.
- 일반 몬스터, 엘리트 몬스터, 중간고사 및 기말고사 보스를 통해 전투의 긴장감을 확보한다.
- 공부량, 멘탈, 현재 학점, 경험치, 아이템 등 핵심 정보를 직관적으로 보여주는 UI를 제공한다.
- 조작 지연을 최소화하여 2D 플랫포머 장르 특유의 즉각적인 입력 반응성을 구현한다.
- 도트 그래픽 환경에서도 정보 가독성과 테마 전달력이 유지되도록 화면을 구성한다.

## 4. Problem to solve

**Project A+**는 실시간 액션 게임이기 때문에 입력 반응성, 충돌 판정, 능력치 밸런스, UI 가독성이 특히 중요하다. 플레이어의 점프 및 공격 입력이 즉각적으로 반영되지 않거나 몬스터 및 보스 패턴의 판정이 부정확하면 게임 몰입도가 저하될 수 있다. 또한 능력치 상승폭과 스테이지 난이도 곡선이 균형을 이루지 못하면 최종 A+ 획득이 지나치게 쉽거나 불가능하게 느껴질 수 있다.

따라서 본 프로젝트에서는 다음과 같은 문제를 중점적으로 해결해야 한다.

- 실시간 입력 반응성과 충돌 판정의 정교함
- 성장 시스템의 밸런스 및 데이터 유지
- 도트 그래픽 환경에서 현재 학점, 멘탈, 경험치 등 UI 정보의 가독성 확보

\pagebreak

# 2. Use case analysis

## 1. System context diagram

(이미지: System context diagram 필요 - Player가 Project A+ Game Client와 상호작용하고, Game Client가 Unity Engine, Save Data, UI System, Input System, Stage/Battle System과 연결되는 구조)

Player는 게임 클라이언트에서 제공하는 입력, 전투, 성장, UI 기능을 사용한다. 본 프로젝트는 독립 실행형 2D 플랫포머 게임이므로 외부 서버를 핵심 actor로 설정하지 않는다. 대신 Unity Engine 내부의 Scene Loader, Rigidbody2D 기반 물리 처리, UI Manager, Save Data Manager 등이 게임 클라이언트 내부 구성 요소로 동작한다.

## 2. Use case diagram

(이미지: Use case diagram 필요 - Actor는 Player 1명, 시스템 경계는 Project A+ Game Client, Use case는 Start Game, Control Character, Pause & Setting, Dividing Ability Points, Use Item, Get Exp & Item으로 구성)

Player 입장에서 시스템이 제공하는 기능을 작성하였다. 본 프로젝트의 핵심 actor는 Player이다. 플레이어는 게임 시작, 캐릭터 조작, 일시정지 및 설정, 능력치 분배, 아이템 사용, 경험치 및 아이템 획득 기능을 수행한다. 각 use case는 독립적인 기능처럼 보이지만 실제 게임 진행 중에는 서로 연결된다. 예를 들어 Get Exp & Item을 통해 성장 자원을 획득한 후 Dividing Ability Points 기능을 통해 능력치를 강화할 수 있으며, 강화된 능력치는 Control Character와 전투 흐름에 영향을 준다.

## 3. Use case description

### Use case #1 : Start Game

#### GENERAL CHARACTERISTICS

| Item | Description |
| :--- | :--- |
| Summary | 플레이어가 게임을 실행하고 Project A+의 초기 화면에서 게임 세션을 시작하기 위한 기능 |
| Scope | Project A+ |
| Level | User level |
| Author | 류재민 |
| Last Update | 2026-03-27 |
| Status | Analysis |
| Primary Actor | Player |
| Preconditions | 게임 실행 파일이 정상적으로 실행되어야 하며, 필요한 리소스가 로드 가능한 상태여야 한다. |
| Trigger | Player가 메인 타이틀 화면에서 ‘학기 시작’ 또는 Start Game 버튼을 누를 때 |
| Success Post Condition | 게임 씬 또는 스테이지 선택 화면으로 정상 이동하고, 플레이어 초기 데이터가 설정된다. |
| Failed Post Condition | 씬 이동에 실패하거나 플레이어 초기 데이터가 정상적으로 설정되지 않는다. |

#### MAIN SUCCESS SCENARIO

| Step | Action |
| :--- | :--- |
| S | Player가 게임을 시작하고 싶을 때 시작된다. |
| 1 | 시스템은 메인 타이틀 화면을 출력한다. |
| 2 | Player는 ‘학기 시작’ 또는 Start Game 버튼을 누른다. |
| 3 | 시스템은 Scene Loader를 호출하여 초기 씬 또는 스테이지 선택 씬을 로드한다. |
| 4 | 시스템은 플레이어 이름, 기본 멘탈, 기본 공부량, 현재 학기 진행 정보를 초기화한다. |
| 5 | 이 use case는 Player가 정상적으로 게임 진입 화면에 도달하면 끝난다. |

#### EXTENSION SCENARIOS

| Step | Branching Action |
| :--- | :--- |
| 2a | Player가 버튼을 누르지 않는 경우 메인 화면을 유지한다. |
| 3a | 씬 로드에 실패한 경우 실패 메시지를 출력하고 다시 시도할 수 있도록 한다. |
| 4a | 저장 데이터가 손상되었거나 초기화에 실패한 경우 기본값을 적용하고 안내 문구를 보여준다. |

#### RELATED INFORMATION

| Item | Description |
| :--- | :--- |
| Performance | < 10 Seconds |
| Frequency | 플레이어가 게임을 실행할 때마다 |
| Concurrency | None |
| Due Date |  |

\pagebreak

### Use case #2 : Control Character

#### GENERAL CHARACTERISTICS

| Item | Description |
| :--- | :--- |
| Summary | 플레이어가 캐릭터를 좌우로 이동시키고 점프시켜 스테이지를 진행하기 위한 기능 |
| Scope | Project A+ |
| Level | User level |
| Author | 류재민 |
| Last Update | 2026-03-27 |
| Status | Analysis |
| Primary Actor | Player |
| Preconditions | Player가 게임 씬에 진입해 있고, 조작 가능한 캐릭터가 생성되어 있어야 한다. |
| Trigger | Player가 키보드 입력을 통해 캐릭터를 움직이려고 할 때 |
| Success Post Condition | 캐릭터가 Player의 입력에 따라 이동하거나 점프한다. |
| Failed Post Condition | Player의 입력이 캐릭터 움직임에 반영되지 않는다. |

#### MAIN SUCCESS SCENARIO

| Step | Action |
| :--- | :--- |
| S | Player가 캐릭터를 조작하고 싶을 때 시작된다. |
| 1 | Player는 이동을 위해 A/D 또는 방향키를 누른다. |
| 2 | 시스템은 입력 값을 감지하고 Rigidbody2D 또는 이동 처리 로직에 전달한다. |
| 3 | 캐릭터는 입력 방향에 따라 좌우로 이동한다. |
| 4 | Player는 장애물을 넘기 위해 Space 키를 누른다. |
| 5 | 시스템은 지면 접촉 여부를 확인한 후 점프 힘을 적용한다. |
| 6 | 이 use case는 캐릭터가 입력에 맞게 정상적으로 이동 및 점프하면 끝난다. |

#### EXTENSION SCENARIOS

| Step | Branching Action |
| :--- | :--- |
| 2a | 입력 장치가 비활성화되어 있거나 키 설정이 잘못된 경우 조작 안내 메시지를 출력한다. |
| 5a | 캐릭터가 공중에 있어 점프할 수 없는 경우 점프 입력을 무시한다. |
| 5b | 캐릭터가 피격, 사망, 일시정지 상태인 경우 이동 및 점프 입력을 제한한다. |

#### RELATED INFORMATION

| Item | Description |
| :--- | :--- |
| Performance | < 0.1 Seconds |
| Frequency | Frequent |
| Concurrency | None |
| Due Date |  |

\pagebreak

### Use case #3 : Pause & Setting

#### GENERAL CHARACTERISTICS

| Item | Description |
| :--- | :--- |
| Summary | 플레이어가 게임을 일시정지하고 사운드, 마우스 감도, 키 설정 등 사용자 환경을 조정하기 위한 기능 |
| Scope | Project A+ |
| Level | User level |
| Author | 류재민 |
| Last Update | 2026-03-27 |
| Status | Analysis |
| Primary Actor | Player |
| Preconditions | Player가 게임 씬 또는 진행 가능한 화면에 있어야 한다. |
| Trigger | Player가 ESC 키를 누를 때 |
| Success Post Condition | 게임 진행이 일시정지되고 설정 UI가 활성화된다. |
| Failed Post Condition | 게임이 멈추지 않거나 설정 UI가 열리지 않는다. |

#### MAIN SUCCESS SCENARIO

| Step | Action |
| :--- | :--- |
| S | Player가 게임을 잠시 멈추거나 설정을 변경하고 싶을 때 시작된다. |
| 1 | Player는 ESC 키를 누른다. |
| 2 | 시스템은 Time Scale을 0으로 설정하여 게임 로직을 일시정지한다. |
| 3 | 시스템은 Pause UI와 Setting UI를 화면에 출력한다. |
| 4 | Player는 사운드 크기, 마우스 감도, 키 매핑 등의 설정 값을 변경한다. |
| 5 | 시스템은 변경된 값을 즉시 적용하거나 저장한다. |
| 6 | Player가 다시 ESC 키 또는 Resume 버튼을 누르면 게임이 재개된다. |
| 7 | 이 use case는 게임이 정상적으로 재개되면 끝난다. |

#### EXTENSION SCENARIOS

| Step | Branching Action |
| :--- | :--- |
| 2a | 게임 오버, 로딩, 컷신 등 일시정지가 불가능한 상태이면 ESC 입력을 무시한다. |
| 4a | 잘못된 키 매핑이 입력된 경우 중복 입력 경고를 출력한다. |
| 5a | 설정 저장에 실패한 경우 현재 세션에만 임시 적용하고 안내 문구를 보여준다. |

#### RELATED INFORMATION

| Item | Description |
| :--- | :--- |
| Performance | < 1 Second |
| Frequency | 플레이어가 설정을 변경하거나 일시정지를 원할 때마다 |
| Concurrency | None |
| Due Date |  |

\pagebreak

### Use case #4 : Dividing Ability Points

#### GENERAL CHARACTERISTICS

| Item | Description |
| :--- | :--- |
| Summary | 플레이어가 획득한 성장 포인트를 공부량, 멘탈 등 능력치에 분배하기 위한 기능 |
| Scope | Project A+ |
| Level | User level |
| Author | 류재민 |
| Last Update | 2026-03-27 |
| Status | Analysis |
| Primary Actor | Player |
| Preconditions | Player가 사용 가능한 능력치 포인트를 보유하고 있어야 한다. |
| Trigger | Player가 능력치 창을 열고 포인트 분배 버튼을 누를 때 |
| Success Post Condition | 선택한 능력치가 증가하고 보유 포인트가 감소한다. |
| Failed Post Condition | 능력치가 증가하지 않거나 포인트 수치가 비정상적으로 처리된다. |

#### MAIN SUCCESS SCENARIO

| Step | Action |
| :--- | :--- |
| S | Player가 캐릭터를 성장시키고 싶을 때 시작된다. |
| 1 | Player는 능력치 또는 성장 UI를 연다. |
| 2 | 시스템은 현재 공부량, 멘탈, 보유 포인트, 현재 학점 관련 수치를 표시한다. |
| 3 | Player는 증가시키고 싶은 능력치의 + 버튼을 누른다. |
| 4 | 시스템은 보유 포인트가 충분한지 확인한다. |
| 5 | 시스템은 선택한 능력치를 증가시키고 보유 포인트를 차감한다. |
| 6 | UI Manager는 변경된 능력치와 남은 포인트를 갱신한다. |
| 7 | 이 use case는 능력치 분배가 정상적으로 적용되면 끝난다. |

#### EXTENSION SCENARIOS

| Step | Branching Action |
| :--- | :--- |
| 3a | Player가 잘못된 버튼을 누르거나 분배를 취소하면 기존 수치를 유지한다. |
| 4a | 보유 포인트가 부족한 경우 ‘사용 가능한 포인트가 부족합니다’라는 메시지를 출력한다. |
| 5a | 능력치 최대치에 도달한 경우 더 이상 증가하지 않고 안내 문구를 보여준다. |

#### RELATED INFORMATION

| Item | Description |
| :--- | :--- |
| Performance | < 1 Second |
| Frequency | 레벨업 또는 스테이지 클리어 후 |
| Concurrency | None |
| Due Date |  |

\pagebreak

### Use case #5 : Use Item

#### GENERAL CHARACTERISTICS

| Item | Description |
| :--- | :--- |
| Summary | 플레이어가 인벤토리 또는 퀵슬롯에 있는 아이템을 사용하기 위한 기능 |
| Scope | Project A+ |
| Level | User level |
| Author | 류재민 |
| Last Update | 2026-03-27 |
| Status | Analysis |
| Primary Actor | Player |
| Preconditions | Player가 사용 가능한 아이템을 보유하고 있어야 한다. |
| Trigger | Player가 퀵슬롯 단축키 또는 아이템 사용 버튼을 누를 때 |
| Success Post Condition | 아이템 효과가 적용되고 아이템 수량이 감소한다. |
| Failed Post Condition | 아이템 효과가 적용되지 않거나 수량이 비정상적으로 처리된다. |

#### MAIN SUCCESS SCENARIO

| Step | Action |
| :--- | :--- |
| S | Player가 전투 또는 스테이지 진행 중 아이템을 사용하고 싶을 때 시작된다. |
| 1 | Player는 퀵슬롯 키 또는 아이템 사용 버튼을 누른다. |
| 2 | 시스템은 해당 슬롯에 아이템이 존재하는지 확인한다. |
| 3 | 시스템은 아이템의 효과 타입을 확인한다. |
| 4 | 회복 아이템이면 멘탈 또는 체력 수치를 증가시킨다. |
| 5 | 버프 아이템이면 일정 시간 동안 이동 속도, 공격력, 방어력 등의 계수를 변경한다. |
| 6 | 시스템은 아이템 수량을 1개 차감하고 UI를 갱신한다. |
| 7 | 이 use case는 아이템 효과가 정상적으로 적용되면 끝난다. |

#### EXTENSION SCENARIOS

| Step | Branching Action |
| :--- | :--- |
| 2a | 해당 슬롯에 아이템이 없는 경우 ‘사용할 아이템이 없습니다’라는 메시지를 출력한다. |
| 3a | 아이템이 사용 불가능한 상태이면 사용 입력을 무시한다. |
| 5a | 이미 동일한 버프가 적용 중이면 지속 시간을 갱신하거나 중복 적용을 제한한다. |
| 6a | 수량 차감 처리에 실패하면 아이템 사용을 취소하고 이전 상태로 복구한다. |

#### RELATED INFORMATION

| Item | Description |
| :--- | :--- |
| Performance | < 0.5 Seconds |
| Frequency | Player가 아이템을 사용할 때마다 |
| Concurrency | None |
| Due Date |  |

\pagebreak

### Use case #6 : Get Exp & Item

#### GENERAL CHARACTERISTICS

| Item | Description |
| :--- | :--- |
| Summary | 플레이어가 적대적 오브젝트를 처치하거나 드롭 오브젝트와 충돌하여 경험치 및 아이템을 획득하기 위한 기능 |
| Scope | Project A+ |
| Level | User level |
| Author | 류재민 |
| Last Update | 2026-03-27 |
| Status | Analysis |
| Primary Actor | Player |
| Preconditions | 경험치 또는 아이템을 제공하는 오브젝트가 존재해야 한다. |
| Trigger | 몬스터 처치, 보스 처치, 드롭 오브젝트와 Player의 충돌이 발생할 때 |
| Success Post Condition | 경험치가 증가하거나 아이템이 인벤토리에 추가된다. |
| Failed Post Condition | 보상이 지급되지 않거나 인벤토리 및 경험치 데이터가 비정상적으로 갱신된다. |

#### MAIN SUCCESS SCENARIO

| Step | Action |
| :--- | :--- |
| S | Player가 스테이지 진행 중 보상을 획득할 때 시작된다. |
| 1 | Player는 몬스터, 과제 오브젝트, 시험 보스 등을 처치한다. |
| 2 | 시스템은 처치된 오브젝트의 보상 테이블을 확인한다. |
| 3 | 시스템은 경험치 값을 계산하여 Player의 경험치 데이터에 더한다. |
| 4 | 아이템 드롭 확률이 충족되면 아이템 오브젝트를 생성한다. |
| 5 | Player가 아이템 오브젝트와 충돌하면 시스템은 아이템을 인벤토리에 추가한다. |
| 6 | UI Manager는 경험치 게이지, 아이템 획득 알림, 현재 점수를 갱신한다. |
| 7 | 이 use case는 경험치 또는 아이템 획득이 정상적으로 처리되면 끝난다. |

#### EXTENSION SCENARIOS

| Step | Branching Action |
| :--- | :--- |
| 2a | 보상 테이블이 비어 있는 경우 경험치 또는 아이템을 지급하지 않는다. |
| 4a | 아이템 드롭 확률이 충족되지 않으면 아이템 오브젝트를 생성하지 않는다. |
| 5a | 인벤토리가 가득 찬 경우 ‘인벤토리가 가득 찼습니다’라는 메시지를 출력하고 아이템을 필드에 유지한다. |
| 6a | UI 갱신에 실패해도 내부 데이터는 유지하고 다음 갱신 시점에 다시 반영한다. |

#### RELATED INFORMATION

| Item | Description |
| :--- | :--- |
| Performance | < 1 Second |
| Frequency | 적 처치 또는 아이템 충돌이 발생할 때마다 |
| Concurrency | None |
| Due Date |  |

\pagebreak

# 3. Domain analysis

## 1) GameManager

게임 전체 흐름을 관리하는 클래스이다. 현재 스테이지 번호, 총 스테이지 진행도, 플레이어의 누적 점수, 최종 성적 산출 정보를 관리한다. StageManager, UIManager, SaveDataManager와 연결되어 게임 시작부터 종료까지의 핵심 흐름을 제어한다.

## 2) PlayerController

플레이어 캐릭터의 이동, 점프, 방향 전환, 애니메이션 상태 전이를 담당하는 클래스이다. 키보드 입력을 받아 Rigidbody2D 또는 물리 기반 이동 로직에 전달하며, 캐릭터가 피격, 사망, 일시정지 상태일 경우 입력을 제한한다.

## 3) PlayerStatus

플레이어의 능력치 정보를 저장하는 클래스이다. 공부량, 멘탈, 현재 경험치, 보유 능력치 포인트, 현재 학점 또는 점수와 같은 데이터를 가진다. 전투, 성장, UI 표시의 기준이 되는 핵심 데이터 클래스이다.

## 4) AbilityPointManager

플레이어가 획득한 성장 포인트를 공부량, 멘탈 등 능력치에 분배하는 기능을 담당한다. 포인트 부족 여부, 능력치 최대치 도달 여부, 분배 후 데이터 갱신 여부를 확인한다.

## 5) StageManager

각 수업 스테이지의 진행 상태를 관리하는 클래스이다. 스테이지 시작, 일반 몬스터 배치, 과제 오브젝트 등장, 중간고사 및 기말고사 보스전 진입 조건을 관리한다. 스테이지 클리어 시 GameManager에 점수를 전달한다.

## 6) EnemyController

일반 몬스터 또는 과제 오브젝트의 이동, 공격, 피격, 사망 처리를 담당하는 클래스이다. 몬스터의 체력, 이동 패턴, 공격 범위, 보상 테이블과 연결된다.

## 7) BossController

중간고사와 기말고사 보스 캐릭터의 패턴을 관리하는 클래스이다. 일반 적보다 복잡한 공격 패턴, 페이즈 전환, 특수 공격, 클리어 보상을 처리한다.

## 8) Health

체력을 가진 오브젝트의 현재 체력과 최대 체력을 관리하는 클래스이다. PlayerStatus의 멘탈 수치 또는 Enemy/Boss의 체력 수치와 연결된다. 피격 시 대미지를 계산하고 체력이 0 이하가 되면 사망 또는 실패 처리를 요청한다.

## 9) Item

플레이어가 획득하고 사용할 수 있는 아이템 정보를 가지는 클래스이다. 회복 아이템, 버프 아이템, 점수 보정 아이템 등으로 구분할 수 있으며, 사용 시 적용할 효과 타입과 지속 시간을 가진다.

## 10) InventoryManager

플레이어가 보유한 아이템 목록과 수량을 관리하는 클래스이다. 아이템 획득, 아이템 사용, 퀵슬롯 등록, 인벤토리 가득 참 여부를 처리한다.

## 11) DropManager

몬스터 또는 보스 처치 시 경험치와 아이템 드롭을 계산하는 클래스이다. 드롭 테이블, 확률, 아이템 생성 위치, 경험치 지급량을 관리한다.

## 12) UIManager

플레이어에게 필요한 정보를 화면에 보여주는 클래스이다. 현재 멘탈, 공부량, 경험치, 스테이지 번호, 현재 학점, 아이템 퀵슬롯, 보스 체력바 등을 표시한다. 능력치가 변경되거나 아이템을 획득하면 UI를 즉시 갱신한다.

## 13) PauseSettingManager

ESC 입력을 기반으로 일시정지와 설정창을 관리하는 클래스이다. Time Scale 조절, 사운드 볼륨, 마우스 감도, 키 매핑, Resume 버튼 처리 등을 담당한다.

## 14) SceneLoader

타이틀 화면, 스테이지 선택 화면, 게임 씬, 결과 화면으로 이동하는 씬 전환 기능을 담당한다. Start Game use case와 스테이지 클리어 이후의 화면 이동에 사용된다.

## 15) SaveDataManager

플레이어의 진행도, 설정 값, 최고 점수, 최종 성적 기록을 저장하고 불러오는 클래스이다. 저장 데이터가 손상되었을 경우 기본값을 적용할 수 있어야 한다.

(이미지: Domain class diagram 필요 - GameManager를 중심으로 PlayerController, PlayerStatus, StageManager, EnemyController, BossController, Item, InventoryManager, UIManager, SceneLoader가 연결되는 클래스 구조도)

\pagebreak

# 4. User Interface prototype

## 1. Game Start Screen (Title Scene)

(이미지: 게임 시작 화면 필요 - 도트 그래픽 대학 캠퍼스 배경, 중앙 상단 Project A+ 로고, 가운데 ‘학기 시작’, ‘설정’, ‘종료’ 버튼 배치)

플레이어가 처음 게임을 실행하면 위와 같은 시작 화면이 보인다. 시작 화면은 게임의 주제를 즉시 전달해야 하므로 대학 캠퍼스, 강의실, 책상, 시험지, A+ 문구 등 학업 테마를 시각적으로 포함하는 것이 좋다. Player는 ‘학기 시작’ 버튼을 누르면 스테이지 선택 화면 또는 첫 번째 스테이지로 이동한다.

## 2. Stage Select Screen

(이미지: 스테이지 선택 화면 필요 - 총 10개의 수업 주차 카드 또는 강의실 문 형태의 스테이지 버튼, 잠긴 스테이지와 열린 스테이지가 구분되는 화면)

스테이지 선택 화면에서는 총 10개의 스테이지를 보여준다. 각 스테이지는 학기 주차 또는 수업 단원처럼 구성할 수 있다. 클리어한 스테이지는 점수와 등급을 함께 표시하고, 아직 도달하지 못한 스테이지는 잠금 아이콘으로 표시한다.

## 3. In-game HUD Screen

(이미지: 게임 씬 HUD 화면 필요 - 좌측 상단 멘탈/체력바, 공부량/공격력 수치, 경험치 게이지, 우측 상단 현재 스테이지와 목표, 하단 퀵슬롯, 중앙 캐릭터와 적 배치)

플레이어가 스테이지에 진입하면 게임 씬 화면이 출력된다. 화면 좌측 상단에는 현재 멘탈 상태와 공부량 수치를 보여준다. 하단에는 아이템 퀵슬롯과 경험치 게이지를 배치한다. 화면 우측 상단에는 현재 스테이지 목표, 현재 학점 또는 누적 점수를 표시한다. 도트 그래픽 환경에서는 작은 글자가 뭉개질 수 있으므로 텍스트보다 아이콘과 게이지를 적극적으로 활용하는 구성이 적합하다.

## 4. Ability Point Screen

(이미지: 능력치 분배 화면 필요 - 공부량, 멘탈, 이동 효율, 아이템 활용 능력 등의 능력치 항목과 + 버튼, 남은 포인트 표시)

플레이어가 레벨업하거나 특정 스테이지를 클리어하면 능력치 포인트를 획득한다. 능력치 분배 화면에서는 현재 보유 포인트와 각 능력치의 현재 수치를 보여준다. Player는 + 버튼을 눌러 원하는 능력치를 강화할 수 있다. 능력치 변경 후에는 전투에 어떤 영향을 주는지 간단한 설명을 제공하는 것이 좋다.

## 5. Pause & Setting Screen

(이미지: 일시정지 및 설정 화면 필요 - 반투명한 어두운 배경 위에 Resume, Sound, Mouse Sensitivity, Key Setting, Quit 버튼 배치)

플레이어가 ESC 키를 누르면 게임이 일시정지되고 설정 화면이 열린다. 설정 화면에서는 사운드 크기, 마우스 감도, 키 설정을 조정할 수 있다. Resume 버튼을 누르거나 ESC 키를 다시 누르면 Time Scale이 정상값으로 돌아가며 게임이 재개된다.

## 6. Item Acquisition Screen

(이미지: 아이템 획득 알림 화면 필요 - 몬스터 처치 후 책, 커피, 에너지 드링크, 노트, 참고서 같은 학업 테마 아이템이 드롭되고 ‘아이템 획득’ 팝업이 표시되는 장면)

몬스터나 과제 오브젝트를 처치하면 경험치와 아이템을 획득할 수 있다. 아이템 획득 시 화면 중앙 또는 우측에 간단한 알림을 표시한다. 예를 들어 ‘커피 획득: 일정 시간 이동 속도 증가’, ‘요약 노트 획득: 공부량 증가’처럼 아이템 효과를 짧게 설명하면 플레이어가 보상의 의미를 직관적으로 이해할 수 있다.

## 7. Boss Battle Screen

(이미지: 중간고사/기말고사 보스전 화면 필요 - 거대한 시험지 또는 교수/시험 몬스터 형태의 보스, 상단 보스 체력바, 플레이어 상태 UI가 동시에 보이는 장면)

중간고사와 기말고사 보스전은 Project A+의 핵심 긴장 구간이다. 보스전 화면에서는 보스 체력바를 화면 상단에 크게 표시하고, 플레이어의 멘탈 및 아이템 상태를 명확히 보여주어야 한다. 이펙트가 많아질 경우 UI 가독성이 떨어질 수 있으므로 보스 패턴과 UI 색상 대비를 충분히 확보해야 한다.

## 8. Result Screen

(이미지: 결과 화면 필요 - 최종 점수, 현재 학점, 획득 등급 A+/A/B/C, 클리어 시간, 처치한 과제 수, 사용한 아이템 수가 정리된 화면)

스테이지를 클리어하면 결과 화면을 보여준다. 결과 화면에서는 클리어 점수, 획득 경험치, 사용한 아이템, 남은 멘탈, 현재 학점 변화를 정리한다. 최종 스테이지까지 완료하면 누적 점수를 기반으로 최종 A+ 획득 여부를 보여준다.

\pagebreak

# 5. Glossary

| Term | Description |
| :--- | :--- |
| Project A+ | 대학 생활의 학점 이수 과정을 2D 액션 플랫포머 장르와 결합한 본 프로젝트의 명칭이다. |
| Player | 게임을 플레이하는 사용자이며, 모든 주요 use case의 primary actor이다. |
| 유니티 엔진 (Unity Engine) | 본 프로젝트의 개발 기반이 되는 게임 엔진이다. |
| 픽셀 아트 / 도트 | 저해상도 그래픽 스타일로, 경쾌한 조작감과 인디 게임 감성을 표현하기 위해 사용한다. |
| 씬 로더 (Scene Loader) | 타이틀, 스테이지, 결과 화면 등 서로 다른 게임 씬을 불러오는 시스템 기능이다. |
| 타임스케일 (Time Scale) | 게임 내 시간 흐름을 제어하는 값이다. 일시정지 시 0으로 설정한다. |
| 공부량 | 플레이어의 공격력 또는 전투 수행 능력을 의미하는 프로젝트 고유 능력치이다. |
| 멘탈 | 플레이어의 체력 또는 생존 가능성을 의미하는 프로젝트 고유 능력치이다. |
| 수업 스테이지 | 플레이어가 진행해야 하는 일반 스테이지이다. 학기 흐름을 구성하는 기본 단위이다. |
| 과제 오브젝트 | 일반 적보다 강한 엘리트 몬스터 역할을 하는 오브젝트이다. |
| 시험 보스 | 중간고사와 기말고사에 해당하는 보스 캐릭터이다. |
| 능력치 포인트 | 플레이어가 성장 과정에서 획득하고 공부량, 멘탈 등에 분배하는 자원이다. |
| 퀵슬롯 | Player가 전투 중 빠르게 아이템을 사용할 수 있도록 아이템을 등록하는 UI 영역이다. |
| 현재 학점 | 플레이어의 누적 성과를 나타내는 점수 또는 랭크 정보이다. |
| A+ | 본 프로젝트에서 플레이어가 최종적으로 달성해야 하는 목표 등급이다. |

\pagebreak

# 6. References

1) Unity Technologies, “Rigidbody2D - Unity Scripting API”  
https://docs.unity3d.com/ScriptReference/Rigidbody2D.html

2) Unity Technologies, “SceneManager.LoadScene - Unity Scripting API”  
https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.LoadScene.html

3) Unity Technologies, “Time.timeScale - Unity Scripting API”  
https://docs.unity3d.com/ScriptReference/Time-timeScale.html

4) Unity Learn, “2D Platformer Character Controller”  
https://learn.unity.com/tutorial/2d-game-kit-reference-guide

5) Game Design Patterns, “Observer Pattern for UI & Stats”  
https://gameprogrammingpatterns.com/observer.html

6) 나무위키, “Skul: The Hero Slayer”  
https://namu.wiki/w/Skul:%20The%20Hero%20Slayer
