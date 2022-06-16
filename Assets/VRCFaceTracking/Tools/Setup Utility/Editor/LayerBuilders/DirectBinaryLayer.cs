using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace VRCFaceTracking.Tools.Setup_Utility.Editor.LayerBuilders
{
    public class DirectBinaryLayer : AnimLayer
    {
        private readonly int _binaryResolution;
        
        public DirectBinaryLayer(string name, Dictionary<float, AnimationClip> clips, int binaryResolution) : base(name, clips) => _binaryResolution = binaryResolution;

        public override void Build(ref AnimatorController controller, bool createParam = true)
        {
            var maximumThresh = _binaryResolution - 1;
            
            // Ensure we have the correct parameters created
            EnsureParam(ref controller, "BinaryBlend", AnimatorControllerParameterType.Float);
            
            var containsNegative = Clips.Any(f => f.Key < 0);
            if (containsNegative)
                EnsureParam(ref controller, Name+"Negative", AnimatorControllerParameterType.Bool);

            // Count in base2
            List<int> requiredBits = new List<int>();
            for (int count = 1; count <= maximumThresh; count *= 2)   // subtract 1 since 0 counts as a possible value
            {
                requiredBits.Add(count);
                EnsureParam(ref controller, Name+count, AnimatorControllerParameterType.Bool);
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
            
            if (AssetDatabase.GetAssetPath(controller) != string.Empty)
                AssetDatabase.AddObjectToAsset(layer.stateMachine, AssetDatabase.GetAssetPath(controller));
            
            controller.AddLayer(layer);
            
            var start = containsNegative ? maximumThresh * -1 : 0;
            // For every state, can start as negative depending on whether we're adding negative parameters
            for (int i = start; i < _binaryResolution; i++)
            {
                var tree = new BlendTree
                {
                    blendType = BlendTreeType.Simple1D,
                    hideFlags = HideFlags.HideInHierarchy,
                    useAutomaticThresholds = false,
                    blendParameter = "BinaryBlend",
                    name = Name+i
                };
                
                if (AssetDatabase.GetAssetPath(layer.stateMachine) != string.Empty)
                    AssetDatabase.AddObjectToAsset(tree, AssetDatabase.GetAssetPath(layer.stateMachine));

                var blendState = new AnimatorState
                {
                    name = Name+i,
                    motion = tree
                };
                
                if (AssetDatabase.GetAssetPath(layer.stateMachine) != string.Empty)
                    AssetDatabase.AddObjectToAsset(blendState, AssetDatabase.GetAssetPath(layer.stateMachine));

                if (i == 0)
                {
                    // If we're at the root state, we want all anims present
                    foreach (var clip in Clips)
                        tree.AddChild(clip.Value, Math.Abs(i - (clip.Key * maximumThresh)));

                    layer.stateMachine.entryPosition = new Vector3
                    (
                        50,
                        layer.stateMachine.anyStatePosition.y - 50 - (100 + _binaryResolution * 4)
                    );

                    layer.stateMachine.defaultState = blendState;
                }
                else if (i < 0)  // If we're negative, we want all anims that are negative or zero
                    foreach (var clip in Clips.Where(c => c.Key <= 0.0f))
                        tree.AddChild(clip.Value, i-(clip.Key*maximumThresh));
                else // Otherwise, we want all anims that are positive or zero
                    foreach (var clip in Clips.Where(c => c.Key >= 0.0f))
                        tree.AddChild(clip.Value, (clip.Key*maximumThresh) - i);

                var x = layer.stateMachine.anyStatePosition.x - 20 - Mathf.Sin(i / (float)_binaryResolution * Mathf.PI) * (200 + _binaryResolution * 8);
                var y = layer.stateMachine.anyStatePosition.y - 5 - Mathf.Cos(i / (float)_binaryResolution * Mathf.PI) * (100 + _binaryResolution * 4);
                layer.stateMachine.AddState(blendState, new Vector3(x, y));

                var transition = layer.stateMachine.AddAnyStateTransition(blendState);
                for (int shift = 0; shift < requiredBits.Count; shift++)
                {
                    bool paramState = ((Mathf.Abs(i) >> shift) & 1) == 1;
                    transition.AddCondition(paramState ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0,
                        Name + requiredBits[shift]);
                }
                if (i < 0)
                    transition.AddCondition(AnimatorConditionMode.If, 0, Name+"Negative");
                else if (i > 0)
                    transition.AddCondition(AnimatorConditionMode.IfNot, 0, Name+"Negative");
            }
        }
    }
}