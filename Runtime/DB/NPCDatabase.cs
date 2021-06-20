using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "NPC Database", menuName = "Oc Dialogue/NPC Database")]
    public class NPCDatabase : ScriptableObject
    {
        public const string AssetPath = "NPC DB/NPC Database";
        public static NPCDatabase Instance => _instance;
        static NPCDatabase _instance;
        [ColorPalette]public Color palette;
        /// <summary> 시스템 NPC </summary>
        public NPC DefaultNPC;
        [TableList]public List<NPC> NPCs = new List<NPC>();
        
        [InitializeOnLoadMethod]
        static void Init()
        {
            _instance = Resources.Load<NPCDatabase>(AssetPath);
        }
#if UNITY_EDITOR
        [Button]
        public void AddNPC()
        {
            var npc = CreateInstance<NPC>();
            var sameNameCount = -1;
            if (NPCs == null || NPCs.Count == 0) sameNameCount = 0;
            else
            {
                sameNameCount = NPCs.Count(x => x.NPCName.Contains("New NPC"));
            }

            npc.name = $"New NPC {sameNameCount}";
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
