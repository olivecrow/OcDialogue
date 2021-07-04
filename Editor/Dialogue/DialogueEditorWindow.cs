using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace OcDialogue.Editor
{
    public class DialogueEditorWindow : EditorWindow
    {
        public static DialogueEditorWindow Instance => _instance;
        static DialogueEditorWindow _instance;
        public DialogueAsset Asset => DialogueAsset.Instance;

        public Toolbar CategoryToolbar;
        public List<ToolbarToggle> CategoryToggles;
        
        public Toolbar ConversationSelectToolbar;
        public DropdownField ConversationField;
        public Button AddConversationButton;
        
        public Conversation Conversation;
        // public ToolbarPopupSearchField ConversationSearchField;
        public DialogueGraphView GraphView { get; private set; }

        string _currentCategory;
        const string lastConversationKey = "OcDialogue_LastConversation";
        [MenuItem("Tools/다이얼로그 에디터")]
        public static void Open()
        {
            DialogueEditorWindow wnd = GetWindow<DialogueEditorWindow>();
            wnd.titleContent = new GUIContent("Dialogue Editor Window");
        }
        
        [MenuItem("Tools/다이얼로그 에디터 닫기")]
        public static void CloseWindow()
        {
            if(_instance != null) _instance.Close();
            else
            {
                DialogueEditorWindow wnd = GetWindow<DialogueEditorWindow>();
                wnd.Close();
            }
        }
        void OnEnable()
        {
            if(_instance != null) _instance.Close();
            _instance = this;
        }

        void OnDisable()
        {
            if (_instance == this) _instance = null;
        }

        void OnInspectorUpdate()
        {
            if (ConversationField.text != Conversation.key) ConversationField.SetValueWithoutNotify(Conversation.key);
        }

        void CreateGUI()
        {
            if (Asset == null)
            {
                var warning = new Label(
                    "Dialogue Asset이 없음\n" +
                    "에셋을 만든 후, DB Manager에 등록할 것.");
                rootVisualElement.Add(warning);
                return;
            }
            if (Asset.Conversations == null || Asset.Conversations.Count == 0)
            {
                var warning = new Label("Conversation이 한 개도 없음.");
                rootVisualElement.Add(warning);
                var addButton = new Button(() =>
                {
                    Asset.AddConversation();
                    Close();
                    Open();
                });
                addButton.text = "Conversation 생성";
                rootVisualElement.Add(addButton);
                return;
            }
            
            if (EditorPrefs.HasKey(lastConversationKey))
            {
                var conversationKey = EditorPrefs.GetString(lastConversationKey);
                Conversation = Asset.Conversations.Find(x => x.key == conversationKey);
            }
            else
            {
                Conversation = Asset.Conversations.Count > 0 ? Asset.Conversations[0] : null;
            }
            GenerateCategoryToolbar();
            GenerateConversationToolbar();
        }

        void GenerateCategoryToolbar()
        {
            if(CategoryToolbar != null) rootVisualElement.Remove(CategoryToolbar);
            CategoryToolbar = new Toolbar();
            rootVisualElement.Add(CategoryToolbar);

            if (string.IsNullOrEmpty(_currentCategory))
            {
                _currentCategory = Asset.Categories[0];
            }
            
            CategoryToggles = new List<ToolbarToggle>();
            foreach (var category in Asset.Categories)
            {
                var toggle = new ToolbarToggle();
                toggle.text = category;
                if (category == _currentCategory) toggle.value = true;
                else toggle.value = false;
                toggle.RegisterValueChangedCallback(e =>
                {
                    CheckCategoryToggles(toggle, e.newValue);
                });
                CategoryToggles.Add(toggle);
                CategoryToolbar.Add(toggle);
            }
        }

        void CheckCategoryToggles(ToolbarToggle t, bool isOn)
        {
            if (!isOn)
            {
                t.SetValueWithoutNotify(true);
                return;
            }
            foreach (var toggle in CategoryToggles)
            {
                if (toggle.text == _currentCategory)
                {
                    toggle.SetValueWithoutNotify(false);
                    break;
                }
            }

            _currentCategory = t.text;
            GenerateConversationToolbar();
        }

        void GenerateConversationToolbar()
        {
            Debug.Log($"On Generate Conversation Toolbar | Category : {_currentCategory}");
            if(ConversationSelectToolbar != null) rootVisualElement.Remove(ConversationSelectToolbar);
            ConversationSelectToolbar = new Toolbar();
            rootVisualElement.Add(ConversationSelectToolbar);
            
            if (Asset.Conversations.Find(x => x.Category == _currentCategory) == null)
            {
                var warning = new Label($"{_currentCategory} 카테고리에 해당되는 대화목록이 없음.");
                ConversationSelectToolbar.Add(warning);

                var addButton = new Button(AddConversation){text = "Conversation 생성"};
                ConversationSelectToolbar.Add(addButton);
                ConversationField.value = null;
                return;
            }
            ConversationField = new DropdownField(
                "Conversation",
                Asset.Conversations.Where(x => x.Category == _currentCategory).Select(x => x.key).ToList(),
                0, s =>
                {
                    Conversation = Asset.Conversations.Find(x => x.key == s);
                    OnConversationChanged();
                    return Conversation == null ? null : Conversation.key;
                });
            
            ConversationField.style.width = 400;
            ConversationSelectToolbar.Add(ConversationField);

            AddConversationButton = new Button(AddConversation);
            AddConversationButton.text = "+";
            ConversationSelectToolbar.Add(AddConversationButton);
        }
        void OnConversationChanged()
        {
            Debug.Log("On Conversation Changed");
            if (Conversation != null)
            {
                EditorPrefs.SetString(lastConversationKey, Conversation.key);
                if(GraphView != null && rootVisualElement.Contains(GraphView)) rootVisualElement.Remove(GraphView);
                GraphView = new DialogueGraphView(Conversation);
                rootVisualElement.Add(GraphView);
                GraphView.StretchToParentSize();
                GraphView.SendToBack();
                Selection.activeObject = Conversation;
            }
            else
            {
                EditorPrefs.DeleteKey(lastConversationKey);
                rootVisualElement.Remove(GraphView);
            }
        }

        void AddConversation()
        {
            var conv = Asset.AddConversation();
            conv.Category = _currentCategory;
            Conversation = conv;
            GenerateConversationToolbar();
        }
    }
}
