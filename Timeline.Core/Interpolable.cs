using Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace Timeline
{
    public class Interpolable : InterpolableModel
    {
        private readonly int _hashCode;

        public override string name { get { return _getFinalName != null ? _getFinalName(_name, oci, parameter) : base.name; } }

        public readonly ObjectCtrlInfo oci;
        public readonly SortedList<float, Keyframe> keyframes = new SortedList<float, Keyframe>();
        public bool enabled = true;
        public Color color = Color.white;
        public string alias = "";

        /// <summary>
        /// Loop group tags this track subscribes to (case-insensitive, unique).
        /// Only business tracks contribute to the global tag list; config tracks do not use this.
        /// </summary>
        public readonly List<string> tags = new List<string>();

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

        public static List<string> ParseTagList(string raw)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(raw))
                return result;
            foreach (string part in raw.Split(new[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string n = NormalizeTag(part);
                if (n.Length == 0)
                    continue;
                if (result.Contains(n) == false)
                    result.Add(n);
            }
            return result;
        }

        public static string FormatTagList(IList<string> list)
        {
            if (list == null || list.Count == 0)
                return "";
            return string.Join(",", list.Select(NormalizeTag).Where(t => t.Length > 0).Distinct().ToArray());
        }

        public void SetTagsFromString(string raw)
        {
            tags.Clear();
            tags.AddRange(ParseTagList(raw));
        }

        public bool AddTag(string tag)
        {
            string n = NormalizeTag(tag);
            if (n.Length == 0)
                return false;
            if (tags.Contains(n))
                return false;
            tags.Add(n);
            return true;
        }

        public bool RemoveTag(string tag)
        {
            string n = NormalizeTag(tag);
            return tags.Remove(n);
        }

        public void ClearTags()
        {
            tags.Clear();
        }

        public bool HasTag(string tag)
        {
            string n = NormalizeTag(tag);
            return n.Length > 0 && tags.Contains(n);
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
