using System.Collections.Generic;
using OcDialogue.DB;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class DBEditorWindowV2 : OdinMenuEditorWindow
    {
        List<string> _DBNames;
        IDBEditor _CurrentDBEditor;

        int CurrentSelectedDBIndex
        {
            get => __currentSelectedDBIndex;
            set
            {
                var isNew = value != __currentSelectedDBIndex;
                __currentSelectedDBIndex = value;
                if(isNew)
                {
                    UpdateDBEditor();
                    ForceMenuTreeRebuild();
                }
            }
        }
        int __currentSelectedDBIndex;
        
        [MenuItem("OcDialogue/DB 에디터 V2")]
        static void Open()
        {
            var window = GetWindow<DBEditorWindowV2>("DB 에디터 V2", true, typeof(SceneView));
            window.minSize = new Vector2(720, 480);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _DBNames = new List<string>();
            _DBNames.Add(GameProcessDB.Instance.Address);
            _DBNames.Add(ItemDatabase.Instance.Address);
            _DBNames.Add(QuestDB.Instance.Address);
            _DBNames.Add(NPCDB.Instance.Address);
            _DBNames.Add(EnemyDB.Instance.Address);
            foreach (var externalDB in DBManager.Instance.ExternalDBs)
            {
                _DBNames.Add(externalDB.Address);
            }

            UpdateDBEditor();
        }

        protected override void OnBeginDrawEditors()
        {
            GUI.color = new Color(1f, 1f, 2f);
            CurrentSelectedDBIndex = OcEditorUtility.DrawCategory(CurrentSelectedDBIndex, _DBNames, GUILayout.Height(40));
            GUI.color = Color.white;
            GUILayout.Label(_DBNames[CurrentSelectedDBIndex], new GUIStyle(GUI.skin.label){fontSize = 20, normal = {textColor = Color.white}});
            
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            _CurrentDBEditor?.AddTreeMenu(tree);

            return tree;
        }

        void UpdateDBEditor()
        {
            if(_CurrentDBEditor != null) DestroyImmediate(_CurrentDBEditor as UnityEditor.Editor);
            _CurrentDBEditor = CurrentSelectedDBIndex switch
            {
                0 => UnityEditor.Editor.CreateEditor(GameProcessDB.Instance) as IDBEditor,
                1 => UnityEditor.Editor.CreateEditor(ItemDatabase.Instance) as IDBEditor,
                _ => UnityEditor.Editor.CreateEditor(DBManager.Instance.ExternalDBs[__currentSelectedDBIndex - 5]) as IDBEditor
            };
        }
    }
}