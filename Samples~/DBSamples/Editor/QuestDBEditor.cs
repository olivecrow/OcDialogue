using System;
using System.Collections.Generic;
using System.Linq;
using OcDialogue;
using OcDialogue.DB;
using OcDialogue.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyDB.Editor
{
    [CustomEditor(typeof(QuestDB))]
    public sealed class QuestDBEditor : DBEditorBase
    {
        Quest CurrentQuest => SelectedData as Quest;

        QuestDB QuestDB
        {
            get
            {
                if(_questDB == null) _questDB = target as QuestDB;
                return _questDB;
            }
        }
        QuestDB _questDB;
        protected override void OnEnable()
        {
            base.OnEnable();
            foreach (var quest in QuestDB.Quests)
            {
                foreach (var checkerFactor in quest.Checker.factors)
                {
                    if(checkerFactor.parent == quest) continue;
                    checkerFactor.parent = quest;
                    EditorUtility.SetDirty(quest);
                }
                
            }
            AssetDatabase.SaveAssets();
        }

        public override void DrawToolbar()
        {
            serializedObject.Update();
            GUI.color = new Color(0.5f, 2f, 0.7f);
            CurrentCategoryIndex =
                OcEditorUtility.DrawCategory(CurrentCategoryIndex, QuestDB.Categories, GUILayout.Height(25));
            GUI.color = Color.white;

            SirenixEditorGUI.BeginHorizontalToolbar();
            
            if(SirenixEditorGUI.ToolbarButton("Select DB")) EditorGUIUtility.PingObject(QuestDB);
            if (SirenixEditorGUI.ToolbarButton("Edit Category"))
            {
                CategoryEditWindow.Open(QuestDB.Categories, 
                    t => OcEditorUtility.EditCategory(QuestDB, t, c => AddQuest(c), d => DeleteQuest(d as Quest)));
            }

            if (SirenixEditorGUI.ToolbarButton("Create"))
            {
                var quest = AddQuest(CurrentCategory);
                Window.ForceMenuTreeRebuild();
                Window.MenuTree.Selection.Add(Window.MenuTree.MenuItems.Find(x => x.Value as Quest == quest));
            }

            if (CurrentQuest != null && SirenixEditorGUI.ToolbarButton("Delete"))
            {
                DeleteQuest(CurrentQuest);
                Window.ForceMenuTreeRebuild();
                SelectCategoryLastItem();
            }
            
            SirenixEditorGUI.EndHorizontalToolbar();
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Quests"));
            if(CurrentQuest != null)
            {
                EditorGUILayout.LabelField($"[{CurrentCategory} Quest] {CurrentQuest.name}",
                    new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold }, GUILayout.Height(22));
            }

            serializedObject.ApplyModifiedProperties();
        }
        Quest AddQuest(string category)
        {
            var quest = CreateInstance<Quest>();
            Undo.RecordObject(quest, "Add Data");
            quest.category = category;
            quest.name = OcDataUtility.CalculateDataName($"New {category} Quest", QuestDB.Quests.Select(x => x.name));
            quest.SetParent(QuestDB);
            if (quest.dataRowContainer == null) quest.dataRowContainer = new DataRowContainer();
            quest.dataRowContainer.Parent = quest;
            EditorUtility.SetDirty(quest);
            
            var dbFolderPath = AssetDatabase.GetAssetPath(QuestDB).Replace($"/{QuestDB.name}.asset", "");
            OcDataUtility.CreateFolderIfNull(dbFolderPath, quest.category);

            var path = AssetDatabase.GetAssetPath(QuestDB)
                .Replace($"{QuestDB.name}.asset", $"{quest.category}/{quest.name}.asset");
            Debug.Log($"[Quest] 에셋 생성 | category : {category} | path : {path}");
            QuestDB.Quests.Add(quest);
            AssetDatabase.CreateAsset(quest, path);
            AssetDatabase.SaveAssets();
            return quest;
        }

        void DeleteQuest(Quest quest)
        {
            var path = AssetDatabase.GetAssetPath(quest);
            QuestDB.Quests.Remove(quest);
            Debug.Log($"[Quest] 에셋 삭제 | category : {quest.category} | path : {path}");
            AssetDatabase.DeleteAsset(path);
            EditorUtility.SetDirty(QuestDB);
            AssetDatabase.SaveAssets();
        }
        
        public override void AddDialogueContextualMenu(ContextualMenuPopulateEvent evt, DialogueGraphView graphView)
        {
            if(graphView.selection.Count == 0) return;
            var selected = graphView.selection[0] as DialogueNode;
            if(selected == null) return;
            evt.menu.AppendAction("퀘스트 분기 노드 추가", a => CreateQuestDialogueNodes(a, graphView));
        }

        void CreateQuestDialogueNodes(DropdownMenuAction action, DialogueGraphView graphView)
        {
            var selected = graphView.selection[0] as DialogueNode;
            if(selected == null) return;
            
            var selector = DataSelectWindow.Open(null, null);
            selector.CurrentDB = QuestDB;
            selector.OnDataSelected += data =>
            {
                if(!(data is Quest)) return;
                Rect sourceRect;
                try
                {
                    sourceRect = selected.GetPosition();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    sourceRect = new Rect(0,0,200,200);
                    // throw;
                }
                
                
                var beforeQuestNode = graphView.CreateLinkedNode(selected, Balloon.Type.Dialogue);
                beforeQuestNode.Balloon.useChecker = true;
                beforeQuestNode.Balloon.checker = new DataChecker();
                beforeQuestNode.Balloon.checker.factors = new[] { new CheckFactor() };
                beforeQuestNode.Balloon.checker.factors[0].TargetData = data;
                beforeQuestNode.Balloon.checker.factors[0].detail = Quest.fieldName_QuestState;
                beforeQuestNode.Balloon.checker.factors[0].StringValue = QuestState.None.ToString();
                beforeQuestNode.RefreshIcons();
                beforeQuestNode.Balloon.text = "퀘스트 수락 전";
                beforeQuestNode.Balloon.description = "퀘스트 수락 전";
                
                var acceptQuestNode = graphView.CreateBalloonAndNode(Balloon.Type.Dialogue, 
                    sourceRect.position + new Vector2(sourceRect.width * 2 + 200f, sourceRect.position.y - sourceRect.height));
                {
                    var edge = beforeQuestNode.OutputPort.ConnectTo(acceptQuestNode.InputPort);
                    var linkData = graphView.CreateLinkDataFromEdge(edge);
                    graphView.Conversation.AddLinkData(linkData);
                    graphView.AddElement(edge);
                }
                acceptQuestNode.Balloon.useSetter = true;
                acceptQuestNode.Balloon.setters = new[] { new DataSetter() };
                acceptQuestNode.Balloon.setters[0].TargetData = data;
                acceptQuestNode.Balloon.setters[0].detail = Quest.fieldName_QuestState;
                acceptQuestNode.Balloon.setters[0].StringValue = QuestState.WorkingOn.ToString();
                acceptQuestNode.RefreshIcons();
                acceptQuestNode.Balloon.text = "퀘스트 수락";
                acceptQuestNode.Balloon.description = "퀘스트 수락";
            
                var workingOnQuestNode = graphView.CreateLinkedNode(selected, Balloon.Type.Dialogue);
                workingOnQuestNode.Balloon.useChecker = true;
                workingOnQuestNode.Balloon.checker = new DataChecker();
                workingOnQuestNode.Balloon.checker.factors = new[] { new CheckFactor(), new CheckFactor() };
                workingOnQuestNode.Balloon.checker.factors[0].TargetData = data;
                workingOnQuestNode.Balloon.checker.factors[0].detail = Quest.fieldName_QuestState;
                workingOnQuestNode.Balloon.checker.factors[0].StringValue = QuestState.WorkingOn.ToString();
            
                workingOnQuestNode.Balloon.checker.factors[1].TargetData = data;
                workingOnQuestNode.Balloon.checker.factors[1].detail = Quest.fieldName_IsAbleToClear;
                workingOnQuestNode.Balloon.checker.factors[1].BoolValue = false;
                workingOnQuestNode.RefreshIcons();
                workingOnQuestNode.Balloon.text = "퀘스트 진행중.";
                workingOnQuestNode.Balloon.description = "퀘스트 진행중.";
            
                var clearableQuestNode = graphView.CreateLinkedNode(selected, Balloon.Type.Dialogue);
                clearableQuestNode.Balloon.useChecker = true;
                clearableQuestNode.Balloon.checker = new DataChecker();
                clearableQuestNode.Balloon.checker.factors = new[] { new CheckFactor(), new CheckFactor() };
                clearableQuestNode.Balloon.checker.factors[0].TargetData = data;
                clearableQuestNode.Balloon.checker.factors[0].detail = Quest.fieldName_QuestState;
                clearableQuestNode.Balloon.checker.factors[0].StringValue = QuestState.WorkingOn.ToString();

                clearableQuestNode.Balloon.checker.factors[1].TargetData = data;
                clearableQuestNode.Balloon.checker.factors[1].detail = Quest.fieldName_IsAbleToClear;
                clearableQuestNode.Balloon.checker.factors[1].BoolValue = true;
                clearableQuestNode.RefreshIcons();
                clearableQuestNode.Balloon.text = "퀘스트 진행중. 클리어 가능.";
                clearableQuestNode.Balloon.description = "퀘스트 진행중. 클리어 가능.";
                
                
                var clearQuestNode = graphView.CreateBalloonAndNode(Balloon.Type.Dialogue, 
                    sourceRect.position + new Vector2(sourceRect.width * 2 + 200f, sourceRect.position.y + sourceRect.height));
                {
                    var edge = clearableQuestNode.OutputPort.ConnectTo(clearQuestNode.InputPort);
                    var linkData = graphView.CreateLinkDataFromEdge(edge);
                    graphView.Conversation.AddLinkData(linkData);
                    graphView.AddElement(edge);
                }
                clearQuestNode.Balloon.useSetter = true;
                clearQuestNode.Balloon.setters = new[] { new DataSetter() };
                clearQuestNode.Balloon.setters[0].TargetData = data;
                clearQuestNode.Balloon.setters[0].detail = Quest.fieldName_QuestState;
                clearQuestNode.Balloon.setters[0].StringValue = QuestState.Done.ToString();
                clearQuestNode.RefreshIcons();
                clearQuestNode.Balloon.text = "퀘스트 완료";
                clearQuestNode.Balloon.description = "퀘스트 완료";
                
            
                var finishQuestNode = graphView.CreateLinkedNode(selected, Balloon.Type.Dialogue);
                finishQuestNode.Balloon.useChecker = true;
                finishQuestNode.Balloon.checker = new DataChecker();
                finishQuestNode.Balloon.checker.factors = new[] { new CheckFactor() };
                finishQuestNode.Balloon.checker.factors[0].TargetData = data;
                finishQuestNode.Balloon.checker.factors[0].detail = Quest.fieldName_QuestState;
                finishQuestNode.Balloon.checker.factors[0].StringValue = QuestState.Done.ToString();
                finishQuestNode.RefreshIcons();
                finishQuestNode.Balloon.text = "퀘스트 완료 후";
                finishQuestNode.Balloon.description = "퀘스트 완료 후";
            };
        }


        /// <summary> 각종 문제들을 해결함. </summary>
        public void Resolve()
        {
            var undo = Undo.GetCurrentGroup();
            Undo.RecordObject(QuestDB, "Quest DB Resolve");
            foreach (var quest in QuestDB.Quests)
            {
                quest.SetParent(QuestDB);
                quest.Resolve();
                Undo.RecordObject(quest, "Quest DB Resolve");
                Undo.CollapseUndoOperations(undo);
            }

            QuestDB.Quests = QuestDB.Quests.OrderBy(x => x.e_order + x.Name).ToList();
            
            AssetDatabase.SaveAssets();
        }
    }
}