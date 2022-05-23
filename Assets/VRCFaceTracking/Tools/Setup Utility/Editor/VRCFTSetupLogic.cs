using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        
        public Texture2D DownloadImage(string url)
        {
            var tex = new Texture2D(200, 200);
            using (WebClient client = new WebClient())
            {
                byte[] data = client.DownloadData(url);
                tex.LoadImage(data);
            }
            return tex;
        }

        public bool BeginRecording(VRCAvatarDescriptor avatar)
        {
            targetAnimator = avatar.GetComponent<Animator>();
            fxController =
                (AnimatorController) avatar.baseAnimationLayers.First(layer =>
                    layer.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;

            if (targetAnimator == null || fxController == null) return false;
            
            var saveStates = Utils.ConstructChildRendererSaveStates(avatar.transform);
            ParamData = new List<ParamData>()
            {
                new ParamData("JawOpen", ParamMeta.ParameterType.Float, new[]
                {
                    ("Negative State", -1f), 
                    ("Zero Step", 0f), 
                    ("Value Step", 1f)
                }, saveStates),
                new ParamData("JawX", ParamMeta.ParameterType.Float, new[] {
                    ("Zero Step", 0f), 
                    ("Value Step", 1f)
                    
                }, saveStates),
                new ParamData("SmileSad", ParamMeta.ParameterType.Float, new[]
                {
                    ("Zero Step", 0f), 
                    ("Value Step", 1f)
                }, saveStates),
            };

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

                if (shape.Type == ParamMeta.ParameterType.Float)
                {
                    layer.BuildFloat(ref fxController);
                }
            }
        }
    }
}