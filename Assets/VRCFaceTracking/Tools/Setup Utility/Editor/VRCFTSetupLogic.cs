using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace VRCFaceTracking.Tools.Setup_Utility.Editor
{
    public class VRCFTSetupLogic
    {
        private Animator targetAnimator;
        private AnimatorController fxController;
        public List<ParamData> ParamData = new List<ParamData>();

        public bool BeginRecording(VRCAvatarDescriptor avatar, string metaJSON)
        {
            ParamData.Clear();
            targetAnimator = avatar.GetComponent<Animator>();
            fxController =
                (AnimatorController) avatar.baseAnimationLayers.First(layer =>
                    layer.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;

            if (targetAnimator == null || fxController == null) return false;
            
            var saveStates = Utils.ConstructChildRendererSaveStates(avatar.transform);
            var paramMetas = JsonConvert.DeserializeObject<ParamMeta>(metaJSON);
            foreach (var param in paramMetas.parameters)
            {
                ParamData.Add(new ParamData(param, saveStates));
            }

            return true;
        }

        public void SaveAndState()
        {
            Utils.EnsureFolder("Assets/VRCFaceTracking", "Animations");
            var saveDir = Utils.EnsureFolder("Assets/VRCFaceTracking/Animations", targetAnimator.name);

            foreach (var shape in ParamData)
            {
                // Save Anims
                var pair = new LinearAnimSet(shape.Name);
                foreach (var step in shape.AnimationSteps)
                {
                    foreach (var saveState in step.Value)
                        pair.AddState(saveState, step.Key.stepValue);
                }

                pair.SaveToAsset(saveDir);
                AssetDatabase.SaveAssets();

                // Add anim to layer
                var layer = new AnimLayerBuilder(shape.Name, pair.Clips);

                if (shape.Type == ParamType.Float)
                {
                    layer.BuildFloat(ref fxController);
                }

                if (shape.Type == ParamType.Binary)
                {
                    layer.BuildBinary(ref fxController, shape.originalParam.data.binaryRes.Value);
                }
            }
        }
    }
}