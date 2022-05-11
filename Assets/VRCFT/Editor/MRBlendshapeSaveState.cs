using UnityEngine;

namespace Editor
{
    public class MRBlendshapeSaveState
    {
        public float[] savedBlendshapes;
        public SkinnedMeshRenderer renderer;
        
        public MRBlendshapeSaveState(SkinnedMeshRenderer renderer)
        {
            this.renderer = renderer;
            
            savedBlendshapes = new float[renderer.sharedMesh.blendShapeCount];
            
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