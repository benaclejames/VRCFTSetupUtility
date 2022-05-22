using System;
using System.Collections.Generic;

namespace Editor
{
    public struct ParamData
    {
        public ParamData(string name, ParamMeta.ParameterType type)
        {
            Name = name;
            Type = type;
            SaveStates = new List<(MRBlendshapeSaveState Zero, MRBlendshapeSaveState Value)>();
        }
        
        public string Name;
        public ParamMeta.ParameterType Type;
        public List<(MRBlendshapeSaveState Zero, MRBlendshapeSaveState Value)> SaveStates;

        public bool IsAssigned() => SaveStates.Capacity > 0;
    }
}