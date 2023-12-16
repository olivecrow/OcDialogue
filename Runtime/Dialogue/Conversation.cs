using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcDialogue.DB;
using OcUtility;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using OcDialogue.Cutscene;
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue
{
    public class Conversation : ScriptableObject
    {
        [ReadOnly]public string GUID;
        [ValueDropdown("GetCategoryList")]public string Category;

        [ShowInInspector][DelayedProperty][PropertyOrder(-1)]
        public string key
        {
            get => name;
            set
            {
                if (Application.isPlaying)
                {
                    Debug.LogWarning($"Conversation의 키값을 런타임에 변경할 수 없음");
                    return;
                }
                var isNew = name != value;
                name = value;
#if UNITY_EDITOR
                if (isNew) AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(this), value);
#endif
            }
        }
        [InfoBox("Main NPC가 설정되어있지 않음", InfoMessageType.Warning, "@MainNPC == null")]
        [ValueDropdown("GetNPCList")]public OcData MainNPC;
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
            return Balloons.Find(x => string.CompareOrdinal(x.GUID, guid) == 0);
        }

        public List<Balloon> FindInputBalloons(Balloon balloon)
        {
            return (from linkData in LinkData 
                where string.CompareOrdinal(linkData.to, balloon.GUID) == 0 
                select FindBalloon(linkData.@from)).ToList();
        }

        public List<Balloon> FindLinkedBalloons(Balloon balloon)
        {
            return (from linkData in LinkData 
                where string.CompareOrdinal(linkData.@from, balloon.GUID) == 0 
                select FindBalloon(linkData.to)).ToList();
        }

        /// <summary> entryName이 입력된 경우 해당되는 서브 엔트리를, 입력되지 않은 경우엔 메인 엔트리를 반환함. </summary>
        public Balloon GetEntry(string entryName = null)
        {
            if (string.IsNullOrWhiteSpace(entryName)) return Balloons.First(x => x.type == Balloon.Type.Entry);

            for (int i = 0; i < Balloons.Count; i++)
            {
                var balloon = Balloons[i];
                if(balloon.type != Balloon.Type.Action) continue;
                if(balloon.actionType != Balloon.ActionType.SubEntry) continue;
                if(balloon.subEntryDataType != Balloon.SubEntryDataType.String) continue;
                if (balloon.subEntryTrigger == entryName) return balloon;
            }

            return null;
        }
        
        public Balloon GetEntry(OcData entryTrigger)
        {
            for (int i = 0; i < Balloons.Count; i++)
            {
                var balloon = Balloons[i];
                if (balloon.type != Balloon.Type.Action) continue;
                if (balloon.actionType != Balloon.ActionType.SubEntry) continue;
                if (balloon.subEntryDataType != Balloon.SubEntryDataType.OcData) continue;
                if (balloon.subEntryTriggerData == entryTrigger) return balloon;
            }

            return null;
        }

#if UNITY_EDITOR
        public event Action onValidate;
        public List<DialogueTrack> e_CutsceneReference;

        public Balloon AddBalloon(Balloon.Type type)
        {
            Balloon balloon;
            switch (type)
            {
                case Balloon.Type.Entry:
                    balloon = CreateInstance<Balloon>();
                    balloon.type = Balloon.Type.Entry;
                    balloon.GUID = $"{type}-{Guid.NewGuid()}";
                    // balloon.GUID = Guid.NewGuid().ToString();
                    balloon.position = new Vector2(20, 60);
                    balloon.text = "Entry";
                    balloon.waitTime = 2;
                    break;
                case Balloon.Type.Dialogue:
                    balloon = CreateInstance<Balloon>();
                    balloon.type = Balloon.Type.Dialogue;
                    balloon.GUID = $"{type}-{Guid.NewGuid()}";
                    // balloon.GUID = Guid.NewGuid().ToString();
                    balloon.text = "New Dialogue";
                    balloon.actor = MainNPC;
                    break;
                case Balloon.Type.Choice:
                    balloon = CreateInstance<Balloon>();
                    balloon.type = Balloon.Type.Choice;
                    balloon.GUID = $"{type}-{Guid.NewGuid()}";
                    // balloon.GUID = Guid.NewGuid().ToString();
                    balloon.text = "New Choice";
                    break;
                case Balloon.Type.Action:
                    balloon = CreateInstance<Balloon>();
                    balloon.type = Balloon.Type.Action;
                    // balloon.GUID = $"{type}-{Guid.NewGuid()}";
                    balloon.GUID = Guid.NewGuid().ToString();
                    balloon.text = "New Action";
                    break;
                default: goto case Balloon.Type.Dialogue;
            }

            var undoID = Undo.GetCurrentGroup();
            Undo.RegisterCreatedObjectUndo(balloon, "Add Balloon");
            Undo.RecordObject(this, "Add Balloon");
            balloon.name = balloon.GUID;
            Balloons.Add(balloon);
            AssetDatabase.AddObjectToAsset(balloon, this);
            
            Undo.CollapseUndoOperations(undoID);
            return balloon;
        }

        public void RemoveBalloon(Balloon balloon)
        {
            if(!Balloons.Contains(balloon)) return;
            if(balloon.type == Balloon.Type.Entry) return;
            var id = Undo.GetCurrentGroup();
            Undo.RecordObject(this, "Remove Balloon");
            Undo.RegisterFullObjectHierarchyUndo(this, "Remove Balloon");
            Balloons.Remove(balloon);
            AssetDatabase.RemoveObjectFromAsset(balloon);
            Undo.CollapseUndoOperations(id);
        }

        public void AddLinkData(LinkData linkData)
        {
            Undo.RecordObject(this, "AddLinkData");
            LinkData.Add(linkData);
            
            UpdateLinkedBalloonList();
        }

        public void RemoveLinkData(string from, string to)
        {
            var target = LinkData.Find(x => string.CompareOrdinal(@from, from) == 0 && string.CompareOrdinal(x.to, to) == 0);
            if(target == null) return;

            var rootBalloon = Balloons.Find(x => string.CompareOrdinal(x.GUID,from) == 0);
            var targetBalloon = Balloons.Find(x => string.CompareOrdinal(x.GUID, to) == 0);
            
            // 여러 노드와 엣지를 선택 후 삭제하면 이미 삭제되어 빈 노드가 전달될 수 있으므로 둘 다 있을때만 실행함.
            if(rootBalloon != null && targetBalloon != null) rootBalloon.linkedBalloons.Remove(targetBalloon);
            
            LinkData.Remove(target);
            
            EditorUtility.SetDirty(this);
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
        ValueDropdownList<OcData> GetNPCList() => DialogueAsset.Instance.GetNPCDropDown();

        
        [BoxGroup("유틸리티 메서드"), Button("서브에셋에 누락된 Balloon 추가하기")]
        void UpdateMissingSubAsset()
        {
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(this));
            for (int i = 0; i < Balloons.Count; i++)
            {
                if(subAssets.Contains(Balloons[i])) continue;
                if(Balloons[i] == null) continue;
                AssetDatabase.AddObjectToAsset(Balloons[i], this);
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
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
                var rootBalloon = Balloons.Find(x => string.CompareOrdinal(x.GUID, linkData.@from) == 0);
                var targetBalloon = Balloons.Find(x => string.CompareOrdinal(x.GUID, linkData.to) == 0);
                
                if(rootBalloon == null || targetBalloon == null) continue;
                
                if(rootBalloon.linkedBalloons.Contains(targetBalloon)) continue;
                
                rootBalloon.linkedBalloons.Add(targetBalloon);
            }

            foreach (var balloon in Balloons)
            {
                if(balloon.linkedBalloons == null || balloon.linkedBalloons.Count < 1) continue;
                balloon.linkedBalloons = balloon.linkedBalloons.OrderBy(x => x.position.y).ThenBy(x => x.position.x).ToList();
            }
        }
        
        [BoxGroup("유틸리티 메서드"), Button("말풍선의 GUID를 최신 형식으로 업데이트")]
        void UpdateBalloonGUID()
        {
            foreach (var balloon in Balloons)
            {
                EditorUtility.SetDirty(balloon);
                if(balloon.GUID.StartsWith(balloon.type.ToString()))continue;
                balloon.GUID = $"{balloon.type}-{balloon.GUID}";
                balloon.name = balloon.GUID;
            }

            foreach (var balloon in Balloons)
            {
                balloon.linkedBalloons = new List<Balloon>();
            }
            foreach (var linkData in LinkData)
            {
                var rootBalloon = Balloons.Find(x => x.GUID.Contains(linkData.from));
                linkData.from = rootBalloon.GUID;
                var targetBalloon = Balloons.Find(x => x.GUID.Contains(linkData.to));
                linkData.to = targetBalloon.GUID;
                
                if(rootBalloon.linkedBalloons.Contains(targetBalloon)) continue;
                
                rootBalloon.linkedBalloons.Add(targetBalloon);
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }


        [BoxGroup("유틸리티 메서드"), Button("AssetObject 삭제")]
        void RemoveAssetObject(ScriptableObject so)
        {
            AssetDatabase.RemoveObjectFromAsset(so);
        }

        [BoxGroup("유틸리티 메서드"), Button("사용하지 않는 이미지 참조 해제")]
        void ReleaseUnusedImage()
        {
            foreach (var balloon in Balloons)
            {
                balloon.ReleaseUnusedImage();
            }
        }

        [BoxGroup("유틸리티 메서드"), Button("서브에셋이 아닌 Balloon 제거")]
        void RemoveUnusedBalloons()
        {
            var undoID = Undo.GetCurrentGroup();
            Undo.RecordObject(this, "Remove Unused Balloons");
            var count = Balloons.Count;
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(this));
            for (int i = 0; i < count; i++)
            {
                if(Balloons[i] == null) continue;
                if(subAssets.Contains(Balloons[i])) continue;
                Debug.Log($"{this.DRT(true)} {Balloons[i].GUID} : {Balloons[i].text} 제거됨");
                Undo.DestroyObjectImmediate(Balloons[i]);
                Balloons[i] = null;
            }

            Balloons = Balloons.Where(x => x != null).ToList();
            Undo.CollapseUndoOperations(undoID);
        }
        [BoxGroup("유틸리티 메서드"), Button("사용하지 않는 LinkData 제거")]
        void RemoveUnusedLinkData()
        {
            var removeList = new List<LinkData>();
            var restoreList = new List<LinkData>();
            foreach (var linkData in LinkData)
            {
                if(FindBalloon(linkData.@from) == null || FindBalloon(linkData.to) == null)
                {
                    removeList.Add(linkData);
                    continue;
                }
                if(LinkData.Any(x => x.@from.equal(linkData.from) && x.to.equal(linkData.to) && x != linkData)) 
                    removeList.Add(linkData);
                if(restoreList.Count(x => x.@from.equal(linkData.@from) && x.to.equal(linkData.to)) == 0) 
                    restoreList.Add(linkData);
            }

            foreach (var linkData in removeList)
            {
                Debug.Log($"{this.DRT(true)} LinkData 제거됨 | from : {linkData.@from} | to : {linkData.to}");
                LinkData.Remove(linkData);
            }
            
            
            foreach (var linkData in restoreList)
            {
                LinkData.Add(linkData);
            }

            LinkData = LinkData.Distinct().ToList();
        }

        [BoxGroup("유틸리티 메서드"), Button("GUID로 Balloon선택")]
        void SelectBalloonFromGUID(string guid)
        {
            var balloon = Balloons.Find(x => string.CompareOrdinal(x.GUID, guid) == 0);
            if (balloon == null)
            {
                Debug.Log($"[{guid}] 해당 GUID를 가진 Balloon이 없음");
                return;
            }

            Selection.activeObject = balloon;
        }
        [BoxGroup("유틸리티 메서드"), Button("DialogueTrack Asset 생성")]
        void CreateTrackAsset()
        {
            var track = CreateInstance<DialogueTrack>();
            track.name = $"{key}_Track";

            var path = AssetDatabase.GetAssetPath(this).Replace($"{name}.asset", $"{track.name}.asset");
            AssetDatabase.CreateAsset(track, path);
        }
#endif
    }
}
