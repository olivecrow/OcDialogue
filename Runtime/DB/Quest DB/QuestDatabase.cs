using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace OcDialogue
{
    [CreateAssetMenu(fileName = "Quest Database", menuName = "Oc Dialogue/DB/Quest Database")]
    public class QuestDatabase : ScriptableObject
    {
        public static QuestDatabase Instance => DBManager.Instance.QuestDatabase;
        [ShowIf("IsEditingCategory"), InlineButton("DoneEditCategory", "Apply")] public string[] Category;
        [InlineEditor(InlineEditorModes.FullEditor)]public List<Quest> Quests;
        
        #if UNITY_EDITOR

        /// <summary> Editor Only. 에디터에서 카테고리 목록별로 볼 때 사용. </summary>
        [ShowInInspector, InlineButton("EditCategory", "Edit")]
        [HideIf("IsEditingCategory")]
        [ValueDropdown("GetCategoryList"), PropertyOrder(-100), HideInInspector] public string CurrentCategory;
        bool IsEditingCategory { get; set; }

        void EditCategory() => IsEditingCategory = true;
        void DoneEditCategory() => IsEditingCategory = false;

        ValueDropdownList<string> GetCategoryList()
        {
            var list = new ValueDropdownList<string>();
            foreach (var s in Category)
            {
                list.Add(s);
            }

            return list;
        }

        [HorizontalGroup("Buttons"), Button(ButtonSizes.Medium), GUIColor(0, 1, 1)]
        public void AddQuest()
        {
            var asset = CreateInstance<Quest>();

            asset.name = OcDataUtility.CalculateDataName("New Quest", Quests.Select(x => x.key));
            asset.key = asset.name;
            asset.Category = CurrentCategory;
            Quests.Add(asset);
            OcDataUtility.Repaint();
            var path = AssetDatabase.GetAssetPath(this).Replace($"{name}.asset", $"{asset.name}.asset");
            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        [HorizontalGroup("Buttons"), Button]
        public void DeleteQuest(string key)
        {
            if(!EditorUtility.DisplayDialog("삭제?", "정말 해당 Quest를 삭제하겠습니까?", "OK", "Cancel"))
                return;
            var asset = Quests.FirstOrDefault(x => x.key == key);
            if (asset == null)
            {
                Debug.LogWarning($"해당 이름의 Quest가 없어서 삭제에 실패함 : {key}");
                return;
            }

            Quests.Remove(asset);
            
            OcDataUtility.Repaint();
            var path = AssetDatabase.GetAssetPath(asset);
            AssetDatabase.DeleteAsset(path);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        /// <summary> 각종 문제들을 해결함. </summary>
        public void Resolve()
        {
            // datarow의 ownerDB가 Quest가 아닌 것을 고침.
            foreach (var quest in Quests)
            {
                foreach (var data in quest.DataRows)
                {
                    if(data.ownerDB != DBType.Quest)
                    {
                        Debug.Log($"[{quest.key}] [{data.key}] ownerDB : {data.ownerDB} => Quest");
                        data.ownerDB = DBType.Quest;
                        EditorUtility.SetDirty(quest);
                    }
                }
            }
            
            
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
