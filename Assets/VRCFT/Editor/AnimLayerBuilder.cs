using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace Editor
{
    public class AnimLayerBuilder
    {
        public string Name;
        public Dictionary<float, AnimationClip> Clips;
        
        public AnimLayerBuilder(string name, Dictionary<float, AnimationClip> clips)
        {
            Name = name;
            Clips = clips;
        }

        public void BuildFloat(ref AnimatorController controller, bool createParam = true)
        {
            if (createParam)
            {
                var containsParamAlready = false;
                foreach (var param in controller.parameters)
                    if (param.name == Name)
                        containsParamAlready = true;
                
                if (!containsParamAlready)
                    controller.AddParameter(Name, AnimatorControllerParameterType.Float);
            }

            var layer = new AnimatorControllerLayer
            {
                name = Name,
                stateMachine = new AnimatorStateMachine
                {
                    hideFlags = HideFlags.HideInHierarchy
                },
                defaultWeight = 1
            };

            var tree = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                hideFlags = HideFlags.HideInHierarchy,
                useAutomaticThresholds = false,
                blendParameter = Name,
                name = "FloatBlendTree"
            };

            foreach (var clip in Clips)
                tree.AddChild(clip.Value, clip.Key);

            var blendState = new AnimatorState
            {
                name = "FloatBlendState",
                motion = tree
            };

            layer.stateMachine.AddState(blendState, new Vector3(0, 0));
            layer.stateMachine.defaultState = blendState;
            
            controller.AddLayer(layer);
        }
    }
}