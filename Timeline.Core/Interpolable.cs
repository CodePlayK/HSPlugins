using Studio;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Timeline
{
    public enum LoopEnterCondition
    {
        Always = 0,
        Manual = 1
    }

    public enum LoopExitCondition
    {
        Never = 0,
        AfterMaxLoops = 1,
        AtLoopEndOnce = 2
    }

    public enum LoopExitBehavior
    {
        ContinueGlobalTime = 0,
        HoldLastValue = 1
    }

    public class Interpolable : InterpolableModel
    {
        private readonly int _hashCode;

        public override string name { get { return _getFinalName != null ? _getFinalName(_name, oci, parameter) : base.name; } }

        public readonly ObjectCtrlInfo oci;
        public readonly SortedList<float, Keyframe> keyframes = new SortedList<float, Keyframe>();
        public bool enabled = true;
        public Color color = Color.white;
        public string alias = "";

        // === Loop support (phase 1) ===
        public bool loopEnabled = false;
        public float loopStart = 0f;
        public float loopEnd = 0f;
        public int maxLoops = -1; // -1 = infinite
        public LoopEnterCondition enterCondition = LoopEnterCondition.Always;
        public LoopExitCondition exitCondition = LoopExitCondition.Never;
        public LoopExitBehavior exitBehavior = LoopExitBehavior.ContinueGlobalTime;

        // Runtime state (not serialized)
        [System.NonSerialized] public bool isInLoop = false;
        [System.NonSerialized] public float loopEnterGlobalTime = 0f;
        [System.NonSerialized] public int currentLoopCount = 0;
        [System.NonSerialized] public bool holdAfterExit = false;
        [System.NonSerialized] public float heldTime = 0f;

        public Interpolable(ObjectCtrlInfo oci, InterpolableModel interpolableModel) : base(interpolableModel.GetParameter(oci), interpolableModel)
        {
            if (useOciInHash)
                this.oci = oci;

            unchecked
            {
                int hash = base.GetHashCode();
                _hashCode = hash * 31 + (this.oci != null ? this.oci.GetHashCode() : 0);
            }
        }

        public Interpolable(ObjectCtrlInfo oci, object parameter, InterpolableModel interpolableModel) : base(parameter, interpolableModel)
        {
            if (useOciInHash)
                this.oci = oci;

            unchecked
            {
                int hash = base.GetHashCode();
                _hashCode = hash * 31 + (this.oci != null ? this.oci.GetHashCode() : 0);
            }
        }

        public void ResetLoopState()
        {
            isInLoop = false;
            loopEnterGlobalTime = 0f;
            currentLoopCount = 0;
            holdAfterExit = false;
            heldTime = 0f;
        }

        public bool InterpolateBefore(object leftValue, object rightValue, float factor)
        {
            if (CheckIntegrity(leftValue, rightValue))
                _interpolateBefore(oci, parameter, leftValue, rightValue, factor);
            else
                return false;
            return true;
        }

        public bool InterpolateAfter(object leftValue, object rightValue, float factor)
        {
            if (CheckIntegrity(leftValue, rightValue))
                _interpolateAfter(oci, parameter, leftValue, rightValue, factor);
            else
                return false;
            return true;
        }

        public object ReadValueFromXml(XmlNode node)
        {
            return _readValueFromXml(parameter, node);
        }

        public void WriteValueToXml(XmlTextWriter writer, object value)
        {
            _writeValueToXml(parameter, writer, value);
        }

        public object GetValue()
        {
            return _getValue(oci, parameter);
        }

        private bool CheckIntegrity(object leftValue, object rightValue)
        {
            return (useOciInHash == false || oci != null) && (_checkIntegrity == null || _checkIntegrity(oci, parameter, leftValue, rightValue));
        }

        public bool ShouldShow()
        {
            if (_shouldShow == null)
                return true;
            return _shouldShow(oci, parameter);
        }

        public int GetBaseHashCode()
        {
            return base.GetHashCode();
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return $"oci: [{oci}] " + base.ToString();
        }

        public static string NormalizeTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return "";
            return tag.Trim().ToLowerInvariant();
        }

        public bool IsLoopConfigTrack()
        {
            return id == Timeline.LoopFromId
                || id == Timeline.LoopToId
                || id == Timeline.LoopStatId
                || id == Timeline.LoopEndId;
        }
    }
}
