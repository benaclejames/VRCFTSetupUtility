using UnityEditor;
using UnityEngine;

namespace Editor
{
    // Creates two animation clips to blend between. One for the target value and one for the 
    public class ZeroToValueAnimPair
    {
        public AnimationClip ZeroClip, ValueClip;
        
        public ZeroToValueAnimPair(string name, MRBlendshapeSaveState origState, MRBlendshapeSaveState diffState)
        {
            ZeroClip = new AnimationClip 
                {name = name+"_ZeroAnim"};
            
            ValueClip = new AnimationClip 
                {name = name+"_ValueAnim"};

            string path = diffState.renderer.gameObject.name;

            for (int i = 0; i < diffState.savedBlendshapes.Length; i++) 
                // Using diffState for iterator here since we don't care about creating a zero anim for a shape we didn't change
            {
                if (diffState.savedBlendshapes[i] == 0) continue; // Continue if this shape hasn't changed
                Debug.Log("Blendshape " + i + ": " + diffState.savedBlendshapes[i]);

                // Get the relative path of the current saveState.renderer
                ValueClip.SetCurve(path, typeof(SkinnedMeshRenderer),
                    "blendShape." + diffState.renderer.sharedMesh.GetBlendShapeName(i),
                    new AnimationCurve(new Keyframe(0, diffState.savedBlendshapes[i]),
                        new Keyframe(0.01f, diffState.savedBlendshapes[i])));

                ZeroClip.SetCurve(path, typeof(SkinnedMeshRenderer),
                    "blendShape." + origState.renderer.sharedMesh.GetBlendShapeName(i),
                    new AnimationCurve(new Keyframe(0, origState.savedBlendshapes[i]),
                        new Keyframe(0.01f, origState.savedBlendshapes[i])));
            }
        }

        public void SaveToAsset(string path)
        {
            AssetDatabase.CreateAsset(ZeroClip, path + "/" + ZeroClip.name + ".anim");
            AssetDatabase.CreateAsset(ValueClip, path + "/" + ValueClip.name + ".anim");
        }
    }
}