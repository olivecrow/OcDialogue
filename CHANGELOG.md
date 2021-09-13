# Changelog
모든 변경사항을 이 곳에 기록함.

### TODO
- 트랜스폼 데이터, 커스텀 값 등의 인게임 데이터를 GameProcessDB에 저장하고 로드하게 하기
- 빌드된 게임에서도 콘솔 상으로 런타임 데이터를 수정할 수 있게 하기

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
