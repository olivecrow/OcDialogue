using System;
using System.Collections;
using System.Collections.Generic;
using OcDialogue.DB;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue
{
    public class Conversation : ScriptableObject
    {
        [ReadOnly]public string GUID;
        [ValueDropdown("GetCategoryList")]public string Category;
        [InlineButton("ApplyName", ShowIf = "@name != key")]public string key;
        [InfoBox("Main NPC가 설정되어있지 않음", InfoMessageType.Warning, "@MainNPC == null")]
        [ValueDropdown("GetNPCList")]public OcNPC MainNPC;
        /// <summary> 에디터에서 참고용으로 사용되는 설명. 인게임에서는 등장하지 않음. </summary>
        [TextArea]public string description;
        public List<Balloon> Balloons;
        public List<LinkData> LinkData;

        /// <summary> Editor Only. 그래프에서의 뷰 위치 </summary>
        [HideInInspector]public Vector3 lastViewPosition;
        /// <summary> Editor Only. 그래프의 줌 스케일 </summary>
        [HideInInspector]public Vector3 lastViewScale = new Vector3(1f, 1f, 1f);


        public Balloon FindBalloon(string guid)
        {
            return Balloons.Find(x => x.GUID == guid);
        }
        
#if UNITY_EDITOR
        public event Action onValidate;

        void ApplyName()
        {
            name = key;
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), key);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public Balloon AddBalloon(Balloon.Type type)
        {
            Balloon balloon;
            switch (type)
            {
                case Balloon.Type.Entry:
                    balloon = CreateInstance<Balloon>();
                    balloon.type = Balloon.Type.Entry;
                    balloon.GUID = Guid.NewGuid().ToString();
                    balloon.position = new Vector2(20, 60);
                    balloon.text = "Entry";
                    break;
                case Balloon.Type.Dialogue:
                    balloon = CreateInstance<Balloon>();
                    balloon.type = Balloon.Type.Dialogue;
                    balloon.GUID = Guid.NewGuid().ToString();
                    balloon.text = "New Dialogue";
                    balloon.actor = MainNPC;
                    break;
                case Balloon.Type.Choice:
                    balloon = CreateInstance<Balloon>();
                    balloon.type = Balloon.Type.Choice;
                    balloon.GUID = Guid.NewGuid().ToString();
                    balloon.text = "New Choice";
                    break;
                case Balloon.Type.Action:
                    balloon = CreateInstance<Balloon>();
                    balloon.type = Balloon.Type.Action;
                    balloon.GUID = Guid.NewGuid().ToString();
                    balloon.text = "New Action";
                    break;
                default: goto case Balloon.Type.Dialogue;
            }
            balloon.name = balloon.GUID;
            Balloons.Add(balloon);
            AssetDatabase.AddObjectToAsset(balloon, this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            
            return balloon;
        }

        public void RemoveBalloon(Balloon balloon)
        {
            if(!Balloons.Contains(balloon)) return;
            Balloons.Remove(balloon);
            AssetDatabase.RemoveObjectFromAsset(balloon);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void AddLinkData(LinkData linkData)
        {
            LinkData.Add(linkData);
            
            UpdateLinkedBalloonList();
        }

        public void RemoveLinkData(string from, string to)
        {
            var target = LinkData.Find(x => x.@from == from && x.to == to);
            if(target == null) return;

            var rootBalloon = Balloons.Find(x => x.GUID == from);
            var targetBalloon = Balloons.Find(x => x.GUID == to);
            
            // 여러 노드와 엣지를 선택 후 삭제하면 이미 삭제되어 빈 노드가 전달될 수 있으므로 둘 다 있을때만 실행함.
            if(rootBalloon != null && targetBalloon != null) rootBalloon.linkedBalloons.Remove(targetBalloon);
            
            LinkData.Remove(target);
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        /// <summary> Conversations 필드에서 대화 이름을 드롭다운으로 보여주기위한 리스트를 반환함. (Odin Inspector용) </summary>
        ValueDropdownList<string> GetCategoryList()
        {
            var list = new ValueDropdownList<string>();
            foreach (var category in DialogueAsset.Instance.Categories)
            {
                list.Add(category, category);
            }

            return list;
        }

        /// <summary> MainActor 필드에서 NPC이름을 드롭다운으로 보여주기위한 리스트를 반환함. (Odin Inspector용) </summary>
        ValueDropdownList<OcNPC> GetNPCList() => DialogueAsset.Instance.DialogueNPCDB.GetOdinDropDown();


        [BoxGroup("유틸리티 메서드")]
        [HorizontalGroup("유틸리티 메서드/1", LabelWidth = 100), LabelText("Replace"), ValueDropdown("GetNPCList")]
        public OcNPC replace_before;
        [HorizontalGroup("유틸리티 메서드/1", LabelWidth = 50), LabelText("  =>"), ValueDropdown("GetNPCList")][InlineButton("ReplaceNPC", "Replace")]
        public OcNPC replace_after;

        void ReplaceNPC()
        {
            foreach (var balloon in Balloons)
            {
                if (balloon.actor == replace_before) balloon.actor = replace_after;
            }
            onValidate?.Invoke();
        }

        [BoxGroup("유틸리티 메서드"), Button("모든 말풍선의 LinkBalloons 리스트 업데이트")]
        public void UpdateLinkedBalloonList()
        {
            foreach (var balloon in Balloons)
            {
                balloon.linkedBalloons = new List<Balloon>();
            }
            foreach (var linkData in LinkData)
            {
                var rootBalloon = Balloons.Find(x => x.GUID == linkData.@from);
                var targetBalloon = Balloons.Find(x => x.GUID == linkData.to);
                
                if(rootBalloon.linkedBalloons.Contains(targetBalloon)) continue;
                
                rootBalloon.linkedBalloons.Add(targetBalloon);
            }
        }


        [BoxGroup("유틸리티 메서드"), Button("AssetObject 삭제")]
        void RemoveAssetObject(ScriptableObject so)
        {
            AssetDatabase.RemoveObjectFromAsset(so);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        [BoxGroup("유틸리티 메서드"), Button("사용하지 않는 이미지 참조 해제")]
        void ReleaseUnusedImage()
        {
            foreach (var balloon in Balloons)
            {
                balloon.ReleaseUnusedImage();
            }
        }

        [BoxGroup("유틸리티 메서드"), Button("사용하지 않는 LinkData 제거")]
        void RemoveUnusedLinkData()
        {
            var removeList = new List<LinkData>();
            foreach (var linkData in LinkData)
            {
                if(FindBalloon(linkData.@from) == null || FindBalloon(linkData.to) == null) removeList.Add(linkData);
            }

            foreach (var linkData in removeList)
            {
                Debug.Log($"[Conversation] LinkData 제거됨 | from : {linkData.@from} | to : {linkData.to}");
                LinkData.Remove(linkData);
            }
        }

        [BoxGroup("유틸리티 메서드"), Button("GUID로 Balloon선택")]
        void SelectBalloonFromGUID(string guid)
        {
            var balloon = Balloons.Find(x => x.GUID == guid);
            if (balloon == null)
            {
                Debug.Log($"[{guid}] 해당 GUID를 가진 Balloon이 없음");
                return;
            }

            Selection.activeObject = balloon;
        }
#endif
    }
}
