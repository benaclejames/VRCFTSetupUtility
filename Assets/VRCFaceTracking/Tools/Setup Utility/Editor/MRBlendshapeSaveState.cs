using System.Collections.Generic;
using UnityEngine;

namespace VRCFaceTracking.Tools.Setup_Utility.Editor
{
    public class MRBlendshapeSaveState
    {
        public Dictionary<int, float> savedBlendshapes = new Dictionary<int, float>();
        public SkinnedMeshRenderer renderer;
        
        public MRBlendshapeSaveState(SkinnedMeshRenderer renderer)
        {
            this.renderer = renderer;

            for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
            {
                savedBlendshapes[i] = renderer.GetBlendShapeWeight(i);
            }
        }

        public void Restore()
        {
            for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
            { 
                renderer.SetBlendShapeWeight(i, savedBlendshapes[i]);
            }
        }

        public static void PruneUnchanged(ref MRBlendshapeSaveState zeroState, ref MRBlendshapeSaveState diffState)
        {
            // Go through and find any blendshapes that are the same, then remove them from both the zeroState and diffState
            int origCount = diffState.savedBlendshapes.Count;
            for (int i = 0; i < origCount; i++)
            {
                if (diffState.savedBlendshapes[i] == 0)
                {
                    zeroState.savedBlendshapes.Remove(i);
                    diffState.savedBlendshapes.Remove(i);
                }
            }
        }

        public static MRBlendshapeSaveState operator -(MRBlendshapeSaveState a, MRBlendshapeSaveState b)
        {
            // Ensure both renderers are the same
            if (a.renderer != b.renderer)
            {
                throw new System.Exception("Cannot subtract blendshapes from different renderers");
            }

            MRBlendshapeSaveState result = new MRBlendshapeSaveState(a.renderer);
            for (int i = 0; i < a.renderer.sharedMesh.blendShapeCount; i++)
            {
                result.savedBlendshapes[i] = a.savedBlendshapes[i] - b.savedBlendshapes[i];
            }
            return result;
        }
    }
}