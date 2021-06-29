# Changelog
모든 변경사항을 이 곳에 기록함.

## [Unreleased]

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
