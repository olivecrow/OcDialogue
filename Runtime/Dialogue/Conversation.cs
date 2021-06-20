using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue
{
    public class Conversation : ScriptableObject
    {
[ReadOnly]public string GUID;
        [ValueDropdown("GetCategoryList")]public string Category;
        public string key;
        /// <summary> 에디터에서 참고용으로 사용되는 설명. 인게임에서는 등장하지 않음. </summary>
        [TextArea]public string description;
        public List<Balloon> Balloons;
        public List<LinkData> LinkData;
        // [ValueDropdown("GetNPCList")]public NPC MainActor;

        /// <summary> Editor Only. 그래프에서의 뷰 위치 </summary>
        [HideInInspector]public Vector3 lastViewPosition;
        /// <summary> Editor Only. 그래프의 줌 스케일 </summary>
        [HideInInspector]public Vector3 lastViewScale = new Vector3(1f, 1f, 1f);

#if UNITY_EDITOR
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
                    // balloon.actor = MainActor;
                    break;
                case Balloon.Type.Choice:
                    balloon = CreateInstance<Balloon>();
                    balloon.type = Balloon.Type.Choice;
                    balloon.GUID = Guid.NewGuid().ToString();
                    balloon.text = "New Choice";
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

        public void RemoveLinkData(string from, string to)
        {
            var target = LinkData.Find(x => x.@from == from && x.to == to);
            if(target == null) return;
            LinkData.Remove(target);
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        /// <summary> Conversations 필드에서 대화 이름을 드롭다운으로 보여주기위한 리스트를 반환함. (Odin Inspector용) </summary>
        ValueDropdownList<string> GetCategoryList()
        {
            var list = new ValueDropdownList<string>();
            Debug.Log($"DialogueAsset.Instance is null | {DialogueAsset.Instance == null}");
            Debug.Log($"Categories is null | {DialogueAsset.Instance.Categories == null}");
            foreach (var category in DialogueAsset.Instance.Categories)
            {
                list.Add(category, category);
            }

            return list;
        }
        
        /// <summary> MainActor 필드에서 NPC이름을 드롭다운으로 보여주기위한 리스트를 반환함. (Odin Inspector용) </summary>
        ValueDropdownList<NPC> GetNPCList()
        {
            var list = new ValueDropdownList<NPC>();
            foreach (var npc in NPCDatabase.Instance.NPCs)
            {
                list.Add(npc.NPCName, npc);
            }
        
            return list;
        }
#endif
    }
}
