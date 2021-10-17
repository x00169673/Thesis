using UnityEngine;

namespace PhilResearch
{
    public class PointerDisplay : MonoBehaviour
    {
        [SerializeField]
        private Transform pointer;
        [SerializeField]
        private LineRenderer line;

        public float length = 0.5f;
        public Color startColor;
        public Color endColor;
        public Transform endTarget;
        public Vector3 targetLocalOffset;
        
        private void LateUpdate()
        {
            if(endTarget != null)
            {
                var targetPoint = endTarget.TransformPoint(targetLocalOffset);
                var distance = Vector3.Distance(pointer.position, targetPoint);
                var stepRange = 1f / (distance * 10f);
                var stepCount = Mathf.RoundToInt(1f / stepRange);
                var buffer = new Vector3[stepCount];
                
                for (int i = 0; i < stepCount; i++)
                {
                    var target = Vector3.Lerp(pointer.position, targetPoint, stepRange * i);
                    var pointing = Vector3.Lerp(pointer.position, pointer.position + pointer.forward * distance, stepRange * i);
                    buffer[i] = Vector3.Lerp(pointing, target, stepRange * i);
                }

                buffer[stepCount - 1] = targetPoint;
                line.positionCount = stepCount;
                line.SetPositions(buffer);
            }
            else
            {
                var buffer = new Vector3[2];
                buffer[0] = pointer.position;
                var ray = new Ray(pointer.position, pointer.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, 10))
                {
                    buffer[1] = hit.point;
                }
                else
                    buffer[1] = pointer.position + pointer.forward * length;

                line.positionCount = 2;
                line.SetPositions(buffer);
            }

            line.startColor = startColor;
            line.endColor = endColor;
        }
    }
}
