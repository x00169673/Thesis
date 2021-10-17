using UnityEngine;
using UnityEngine.Events;

namespace PhilResearch
{
    public class Test1Logic : MonoBehaviour
    {
        [SerializeField]
        private XrFloatSliderInterface slider;
        [SerializeField]
        private UnityEvent testComplete;
        [SerializeField]
        private Color normalColor;
        [SerializeField]
        private Color disabledColor;
        [SerializeField]
        private Color grabbedColor;
        [SerializeField]
        private Color missColor;
        [SerializeField]
        private PointerDisplay leftDisplay;
        [SerializeField]
        private PointerDisplay rightDisplay;

        private bool testStarted;
        private bool leftHasBlock;
        private bool rightHasBlock;

        private void Start()
        {
            ReportingLog.current.leftTriggerAction.actionReference.action.performed += (ctx) =>
            {
                if (!testStarted)
                    return;

                if (rightHasBlock)
                {
                    ReportingLog.current.Record("Grab", "rejected", 0, "Left Trigger", "Block 1", "Right Held");
                }
                else
                {
                    var value = ctx.valueType == typeof(float) ? ctx.action.ReadValue<float>() > 0.5f : ctx.action.ReadValue<bool>();
                    if (value)
                    {
                        if (CalculateGrab("Left", ReportingLog.current.leftPoint, out Vector3 point, out Transform target))
                        {
                            leftDisplay.endTarget = target;
                            leftDisplay.targetLocalOffset = leftDisplay.endTarget.InverseTransformPoint(point);
                            leftDisplay.endColor = grabbedColor;
                            leftHasBlock = true;
                        }
                        else
                        {
                            leftDisplay.endTarget = null;
                            leftDisplay.length = 10;
                            leftDisplay.endColor = missColor;
                            leftHasBlock = false;
                        }
                    }
                    else
                    {
                        leftDisplay.endTarget = null;
                        leftHasBlock = false;
                        ReportingLog.current.Record("Grab", "release", value ? 1 : 0, "Left Trigger", "Block 1", "Left Held");
                    }
                }
            };

            ReportingLog.current.rightTriggerAction.actionReference.action.performed += (ctx) =>
            {
                if (!testStarted)
                    return;

                if (leftHasBlock)
                {
                    ReportingLog.current.Record("Grab", "rejected", 0, "Right Trigger", "Block 1", "Left Held");
                }
                else
                {
                    var value = ctx.valueType == typeof(float) ? ctx.action.ReadValue<float>() > 0.5f : ctx.action.ReadValue<bool>();
                    if (value)
                    {
                        if (CalculateGrab("Right", ReportingLog.current.rightPoint, out Vector3 point, out Transform target))
                        {
                            rightDisplay.endTarget = target;
                            rightDisplay.targetLocalOffset = rightDisplay.endTarget.InverseTransformPoint(point);
                            rightDisplay.endColor = grabbedColor;
                            rightHasBlock = true;
                        }
                        else
                        {
                            rightDisplay.endTarget = null;
                            rightDisplay.length = 10;
                            rightDisplay.endColor = missColor;
                            rightHasBlock = false;
                        }
                    }
                    else
                    {
                        rightDisplay.endTarget = null;
                        rightHasBlock = false;
                        ReportingLog.current.Record("Grab", "release", value ? 1 : 0, "Right Trigger", "Block 1", "Left Held");
                    }
                }
            };
        }

        private bool CalculateGrab(string name, Transform pointer, out Vector3 hitPoint, out Transform target)
        {
            Ray ray = new Ray(pointer.position, pointer.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100))
            {
                if (hitInfo.collider != null)
                {
                    var slider = hitInfo.collider.GetComponentInParent<XrFloatSliderInterface>();
                    if (slider != null)
                    {
                        ReportingLog.current.Record("Grab", "success", slider.Value, name + " Trigger", "Block 1");
                        target = hitInfo.transform;
                        hitPoint = hitInfo.point;
                        return true;
                    }
                    else
                    {
                        ReportingLog.current.Record("Grab", "failed", 0, name + " Trigger", "Hit Miss Slider");
                    }
                }
                else
                {
                    ReportingLog.current.Record("Grab", "failed", 0, name + " Trigger", "Hit Miss Collider");
                }
            }
            else
            {
                ReportingLog.current.Record("Grab", "failed", 0, name + " Trigger", "No Hit");
            }
            hitPoint = Vector3.zero;
            target = null;
            return false;
        }

        private void Update()
        {
            if (testStarted)
            {
                if (leftHasBlock)
                {
                    rightDisplay.endTarget = null;
                    rightDisplay.length = 0.5f;
                    rightDisplay.endColor = disabledColor;

                    slider.SetPosition(new Ray(ReportingLog.current.leftPoint.position, ReportingLog.current.leftPoint.forward), "Left", 1);

                    var ctx = ReportingLog.current.leftTriggerAction.actionReference;
                    leftHasBlock = ctx.action.ReadValue<float>() > 0.5f;
                    if(!leftHasBlock)
                    {
                        ReportingLog.current.Record("Grab", "release", slider.Value, "Left Trigger", "Block 1");
                    }
                }
                else if (rightHasBlock)
                {
                    leftDisplay.endTarget = null;
                    leftDisplay.length = 0.5f;
                    leftDisplay.endColor = disabledColor;

                    slider.SetPosition(new Ray(ReportingLog.current.rightPoint.position, ReportingLog.current.rightPoint.forward), "Right", 1);

                    var ctx = ReportingLog.current.rightTriggerAction.actionReference;
                    rightHasBlock = ctx.action.ReadValue<float>() > 0.5f;
                    if (!rightHasBlock)
                    {
                        ReportingLog.current.Record("Grab", "release", slider.Value, "Right Trigger", "Block 1");
                    }
                }
                else
                {
                    rightDisplay.endTarget = null;
                    rightDisplay.length = 10f;
                    rightDisplay.endColor = normalColor;

                    leftDisplay.endTarget = null;
                    leftDisplay.length = 10f;
                    leftDisplay.endColor = normalColor;
                }

                if(slider.Value >= 1f)
                {
                    testStarted = false;
                    leftDisplay.endTarget = null;
                    leftDisplay.length = 0.5f;
                    leftDisplay.endColor = normalColor;
                    rightDisplay.endTarget = null;
                    rightDisplay.length = 0.5f;
                    rightDisplay.endColor = normalColor;
                    ReportingLog.current.Record("Test", "completed", 1);
                    ReportingLog.current.Recording = false;
                    testComplete?.Invoke();
                }
            }
        }

        public void StartTest()
        {
            testStarted = true;
            ReportingLog.current.Recording = true;
            ReportingLog.current.Record("Test", "started", 0);
        }
    }
}
