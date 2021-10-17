using UnityEngine;

namespace PhilResearch
{
    public class PositionTracker : MonoBehaviour
    {
        public Transform root;
        public MeshRenderer body;

        private void OnEnable()
        {
            ReportingLog.current.trackers.Add(this);
        }

        private void OnDisable()
        {
            ReportingLog.current.trackers.Remove(this);
        }
    }
}
