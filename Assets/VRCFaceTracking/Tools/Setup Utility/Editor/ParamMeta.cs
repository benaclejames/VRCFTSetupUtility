using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRCFaceTracking.Tools.Setup_Utility.Editor
{
    [CreateAssetMenu(fileName = "Parameter Meta", menuName = "VRCFT/Parameter Meta")]
    public class ParamMeta
    {
        public enum ParameterType
        {
            Float,
            Binary
        }
        
        [Serializable]
        public class Parameter
        {
            public string Name;
            public ParameterType Type;
        }

        [SerializeField] public List<Parameter> Parameters;
    }
}