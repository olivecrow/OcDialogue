using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.DB
{
    [CreateAssetMenu(fileName = "NPC DB", menuName = "Oc Dialogue/DB/NPC DB")]
    public class NPCDB : OcData, IStandardDB
    {
        public string[] CategoryRef => Category;
        public IEnumerable<OcData> AllData => NPCs;
        public override string Address => "NPCDB";
        public static NPCDB Instance => DBManager.Instance.NpcDatabase;
        [HideInInspector]public string[] Category;
        public List<NPC> NPCs;

        public void Load()
        {
            foreach (var npC in NPCs)
            {
                npC.GenerateRuntimeData();
            }
        }


#if UNITY_EDITOR
        [ColorPalette]public Color palette;
        void Reset()
        {
            if (Category == null || Category.Length == 0) Category = new[] {"Main"};
        }

        public NPC AddNPC(string category)
        {
            var npc = CreateInstance<NPC>();

            npc.Category = category;
            npc.name = OcDataUtility.CalculateDataName($"New {category} NPC", NPCs.Select(x => x.name));
            npc.SetParent(this);
            NPCs.Add(npc);
            OcDataUtility.Repaint();
            
            var assetFolderPath = AssetDatabase.GetAssetPath(this).Replace($"/{name}.asset", "");
            var targetFolderPath = OcDataUtility.CreateFolderIfNull(assetFolderPath, category);
            
            var path = targetFolderPath + $"/{npc.name}.asset";
            AssetDatabase.CreateAsset(npc, path);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            return npc;
        }
        
        public void DeleteNPC(string npcName)
        {
            var npc = NPCs.FirstOrDefault(x => x.name == npcName);
            if (npc == null)
            {
                var path = AssetDatabase.GetAssetPath(this);
                var allAssets = AssetDatabase.LoadAllAssetsAtPath(path).Select(x => x as NPC);
                npc = allAssets.FirstOrDefault(x => x.name == npcName);
                if(npc == null)
                {
                    Debug.LogWarning($"해당 이름의 NPC가 없어서 삭제에 실패함 : {npcName}");
                    return;
                }
            }

            NPCs.Remove(npc);
            
            OcDataUtility.Repaint();
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(npc));
            NPCs = NPCs.Where(x => x != null).ToList();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        /// <summary> 각종 문제들을 해결함. </summary>
        public void Resolve()
        {
            foreach (var npc in NPCs)
            {
                npc.SetParent(this);
                npc.Resolve();
            }

            AssetDatabase.SaveAssets();
        }
#endif
    }
}
