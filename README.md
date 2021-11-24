# README.md

## Oc Dialogue 1.3.1

이 패키지는 데이터를 포함한 대화 시스템임
기본적으로, 유니티에서 셰이더그래프용으로 작성된 그래픽 API를 이용함

### 작성된 유니티 버전 : 2021.2.3

### 설치 방법
유니티의 패키지 매니페스트 파일에
"com.olivecrow.ocdialogue" : "https://github.com/olivecrow/OcDialogue.git"
을 적으면 됨


### 데이터베이스 사용법
Assets 폴더 하위에 Resources폴더를 만들고 DB Manager를 생성해야함.
이후 원하는 디렉토리 아무곳에나 ItemDatabase, NPC Database 등을 생성하고, DB Manager에 참조시킴.

기본적으로 추가적인 스크립터블 오브젝트 에셋이 생성되기 때문에, 각각 디렉토리를 나눠서 저장하는게 좋음