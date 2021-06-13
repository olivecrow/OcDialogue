using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace OcUtility.Editor
{
    [InitializeOnLoad]
    public class HierarchyIconDrawer
    {
        public class IconInstanceRegistry
        {
            public IHierarchyIconDrawable drawable;
            List<int> _instanceIDs;
            Texture2D _icon;

            public IconInstanceRegistry(IHierarchyIconDrawable iconDrawable, int id)
            {
                drawable = iconDrawable;
                _instanceIDs = new List<int>();
                _instanceIDs.Add(id);
                _icon = (Texture2D) EditorGUIUtility.Load(drawable.IconPath);
                if (_icon == null) _icon = Resources.Load<Texture2D>(drawable.IconPath);
                if (_icon == null) _icon = AssetDatabase.LoadAssetAtPath<Texture2D>(drawable.IconPath);
            }

            public void AddInstanceID(int id)
            {
                if (_instanceIDs.Contains(id))
                {
                    Debug.LogWarning($"이미 해당 ID가 있음 : {id}");
                    return;
                }

                _instanceIDs.Add(id);
            }

            public bool IsExistID(int id)
            {
                for (int i = 0; i < _instanceIDs.Count; i++)
                {
                    if (_instanceIDs[i] == id) return true;
                }

                return false;
            }

            public bool TryGetIcon(int id, out Texture2D icon, out int xRext)
            {
                if (!IsExistID(id))
                {
                    icon = null;
                    xRext = 0;
                    return false;
                }

                icon = _icon;
                xRext = drawable.DistanceToText;
                return true;
            }
        }

        static List<IconInstanceRegistry> _iconInstanceRegistries;
        static List<int> _registeredInstanceIDs;

        static HierarchyIconDrawer()
        {
            _iconInstanceRegistries = new List<IconInstanceRegistry>();
            _registeredInstanceIDs = new List<int>();
            EditorApplication.hierarchyWindowItemOnGUI += DrawAllIcon;
        }

        static void Register(GameObject gameObject)
        {
            var id = gameObject.GetInstanceID();
            var drawable = gameObject.GetComponent<IHierarchyIconDrawable>();
            if (drawable == null) return;

            for (int i = 0; i < _iconInstanceRegistries.Count; i++)
            {
                if (_iconInstanceRegistries[i].IsExistID(id))
                {
                    _iconInstanceRegistries[i].AddInstanceID(id);
                    _registeredInstanceIDs.Add(id);
                    return;
                }
            }

            _registeredInstanceIDs.Add(id);
            _iconInstanceRegistries.Add(new IconInstanceRegistry(drawable, id));
        }

        static Texture2D GetIcon(int id, out int xRect)
        {
            Texture2D icon;
            for (int i = 0; i < _iconInstanceRegistries.Count; i++)
            {
                if (_iconInstanceRegistries[i].TryGetIcon(id, out icon, out var dist))
                {
                    xRect = dist;
                    return icon;
                }
            }

            xRect = 0;
            return null;
        }

        static void DrawAllIcon(int instanceID, Rect rect)
        {
            if (!_registeredInstanceIDs.Contains(instanceID))
            {
                var gao = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                if (gao == null) return;
                Register(gao);
            }

            var iconGUIContent = new GUIContent(GetIcon(instanceID, out var xRect));

            var iconDrawRect = new Rect(
                rect.width * 0.75f + xRect,
                rect.yMin,
                rect.width,
                rect.height);
            EditorGUIUtility.SetIconSize(new Vector2(15, 15));
            EditorGUI.LabelField(iconDrawRect, iconGUIContent);
        }
    }
}