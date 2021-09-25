using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcDialogue;
using OcDialogue.Samples;
using UnityEngine;

public class RuntimeSaveGUI : MonoBehaviour
{
    bool _showPresetNames;
    bool _showSaveGUI;
    string _tempInput;
    string _dbType;
    List<string> _dbTypeNames;
#if !DEBUG
    void Awake()
    {
        Destroy(gameObject);   
    }
#endif

    void OnEnable()
    {
        var enumNames = Enum.GetNames(typeof(DBType)).ToList();
        _dbTypeNames = new List<string>();
        _dbTypeNames.Add("All");
        _dbTypeNames.AddRange(enumNames);
        _dbType = _dbTypeNames[0];
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(_dbType, GUILayout.Width(150)))
        {
            var currIdx = _dbTypeNames.IndexOf(_dbType);
            currIdx++;
            currIdx = (int)Mathf.Repeat(currIdx, _dbTypeNames.Count);
            _dbType = _dbTypeNames[currIdx];
        }
        if (GUILayout.Button("Load"))
        {
            _showPresetNames = !_showPresetNames;
            _showSaveGUI = false;
        }

        if (GUILayout.Button("Save"))
        {
            _showPresetNames = false;
            _showSaveGUI = !_showSaveGUI;
            _tempInput = "";
        }

        if (GUILayout.Button("Print Exist Keys"))
        {
            var keys = ES3.GetKeys();
            foreach (var key in keys)
            {
                Debug.Log($"key : {key}");
            }
        }
        GUILayout.EndHorizontal();
        if (_showPresetNames)
        {
            List<string> names;
            {
                if (Enum.TryParse(_dbType, out DBType t))
                {
                    names = EasySaveIntegration.GetPresetNames(t);
                }
                else names = EasySaveIntegration.GetPresetNames();
            }
            if (names.Count == 0)
            {
                _showPresetNames = false;
                return;
            }
            foreach (var preset in names)
            {
                GUILayout.BeginHorizontal();
                if(GUILayout.Button(preset))
                {
                    if (Enum.TryParse(_dbType, out DBType t))
                    {
                        Debug.Log($"preset : {preset}");
                        EasySaveIntegration.LoadFromPreset(t, preset);
                    }
                    else EasySaveIntegration.LoadFromPreset(preset);
                    toDefault();
                    GUILayout.EndHorizontal();
                    return;
                }

                GUI.color = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    if (Enum.TryParse(_dbType, out DBType t))
                    {
                        EasySaveIntegration.RemovePreset(t, preset);
                    }
                    else EasySaveIntegration.RemovePreset(preset);
                    toDefault();
                    GUILayout.EndHorizontal();
                    return;
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
            }
        }

        if (_showSaveGUI)
        {
            GUILayout.BeginArea(new Rect(20,20,400,30));
            GUILayout.BeginHorizontal();
            GUILayout.Label("PresetName", GUI.skin.label);
            _tempInput = GUILayout.TextField(_tempInput, 15, GUILayout.Width(150));
            if (GUILayout.Button("OK"))
            {
                if (Enum.TryParse(_dbType, out DBType t))
                {
                    switch (t)
                    {
                        case DBType.GameProcess:
                            EasySaveIntegration.SaveGameProcessDB(_tempInput);
                            break;
                        case DBType.Item:
                            EasySaveIntegration.SaveInventory(_tempInput);
                            break;
                        case DBType.Quest:
                            EasySaveIntegration.SaveQuestDB(_tempInput);
                            break;
                        case DBType.NPC:
                            EasySaveIntegration.SaveNPCDB(_tempInput);
                            break;
                        case DBType.Enemy:
                            EasySaveIntegration.SaveEnemyDB(_tempInput);
                            break;
                    }
                }
                else EasySaveIntegration.Save(_tempInput);
                toDefault();
            }

            if (GUILayout.Button("Cancel"))
            {
                toDefault();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void toDefault()
        {
            _showPresetNames = false;
            _showSaveGUI = false;
        }
    }
}
