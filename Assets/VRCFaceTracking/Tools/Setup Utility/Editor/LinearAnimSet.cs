using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRCFaceTracking.Tools.Setup_Utility.Editor
{
    // Creates two animation clips to blend between. One for the target value and one for the 
    public class LinearAnimSet
    {
        // Store as many animation clips as needed, corresponds to the value of the parameter, and the clip
        public Dictionary<float, AnimationClip> Clips = new Dictionary<float, AnimationClip>();
        public string Name;

        public LinearAnimSet(string name)
        {
            Name = name;
        }

        public void AddState(MRBlendshapeSaveState state, float value)
        {
            string path = state.renderer.gameObject.name;
            // Check if we already have a clip for this value, and if not, create one
            if (!Clips.ContainsKey(value))
            {
                var clip = new AnimationClip(){name = Name + "_" + value};
                Clips.Add(value, clip);
            }
            
            foreach (var shape in state.savedBlendshapes)
                Clips[value].SetCurve(path, typeof(SkinnedMeshRenderer),
                    "blendShape." + state.renderer.sharedMesh.GetBlendShapeName(shape.Key),
                    new AnimationCurve(new Keyframe(0, shape.Value),
                        new Keyframe(0.01f, shape.Value)));
        }

        public void SaveToAsset(string path)
        {
            foreach (var clip in Clips)
                AssetDatabase.CreateAsset(clip.Value, path + "/" + clip.Value.name + ".anim");
        }
    }
}