# Changelog
모든 변경사항을 이 곳에 기록함.

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
