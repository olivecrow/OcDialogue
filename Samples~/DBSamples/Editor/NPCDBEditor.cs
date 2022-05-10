using System.Collections.Generic;
using System.Linq;
using OcDialogue;
using OcDialogue.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyDB.Editor
{
    [CustomEditor(typeof(NPCDB))]
    public class NPCDBEditor : DBEditorBase
    {
        NPC CurrentNPC => SelectedData as NPC;

        NPCDB NpcDB
        {
            get
            {
                if(_npcDB == null) _npcDB = target as NPCDB;
                return _npcDB;
            }
        }
        NPCDB _npcDB;
        public override void DrawToolbar()
        {
            serializedObject.Update();
            GUI.color = new Color(0.5f, 2f, 0.7f);
            CurrentCategoryIndex =
                OcEditorUtility.DrawCategory(CurrentCategoryIndex, NpcDB.Categories, GUILayout.Height(25));
            GUI.color = Color.white;

            SirenixEditorGUI.BeginHorizontalToolbar();
            
            if(SirenixEditorGUI.ToolbarButton("Select DB")) EditorGUIUtility.PingObject(NpcDB);
            if (SirenixEditorGUI.ToolbarButton("Edit Category"))
            {
                CategoryEditWindow.Open(NpcDB.Categories, 
                    t => OcEditorUtility.EditCategory(NpcDB, t, c => AddNPC(c), d => DeleteNPC(d as NPC)));
            }

            if (SirenixEditorGUI.ToolbarButton("Create"))
            {
                var npc = AddNPC(CurrentCategory);
                Window.ForceMenuTreeRebuild();
                Window.MenuTree.Selection.Add(Window.MenuTree.MenuItems.Find(x => x.Value as NPC == npc));
            }

            if (CurrentNPC != null && SirenixEditorGUI.ToolbarButton("Delete"))
            {
                DeleteNPC(CurrentNPC);
                Window.ForceMenuTreeRebuild();
                SelectCategoryLastItem();
            }
            
            SirenixEditorGUI.EndHorizontalToolbar();
            serializedObject.ApplyModifiedProperties();
        }

        public override string[] GetCSVFields()
        {
            return new[] {"분류", "이름", "설명"};
        }

        public override IEnumerable<string[]> GetCSVData()
        {
            foreach (var npc in _npcDB.NPCs.OrderBy(x => x.Category).ThenBy(x => x.Name))
            {
                yield return new[] { npc.Category, npc.name, npc.Description };
            }
        }

        public override IEnumerable<LocalizationCSVRow> GetLocalizationData()
        {
            foreach (var npc in NpcDB.NPCs.OrderBy(x => x.Category).ThenBy(x => x.name))
            {
                var nameRow = new LocalizationCSVRow
                {
                    key = npc.name,
                    korean = npc.name
                };
                yield return nameRow;

                var descriptionRow = new LocalizationCSVRow();
                descriptionRow.key = $"{npc.name}_Description";
                descriptionRow.korean = npc.Description;
                
                yield return descriptionRow;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("NPCs"));
            
            if(CurrentNPC != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"[{CurrentCategory} NPC] {CurrentNPC.name}",
                    new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold }, GUILayout.Height(22));
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

        public NPC AddNPC(string category)
        {
            var npc = CreateInstance<NPC>();
            npc.Category = category;
            npc.name = OcDataUtility.CalculateDataName($"New {category} NPC", _npcDB.NPCs.Select(x => x.name));
            npc.SetParent(_npcDB);
            OcDataUtility.Repaint();
            
            var assetFolderPath = AssetDatabase.GetAssetPath(_npcDB).Replace($"/{_npcDB.name}.asset", "");
            var targetFolderPath = OcDataUtility.CreateFolderIfNull(assetFolderPath, category);
            
            var path = targetFolderPath + $"/{npc.name}.asset";
            AssetDatabase.CreateAsset(npc, path);
            _npcDB.NPCs.Add(npc);
            EditorUtility.SetDirty(_npcDB);
            AssetDatabase.SaveAssets();

            return npc;
        }
        
        public void DeleteNPC(NPC npc)
        {
            _npcDB.NPCs.Remove(npc);
            
            OcDataUtility.Repaint();
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(npc));
            
            EditorUtility.SetDirty(_npcDB);
            AssetDatabase.SaveAssets();
        }
    }
}