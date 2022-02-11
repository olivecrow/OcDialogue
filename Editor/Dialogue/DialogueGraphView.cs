using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcDialogue.DB;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
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

        Vector2 _lastMousePosition;
        readonly Vector2 DefaultNodeSize = new Vector2(160, 200);
        public DialogueGraphView(Conversation conversation)
        {
            // Debug.Log("[GraphView] New Instanciated");
            Conversation = conversation;
            InitOutline();
            CheckBalloons();
            DrawNodesAndEdges();
        }

        /// <summary> 배경이나 줌 상태 등, 개괄적인 것을 초기화함. </summary>
        void InitOutline()
        {
            // Debug.Log("[GraphView] InitOutline");
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

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
            EditorUtility.SetDirty(DialogueAsset.Instance);
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
                EditorUtility.SetDirty(Conversation);
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
            return change;
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
                evt.menu.AppendAction("Cut", (a => CutSelectionCallback()),
                    (a => 
                        canCutSelection  && (selected.capabilities & Capabilities.Copiable) != 0 ? 
                            DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
                evt.menu.AppendAction("Copy",(a => CopySelectionCallback()), 
                    (a => 
                        canCopySelection  && (selected.capabilities & Capabilities.Copiable) != 0 ? 
                            DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
                evt.menu.AppendAction("Paste",(a => PasteCallback()), 
                    (a => 
                        canPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
                evt.menu.AppendAction("Duplicate", a => DuplicateNode(selected), 
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
                DrawEdge(linkData);
            }
            
        }

        public DialogueNode CreateBalloonAndNode(Balloon.Type type, Vector2 position)
        {
            var newBalloon = Conversation.AddBalloon(type);
            var node = CreateNode(newBalloon, position);
                    
            EditorUtility.SetDirty(newBalloon);
            AssetDatabase.SaveAssets();

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
            var node = CreateNode(newBalloon,  rect.position + new Vector2(rect.width + 50f, source.Balloon.linkedBalloons.Count * rect.height));
            var edge = source.OutputPort.ConnectTo(node.InputPort);
            
            var linkData = CreateLinkDataFromEdge(edge);
            Conversation.AddLinkData(linkData);

            AddElement(edge);
            
            EditorUtility.SetDirty(newBalloon);
            AssetDatabase.SaveAssets();
            ClearSelection();
            AddToSelection(node);
            return node;
        }
        
        Edge DrawEdge(LinkData linkData)
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
                DrawEdge(linkData);
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
                _lastMousePosition = (evt.originalMousePosition - (Vector2) viewTransform.position) / viewTransform.scale.x + new Vector2(0, -DefaultNodeSize.y * 0.2f);
        }

        public void OnSelectionChanged()
        {
            Selection.objects = selection.Where(x => x is DialogueNode)
                .Select(x => ((DialogueNode)x).Balloon).ToArray();
        }
    }
}
