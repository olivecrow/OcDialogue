# README.md

## Oc Dialogue 2.3.10

이 패키지는 데이터를 포함한 대화 시스템임
기본적으로, 유니티에서 셰이더그래프용으로 작성된 그래픽 API를 이용함

### 설치 방법
유니티의 패키지 매니페스트 파일에
"com.olivecrow.ocdialogue" : "https://github.com/olivecrow/OcDialogue.git"
을 적으면 됨


### 데이터베이스 사용법
Assets 폴더 하위에 Resources폴더를 만들고 DB Manager를 생성해야함.
이후 원하는 디렉토리 아무곳에나 ItemDatabase, NPC Database 등을 생성하고, DB Manager에 참조시킴.

기본적으로 추가적인 스크립터블 오브젝트 에셋이 생성되기 때문에, 각각 디렉토리를 나눠서 저장하는게 좋음

### 타임라인에서 사용하기
1. 타임라인 윈도우에서 우클릭 -> OcDialogue.Cutscene -> DialogueTrack 을 선택해서 트랙 생성
2. 트랙에 원하는 Conversation을 바인딩
3. 트랙 라인에서 우클릭 -> Query Conversation => 각각의 Balloon에 대한 클립이 생성됨
- 클립은 시작, 페이드아웃, 종료 시점에 이벤트를 호출함
  - 시작 : CutsceneBehaviour.OnClipStart
  - 페이드 아웃 : CutsceneBehaviour.OnClipFadeOut
  - 종료 : CutsceneBehaviour.OnClipEnd
  - 만약 클립이 HasToPause == true면, 페이드 아웃 없이 End 전에 멈춤.
- Dialogue UI Sample에서 바로 사용할 수 있음
  - 각각의 클립은 StartConversation이 아닌, 개별 Balloon을 출력하는 DisplayBalloon으로 표시됨