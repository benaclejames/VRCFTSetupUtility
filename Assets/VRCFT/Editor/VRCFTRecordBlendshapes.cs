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
    private MRBlendshapeSaveState[] SaveStates;

    List<MRBlendshapeSaveState> ConstructChildRendererSaveStates()
    {
        List<MRBlendshapeSaveState> saveStates = new List<MRBlendshapeSaveState>();
        foreach (SkinnedMeshRenderer smr in targetAnimator.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            saveStates.Add(new MRBlendshapeSaveState(smr));
        }
        return saveStates;
    }

    private bool isRecording = false;
    
    void OnGUI()
    {
        if (!isRecording || avatarDescriptor == null || SaveStates == null)
        {
            // Create object field for skinned mesh renderer
            avatarDescriptor =
                (VRCAvatarDescriptor) EditorGUILayout.ObjectField("Avatar Descriptor", avatarDescriptor, typeof(VRCAvatarDescriptor), true);

            // Create a button
            if (GUILayout.Button("Start!"))
            {
                // print all blendshape values of skinnedmeshrenderer
                targetAnimator = avatarDescriptor.GetComponent<Animator>();
                fxController =
                    (AnimatorController)avatarDescriptor.baseAnimationLayers.First(layer =>
                        layer.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;
                
                if (targetAnimator != null && fxController != null)
                {
                    SaveStates = ConstructChildRendererSaveStates().ToArray();
                    isRecording = true;
                }
            }
        }
        else if (GUILayout.Button("Diff"))
        {
            // print all blendshape values of skinnedmeshrenderer
            if (targetAnimator == null) return;
            
            if (!AssetDatabase.IsValidFolder("Assets/VRCFT/Animations")) 
                AssetDatabase.CreateFolder("Assets/VRCFT", "Animations");
            
            AssetDatabase.CreateFolder("Assets/VRCFT/Animations", targetAnimator.name);
            
            foreach (var saveState in SaveStates)
            {
                var currentSave = new MRBlendshapeSaveState(saveState.renderer);
                var diff = currentSave - saveState;

                saveState.Restore();

                var pair = new ZeroToValueAnimPair("TestShapeName", saveState, diff);
                var saveDir = $"Assets/VRCFT/Animations/{targetAnimator.name}/{saveState.renderer.name}";
                Directory.CreateDirectory(saveDir);
                
                pair.SaveToAsset(saveDir);
                
                AssetDatabase.SaveAssets();
                
                var layer = AnimLayerBuilder.BuildFloat(saveState.renderer.name, pair);
                fxController.AddLayer(layer);
            }
            
            isRecording = false;
        }
    }
}
