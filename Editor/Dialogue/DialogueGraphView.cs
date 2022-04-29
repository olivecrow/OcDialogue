using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using OcDialogue.DB;
using OcUtility;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

namespace OcDialogue.Editor
{
    public class DialogueGraphView : GraphView
    {
        public Conversation Conversation;
        public GridBackground grid;
        public List<DialogueNode> Nodes;
        public DialogueNode EntryNode;

        protected override bool canPaste => true;
        protected override bool canCopySelection => true;
        protected override bool canDuplicateSelection => true;
        protected override bool canCutSelection => false;
        public event Action<DialogueGraphViewChange> OnChanged;

        Vector2 _lastMousePosition;
        readonly Vector2 DefaultNodeSize = new Vector2(160, 200);
        List<Balloon> _copyBuffer;
        public DialogueGraphView(Conversation conversation)
        {
            // Debug.Log("[GraphView] New Instanciated");
            Conversation = conversation;
            InitOutline();
            CheckBalloons();
            DrawNodesAndEdges();
            RegisterCallback<ExecuteCommandEvent>(new EventCallback<ExecuteCommandEvent>(this.OnExecuteCommand));
            _copyBuffer = new List<Balloon>();
        }

        /// <summary> 배경이나 줌 상태 등, 개괄적인 것을 초기화함. </summary>
        void InitOutline()
        {
            // Debug.Log("[GraphView] InitOutline");
            SetupZoom(ContentZoomer.DefaultMinScale, 1.5f);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            grid = new GridBackground();
            styleSheets.Add(Resources.Load<StyleSheet>("GridBackground"));
            // 그냥 add로 하면 노드가 그리드 아래로 정렬돼서 insert로 함.
            Insert(0, grid);
            grid.StretchToParentSize();
            
            UpdateViewTransform(Conversation.lastViewPosition, Conversation.lastViewScale);
            graphViewChanged += OnGraphViewChanged;
            viewTransformChanged += g =>
            {
                Conversation.lastViewPosition = g.viewTransform.position;
                Conversation.lastViewScale = g.viewTransform.scale;
            };
        }
        
        GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            EditorUtility.SetDirty(Conversation);
            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    if (element is DialogueNode node)
                    {
                        node.Balloon.position = node.GetPosition().position;
                        EditorUtility.SetDirty(node.Balloon);
                    }
                }
                
            }
            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    var linkData = CreateLinkDataFromEdge(edge);
                    Conversation.AddLinkData(linkData);
                }
            }

            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is DialogueNode node)
                    {
                        if (node.Balloon.type == Balloon.Type.Entry)
                        {
                            CreateNode(node.Balloon, node.Balloon.position);
                        }
                        else Conversation.RemoveBalloon(node.Balloon);
                    }
                    else if (element is Edge edge)
                    {
                        var temp = CreateLinkDataFromEdge(edge);
                        Conversation.RemoveLinkData(temp.@from, temp.to);
                        
                        
                    }
                }
                Conversation.UpdateLinkedBalloonList();
            }

            var myChange = new DialogueGraphViewChange();
            myChange.built_in = change;
            OnChanged?.Invoke(myChange);
            return change;
        }

        /// <summary> Conversation을 새로 생성했을때, Balloons리스트와 Entry Balloon이 없는지 체크하고 새로 생성함. </summary>
        void CheckBalloons()
        {
            if (Conversation.Balloons == null)
            {
                Conversation.Balloons = new List<Balloon>();
                EditorUtility.SetDirty(Conversation);
            }

            if (Conversation.LinkData == null)
            {
                Conversation.LinkData = new List<LinkData>();
                EditorUtility.SetDirty(Conversation);
            }
            
            if (Conversation.Balloons.Count == 0)
            {
                Conversation.AddBalloon(Balloon.Type.Entry);
            }
        }

        /// <summary> Conversation에 있는 정보를 바탕으로 노드와 엣지를 그림. </summary>
        void DrawNodesAndEdges()
        {
            Nodes = new List<DialogueNode>();
            foreach (var balloon in Conversation.Balloons)
            {
                var node = CreateNode(balloon, balloon.position);
                if (node.Balloon.type == Balloon.Type.Entry) EntryNode = node;
            }

            foreach (var linkData in Conversation.LinkData)
            {
                AddEdge(linkData);
            }

        }

        public DialogueNode CreateBalloonAndNode(Balloon.Type type, Vector2 position)
        {
            var newBalloon = Conversation.AddBalloon(type);
            var node = CreateNode(newBalloon, position);

            var myChange = new DialogueGraphViewChange();
            myChange.createdNode = node;
            OnChanged?.Invoke(myChange);

            EditorUtility.SetDirty(newBalloon);
            EditorUtility.SetDirty(Conversation);

            return node;
        }
        
        /// <summary> Balloon 하나에 대응되는 노드 하나를 그림. </summary>
        public DialogueNode CreateNode(Balloon balloon, Vector2 position)
        {
            var node = new DialogueNode(balloon);
            AddElement(node);

            if(balloon.type != Balloon.Type.Entry)
            {
                node.SetPosition(new Rect(position, DefaultNodeSize));
                node.style.minWidth = DefaultNodeSize.x;
                balloon.position = position;
            }
            else
            {
                node.SetPosition(new Rect(position, new Vector2(150, DefaultNodeSize.y)));
            }

            Nodes.Add(node);
            return node;
        }

        /// <summary> 전달받은 edge의 정보를 바탕으로 linkData를 생성함. 생성만 하기 때문에 Conversation에 추가하는건 직접 해야함. </summary>
        public LinkData CreateLinkDataFromEdge(Edge edge)
        {
            var from = FindNode(edge.output).Balloon.GUID;
            var to = FindNode(edge.input).Balloon.GUID;
            var linkData = new LinkData()
            {
                @from = from,
                to = to
            };
            return linkData;
        }
        /// <summary> 선택된 노드에 연결된 노드를 생성함. </summary>
        public DialogueNode CreateLinkedNode(DialogueNode source, Balloon.Type balloonType)
        {
            var newBalloon = Conversation.AddBalloon(balloonType);
            var rect = source.GetPosition();
            if (source.Balloon.linkedBalloons == null) source.Balloon.linkedBalloons = new List<Balloon>();
            var node = CreateNode(newBalloon,  rect.position + new Vector2(rect.width + 10f, source.Balloon.linkedBalloons.Count * rect.height));
            var edge = source.OutputPort.ConnectTo(node.InputPort);
            
            var linkData = CreateLinkDataFromEdge(edge);
            Conversation.AddLinkData(linkData);
            
            AddElement(edge);
            
            EditorUtility.SetDirty(newBalloon);
            EditorUtility.SetDirty(Conversation);
            
            var myChange = new DialogueGraphViewChange();
            myChange.createdNode = node;
            OnChanged?.Invoke(myChange);
            
            ClearSelection();
            AddToSelection(node);
            return node;
        }
        
        Edge AddEdge(LinkData linkData)
        {
            var fromBalloon = Conversation.Balloons.Find(x => x.GUID == linkData.@from);
            var toBalloon = Conversation.Balloons.Find(x => x.GUID == linkData.to);
            var fromNode = FindNode(fromBalloon);
            var toNode = FindNode(toBalloon);
            if (fromNode == null || toNode == null)
            {
                Debug.LogError("[GraphView] linkData에 맞는 노드가 없음");
                return null;
            }
            var edge = fromNode.OutputPort.ConnectTo(toNode.InputPort);
            AddElement(edge);

            return edge;
        }


        Edge FindEdge(DialogueNode from, DialogueNode to)
        {
            return edges.FirstOrDefault(x => x.output == from.OutputPort && x.input == to.InputPort);
        }

        DialogueNode DuplicateNode(DialogueNode node)
        {
            var undo = Undo.GetCurrentGroup();
            Undo.RecordObject(Conversation, "Duplicate Node");
            var balloon = Conversation.AddBalloon(node.Balloon.type);
            var guid = balloon.GUID;
            Undo.RegisterCreatedObjectUndo(balloon, "DuplicateNode");
            EditorUtility.CopySerialized(node.Balloon, balloon);
            balloon.GUID = guid;
            var newNode = CreateNode(balloon, node.GetPosition().position + new Vector2(0, node.GetPosition().height));
            
            var inputBalloons = Conversation.FindInputBalloons(node.Balloon);
            foreach (var inputBalloon in inputBalloons)
            {
                var linkData = new LinkData(inputBalloon.GUID, balloon.GUID);
                Conversation.AddLinkData(linkData);
                AddEdge(linkData);
            }
            Undo.CollapseUndoOperations(undo);
            return newNode;
        }

        /// <summary> balloon을 가진 노드를 찾음. 없으면 null을 반환함. </summary>
        public DialogueNode FindNode(Balloon balloon)
        {
            foreach (var node in Nodes)
            {
                if (node.Balloon == balloon) return node;
            }

            if (balloon == null) Debug.LogError($"[GraphView] 빈 balloon이 전달됨");
            else Debug.LogError($"[GraphView] 해당되는 노드를 찾지 못 함. balloon => actor : {balloon.actor} | text : {balloon.text}");
            return null;
        }
        /// <summary> 인풋 혹은 아웃풋 포트가 일치하는 노드를 찾음. 없으면 null을 반환함. </summary>
        public DialogueNode FindNode(Port port)
        {
            foreach (var node in Nodes)
            {
                if (node.InputPort == port || node.OutputPort == port) return node; 
            }
            Debug.LogError($"[GraphView] 해당되는 노드를 찾지 못 함. port => type : {port.portType}");
            return null;
        }
        
        // 이 메서드를 오버라이드 하지 않으면 노드끼리 연결이 안 됨.
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatibles = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                    compatibles.Add(port);
            });
            return compatibles;
        }
        
        public override EventPropagation DeleteSelection()
        {
            foreach (var s in selection)
            {
                if(s is DialogueNode node) Conversation.RemoveBalloon(node.Balloon);
                else if(s is Edge edge)
                {
                    var fromNode = Nodes.Find(x => x.OutputPort == edge.output);
                    var toNode = Nodes.Find(x => x.InputPort == edge.input);
                    Conversation.RemoveLinkData(fromNode.Balloon.GUID, toNode.Balloon.GUID);
                }
            }
            return base.DeleteSelection();
        }
        

        public override void HandleEvent(EventBase evt)
        {
            base.HandleEvent(evt);
            if (evt.originalMousePosition != Vector2.zero)
            {
                _lastMousePosition =
                    (evt.originalMousePosition - (Vector2)viewTransform.position) /
                    viewTransform.scale.x + new Vector2(0, -DefaultNodeSize.y * 0.2f);
            }
        }

        public void OnSelectionChanged()
        {
            Selection.objects = selection.Where(x => x is DialogueNode)
                .Select(x => ((DialogueNode)x).Balloon).ToArray();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            DialogueNode selected = null;
            foreach (var node in Nodes)
            {
                if(node != evt.target) continue;
                selected = node;
                break;
            }
            if(selected != null) evt.menu.RemoveItemAt(0);

            evt.menu.AppendAction("Add Dialogue", a => 
            {
                if(selected != null) CreateLinkedNode(selected, Balloon.Type.Dialogue);
                else
                {
                    CreateBalloonAndNode(Balloon.Type.Dialogue, _lastMousePosition);
                }
            });
            evt.menu.AppendAction("Add Choice", a => 
                {
                    if (selected != null) CreateLinkedNode(selected, Balloon.Type.Choice);
                    else
                    {
                        CreateBalloonAndNode(Balloon.Type.Choice, _lastMousePosition);
                    }
                },
                a =>
                {
                    if (selected == null) return DropdownMenuAction.Status.Normal;
                    else
                        return selected.Balloon.type == Balloon.Type.Dialogue
                            ? DropdownMenuAction.Status.Normal
                            : DropdownMenuAction.Status.Disabled; 
                });
            evt.menu.AppendAction("Add Action", a => 
                {
                    if (selected != null) CreateLinkedNode(selected, Balloon.Type.Action);
                    else
                    {
                        CreateBalloonAndNode(Balloon.Type.Action, _lastMousePosition);
                    }
                });
            evt.menu.AppendSeparator();
            if (selected == null)
            {
                // 그리드에 대고 우클릭.
                evt.menu.AppendAction("Select Conversation", a =>
                {
                    EditorGUIUtility.PingObject(Conversation);
                    Selection.activeObject = Conversation;
                });
                evt.menu.AppendAction("Select Dialogue Asset", a =>
                {
                    EditorGUIUtility.PingObject(DialogueEditorWindow.Instance.Asset);
                    Selection.activeObject = DialogueEditorWindow.Instance.Asset;
                });
            }
            else
            {
                // 노드를 선택해서 우클릭.
                evt.menu.AppendAction("Copy",(a => CopyCallback()), 
                    (a => 
                        canCopySelection  && (selected.capabilities & Capabilities.Copiable) != 0 ? 
                            DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
                evt.menu.AppendAction("Paste",(a => PasteCallback()), 
                    (a => 
                        canPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
                evt.menu.AppendAction("Duplicate", a =>
                    {
                        CopyCallback();
                        PasteCallback();
                    }, 
                    canDuplicateSelection && (selected.capabilities & Capabilities.Copiable) != 0 ? 
                        DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

                evt.menu.AppendSeparator();
                //------
                evt.menu.AppendAction("Delete", a =>
                {
                    DeleteSelection();
                });
            }
            evt.menu.AppendSeparator();
            foreach (var db in DBManager.Instance.DBs)
            {
                var editor = UnityEditor.Editor.CreateEditor(db) as IDBEditor;
                editor.AddDialogueContextualMenu(evt, this);
            }
        }
        
        
        void OnExecuteCommand(ExecuteCommandEvent evt)
        {
            if (this.panel.GetCapturingElement(PointerId.mousePointerId) != null)
                return;
            if (evt.commandName == "Copy")
            {
                CopyCallback();
                evt.StopPropagation();
            }
            else if (evt.commandName == "Paste")
            {
                PasteCallback();
                evt.StopPropagation();
            }
            else if (evt.commandName == "Duplicate")
            {
                CopyCallback();
                PasteCallback();
                evt.StopPropagation();
            }
            evt.imguiEvent.Use();
        }

        void CopyCallback()
        {
            _copyBuffer.Clear();
            var selected = selection
                .FindAll(x => x is DialogueNode d && d.Balloon.type != Balloon.Type.Entry)
                .Cast<DialogueNode>()
                .ToList();

            var guidMatch = new Dictionary<string, string>();
            
            for (int i = 0; i < selected.Count; i++)
            {
                var copy = ScriptableObject.CreateInstance<Balloon>();
                EditorUtility.CopySerialized(selected[i].Balloon, copy);
                var newGUID = Guid.NewGuid().ToString();
                copy.GUID = newGUID;
                guidMatch[selected[i].Balloon.GUID] = newGUID;
                _copyBuffer.Add(copy);
            }

            try
            {
                foreach (var balloon in _copyBuffer)
                {
                    if (balloon.linkedBalloons.Count == 0) continue;
                    var newLinked = new List<Balloon>();
                    foreach (var linkedBalloon in balloon.linkedBalloons)
                    {
                        var newLinkedBalloon = _copyBuffer.First(x => x.GUID == guidMatch[linkedBalloon.GUID]);
                        newLinked.Add(newLinkedBalloon);
                    }

                    balloon.linkedBalloons = newLinked;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _copyBuffer.Clear();
                throw;
            }
            
        }

        new void PasteCallback()
        {
            if(_copyBuffer.Count == 0) return;
            
            var firstOriginal = _copyBuffer[0];
            var created = new List<DialogueNode>();
            var guidMatch = new Dictionary<string, string>();

            Undo.RecordObject(Conversation, "다이얼로그 에디터 Paste");
            try
            {
                for (int i = 0; i < _copyBuffer.Count; i++)
                {
                    var copy = ScriptableObject.CreateInstance<Balloon>();
                    // copyBuffer에 있는 인스턴스를 템플릿으로 사용해서 복사함.
                    EditorUtility.CopySerialized(_copyBuffer[i], copy);

                    var offset = copy.position - firstOriginal.position;
                    var position = _lastMousePosition + offset;
                    var node = CreateNode(copy, position);
                    var newGUID = Guid.NewGuid().ToString();

                    node.Balloon.name = newGUID;
                    node.Balloon.GUID = newGUID;
                    
                    created.Add(node);
                    guidMatch[_copyBuffer[i].GUID] = newGUID;
                }

                foreach (var node in created)
                {
                    Conversation.Balloons.Add(node.Balloon);
                    AssetDatabase.AddObjectToAsset(node.Balloon, Conversation);
                }
            }
            catch (Exception e)
            {
                // 도중에 에러가 발생할 경우, Conversation에 추가된 balloon을 삭제하고, GraphView에서도 삭제함
                Console.WriteLine(e);

                foreach (var node in created)
                {
                    Conversation.RemoveBalloon(node.Balloon);
                    if(Contains(node))RemoveElement(node);
                }
                Undo.ClearUndo(Conversation);
                throw;
            }
            
            var newLinkData = new List<LinkData>();

            try
            {
                foreach (var node in created)
                {
                    if(node.Balloon.linkedBalloons.Count == 0) continue;

                    var newLinked = new List<Balloon>();
                    foreach (var linkedBalloon in node.Balloon.linkedBalloons)
                    {
                        var newLinkedBalloon = created.Select(x => x.Balloon).First(x => x.GUID == guidMatch[linkedBalloon.GUID]);
                    
                        newLinked.Add(newLinkedBalloon);
                        var linkData = new LinkData(node.Balloon.GUID, newLinkedBalloon.GUID);
                        newLinkData.Add(linkData);
                        AddEdge(linkData);
                    }

                    node.Balloon.linkedBalloons = newLinked;
                }


                foreach (var linkData in newLinkData)
                {
                    Conversation.LinkData.Add(linkData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                foreach (var node in created)
                {
                    Conversation.RemoveBalloon(node.Balloon);
                    if(Contains(node))RemoveElement(node);
                }
                Undo.ClearUndo(Conversation);
                throw;
            }
            
            ClearSelection();
            foreach (var node in created)
            {
                AddToSelection(node);   
            }
        }
    }
}
