using System.Collections.Generic;
using JetBrains.Annotations;

namespace VRCFaceTracking.Tools.Setup_Utility.Editor
{
    public enum ParamType
    {
        Float,
        Binary
    }

    public class ParamMeta
    {
        public Parameter[] parameters { get; set; }
    }
    
    public class Data
    {
        public List<State> states { get; set; }
        public double? defaultState { get; set; }
    }

    public class Example
    {
        public string type { get; set; }
        public string url { get; set; }
    }

    public class Parameter
    {
        public string name { get; set; }
        public int type { get; set; }
        public int? animatorLayer { get; set; }
        public Data data { get; set; }
    }

    public class State
    {
        public string name { get; set; }
        public double pos { get; set; }
        [CanBeNull] public string desc { get; set; }
        [CanBeNull] public List<Example> examples { get; set; }
    }
}