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
    private ParamMeta paramMeta;
    private Animator targetAnimator;
    private AnimatorController fxController;

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
    private List<ParamData> ParamData = new List<ParamData>();


    void OnGUI()
    {
        // Double check we have an avatar descriptor and child renderer states
        if (avatarDescriptor == null || paramMeta == null)
            isRecording = false;    // Set isRecording to false since we can't record without these two things
        
        if (!isRecording)
        {
            // Create object field for skinned mesh renderer
            avatarDescriptor =
                (VRCAvatarDescriptor) EditorGUILayout.ObjectField("Avatar Descriptor", avatarDescriptor,
                    typeof(VRCAvatarDescriptor), true);
            
            paramMeta =
                (ParamMeta) EditorGUILayout.ObjectField("Parameter Meta", paramMeta,
                    typeof(ParamMeta), false);
            
            // Create a button
            GUI.enabled = avatarDescriptor != null && paramMeta != null;
            if (GUILayout.Button("Start!"))
                BeginRecording();
            GUI.enabled = true;
        }
        
        if (isRecording)    // If we're currently recording
        {
            // TODO: Handle multiple diffs
            
            // Create the next anim
            var currentShape = ParamData.First(item => !item.IsAssigned());
            GUILayout.Label("Currently Animating: "+currentShape.Name, EditorStyles.boldLabel);
            
            if (GUILayout.Button("Next"))
            {
                foreach (var zeroState in currentShape.AnimationSteps[currentShape.DefaultStep]) // For every child renderer
                {
                    // Get current diff
                    var currentSave = new MRBlendshapeSaveState(zeroState.renderer);
                    var diff = currentSave - zeroState;
                    MRBlendshapeSaveState.PruneUnchanged(ref currentSave, ref diff);

                    // Reset back to original state
                    zeroState.Restore();
                    
                    // If there isn't a diff, skip
                    if (diff.savedBlendshapes.All(item => item.Value == 0))
                        continue;

                    // Create the zero and 100 anim clips and save them
                    currentShape.AnimationSteps[1].Add(diff);
                }
            }

            if (GUILayout.Button("Cancel"))
            {
                isRecording = false;
                return;
            }
            
            // If we've fufilled every pair requirement, save the anims and create state machines
            if (ParamData.All(item => item.IsAssigned()))
            {
                isRecording = false;
                SaveAndState();
                return;
            }
        }
    }

    void BeginRecording()
    {
        targetAnimator = avatarDescriptor.GetComponent<Animator>();
        fxController =
            (AnimatorController) avatarDescriptor.baseAnimationLayers.First(layer =>
                layer.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;

        if (targetAnimator != null && fxController != null)
        {
            var saveStates = ConstructChildRendererSaveStates();
            ParamData = new List<ParamData>()
            {
                new ParamData("JawOpen", ParamMeta.ParameterType.Float, new []{0f, 1f}, saveStates),
                new ParamData("JawX", ParamMeta.ParameterType.Float, new []{0f, 1f}, saveStates),
                new ParamData("SmileSad", ParamMeta.ParameterType.Float, new []{0f, 1f}, saveStates),
            };
            isRecording = true;
        }
    }

    void SaveAndState()
    {
        EnsureFolder("Assets/VRCFT", "Animations");
        var saveDir = EnsureFolder("Assets/VRCFT/Animations", targetAnimator.name);

        foreach (var shape in ParamData)
        {
            // Save Anims
            var pair = new LinearAnimSet(shape.Name);
            foreach (var step in shape.AnimationSteps)
            {
                foreach (var saveState in step.Value)
                    pair.AddState(saveState, step.Key);
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

    static string EnsureFolder(string parent, string child)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + child))
            AssetDatabase.CreateFolder(parent, child);

        return parent + "/" + child;
    }
}
