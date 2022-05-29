using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace VRCFaceTracking.Tools.Setup_Utility.Editor
{
    public class VRCFTSetupUtil : EditorWindow
    {
        [MenuItem("Tools/VRCFaceTracking/Setup Utility")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            VRCFTSetupUtil window = (VRCFTSetupUtil) GetWindow(typeof(VRCFTSetupUtil), false, "VRCFT Setup Utility");
            window.maxSize = new Vector2(300f, 370f);
            window.minSize = window.maxSize;
            window.Show();
        }

        private VRCAvatarDescriptor _avatarDescriptor;
        private TextAsset paramMetaFile;
        private Texture2D _tex;
        private readonly VRCFTSetupLogic _logic = new VRCFTSetupLogic();
        private bool _isRecording;


        void OnGUI()
        {
            // Double check we have an avatar descriptor and child renderer states
            if (_avatarDescriptor == null)
                _isRecording = false; // Set isRecording to false since we can't record without these two things

            if (!_isRecording)
            {
                // Create object field for skinned mesh renderer
                _avatarDescriptor =
                    (VRCAvatarDescriptor) EditorGUILayout.ObjectField("Avatar Descriptor", _avatarDescriptor,
                        typeof(VRCAvatarDescriptor), true);

                paramMetaFile =
                    (TextAsset) EditorGUILayout.ObjectField("Avatar Descriptor", paramMetaFile,
                        typeof(TextAsset), true);

                // Create a button
                GUI.enabled = _avatarDescriptor != null && paramMetaFile != null;
                if (GUILayout.Button("Start!"))
                    _isRecording = _logic.BeginRecording(_avatarDescriptor, paramMetaFile.text);
                GUI.enabled = true;
            }

            if (_isRecording) // If we're currently recording
            {
                // TODO: Handle multiple diffs

                // Get the next param that needs data
                var currentShape = _logic.ParamData.First(item => !item.IsAssigned());
                
                DisplaySetupStep(currentShape);

                if (GUILayout.Button("Cancel"))
                {
                    foreach (var defaultState in currentShape.GetDefaultValues())
                        defaultState.Restore();
                    _tex = null;
                    _isRecording = false;
                    return;
                }

                // If we've fufilled every pair requirement, save the anims and create state machines
                if (_logic.ParamData.All(item => item.IsAssigned()))
                {
                    _isRecording = false;
                    _logic.SaveAndState();
                    return;
                }
            }
        }

        private void DisplaySetupStep(ParamData nextData)
        {
            GUILayout.Label("Currently Animating: " + nextData.Name, EditorStyles.boldLabel);

            // Find our current step
            var remainingSteps = nextData.AnimationSteps.Where(item => item.Value.Count == 0);
            var currentStep = remainingSteps.First();

            GUILayout.Label(
                "Animating step name: " + currentStep.Key.stepName + " of value: " + currentStep.Key.stepValue,
                EditorStyles.boldLabel);
            
            if (currentStep.Key.description != null)
                GUILayout.Label("Description: "+currentStep.Key.description);

            GUILayout.Label((-1 + remainingSteps.Count()) + " step(s) remaining in this parameter.");

            if (_tex != null)
                EditorGUI.DrawPreviewTexture(new Rect(25, 110, 250, 250), _tex);
            
            if (_tex == null && currentStep.Key.imageUrl != null)
                _tex = Utils.DownloadImage(currentStep.Key.imageUrl);


// If the next button isn't currently being pressed, return.
            if (!GUILayout.Button("Next")) return;

            _tex = null;
// If we're currently pressing the next button
// For every child renderer zero state (which we can get from checking the default state which was set on recording start)
            foreach (var defaultState in nextData.GetDefaultValues())
            {
// Get current diff
                var currentSave = new MRBlendshapeSaveState(defaultState.renderer);
                var diff = currentSave - defaultState;
                MRBlendshapeSaveState.PruneUnchanged(ref currentSave, ref diff);

// Reset back to original state
                defaultState.Restore();

// If there isn't a diff, skip
                if (diff.savedBlendshapes.All(item => item.Value == 0))
                    continue;

// Create the zero and 100 anim clips and save them
                currentStep.Value.Add(diff);
            }
        }
    }
}
