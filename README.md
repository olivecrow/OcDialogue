# README.md

이 패키지는 데이터를 포함한 대화 시스템임
기본적으로, 유니티에서 셰이더그래프용으로 작성된 그래픽 API를 이용함

### 작성된 유니티 버전 : 2021.1.12

### 설치 방법
유니티의 패키지 매니페스트 파일에
"com.olivecrow.ocdialogue" : "https://github.com/olivecrow/OcDialogue.git#upm"
혹은
"com.olivecrow.ocdialogue" : "https://github.com/olivecrow/OcDialogue.git#1.0.0"
을 적으면 됨

후자의 경우, 원하는 버전넘버를 맨 뒤에 적으면 그 버전이 설치됨.


### 데이터베이스 사용법
Assets 폴더 하위에 Resources폴더를 만들고 DB Manager를 생성해야함.
이후 원하는 디렉토리 아무곳에나 ItemDatabase, NPC Database 등을 생성하고, DB Manager에 참조시킴.

인벤토리 에디터 프리셋을 사용하기 위해선 Editor Default Resources 폴더를 만들고
거기에 Inventory Editor Preset을 생성해야함. 기본 Resources폴더에 만들면 빌드 오류가 남.