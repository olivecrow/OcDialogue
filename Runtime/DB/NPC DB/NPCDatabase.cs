using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "NPC Database", menuName = "Oc Dialogue/DB/NPC Database")]
    public class NPCDatabase : ScriptableObject
    {
        public static NPCDatabase Instance => DBManager.Instance.NpcDatabase;
#if UNITY_EDITOR
        [ColorPalette]public Color palette;
        /// <summary> Editor Only. 시스템 NPC </summary>
        public NPC DefaultNPC;
#endif
        public string[] Category;
        public List<NPC> NPCs = new List<NPC>();
        
#if UNITY_EDITOR
        /// <summary> Editor Only. 이름 수정 후 매칭할때 사용함. </summary>
        [HideInInspector]public bool isNameDirty;
        
        void OnValidate()
        {
            foreach (var npc in NPCs)
            {
                if(npc.name == npc.NPCName) continue;
                isNameDirty = true;
                break;
            }
        }
        
        [HorizontalGroup("Buttons"), Button(ButtonSizes.Medium), GUIColor(1, 1, 0), EnableIf("isNameDirty")]
        public void MatchAllNames()
        {
            foreach (var npc in NPCs)
            {
                if(npc.name == npc.NPCName) continue;
                npc.name = npc.NPCName;
            }

            isNameDirty = false;
            AssetDatabase.SaveAssets();
        }
        
        [HorizontalGroup("Buttons"), Button(ButtonSizes.Medium), GUIColor(0, 1, 1)]
        public void AddNPC()
        {
            var npc = CreateInstance<NPC>();

            npc.name = OcDataUtility.CalculateDataName("New NPC", NPCs.Select(x => x.NPCName));
            npc.NPCName = npc.name;
            NPCs.Add(npc);
            OcDataUtility.Repaint();
            var path = AssetDatabase.GetAssetPath(this).Replace($"{name}.asset", $"{npc.name}.asset");
            AssetDatabase.AddObjectToAsset(npc, this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        [Button]
        public void DeleteNPC(string npcName)
        {
            if(!EditorUtility.DisplayDialog("삭제?", "정말 해당 NPC를 삭제하겠습니까?", "OK", "Cancel"))
                return;
            var npc = NPCs.FirstOrDefault(x => x.NPCName == npcName);
            if (npc == null)
            {
                var path = AssetDatabase.GetAssetPath(this);
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(path).Select(x => x as NPC);
                npc = allAssets.FirstOrDefault(x => x.NPCName == npcName);
                if(npc == null)
                {
                    Debug.LogWarning($"해당 이름의 NPC가 없어서 삭제에 실패함 : {npcName}");
                    return;
                }
            }

            NPCs.Remove(npc);
            
            OcDataUtility.Repaint();
            AssetDatabase.RemoveObjectFromAsset(npc);
            NPCs = NPCs.Where(x => x != null).ToList();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }  
#endif
    }
}