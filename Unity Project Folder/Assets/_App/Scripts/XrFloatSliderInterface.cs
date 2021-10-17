using UnityEngine;
using UnityEngine.XR.OpenXR.Samples.ControllerSample;

namespace PhilResearch
{
    public class XrFloatSliderInterface : MonoBehaviour
    {
        [SerializeField]
        public Transform knob;
        [SerializeField]
        private Transform startPoint;
        [SerializeField]
        private Transform endPoint;
        [SerializeField]
        private float defaultValue;
        [SerializeField]
        private LineRenderer line;
        [SerializeField]
        private Color startColor;
        [SerializeField]
        private Color goalColor;
        [SerializeField]
        public float maxSpeed = 1f;

        public float Value { get; private set; }

        private float reportedValue = -1;

        private void Start()
        {
            Value = Mathf.Clamp01(defaultValue);

            knob.localPosition = Vector3.Lerp(startPoint.localPosition, endPoint.localPosition, Value);
        }

        public void SetPosition(Ray ray, string hand, float delta)
        {
            startPoint.LookAt(endPoint);
            var nPlane = new Plane(startPoint.right, startPoint.position);
            if(nPlane.Raycast(ray, out float enter))
            {
                SetPosition(ray.GetPoint(enter), hand, delta);
            }
        }

        public void SetPosition(Vector3 worldPoint, string hand, float delta)
        {
            startPoint.LookAt(endPoint);
            var heading = worldPoint - startPoint.position;
            var dot = Vector3.Dot(heading, startPoint.forward);
            var time = Mathf.Clamp01(dot / Vector3.Distance(startPoint.localPosition, endPoint.localPosition));
            SetPosition(time, hand, delta);
        }

        public void SetPosition(float value, string hand, float delta)
        {
            if(reportedValue < 0)
            {
                ReportingLog.current.Record("Move", "inital", value, gameObject.name, hand);
                reportedValue = value;
            }
            else
            {
                if(Mathf.Abs(value - reportedValue) >= 0.1f)
                {
                    ReportingLog.current.Record("Move", "update", value, gameObject.name, hand);
                    reportedValue = value;
                }
            }

            var diff = value - Value;

            if (Mathf.Abs(diff) > maxSpeed * delta)
            {
                if (diff < 0)
                    diff = -maxSpeed * delta;
                else
                    diff = maxSpeed * delta;
            }

            Value += diff;

            knob.localPosition = Vector3.Lerp(startPoint.localPosition, endPoint.localPosition, Value);

            line.startColor = Color.Lerp(startColor, goalColor, Value);
            line.endColor = line.startColor;
        }
    }
}
