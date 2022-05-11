using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Editor
{
    public static class AnimLayerBuilder
    {
        public static AnimatorControllerLayer BuildFloat(string name,
            ZeroToValueAnimPair pair)
        {
            var layer = new AnimatorControllerLayer
            {
                name = name,
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
                blendParameter = "JawOpen",
                name = "FloatBlendTree"
            };
            
            tree.AddChild(pair.ZeroClip);
            tree.AddChild(pair.ValueClip, 1);

            var blendState = new AnimatorState
            {
                name = "FloatBlendState",
                motion = tree
            };
            
            layer.stateMachine.AddState(blendState, new Vector3(0, 0));
            layer.stateMachine.defaultState = blendState;
            return layer;
        }
    }
}