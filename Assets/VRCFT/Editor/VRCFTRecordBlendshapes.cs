using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Windows;
using VRC.SDK3.Avatars.Components;

public class VRCFTRecordBlendshapes : EditorWindow
{
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/VRCFT")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        VRCFTRecordBlendshapes window = (VRCFTRecordBlendshapes)EditorWindow.GetWindow(typeof(VRCFTRecordBlendshapes));
        window.Show();
    }

    private VRCAvatarDescriptor avatarDescriptor;
    private Animator targetAnimator;
    private AnimatorController fxController;
    private MRBlendshapeSaveState[] ChildRendererStates;

    List<MRBlendshapeSaveState> ConstructChildRendererSaveStates()
    {
        List<MRBlendshapeSaveState> saveStates = new List<MRBlendshapeSaveState>();
        foreach (SkinnedMeshRenderer smr in targetAnimator.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            saveStates.Add(new MRBlendshapeSaveState(smr));
        }
        return saveStates;
    }

    private bool isRecording;
    private Dictionary<string, List<ZeroToValueAnimPair>> animPairs = new Dictionary<string, List<ZeroToValueAnimPair>>();


    void OnGUI()
    {
        if (!isRecording || avatarDescriptor == null || ChildRendererStates == null)
        {
            // Create object field for skinned mesh renderer
            avatarDescriptor =
                (VRCAvatarDescriptor) EditorGUILayout.ObjectField("Avatar Descriptor", avatarDescriptor,
                    typeof(VRCAvatarDescriptor), true);

            // Create a button
            if (GUILayout.Button("Start!"))
            {
                // print all blendshape values of skinnedmeshrenderer
                targetAnimator = avatarDescriptor.GetComponent<Animator>();
                fxController =
                    (AnimatorController) avatarDescriptor.baseAnimationLayers.First(layer =>
                        layer.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;

                if (targetAnimator != null && fxController != null)
                {
                    ChildRendererStates = ConstructChildRendererSaveStates().ToArray();
                    animPairs = new Dictionary<string, List<ZeroToValueAnimPair>>()
                    {
                        {"JawOpen", null},
                        {"JawClose", null},
                        {"JawFwd", null},
                    };
                    isRecording = true;
                }
            }
        }
        else
        {
            if (GUILayout.Button("Next"))
            {
                // print all blendshape values of skinnedmeshrenderer
                if (targetAnimator == null || animPairs.All(item => item.Value != null))
                {
                    isRecording = false;
                    return;
                }

                string currentShape = animPairs.First(item => item.Value == null).Key;

                if (!AssetDatabase.IsValidFolder("Assets/VRCFT/Animations"))
                    AssetDatabase.CreateFolder("Assets/VRCFT", "Animations");

                AssetDatabase.CreateFolder("Assets/VRCFT/Animations", targetAnimator.name);

                List<ZeroToValueAnimPair> pairs = new List<ZeroToValueAnimPair>();
                foreach (var saveState in ChildRendererStates) // For every child renderer
                {
                    // Get current diff
                    var currentSave = new MRBlendshapeSaveState(saveState.renderer);
                    var diff = currentSave - saveState;

                    // Reset back to original state
                    saveState.Restore();

                    // Create the zero and 100 anim clips and save them
                    var pair = new ZeroToValueAnimPair(currentShape, saveState, diff);
                    var saveDir = $"Assets/VRCFT/Animations/{targetAnimator.name}/{saveState.renderer.name}";
                    Directory.CreateDirectory(saveDir);
                    pair.SaveToAsset(saveDir);
                    AssetDatabase.SaveAssets();

                    pairs.Add(pair);
                }

                animPairs[currentShape] = pairs;

                // Create the anim layer for the blendshape
                //var layer = AnimLayerBuilder.BuildFloat(saveState.renderer.name, pair);
                //fxController.AddLayer(layer);
            }
        }
    }
}
