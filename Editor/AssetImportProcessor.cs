using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.Presets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OcUtility.Editor
{
    public class AssetImportProcessor : AssetPostprocessor
    {
        void OnPreprocessAsset()
        {
            if (assetImporter.importSettingsMissing)
            {
                // Get the current imported asset folder.
                var path = Path.GetDirectoryName(assetPath);
                while (!string.IsNullOrEmpty(path))
                {
                    // Find all Preset assets in this folder.
                    var presetGuids = AssetDatabase.FindAssets("t:Preset", new[] {path});
                    foreach (var presetGuid in presetGuids)
                    {
                        // Make sure we are not testing Presets in a subfolder.
                        string presetPath = AssetDatabase.GUIDToAssetPath(presetGuid);
                        if (Path.GetDirectoryName(presetPath) == path)
                        {
                            // Load the Preset and try to apply it to the importer.
                            var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
                            if (preset.ApplyTo(assetImporter))
                                return;
                        }
                    }

                    // Try again in the parent folder.
                    path = Path.GetDirectoryName(path);
                }
            }
        }
    }
}
