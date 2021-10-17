using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhilResearch
{
    public class XrSliderInterface : MonoBehaviour
    {
        [SerializeField]
        private Transform knob;
        [SerializeField]
        private Transform negativeEnd;
        [SerializeField]
        private Transform positiveEnd;
        [SerializeField]
        private int steps;
        [SerializeField]
        private int defaultValue;

        private Transform root;
        private readonly List<Vector3> stepStops = new List<Vector3>();

        public int Value { get; private set; }

        private void Start()
        {
            root = transform;

            var stepTime = 1f / (steps - 1);
            for (int i = 0; i < steps; i++)
            {
                stepStops.Add(Vector3.Lerp(negativeEnd.localPosition, positiveEnd.localPosition, i * stepTime));
            }

            Value = defaultValue;

            if (stepStops.Count > defaultValue && defaultValue > 0)
                knob.localPosition = stepStops[defaultValue];
        }

        public void SetPosition(Vector3 worldPoint)
        {
            var local = root.InverseTransformPoint(worldPoint);
            var distance = float.MaxValue;
            var pos = stepStops[0];
            for (int i = 0; i < stepStops.Count; i++)
            {
                var dis = Vector3.Distance(local, stepStops[i]);
                if(dis < distance)
                {
                    distance = dis;
                    pos = stepStops[i];
                    Value = i;
                }
            }

            knob.localPosition = pos;
        }
    }
}
