using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "DB Manager", menuName = "Oc Dialogue/DB Manager")]
    public class DBManager : ScriptableObject
    {
        /*
         * 이 에셋은 실제 이 패키지를 사용할 프로젝트의 데이터베이스를 캐싱하기 위한 용도임.
         * 이 에셋은 무조건 Assets/Resources에 생성해아함.
         *
         * 각각에 데이터베이스 필드에 자신이 런타임에 사용하는 데이터베이스를 올려놓으면 됨.
         * 이렇게 하는 이유는, 패키지를 업데이트 할 때마다 패키지 디렉토리 전체가 초기화되기 때문에 기껏 만들어놓은 데이터베이스가 날아가기 때문임.
         * 물론 업데이트 이후에 다시 여기에 드래그 앤 드롭으로 자신의 데이터베이스를 올려놔야함.
         */
        public const string AssetPath = "DB Manager";
        
        public static DBManager Instance => _instance;
        static DBManager _instance;

        public GameProcessDatabase GameProcessDatabase;
        public ItemDatabase ItemDatabase;
        // TODO : Quest Database.
        public NPCDatabase NpcDatabase;
        // TODO : Enemy Database.

        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInit()
        {
            _instance = Resources.Load<DBManager>(AssetPath);
        }
#if UNITY_EDITOR
        /// <summary> Editor Only. </summary>
        [InitializeOnLoadMethod]
        static void EditorInit()
        {
            _instance = Resources.Load<DBManager>(AssetPath);
        }
        
#endif
    }
}
