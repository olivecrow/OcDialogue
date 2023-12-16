# Changelog
모든 변경사항을 이 곳에 기록함.

### [2.6.0] -2023-12-16
- DBDebugUI에서 오류를 발생시키는 디버그 로그 라인 삭제
- DBDebugUI가 켜져있을때 매 업데이트마다 커서를 활성화하도록 변경
- DataRowContainer를 초기화할때 런타임 데이터로 인해 발생할 수 있는 오류 해결
- 다이얼로그 에디터에서 서브 엔트리 노드를 포함하는 다수의 UI를 삭제할때 오류가 발생하는 문제 해결
- 다이얼로그 에디터에서 Conversation 드롭다운의 순서를 알파벳 순/역순으로 정렬할 수 있는 옵션 추가
- Balloon에 번역용 주석을 적어넣을 수 있는 localizationComment 필드 추가
- Conversation에 string 혹은 OcData로 해당되는 엔트리를 반환해주는 GetEntry 메서드 추가

### [2.5.6] -2023-12-01
- Balloon의 GetNext에서 선택지를 현재 Balloon 기준으로 반환하도록 변경
- package 버전 넘버 오류 수정

### [2.5.5] -2023-11-30
- 삭제한 Balloon을 Undo 했을때 서브에셋이 복구되지 않던 문제 해결.
- Balloon을 삭제했을때 오류메세지가 출력되던 문제 해결
- Conversation에 Balloons에는 존재하나, 서브에셋이 누락되어있던 Balloon을 복구하는 유틸리티 버튼 추가
- Balloon의 GUID형식 변경
  - Conversation에 모든 Balloon의 GUID를 최신 형식으로 업데이트하는 유틸리티 버튼 추가
  - 각 Balloon 아래에 GUID의 첫 24글자가 표시되도록 변경.
- 컴파일 직전에, 변경사항이 있는 Conversation을 자동으로 저장하도록 변경.
- DBDebugUI에서 카테고리별로 TreeView가 보이도록 변경
- SavedDataRow에서 중복된 type 삭제.
- Balloon에 ToString 오버라이드 추가

### [2.5.4] -2023-11-21
- UnInitialization을 동사형인 UnInitialize로 변경
- DataRowContainer에 UnInitialize 추가
- Balloon에서 상황에 따라 적절한 연결된 다음 Balloon을 얻는 GetNext() 메서드 추가
  - GetNextDialogue는 삭제
- IDialogueUser에 대화 사이클을 조절하는 DialogueCycleIndex 프로퍼티 추가
- 미리 대화문을 시뮬레이션할 수 있는 Dialogue Simulator 추가 (OcDialgue > Dialogue Simulator)

### [2.5.3] -2023-11-19
- DBManager의 인스턴스가 자동으로 로드되지 않는 버그때문에 EditorInit 다시 추가

### [2.5.2] -2023-11-18
- 누락된 변경사항 추가

### [2.5.1] -2023-11-18
- DataRowContainer의 Initialize와 InitFromEditor, OnRuntimeValueChagned를 public으로 다시 바꿈.
- DataRowContainer.LoadFromEditorPreset이 InitFromEditor와 중복되기때문에 삭제함.

### [2.5.0] -2023-11-18
- DBManager의 자체 초기화를 없앰. 이제 각 DB를 직접 초기화해야됨.
- DataRowContainer와 DataRow의 OnRuntimeValueChaged 이벤트를 internal로 제한함.
- DataRowContainer의 초기화와 DataRow의 초기화 메서드를 internal로 제한함.
- 이제 Initialize() 메서드 없이 InitFromEditor() 혹은 Initialize(List<CommonSaveData> datas) 메서드로 초기화하도록 변경함.
- 각 DB의 초기화 상태를 해제하는 UnInitialization() 메서드 추가.

### [2.4.15] -2023-11-17
-CutsceneBehaviour에서 스킵관련 오류 해결(duration이 time으로 적혀있던 문제)
-CutsceneBehaviour에서 IsSkipToEndAvailable을 스킵 내용에 맞게 변경

### [2.4.14] -2023-11-16
- CutsceneBehaviour에 인스턴스 이벤트인 start, end 추가
- CutsceneBehaviour에서 스킵 도착시간을 인스펙터에서 정할 수 있는 변수 추가

### [2.4.13] -2023-11-05
- CutsceneBehaviour의 클립 이벤트를 없애고 내부 콜백으로 전환함.

### [2.4.12] -2023-08-30
- 컷씬의 자막 프리뷰를 플레이중이 아닐때 표시되지 않도록 수정
- Entry노드의 기본 waitTime 값을 2로 변경
- 그래픽뷰를 우클릭하여 텍스트를 대화문으로 만드는 옵션 추가
- 그래픽뷰에서 Focus 버튼을 통해 원점으로 되돌아오는 기능 추가

### [2.4.11] -2023-08-30
- 컷씬의 타임라인을 프리뷰로 볼 때 게임뷰에 자막을 보여주는 기능 추가
- 타임라인에 대화를 가져올때, wait 시간에따라 텀을 주면서 가져오도록함.

### [2.4.10] -2023-08-29
- DBDebugObjectField 에서 null 체크 추가
- Balloon에 useHighlight 변수 추가

### [2.4.9] -2023-08-28
- IOcNPC에서 불필요한 프로퍼티 삭제
- DataRowCotainer에 포함된 메서드 들에서 DataRow == null 일때의 처리 추가
- DialogueNode의 텍스트 부분의 가독성 향상
- Balloon 에 HighlightCondition 추가 및 ChoiceCheckerReaction enum을 ChoiceCheckerDisplayMode로 변경
- 다이얼로그 에디터에서 Add Balloon 할때, Balloon 타입에 상관 없이 추가할 수 있도록 변경

### [2.4.8] -2023-08-24
- DBManager의 EditorInit에서도 각 DB 초기화하도록 변경
- DataRowContainer의 HasKey에서 목록이 비어있으면 ArgumentNullException이 발생하기때문에 리스트의 Count가 0 일 경우 false를 반환하는 코드 추가.

### [2.4.7] -2023-04-14
- DBDebugStandardField 및 DBDebugObjectField 버그 수정

### [2.4.6] -2023-04-14
- DataRow 및 OcData에서 전처리기 문제때문에 빌드 오류가 나던 문제 해결
- DBDebugUI 개선
- SaveDataRow 및 CommonSaveData에 ToString 오버라이드 추가

### [2.4.5] -2023-01-10
- 런타임 데이터를 읽어서 초기화할때 SaveDataRow.CopyTo에서 id가 달라서 생기는 오류 해결.

### [2.4.4] -2023-01-10
- DataRowContainer.AddDataRuntime에서 isCreatedRuntime이 설정되지 않던 문제 해결.

### [2.4.3] -2023-01-09
- DataRowContainer에서 초기화 시, null인 데이터를 삭제함. 런타임 데이터는 게임 종료 후 null로 남기 때문.

### [2.4.2] -2022-12-23
- DataRow의 InitialValue를 외부에서 변경할 수 있도록 ChangeInitialValue 메서드 추가.

### [2.4.1] -2022-11-09
- DataRowContainer에서 AddDataRuntime할때 OnRuntimeValueChanged가 설정되지 않던 문제 해결

### [2.4.0] -2022-10-27
- 런타임에 DB 상태를 확인하고 수정할 수 있는 DB Debug UI 샘플 추가
- CommonSaveData의 DataRowContainerDict를 DataRows로 이름 변경
- CommonSaveData가 생성될때 SaveTime에 현재시간이 자동 등록되도록 추가
- DataRowContainer.OnRuntimeValueChanged가 DataRow.OnRuntimeValueChanged가 호출될때 따라서 호출되지 않던 문제 해결

### [2.3.11] -2022-10-19
- DB Export 개선 및 버그 수정
- 런타임에 DataRow 생성할 수 있는 기능 추가

### [2.3.10] -2022-08-19
- IDBEditor에서 불필요해진 CSV 추출 관련 메서드 삭제.

### [2.3.9] -2022-08-19
- DBExportWizard에서 CSV 추출하는 방식을 LocalizationCSVRow에 맞게 고정함.

### [2.3.8] -2022-08-19
- ExportWizard제거 및 새로운 버전인 DBExportWizard 추가
- 로컬라이징용 CSV를 추출하기 위한 DB용 인터페이스인 ICSVExportable 추가
- Dialogue 에디터에서 Undo 안정성 증가

### [2.3.7] -2022-05-19
- ExportWizard에 로컬라이징 관련 디버그 로그 추가
- OcData를 기본값으로 되돌리는 Initialize 메서드 추가. 기존의 GenerateRuntimeData를 대체함
- OcDB의 Init을 Initialize로 대체함. API를 동일하게 유지하기 위한 것.
- CutsceneBehaviour.Play를 virtual 메서드로 시그니처 변경.

### [2.3.6] -2022-05-10
- EasySave Integration 업데이트
- DataBasicTest의 샘플화
- ExportWizard에서 폴더 선택 및 이름 지정 방식을 파일선택창에서 하는 것으로 변경.
- OcData의 id를 테이블 리스트에서 출력되지 않도록 변경

### [2.3.5] -2022-05-07
- OcData에 int형 id를 기본적으로 포함하도록 추가함
  - 유닛 테스트에 id가 0일때와 중복되는 경우의 테스트 추가함
- DataRowContainer에 Vector타입에 대한 Overwrite추가함
- OcUtility의존성 1.5.7로 증가
- LocalizationCSVRow에 id 변수 추가
- ExportWizard
  - 번역용 CSV Export에 LocalizationCSVRow id 추가
  - id를 기준으로 값 덮어쓸 수 있게 변경.
  - Dialogue DB에 대한 Localization CSV Export 가능하게 변경

### [2.3.4] -2022-05-05
- 일반화시킨 DB에 맞게 ExportWizard 새로 작성
- CutsceneBehaviour의 Release를 EnterEditMode 타이밍으로 변경

### [2.3.3] -2022-04-29
- DialogueEditor
  - 노드의 복사, 붙여넣기에 대한 안정성 증가 및 잘못된 LinkedBalloons 리스트가 포함되던 문제 해결
  - DialogueAsset에 Conversation이 포함되지 않던 문제 해결.
  - 최대 줌을 1.5로 늘림.

- DB
  - DataRowContainer에 Hierarchical Data 추가.
    - bool 타입의 DataRow만 해당됨
    - key는 단순히 에디터상에서 구분하기 쉽게 만든거고, 기능은 없음.
  - DataRow Type에 Vector타입 추가. Vector4까지 저장 가능.

- CutsceneBehaviour
  - 일부 null 체크가 필요한 곳에 null 체크 추가.

- Test
  - DialogueAsset에 null인 Conversaion이 포함되었는지 확인하는 테스트 추가.

### [2.3.2] -2022-04-13
- Cutscene
  - InternalEnd에서 OnCutsceneEnd에 대한 이벤트가 호출되지 않던 문제 해결.
  - 불필요하게 static으로 선언된 메서드를 일반 메서드로 변경.
  - auto-setter로 변환 가능한 프로퍼티 정리.
  - Skip 메서드와 SkipToNextClip 메서드의 혼란을 피하기 위해 Skip -> SkipToEnd로 변경
  - IsSkipToEndAvailable 프로퍼티 추가. 상속받아서 사용할 경우, 오버라이드 해서 변경할 수 있음.

### [2.3.1] -2022-04-12
- Cutscene
  - CutsceneBehaviour의 InternalEnd에 있던 DialogueTrack관련 Null 오류 해결
  - CutsceneBehaviour에서 타임라인의 time을 마지막에 가까운 지점으로 변경하는 정적 메서드인 Skip 추가
  - Skip, Stop을 인스펙터에 버튼으로 표시
  - SkipToNextClip에서 마지막 클립이면 Stop을 실행하던 부분을 return으로 바꿈.
    - 그 자리에서 바로 끝내기보단 남은 컷씬 보여주는게 나을 것 같아서 바꿨는데, 나중에 문제가 있거나 별로면 다시 Stop이나 Skip을 넣기.
- 빌드 오류로 인한 OcUtility 종속성 버전 변경 (1.5.1 버전 이상) 

## [2.3.0] -2022-04-12
- OcUtility 1.5.0 이상에 대한 종속성 설정.
- Dialogue
  - Editor
    - Priority 라벨 위치 및 GUI 생성 부분 개선
    - Priority 라벨이 포함된 구성요소를 한꺼번에 삭제할때 오류가 발생하던 문제 해결
    - 노드 및 엣지 생성, 삭제를 SetDirty로만 처리해서 처리시간 감소시킴
    - 스크립트가 다시 컴파일 돼도 이전의 Conversation을 계속 유지하도록 변경
    - 복사, 붙여넣기, 복제 기능을 단축키로도 이용할 수 있도록 추가.
    - NPC Dropdown이 출력되지 않던 문제 해결.
  - Runtime
    - Action 타입 Balloon에 ActionType 및 SubEntryDataType enum 추가.
      - 커스텀 데이터를 이용한 엔트리로써 사용가능함
    - Balloon의 DisplayTargetImage를 Sprite로 변경
- DB
  - string 비교를 string.CompareOrdinal로 변경.
  - 유니티의 검색시스템을 이용해서 데이터 선택을 도와주는 DataSearch 애트리뷰트 추가.
  - DataRow의 description을 UNITY_EDITOR전처리기 밖으로 옮김.
  - DataRowContainer에서 DataRow 순서 변경 가능하게 바꿈.
  - DataRow에 자기 자신을 Ping할 수 있는 버튼 추가.
- Cutscene
  - DialogueTrack을 사용하는 타임라인 에셋에 대한 에디터 단위 테스트 추가.
  - BindingOverride가 불필요하다 생각돼서 삭제함. 만약 구현할거면 여기가 아니라 이 패키지를 사용하는 프로젝트에서 직접 구현하는게 나음.
  - DialogueDisplayParameter가 불필요하다 생각돼서 삭제함. 그냥 컷씬용 DialogueUI를 따로 두는게 나을듯.
  - CutsceneBehaviour의 Awake에서 Null처리가 없어서 DialogueTrack이 없으면 오류가 나던 문제 해결.
- 오류가 나지 않도록 Samples 업데이트.

## [2.2.4] -2022-3-13
### Fixed
- 다이얼로그 에디터 수정사항
  - Conversation의 이름 변경시, 에디터에 변경사항이 곧바로 적용되지 않던 문제 해결
  - 툴바에 현재 Conversation을 선택하는 버튼 추가
  - 편집과정 최적화
  - Balloon에 ChoiceCheckerReaction enum 추가
    - 값으로만 존재하며, 실제 적용은 직접 해야함.
  - TMP Sprite Asset Window를 제대로 쓸 수 없던 문제 해결
- CutsceneBehaviour 인스펙터 오류 해결
- 일반 인스펙터에서 DataSetter의 ExplicitToggle의 레이아웃이 밖으로 벗어나던 문제 해결
- UpdateExpression -> OnDataApplied로 메서드 이름 변경
- OcUtility 의존성 버전 업데이트 (1.1.12 -> 1.4.8)

## [2.2.3] -2022-2-11
### Fixed
- 다이얼로그 에디터 수정사항
  - 에디터 윈도우에서 새 Conversation 생성시, balloon의 LinkedBalloons가 null이라서 발생하던 오류 해결
  - 카테고리에 따른 Conversation의 목록이 제대로 출력되지 않던 문제 해결
  - 현재의 Conversation에 속하지 않은 Balloon 선택시, 오류가 발생하던 문제 해결

- Balloon의 TextArea 크기 늘림.
- CutsceneBehaviour에 바인딩 오버라이드를 위한 딕셔너리만 있고 리바인드를 하지 않던 문제 해결
- 
### Added
- Balloon의 인스펙터에 TextMesh Pro의 Sprite Atlas용 키워드를 삽입할 수 있는 버튼 추가
- CutsceneBehaviour의 인스펙터에 게임 플레이 도중에 보여지는 플레이 버튼 추가

## [2.2.2] -2022-2-5
### Added
- 컷씬을 관리하는 기본 클래스인 CutsceneBehaviour 추가.
  - MonoBehaviour를 상속하기때문에, PlayableDirector를 가지는 게임 오브젝트에 부착해서 사용하면 됨.
  - 추가로 기능을 작성하려면 해당 클래스를 상속받아서 사용하면 됨.
  - 대부분의 컷씬 조작 기능을 static으로 만들어서 인스턴스에 접근하지 않고 사용할 수 있음.
- 컷씬과 관련된 멤버들에 각자의 참조를 추가함.
  - DialogueClip, DialogueClipBehaviour, DialogueTrack, CutsceneBehaviour 등.
  
### Fixed
- DialogueDisplayParameter 변경
  - DialogueDisplayParameter를 ScriptableObject로 만들고, 기본 에셋을 OcDialogue>Dialogue Display Parameter에서 수정할 수 있도록 함.
  - 파라미터를 가지는 주체가 DialogueTrack에서 CutsceneBehaviour로 변경됨.
- DialogueClipBehaviour의 OnStart, OnFadeOut, OnEnd를 CutsceneBehaviour의 OnClipStart, OnClipFadeOut, OnClipEnd로 변경함.
- 변경된 스크립트에 맞게 Dialogue UI Sample을 변경함.

## [2.2.1] -2022-2-2
### Fixed
- 타임라인 뷰에서 편집 중, DialogueClipBehaviour의 이벤트가 호출되지 않도록 변경
- DialogueTrack에서 매번 에셋의 저장이 이루어지던 문제 해결
- Dialogue UI Sample이 최신 버전의 샘플을 반영하고 있지 않던 문제 해결

## [2.2.0] -2022-1-30
### Added
- 타임라인에서의 컷씬 지원을 위한 스크립트 추가
  - DialogueTrack : 타임라인 내에서 쓰이는 트랙
  - DialogueClip : 트랙 내에서 각각의 Balloon의 재생타이밍을 나타내는 클립
  - DialogueClipBehaviour : 클립의 내부 기능. 시작과 종료, 페이드아웃 등의 이벤트를 호출함
  - DisplayParameter : 컷씬의 대화 속도 등을 조절하는 파라미터.

## [2.1.0] -2022-1-29
### Added
- 각 Balloon의 Description을 노드 아래에 표시함
- 연결된 Balloon이 2개 이상인 경우, Edge에 우선 순위의 인덱스를 표시함
- Graph View를 우클릭해서 Conversation에셋을 선택할 수 있는 메뉴 추가

### Fixed
- Conversation 에셋의 key를 변수가 아닌 이름을 직접 지정하는 프로퍼티로 설정하고 DelayedAttribute를 적용함
- 한 번에 여러 노드를 선택해서 편집할 수 있도록 변경
- 편집중이지 않은 Conversation 에셋의 이름 변경이 다이얼로그 에디터에 반영되지 않던 문제 해결
- 다이얼로그 노드의 크기 고정
- 다이얼로그 노드의 TextField를 Label로 변경함
  - 노드를 통한 직접 수정이 불가능해짐
  - 15글자 이상은 ...으로 생략


## [2.0.1] -2021-12-31
### Fixed
- C# 버전에 따른 is not 키워드 컴파일 오류 수정

## [2.0.0] -2021-12-31
### Added
- 툴 내부에 구현되어있던 DB와 하위 데이터를 샘플로써 만듬
  - DBSamples
- 인벤토리 디버그 윈도우 만듬
  - OcDialogue > 인벤토리 디버그 윈도우
  - 런타임에서 생성된 모든 인벤토리에 접근 가능
  - 디버그 윈도우 내에서 아이템 추가, 삭제, 복제 가능

### Fixed
- 툴 내부에 구현되어있던 DB와 하위 데이터들을 외부로 분리함
- 외부에서 데이터를 구현해서 사용할 수 있도록 기저클래스와 인터페이스를 마련함
  - OcDB, DBEditorBase, IDBEditor
  - OcData 내부의 추상 클래스들 추가
- 각 샘플을 리팩토링된 패키지에 맞게 수정함
- DataChecker, DataSetter를 리팩토링된 패키지에 맞게 수정함
- DBEditorWindow를 수정함
- Localization 패키지에 관한 의존성 오류를 수정함

## [1.4.1] -2021-12-19
### Fixed
- ItemBase의 CanBeTrashed가 카피에서 적용되지 않던 문제 해결
- IUsableItem.Use가 ItemUsageResult 값을 반환하도록 변경
- ItemBase의 CurrentStack이 0이 되면 자동으로 인벤토리에서 해제되도록 변경 

## [1.4.0]
### Added
- ItemBase에 CanBeTrashed 필드 추가
- ItemBase에 자신이 속한 Inventory를 알 수 있도록 프로퍼티를 추가함
  - 인벤토리에 추가될때 자동으로 할당되고, 제거될 때, 자동으로 해제됨
- InventoryEditorPreset에 isLeft 변수 추가함
- Enemy에 WeaponItem의 정보를 바탕으로 DamagerInfo를 업데이트 할 수 있는 기능 추가

### Fixed
- WeaponItem의 Weight을 int에서 float으로 변경
- BattleStat의 모든 속성의 총 합 표시
- Inventory.Remove의 아이템 매칭 방식을 GUID, Instance중에서 선택할 수 있도록 변경
- Inventory.RemoveSingleItem에서도 OnInventoryChanged 이벤트가 호출되도록 변경
- Inventory의 OnInventoryChanged이벤트에 ItemBase, InventoryChangeType 파라미터 추가
- ItemBase의 AddStack, RemoveStack, GetCopy 메서드의 접근자를 internal로 제한함

- EasySave Integration 샘플에서 ES3 캐싱을 기본적으로 사용하도록 설정함
- Debugging Essential Integraion을 변경된 코드에 맞게 수정함


## [1.3.10] -2021-12-10
### Added
- ExportWizard에서 ItemDB를 추출할 때 선택 가능한 옵션 추기
  - 각 타입별로 추출하는 경우, 추출할 타입을 선택하는 기능 추가
  - 각 타입별로 추출하는 경우, 각 타입의 변수 또한 추출할지 선택하는 기능 추가

### Fixed
- DamagerTag 항목 추가
- Enemy에 Stability 다시 추가함

## [1.3.9] -2021-12-09
### Added
- DamagerTag및 DamagerInfo를 만들어서 Enemy의 공격부위에 대한 프리셋을 만들 수 있게 함

### Fixed
- Enemy의 Stabilty 필드를 일단 삭제함. 나중에 필요해지면 그때 생성할 것.
- EnemyLevel struct를 EnemyClass로 이름을 변경함
- ItemBase의 ItemName에 [Delayed] 어트리뷰트를 추가함.

## [1.3.8] - 2021-11-28
### Fixed
- ItemBase의 CurrentStack을 virtual로 변경.

## [1.3.7] - 2021-11-28
### Added
- IUsableItem에 ItemBase 및 CurrentStack 프로퍼티 추가.
  - stackable이 아닌 아이템은 별도의 사용 횟수를 표시해야 하기때문에 추가했음.

## [1.3.6] - 2021-11-28
### Fixed
- Inventory에 넣었던 isCopy 체크 해제함. 
  - 카피를 넣어서 결국 카피가 두 번 생성되는 문제가 생겼기 때문.
- DataRowContainer가 과도한 데이터를 가졌을때 에디터 속도가 저하되는 문제 해결
  - 한 페이지에 50개까지만 그리도록 함.

## [1.3.5] - 2021-11-28
### Fixed
- DB 편집기에서 WeaponItem 출력이 제대로 되지 않던 문제 해결.
- item의 subtype들을 직렬화 가능하도록 virtual 구문 삭제함.
- itemBase의 ApplyBase에서 iconReference가 적용되지 않던 문제 해결
- Inventory에서 isCopy 체크가 되어있지 않은 원본이 인벤토리로 들어가는 문제 해결

## [1.3.4] - 2021-11-26

### Added
- Inventory에서 아이템 이름 및 guid로 아이템을 찾을 수 있는 Find 메서드 추가.

### Fixed
- DB 편집기에서 WeaponItem의 서브타입이 너무 길어서 EnumPopUp으로 바꿈.

## [1.3.3] - 2021-11-26
### Added
- GenericItem이 아닌 중요 아이템 등도 사용 가능하도록 IUsable 인터페이스를 추가.

### Fixed
- 각 아이템의 프로퍼티를 외부 클래스에서 변경 가능하게끔 virtual로 변경.

## [1.3.2] - 2021-11-26
### Added
- BattleStat에 더하기, 빼기 연산 추가

### Fixed
- Enemy 클래스의 ItemDropInfo 구조체를 클래스로 변경하고, 드롭 확률의 알고리즘을 개선함

## [1.3.1] - 2021-11-24
### Fixed
- 에디터 구문이 포함되서 빌드가 되지 않던 문제 해결

## [1.3.0] - 2021-09-21
### Added
- 각종 게임데이터의 저장을 위한 DynamicDataUser 추가
  - 이미 열었던 보물상자, 문, 획득한 필드 아이템 등에 MonoBehaviour로서 추가하고 GameProcessDB에 데이터를 연결해서 사용.
  
- Debugging Essentials와 통합해주기 위한 Debugging Essentials Integration 샘플 추가
  - 샘플을 임포트하면 Debugging Essentials의 콘솔에서 DB를 런타임에 수정할 수 있는 메서드를 사용할 수 있음

- Export Wizard에 번역용 템플릿 Export 추가
- DialogueUI Sample의 DataCheckEventTrigger에 OnAwake, TriggerEnter등의 타이밍을 정할 수 있도록 변수를 추가함.

### Fixed
- Export Wizard에서 NPCDB와 EnemyDB가 잘못 출력되던 문제 해결
- Dialogue를 CSV로 추출할때 Choice Balloon에서 키의 개수가 맞지 않던 문제 해결
- DialogueUI Sample의 DialogueUI 작동방식 변경
  - Cutscene모드와 Default 모드로 나누던 것을 하나로 합침. 
  - 그 대신, 그 안에 있는 설정을 각각 나눠서 씬 별로 작동할 수 있게 함.
  - 컷씬용 UI와 일반 대화용 UI, 나레이션용 UI 등을 나눠서 작업할 것.
  - ENABLE_LOCALIZATION 전처리기 추가함. Localization 패키지가 활성화된 경우, 텍스트를 대사가 아니라 Balloon의 주소로 표시함.

## [1.2.0] - 2021-09-13
### Added
- 세이브/로드를 위한 기능 (GetSaveData / Overwrite) 추가
  - GetSaveData로 해당 데이터 저장에 필요한 요소를 리스트나 딕셔너리로 얻을 수 있음
  - 이후 해당 리스트나 딕셔너리를 Overwrite로 다시 덮어쓸 수 있음
  - 데이터만 입출력하는 기능이기 때문에 어떤 세이브 시스템과도 호환 가능
- Easy Save3 통합을 위한 샘플 작성
  - 샘플을 받으면 다른 작업 없이도 런타임에서 값이 변경될 때마다 저장하고, 게임 시작시 로드함
  - 만약 빌드나 런타임 중에서 데이터의 프리셋을 저장하고 불러오고 싶다면 RuntimeSaveGUI를 부착한
  게임 오브젝트를 씬에 위치시키면 됨

## [1.1.0] - 2021-09-05
### Added
- 유닛테스트 추가.
  - 에디터 모드 : 작성된 데이터베이스에서 무결성 검사를 실행
  - 플레이 모드 : 임시 데이터를 몇 개 만들어서 예상된 결과가 나오는지 테스트
- 테스트 프레임워크 및 코드 커버리지 의존성 추가.
- BattleStat의 연산자 오버로드, ToString 오버로드 추가함.

### Fixed
- 인벤토리에 아이템 추가, 제거시, unStackable 아이템도 지정된 개수만큼 실행하도록 변경.
- ItemBase.RemoveStack에서 잘못된 조건문 수정.
- 사용되지 않던 Balloon.imageSizeOverride를 샘플의 DialogueUI 스크립트에 적용.
- 다이얼로그 에디터에서 icon새로고침이 매 프레임마다 되도록 변경.

## [1.0.3] - 2021-08-27
### Fixed
- DataChecker의 인덱스 표현식 제거. C# 8.0과 2021.2 미만은 호환 안 되는듯.

## [1.0.2] - 2021-08-27
### Fixed
- 빌드 오류 해결
- OcUtility 의존성 버전 1.1.12로 상향

## [1.0.1]
### Added
- DatabaseEditorWindow에 2021.2 미만 호환성 추가 (ConversationField)

### Fixed
- README 업데이트
- 에디터 윈도우 메뉴를 메뉴바 최상위로 옮김.
  - Window/OcDialogue/DatabaseEditorWindow => OcDialogue/DatabaseEditorWindow

## [1.0.0]

### Added
- 대화에 이미지 뷰어 기능 추가
- 대화에 타임라인의 시그널 리시버를 이용한 이벤트 기능 추가
- 모든 데이터베이스 타입에 런타임 디버그 기능 추가.
- 대화에 Checker, Setter 기능 추가.
- 주요 타입에 아이콘 추가.
- 다이얼로그 에디터에 Action노드 타입 추가. 텍스트 출력을 제외한 기능이 가능함
  checker만 사용해 필터 기능으로 쓰거나, 이벤트만 실행하는 등의 응용을 할 수 있음.
 
- 패키지에 샘플 추가
  - External Item Adapter : 기본 아이템 타입을 상속받는 커스텀 아이템을 적용하는 샘플
  - Dialogue UI Sample : 대화 UI 샘플.

- 타임라인 의존성 추가.

### Fixed
- 런타임 값을 데이터 내부로 합침. 이제 Quest, DataRow등에 직접 접근하여 런타입 값을 획득함.
- 장비 아이템의 물리, 속성 스탯을 BattleStat 구조체로 합침.
- 데이터베이스 에디터 윈도우 개선
- asmdef에서 에셋스토어 패키지의 어셈블리 참조를 해제함.


## [0.2.1] - 2021.08.01

### Added
- DataChecker의 간단한 기능 구현. 현재 런타임 데이터가 아닌 EditorPreset으로 값 검사 가능.

### Fixed
- 패키지 의존성에 OcUtility 패키지 추가
- GameProcessData, ItemDatabase, QuestDatabase, NPCDatabase의 데이터베이스 - 에디터프리셋 구조 리팩토링.

## [0.2.0] - 2021.07.06

### Added
- Dialogue Editor Window추가. 간단한 기능만 존재함. 아직 작업중.
- IEquipment에서 ItemBase 프로퍼티 추가. 자신을 반환함.
- 빌드 시 오류가 없도록 Editor asmdef 변경 및 전처리기 수정함.

## [0.1.12] - 2021.06.30
### Added
- 아이템 확장 샘플 추가.
  - git에서 소프트 링크로 Sample 폴더를 연결함.
  - upm의 샘플 형식이 제대로 작동하는지는 모르겠음.

### Fixed
- 기존 아이템을 확장할 수 있도록 일부 속성을 virtual로 변경.

## [0.1.11] - 2021.06.30
### Added
- ItemBase에 IsUsable 및 추상 메서드인 Use(), IsNowUsable() 추가.

### Fixed
- IEquipment의 Equipped가 float이던 문제 해결.
- Database Edit Window에서 TreeRebuild를 시도할때 오류가 나던 코드를 수정함.
이게 해결될진 두고봐야함.


## [0.1.10] - 2021.06.30
### Fixed
- 아이템의 카피를 획득할 때, 일부 필드가 반영되지 않던 문제 해결.

## [0.1.9] - 2021.06.29
### Fixed
- Inventory Editor Preset이 제대로 로드되지 않던 문제 해결.


## [0.1.8] - 2021.06.29
### Fixed
- Inventory Editor Preset에서 곧바로 GetCopy를 통해 아이템 얻을 수 있게 함.
- Inventory Editor Preset 런타임에서 실행 되게 고침.
- Item Editor Preset에서 DropDesk를 통해 아이템 쉽게 설정할 수 있게 함.
- ItemBase의 AddStack의 OnStackOverflow에 null을 기본값으로 둠.

## [0.1.7] - 2021.06.29

### Fixed
- 아이템 설명 참조할때, 참조의 원본이 아닌 걸 드래그 해도 원본을 참조하도록 변경.
- 방어구 무게로 내구도 및, 속성 별 상세 설정이 적용되도록 변경.
- 악세사리를 방어구의 서브타입에서 별도의 아이템 타입으로 변경.
- Inventory Editor Preset을 패키지 내부에서 제거. 앞으로는 Editor Default Resources에 만들면 됨.
  

## [0.1.6] - 2021.06.29
### Added
- ArmorItem에서 무게를 기준으로 방어력을 산출할 수 있는 기능 추가.
- 아이템의 설명을 참조할 수 있는 기능 추가.

## [0.1.5] - 2021.06.28
### Fixed
- Database Editor Window의 TreeMenu가 항상 펼쳐져있도록 변경.
- Addressables 의존성 추가.
- ArmorItem 및 WeaponItem에 Assetference의 avatar추가.
- ArmorItem 및 WeaponItem에 기본적인 스텟(공격/방어/속성) 추가.

## [0.1.4] - 2021.06.28
### Fixed
- DB Manager를 패키지 외부에서 직접 생성하도록 변경.
    - 패키지 내부에 생성하면 에셋을 수정할 수 없기 때문.
    - 나머지 DB는 0.1.3에서의 수정대로 아무데나 놔둬도 DB Manager의 참조로써 이용할 수 있게 함.

## [0.1.3] - 2021.06.28
### Added
- 프로젝트 마다 개별 데이터베이스를 만들 수 있도록 DB Manager추가.
- 각 데이터베이스의 싱글톤을 DB Manager의 싱글톤의 참조로 변경.

## [0.1.2] - 2021.06.28
### Added
- Database Editor Window에서 treeMenu를 더블클릭하면 프로젝트 뷰에서 해당 에셋을 인스펙터로 선택하는 기능 추가.


## [0.1.1] - 2021.06.28
### Added
- IEquipment 인터페이스를 WeaponItem 및 ArmorItem에 구현
- ItemBase에서 GetCopy로 아이템 사본을 얻는 기능 추가.

## [0.1.0] - 2021.06.28
### Added
- 간단한 인벤토리 시스템(Inventory) 추가
- 에디터용 인벤토리 프리셋 시스템 (Inventory Editor Preset) 추가

## [0.0.1] - 2021.06.19
### 기존에 작업하던 내용을 리팩토링 하기 위해 새로 프로젝트를 작성함.
