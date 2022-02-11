using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace OcDialogue.Editor
{
    public class TMPSpriteSearchWindow : EditorWindow
    {
        TMP_SpriteAsset _spriteAsset;
        TMP_SpriteCharacter _selected;
        Sprite _sprite;
        Action<TMP_SpriteAsset,Sprite> OnApply;
        SerializedObject so;
        public static void Open(Action<TMP_SpriteAsset,Sprite> onApply)
        {
            var window = GetWindow<TMPSpriteSearchWindow>(true);
            window.Show();
            window.maxSize = new Vector2(512, 720);
            window.OnApply = onApply;
        }

        void OnEnable()
        {
            so = new SerializedObject(this);
        }

        void OnGUI()
        {
            GUILayout.Box("이미지 출력을 위한 리치텍스트를 적용해도 이미지가 보이지 않는 경우, \n" +
                          "에셋을 Assets/TextMesh Pro/Resources/Sprite Assets 폴더에 넣어야함.");
            so.Update();
            _spriteAsset = EditorGUILayout.ObjectField(_spriteAsset, typeof(TMP_SpriteAsset), false) as TMP_SpriteAsset;
            if (_spriteAsset != null)
            {
                var texturePath = AssetDatabase.GetAssetPath(_spriteAsset.spriteSheet);
                var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(texturePath).Cast<Sprite>();
                foreach (var character in _spriteAsset.spriteCharacterTable)
                {
                    var sprite = sprites.FirstOrDefault(x => x.name == character.name);
                    if(sprite == null) continue;
                    var content = new GUIContent(character.name, sprite.texture);

                    var selected = GUILayout.Toggle(_selected == character, content, GUILayout.Height(64));

                    if (selected)
                    {
                        _selected = character;
                        _sprite = sprite;
                    }
                    else
                    {
                        if (_selected == character) _selected = null;
                    }
                }
            }

            so?.ApplyModifiedProperties();
            if (GUILayout.Button("Apply"))
            {
                if(_selected != null) OnApply?.Invoke(_spriteAsset, _sprite);
                Close();
            }
        }
        
        
    }
}