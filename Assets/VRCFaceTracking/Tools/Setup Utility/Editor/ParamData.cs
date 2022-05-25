using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace VRCFaceTracking.Tools.Setup_Utility.Editor
{
    public class ParamData
    {
        public struct Identifier
        {
            public string stepName;
            public float stepValue;
            [CanBeNull] public string imageUrl;
            [CanBeNull] public string description;
        }
        
        public ParamData(string name, ParamType type, Identifier[] steps, 
            List<MRBlendshapeSaveState> defaultValues, float defaultStep = 0)
        {
            Name = name;
            Type = type;
            AnimationSteps = new Dictionary<Identifier, List<MRBlendshapeSaveState>>();
            // Add an entry for each step
            foreach (var step in steps)
                AnimationSteps.Add(step, new List<MRBlendshapeSaveState>());
            
            // Add the default values
            DefaultStep = steps.First(s => s.stepValue == defaultStep);
            AnimationSteps[DefaultStep] = defaultValues;
        }
        
        public readonly string Name;
        public readonly ParamType Type;
        public readonly Parameter originalParam;
        public Identifier DefaultStep;
        public Dictionary<Identifier /*-1,0,1*/, List<MRBlendshapeSaveState>/*One for every renderer*/> AnimationSteps;

        public bool IsAssigned() => AnimationSteps.All(kvp => kvp.Value.Count != 0);
        public List<MRBlendshapeSaveState> GetDefaultValues() => AnimationSteps[DefaultStep];

        public ParamData(Parameter param, List<MRBlendshapeSaveState> defaultValues)
        {
            originalParam = param;
            Name = param.name;
            Type = (ParamType) param.type;
            var defaultStepVal = param.data.defaultState ?? 0.0f;
            AnimationSteps = new Dictionary<Identifier, List<MRBlendshapeSaveState>>();
            foreach (var state in param.data.states)
            {
                AnimationSteps.Add(new Identifier()
                {
                    description = state.desc,
                    imageUrl = state.examples[0].url,
                    stepName = state.name,
                    stepValue = (float)state.pos
                }, new List<MRBlendshapeSaveState>());
            }
            DefaultStep = AnimationSteps.First(s => s.Key.stepValue == defaultStepVal).Key;
            AnimationSteps[DefaultStep] = defaultValues;
        }
    }
}