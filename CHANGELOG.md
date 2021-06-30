# Changelog
모든 변경사항을 이 곳에 기록함.

## [Unreleased]

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
