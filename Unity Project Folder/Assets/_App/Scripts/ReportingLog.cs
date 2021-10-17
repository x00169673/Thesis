using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Samples.ControllerSample;

namespace PhilResearch
{
    public class ReportingLog : MonoBehaviour
    {
        public static ReportingLog current;
        public static Guid sessionId = Guid.NewGuid();

        [Serializable]
        public class Report
        {
            [Serializable]
            public class Renderer
            {
                public string name;
                public int id;
                public float3 position;
                public quaternion rotation;
                public float2 screenSpace;
                public float screenDistance;
            }

            [Serializable]
            public class Transform
            {
                public float3 position;
                public quaternion rotation;
            }

            [Serializable]
            public class TrackingTransforms
            {
                public Transform head = null;
                public Transform leftPoint = null;
                public Transform leftGrip = null;
                public Transform righPoint = null;
                public Transform rightGrip = null;
            }

            [Serializable]
            public class Action
            {
                public string id;
                public string status;
                public float value;
                public string[] tags;
            }

            [Serializable]
            public class Record
            {
                public string utc;
                public TrackingTransforms trackingTransforms = null;
                public List<Renderer> renderers = null;
                public List<Action> actions = null;

                public bool Dirty
                {
                    get
                    {
                        if ((actions != null && actions.Count > 0)
                            || (renderers != null && renderers.Count > 0)
                            || trackingTransforms != null)
                        {
                            
                            return true;
                        }
                        else
                            return false;
                    }
                }
            }

            public string version;
            public string testName;
            public string utcStart;
            public string utcSubmit;
            public string deviceType;
            public string deviceModel;
            public string deviceId;
            public string sessionId;
            public int2 resolution;
            public List<string> trackingDevices = new List<string>();
            public List<int> scores = new List<int>();
            public List<Record> records = new List<Record>();
        }

        public string SessionId => report.sessionId;
        public string testName;
        public float frequency;

        public Transform head;
        public Transform leftPoint;
        public Transform rightPoint;
        public Transform leftGrip;
        public Transform rightGrip;

        public ActionToButtonISX leftGripAction;
        public ActionToButtonISX rightGripAction;
        public ActionToButtonISX leftTriggerAction;
        public ActionToButtonISX rightTriggerAction;
        public ActionToVector2SliderISX leftAxisPositionAction;
        public ActionToVector2SliderISX rightAxisPositionAction;
        public ActionToButtonISX leftAxisTouchAction;
        public ActionToButtonISX rightAxisTouchAction;
        public ActionToButtonISX leftAxisPressAction;
        public ActionToButtonISX rightAxisPressAction;
        public ActionToButtonISX leftPrimaryTouchAction;
        public ActionToButtonISX rightPrimaryTouchAction;
        public ActionToButtonISX leftSecondaryTouchAction;
        public ActionToButtonISX rightSecondaryTouchAction;
        public ActionToButtonISX leftPrimaryPressAction;
        public ActionToButtonISX rightPrimaryPressAction;
        public ActionToButtonISX leftSecondaryPressAction;
        public ActionToButtonISX rightSecondaryPressAction;

        public List<XrSliderInterface> questionSliders;
        public List<PositionTracker> trackers = new List<PositionTracker>();
        private bool recording = false;
        public bool Recording
        {
            get => recording;
            set
            {
                if(recording && !value)
                {
                    if (working.Dirty)
                    {
                        working.utc = DateTime.UtcNow.ToString("o");
                        report.records.Add(working);
                        working = new Report.Record();
                    }
                }
                recording = value;
            }
        }

        public string ReportData
        {
            get
            {
                report.scores = questionSliders.ConvertAll((slider) => slider.Value);
                report.utcSubmit = DateTime.UtcNow.ToString("o");
                report.trackingDevices = new List<string>();
                var devices = new List<InputDevice>();
                InputDevices.GetDevices(devices);
                foreach(var device in devices)
                {
                    report.trackingDevices.Add(device.name);
                }
                return JsonUtility.ToJson(report);
            }
        }

        private float next;
        private Report report = new Report();
        private Report.Record working = new Report.Record();

        private void Start()
        {
            current = this;
            report = new Report();
            report.testName = testName;
            report.resolution = new int2(Screen.width, Screen.height);
            report.utcStart = DateTime.UtcNow.ToString("o");
            report.version = Application.version;
            report.deviceId = SystemInfo.deviceUniqueIdentifier;
            report.deviceType = SystemInfo.deviceType.ToString();
            report.deviceModel = SystemInfo.deviceModel;
            report.sessionId = sessionId.ToString("N");
            working = new Report.Record();

            leftGripAction.actionReference.action.performed += (ctx) => Record("Left Grip", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            rightGripAction.actionReference.action.performed += (ctx) => Record("Right Grip", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            leftTriggerAction.actionReference.action.performed += (ctx) => Record("Left Trigger", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            rightTriggerAction.actionReference.action.performed += (ctx) => Record("Right Trigger", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            leftAxisTouchAction.actionReference.action.performed += (ctx) => Record("Left Axis Touch", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            rightAxisTouchAction.actionReference.action.performed += (ctx) => Record("Right Axis Touch", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            leftAxisPressAction.actionReference.action.performed += (ctx) => Record("Left Axis Press", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            rightAxisPressAction.actionReference.action.performed += (ctx) => Record("Right Axis Touch", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            leftPrimaryTouchAction.actionReference.action.performed += (ctx) => Record("Left Primary Touch", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            rightPrimaryTouchAction.actionReference.action.performed += (ctx) => Record("Right Primary Touch", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            leftSecondaryTouchAction.actionReference.action.performed += (ctx) => Record("Left Primary Press", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            rightSecondaryTouchAction.actionReference.action.performed += (ctx) => Record("Right Primary Press", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            leftPrimaryPressAction.actionReference.action.performed += (ctx) => Record("Left Secondary Touch", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            rightPrimaryPressAction.actionReference.action.performed += (ctx) => Record("Right Secondary Touch", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            leftSecondaryPressAction.actionReference.action.performed += (ctx) => Record("Left Secondary Press", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);
            rightSecondaryPressAction.actionReference.action.performed += (ctx) => Record("Right Secondary Press", "performed", ctx.valueType == typeof(float) ? ctx.ReadValue<float>() : ctx.ReadValue<bool>() ? 0 : 1);

            leftAxisPositionAction.actionReference.action.performed += (ctx) =>
            {
                var value = ctx.ReadValue<Vector2>();
                Record("Left Axis X", "performed", value.x);
                Record("Left Axis Y", "performed", value.y);
            };

            rightAxisPositionAction.actionReference.action.performed += (ctx) =>
            {
                var value = ctx.ReadValue<Vector2>();
                Record("Right Axis X", "performed", value.x);
                Record("Right Axis Y", "performed", value.y);
            };
        }

        private void Update()
        {
            if (!Recording)
                return;

            if (working == null)
                working = new Report.Record();

            if (Time.unscaledTime > next)
            {
                next = Time.unscaledTime + frequency;
                
                var tracking = new Report.TrackingTransforms();

                if (head.position != Vector3.zero || head.rotation.eulerAngles != Vector3.zero)
                    tracking.head = new Report.Transform { position = head.position, rotation = head.rotation };
                if (head.position != Vector3.zero || head.rotation.eulerAngles != Vector3.zero)
                    tracking.leftGrip = new Report.Transform { position = leftGrip.position, rotation = leftGrip.rotation };
                if (head.position != Vector3.zero || head.rotation.eulerAngles != Vector3.zero)
                    tracking.leftPoint = new Report.Transform { position = leftPoint.position, rotation = leftPoint.rotation };
                if (head.position != Vector3.zero || head.rotation.eulerAngles != Vector3.zero)
                    tracking.rightGrip = new Report.Transform { position = rightGrip.position, rotation = rightGrip.rotation };
                if (head.position != Vector3.zero || head.rotation.eulerAngles != Vector3.zero)
                    tracking.righPoint = new Report.Transform { position = rightPoint.position, rotation = rightPoint.rotation };

                if (tracking.head != null
                    || tracking.leftGrip != null
                    || tracking.leftPoint != null
                    || tracking.rightGrip != null
                    || tracking.righPoint != null)
                    working.trackingTransforms = tracking;

                foreach(var tracker in trackers)
                {
                    Record(tracker);
                }

                if (working.Dirty)
                {
                    working.utc = DateTime.UtcNow.ToString("o");
                    report.records.Add(working);
                    working = new Report.Record();
                }
            }
            else if(working.Dirty)
            {
                next = Time.unscaledTime + frequency;

                var tracking = new Report.TrackingTransforms();

                if (head.position != Vector3.zero || head.rotation.eulerAngles != Vector3.zero)
                    tracking.head = new Report.Transform { position = head.position, rotation = head.rotation };
                if (head.position != Vector3.zero || head.rotation.eulerAngles != Vector3.zero)
                    tracking.leftGrip = new Report.Transform { position = leftGrip.position, rotation = leftGrip.rotation };
                if (head.position != Vector3.zero || head.rotation.eulerAngles != Vector3.zero)
                    tracking.leftPoint = new Report.Transform { position = leftPoint.position, rotation = leftPoint.rotation };
                if (head.position != Vector3.zero || head.rotation.eulerAngles != Vector3.zero)
                    tracking.rightGrip = new Report.Transform { position = rightGrip.position, rotation = rightGrip.rotation };
                if (head.position != Vector3.zero || head.rotation.eulerAngles != Vector3.zero)
                    tracking.righPoint = new Report.Transform { position = rightPoint.position, rotation = rightPoint.rotation };

                if (tracking.head != null
                    || tracking.leftGrip != null
                    || tracking.leftPoint != null
                    || tracking.rightGrip != null
                    || tracking.righPoint != null)
                    working.trackingTransforms = tracking;

                working.utc = DateTime.UtcNow.ToString("o");
                report.records.Add(working);
                working = new Report.Record();
            }
        }

        public void Record(string id, string status, float value, params string[] tags)
        {
            if (!Recording)
                return;

            if (working == null)
                working = new Report.Record();

            var action = new Report.Action
            {
                id = id,
                status = status,
                value = value,
                tags = tags,
            };

            if (working.actions == null)
                working.actions = new List<Report.Action>();

            working.actions.Add(action);
        }

        public void Record(PositionTracker tracker)
        {
            if (!Recording)
                return;

            if (working == null)
                working = new Report.Record();

            var trans = tracker.root;
            var screenPos = Camera.current != null ? Camera.current.WorldToScreenPoint(trans.position) : Vector3.zero;
            screenPos.x = (screenPos.x / Screen.width * 2) - 1;
            screenPos.y = (screenPos.y / Screen.height * 2) - 1;
            var geometry = new Report.Renderer
            {
                name = tracker.gameObject.name,
                id = tracker.gameObject.GetInstanceID(),
                position = trans.position,
                rotation = trans.rotation,
                screenSpace = new float2(screenPos.x, screenPos.y),
                screenDistance = screenPos.z
            };

            if (working.renderers == null)
                working.renderers = new List<Report.Renderer>();

            working.renderers.Add(geometry);
        }
    }
}
