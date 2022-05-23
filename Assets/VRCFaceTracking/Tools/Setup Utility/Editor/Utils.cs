using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRCFaceTracking.Tools.Setup_Utility.Editor
{
    public static class Utils
    {
        public static string EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
                AssetDatabase.CreateFolder(parent, child);

            return parent + "/" + child;
        }

        public static List<MRBlendshapeSaveState> ConstructChildRendererSaveStates(Transform parent)
        {
            List<MRBlendshapeSaveState> saveStates = new List<MRBlendshapeSaveState>();
            foreach (SkinnedMeshRenderer smr in parent.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                saveStates.Add(new MRBlendshapeSaveState(smr));
            }

            return saveStates;
        }
    }
}