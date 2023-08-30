using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class TextToDialogueWindow : OdinEditorWindow
    {
        [TextArea(10, 100)]public string text;
        Vector3 _startPos;
        public static void Open(Vector3 pos)
        {
            var wnd = GetWindow<TextToDialogueWindow>();
            wnd.Show();
            wnd._startPos = pos;
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            if (GUILayout.Button("Append"))
            {
                Append();
                Close();
            }
        }

        void Append()
        {
            if (DialogueEditorWindow.Instance == null)
            {
                Debug.LogWarning($"다이얼로그 에디터가 열려있어야함");
                return;
            }

            if (DialogueEditorWindow.Instance.Conversation == null)
            {
                Debug.LogWarning($"다이얼로그 에디터에서 대화가 선택되어있지 않음");
                return;
            }
            
            var editor = DialogueEditorWindow.Instance;
            var conv = editor.Conversation;
            
            
            var undoID = Undo.GetCurrentGroup();
            Undo.RecordObject(DialogueAsset.Instance, "텍스트를 대화로 변환");
            Undo.RecordObject(editor, "텍스트를 대화로 변환");
            Undo.RecordObject(conv, "텍스트를 대화로 변환");

            


            var lines = text.Split('\n');
            Vector3 pos = Vector3.zero;

            var offsetIdx = Vector2Int.zero;
            var offset = Vector3.zero;
            Balloon prevBalloon = null;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    offsetIdx.x = 0;
                    offsetIdx.y++;
                    
                    prevBalloon.waitTime = 2;
                }
                else
                {
                    Balloon balloon = conv.AddBalloon(Balloon.Type.Dialogue);
                    Undo.RegisterCreatedObjectUndo(balloon, "");
                    balloon.text = line;
                    balloon.actor = conv.MainNPC;

                    offset.x = offsetIdx.x * DialogueGraphView.DefaultNodeSize.x;
                    offset.y = offsetIdx.y * DialogueGraphView.DefaultNodeSize.y / 2;
                
                    pos = _startPos + offset;

                    balloon.position = pos;
                    
                    if (i != 0)
                    {
                        var linkData = new LinkData(prevBalloon.GUID, balloon.GUID);
                        conv.AddLinkData(linkData);
                    }
                    
                    prevBalloon = balloon;
                    offsetIdx.x++;
                }
            }
            
            Undo.CollapseUndoOperations(undoID);
            
            editor.ForceRepaint();
        }
    }
}