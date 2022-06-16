using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace VRCFaceTracking.Tools.Setup_Utility.Editor.LayerBuilders
{
    public class LinearLayer : AnimLayer
    {
        public LinearLayer(string name, Dictionary<float, AnimationClip> clips) : base(name, clips)
        {
        }

        public override void Build(ref AnimatorController controller, bool createParam = true)
        {
            if (createParam)
                EnsureParam(ref controller, Name, AnimatorControllerParameterType.Float);

            var layer = new AnimatorControllerLayer
            {
                name = Name,
                stateMachine = new AnimatorStateMachine
                {
                    hideFlags = HideFlags.HideInHierarchy
                },
                defaultWeight = 1
            };
            
            if (AssetDatabase.GetAssetPath(controller) != string.Empty)
                AssetDatabase.AddObjectToAsset(layer.stateMachine, AssetDatabase.GetAssetPath(controller));

            var tree = new BlendTree
            {
                blendType = BlendTreeType.Simple1D,
                hideFlags = HideFlags.HideInHierarchy,
                useAutomaticThresholds = false,
                blendParameter = Name,
                name = "FloatBlendTree"
            };
            
            if (AssetDatabase.GetAssetPath(layer.stateMachine) != string.Empty)
                AssetDatabase.AddObjectToAsset(tree, AssetDatabase.GetAssetPath(layer.stateMachine));

            foreach (var clip in Clips)
                tree.AddChild(clip.Value, clip.Key);

            var blendState = new AnimatorState
            {
                name = "FloatBlendState",
                motion = tree
            };
            
            if (AssetDatabase.GetAssetPath(layer.stateMachine) != string.Empty)
                AssetDatabase.AddObjectToAsset(blendState, AssetDatabase.GetAssetPath(layer.stateMachine));

            layer.stateMachine.AddState(blendState, new Vector3(0, 0));
            layer.stateMachine.defaultState = blendState;
            
            controller.AddLayer(layer);
        }
    }
}