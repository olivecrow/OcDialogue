using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OcDialogue;
using OcDialogue.DB;
using OcDialogue.Samples;
using UnityEngine;

public class RuntimeSaveGUI : MonoBehaviour
{
    bool _showPresetNames;
    bool _showSaveGUI;
    string _tempInput;
    string _dbName;
    List<string> _dbNames;
    OcDB _db;
    ES3Settings ES3Settings => EasySaveIntegration.ES3Settings;
#if !DEBUG
    void Awake()
    {
        Destroy(gameObject);   
    }
#endif

    void OnEnable()
    {
        _dbNames = new List<string>();
        _dbNames.Add("All");
        _dbNames.AddRange(DBManager.Instance.DBs.Select(x => x.Address));
        _dbName = _dbNames[0];
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(_dbName, GUILayout.Width(150)))
        {
            var currIdx = _dbNames.IndexOf(_dbName);
            currIdx++;
            currIdx = (int)Mathf.Repeat(currIdx, _dbNames.Count);
            _dbName = _dbNames[currIdx];
            _db = DBManager.Instance.DBs.Find(x => x.Address == _dbName);
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
            var keys = ES3.GetKeys(ES3Settings);
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
                if (_db != null) names = EasySaveIntegration.GetPresetNames(_db);
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
                    if (_db != null)
                    {
                        Debug.Log($"preset : {preset}");
                     
                        EasySaveIntegration.LoadFromPreset(_db, preset);
                    }
                    else EasySaveIntegration.LoadFromPreset(preset);
                    toDefault();
                    GUILayout.EndHorizontal();
                    return;
                }

                GUI.color = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    if (_db != null) EasySaveIntegration.RemovePreset(_db, preset);
                    else EasySaveIntegration.RemovePreset(preset);
                    EasySaveIntegration.StoreCachedFile();
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
                if (_db != null) EasySaveIntegration.SaveDB(_db, _tempInput, true);
                else EasySaveIntegration.Save(_tempInput, true);
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
