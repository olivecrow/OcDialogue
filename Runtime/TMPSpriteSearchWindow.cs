#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using OcUtility;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace OcDialogue
{
    public class TMPSpriteSearchWindow : EditorWindow
    {
        TMP_SpriteAsset _spriteAsset;
        TMP_SpriteCharacter _selected;
        Sprite _sprite;
        Action<TMP_SpriteAsset,Sprite> OnApply;
        SerializedObject so;

        Vector2 _scrollPos;
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
                var texture = _spriteAsset.spriteSheet as Texture2D;
                var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(texturePath).Cast<Sprite>();

                _scrollPos = EditorGUILayout.BeginScrollView(
                    _scrollPos, false, false, 
                    GUILayout.MaxWidth(position.width));
                var width = texture.width;
                var height = texture.height;
                foreach (var character in _spriteAsset.spriteCharacterTable)
                {
                    var sprite = sprites.FirstOrDefault(x => x.name == character.name);
                    if(sprite == null) continue;
                    
                    GUILayout.Label(sprite.name);
                    var skin = new GUIStyle(GUI.skin.button);
                    skin.normal.background = GUI.skin.box.normal.background;
                    skin.fixedWidth = 64;
                    skin.fixedHeight = 64;
                    var backgroundColor = GUI.backgroundColor;
                    if(_selected == character)
                    {
                        GUI.backgroundColor = Color.cyan;
                    }
                    var rect = EditorGUILayout.GetControlRect(false, 64, GUILayout.Width(64));
                    var selected = GUI.Button(rect, "", skin);
                    
                    var sR = sprite.rect;
                    var normalizedPos = new Vector2(sR.x / width, sR.y / height);
                    var normalizedSize = new Vector2(sR.width / width, sR.height / height);
                    var normalizedCoord = new Rect(normalizedPos, normalizedSize);
                    GUI.DrawTextureWithTexCoords(rect, sprite.texture, normalizedCoord);
                    EditorGUILayout.Space();
                    
                    GUI.backgroundColor = backgroundColor;
                    if (selected)
                    {
                        _selected = character;
                        _sprite = sprite;
                    }
                }
                EditorGUILayout.EndScrollView();
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
#endif
