using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace VRCFaceTracking.Tools.Setup_Utility.Editor
{
    public abstract class AnimLayer
    {
        public string Name;
        public Dictionary<float, AnimationClip> Clips;
        
        public AnimLayer(string name, Dictionary<float, AnimationClip> clips)
        {
            Name = name;
            Clips = clips;
        }
        
        protected static void EnsureParam(ref AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            var containsParamAlready = false;
            foreach (var param in controller.parameters)
                if (param.name == name && param.type == type)
                    containsParamAlready = true;
                
            if (!containsParamAlready)
                controller.AddParameter(name, type);
        }
        
        public abstract void Build(ref AnimatorController controller, bool createParam = true);
    }
}