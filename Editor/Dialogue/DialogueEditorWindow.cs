using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
#if UNITY_2021_2_OR_NEWER
        public DropdownField ConversationField;
#else
        public PopupField<string> ConversationField;
#endif
        public Button AddConversationButton;
        public Button RemoveConversationButton;
        
        public Conversation Conversation;
        // public ToolbarPopupSearchField ConversationSearchField;
        public DialogueGraphView GraphView { get; private set; }

        public string[] CategoryRef
        {
            get => Asset.Categories;
            set => Asset.Categories = value;
        }

        string _currentCategory;
        static Dictionary<string, int> _lastViewConversation;

        static DialogueEditorWindow()
        {
            _lastViewConversation = new Dictionary<string, int>();
        }
        [MenuItem("OcDialogue/다이얼로그 에디터")]
        public static void Open()
        {
            DialogueEditorWindow wnd = GetWindow<DialogueEditorWindow>(false, "다이얼로그 에디터", true);
            wnd.minSize = new Vector2(720, 480);
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
            if(DialogueAsset.Instance == null) return;
            if(Conversation == null) return;
            if (ConversationField != null && ConversationField.text != Conversation.key) ConversationField.SetValueWithoutNotify(Conversation.key);
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
                    if (string.IsNullOrEmpty(_currentCategory)) _currentCategory = Asset.Categories[0];
                    AddConversation();
                    Close();
                    Open();
                });
                addButton.text = "Conversation 생성";
                rootVisualElement.Add(addButton);
                return;
            }

            GenerateCategoryToolbar();
            GenerateConversationToolbar(0);
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

            // TODO : 카테고리 별로 인덱스를 저장해서 그걸 불러오기.
            _currentCategory = t.text;
            var convIndex = _lastViewConversation.ContainsKey(_currentCategory) ? _lastViewConversation[_currentCategory] : 0;
            GenerateConversationToolbar(convIndex);
        }

        void GenerateConversationToolbar(int index)
        {
            Debug.Log($"On Generate Conversation Toolbar | Category : {_currentCategory} | index : {index}");
            if (ConversationSelectToolbar != null) rootVisualElement.Remove(ConversationSelectToolbar);
            ConversationSelectToolbar = new Toolbar();
            rootVisualElement.Add(ConversationSelectToolbar);

#if UNITY_2021_2_OR_NEWER
            ConversationField = new DropdownField(
#else
            ConversationField = new PopupField<string>(
                
#endif
                "Conversation",
                Asset.Conversations.Where(x => x.Category == _currentCategory).Select(x => x.key).ToList(),
                index, s =>
                {
                    // 이전 Conversation이 있다면, OnConversationChanged 콜백을 해제함.
                    if (Conversation != null)
                    {
                        Conversation.onValidate -= OnConversationChanged;
                    }
                    Conversation = Asset.Conversations.Find(x => x.key == s);
                    
                    // 이번 Conversation이 null이 아니면, OnConversationChanged 콜백을 할당함.
                    if (Conversation != null)
                    {
                        Conversation.onValidate += OnConversationChanged;
                    }
                    Debug.Log($"Generate Conversation Toolbar : key : {s}");
                    OnConversationChanged();
                    return Conversation == null ? null : Conversation.key;
                });
            

            if (Asset.Conversations.Find(x => x.Category == _currentCategory) == null)
            {
                var warning = new Label($"{_currentCategory} 카테고리에 해당되는 대화목록이 없음.");
                ConversationSelectToolbar.Add(warning);

                var addButton = new Button(AddConversation) {text = "Conversation 생성"};
                ConversationSelectToolbar.Add(addButton);
                ConversationField.value = null;
                return;
            }


            ConversationField.style.width = 400;
            ConversationSelectToolbar.Add(ConversationField);

            AddConversationButton = new Button(AddConversation);
            AddConversationButton.text = "+";
            ConversationSelectToolbar.Add(AddConversationButton);

            RemoveConversationButton = new Button(RemoveConversation);
            RemoveConversationButton.text = "Remove";
            RemoveConversationButton.style.backgroundColor = new Color(0.8f, 0f, 0f);
            ConversationSelectToolbar.Add(RemoveConversationButton);
        }
        /// <summary> DropDown에서 다른 Conversation으로 변경되었을때 호출됨. 근데 Conversation의 key를 변경해도 호출됨...? </summary>
        void OnConversationChanged()
        {
            Debug.Log("On Conversation Changed");
            if (Conversation != null)
            {
                var targetConvList = Asset.Conversations.Where(x => x.Category == _currentCategory).ToList();
                _lastViewConversation[_currentCategory] = targetConvList.IndexOf(Conversation);
                if(GraphView != null && rootVisualElement.Contains(GraphView)) rootVisualElement.Remove(GraphView);
                GraphView = new DialogueGraphView(Conversation);
                rootVisualElement.Add(GraphView);
                GraphView.StretchToParentSize();
                GraphView.SendToBack();
                Selection.activeObject = Conversation;
            }
            else
            {
                if(GraphView != null && rootVisualElement.Contains(GraphView)) rootVisualElement.Remove(GraphView);
            }
        }

        void AddConversation()
        {
            var categoryIndex = Asset.Categories.ToList().IndexOf(_currentCategory);
            var conv = Asset.AddConversation(categoryIndex);
            conv.Category = _currentCategory;
            
            var targetConvList = Asset.Conversations.Where(x => x.Category == _currentCategory).ToList();
            var idx = targetConvList.IndexOf(conv);
            GenerateConversationToolbar(idx);
        }

        void RemoveConversation()
        {
            if(Conversation == null) return;
            if(EditorUtility.DisplayDialog("Remove Conversation", "이 Conversation을 삭제하시겠습니까? 되돌릴 수 없는 작업입니다.", "안돼!", "질러!")) return;
            Asset.RemoveConversation(Conversation);
            GenerateConversationToolbar(0);
        }
        
    }
}
