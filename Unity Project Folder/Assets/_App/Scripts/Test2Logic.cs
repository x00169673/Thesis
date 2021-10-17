using UnityEngine;
using UnityEngine.Events;

namespace PhilResearch
{
    public class Test2Logic : MonoBehaviour
    {
        [SerializeField]
        private XrFloatSliderInterface slider1;
        [SerializeField]
        private XrFloatSliderInterface slider2;
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
        [SerializeField]
        private float leftTensionAngle = 35;
        [SerializeField]
        private float rightTensionAngle = 35;

        private bool testStarted;
        private XrFloatSliderInterface leftHasBlock;
        private XrFloatSliderInterface rightHasBlock;

        private void Start()
        {
            ReportingLog.current.leftTriggerAction.actionReference.action.performed += (ctx) =>
            {
                if (!testStarted)
                    return;

                    var value = ctx.valueType == typeof(float) ? ctx.action.ReadValue<float>() > 0.5f : ctx.action.ReadValue<bool>();
                if (value)
                {
                    var grab = CalculateGrab("Left", ReportingLog.current.leftPoint, out Vector3 point, out Transform target);
                    if (grab != null)
                    {
                        if (grab != rightHasBlock)
                        {
                            leftDisplay.endTarget = target;
                            leftDisplay.targetLocalOffset = leftDisplay.endTarget.InverseTransformPoint(point);
                            leftDisplay.endColor = grabbedColor;
                            leftHasBlock = grab;
                        }
                        else
                        {
                            ReportingLog.current.Record("Grab", "rejected", 0, "Left Trigger", grab.gameObject.name, "Right Held");
                            leftDisplay.endTarget = null;
                            leftDisplay.length = 10;
                            leftDisplay.endColor = missColor;
                            leftHasBlock = null;
                        }
                    }
                    else
                    {
                        leftDisplay.endTarget = null;
                        leftDisplay.length = 10;
                        leftDisplay.endColor = missColor;
                        leftHasBlock = null;
                    }
                }

            };

            ReportingLog.current.rightTriggerAction.actionReference.action.performed += (ctx) =>
            {
                if (!testStarted)
                    return;


                var value = ctx.valueType == typeof(float) ? ctx.action.ReadValue<float>() > 0.5f : ctx.action.ReadValue<bool>();
                if (value)
                {
                    var grab = CalculateGrab("Right", ReportingLog.current.rightPoint, out Vector3 point, out Transform target);
                    if (grab != null)
                    {
                        if (grab != leftHasBlock)
                        {
                            rightDisplay.endTarget = target;
                            rightDisplay.targetLocalOffset = rightDisplay.endTarget.InverseTransformPoint(point);
                            rightDisplay.endColor = grabbedColor;
                            rightHasBlock = grab;
                        }
                        else
                        {
                            ReportingLog.current.Record("Grab", "rejected", 0, "Right Trigger", grab.gameObject.name, "Left Held");
                            rightDisplay.endTarget = null;
                            rightDisplay.length = 10;
                            rightDisplay.endColor = missColor;
                            rightHasBlock = null;
                        }
                    }
                    else
                    {
                        rightDisplay.endTarget = null;
                        rightDisplay.length = 10;
                        rightDisplay.endColor = missColor;
                        rightHasBlock = null;
                    }
                }

            };

            testStarted = true;
        }

        private XrFloatSliderInterface CalculateGrab(string name, Transform pointer, out Vector3 hitPoint, out Transform target)
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
                        return slider;
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
            return null;
        }

        private void Update()
        {
            if (testStarted)
            {
                if (!ReportingLog.current.Recording)
                    StartTest();

                if (leftHasBlock != null)
                {
                    var tensionStrength = Mathf.Clamp01(1f - Mathf.Clamp01(Vector3.Angle((leftHasBlock.knob.position - ReportingLog.current.leftPoint.position).normalized, ReportingLog.current.leftPoint.forward) / leftTensionAngle));

                    if (tensionStrength > 0)
                    {
                        leftHasBlock.SetPosition(new Ray(ReportingLog.current.leftPoint.position, ReportingLog.current.leftPoint.forward), "Left", tensionStrength * Time.deltaTime);

                        var ctx = ReportingLog.current.leftTriggerAction.actionReference;
                        if (ctx.action.ReadValue<float>() < 0.5f)
                        {
                            ReportingLog.current.Record("Grab", "release", leftHasBlock.Value, "Left Trigger", leftHasBlock.gameObject.name);
                            leftDisplay.endTarget = null;
                            leftHasBlock = null;
                        }
                    }
                    else
                    {
                        ReportingLog.current.Record("Grab", "break", leftHasBlock.Value, "Left Trigger", leftHasBlock.gameObject.name);
                        leftDisplay.endTarget = null;
                        leftHasBlock = null;
                    }
                }
                else
                {
                    leftDisplay.endTarget = null;
                    leftDisplay.length = 10f;
                    leftDisplay.endColor = normalColor;
                }

                if (rightHasBlock != null)
                {

                    var tensionStrength = Mathf.Clamp01(1f - Mathf.Clamp01(Vector3.Angle((rightHasBlock.knob.position - ReportingLog.current.rightPoint.position).normalized, ReportingLog.current.rightPoint.forward) / rightTensionAngle));

                    if (tensionStrength > 0)
                    {
                        rightHasBlock.SetPosition(new Ray(ReportingLog.current.rightPoint.position, ReportingLog.current.rightPoint.forward), "Right", tensionStrength * Time.deltaTime);

                        var ctx = ReportingLog.current.rightTriggerAction.actionReference;
                        if (ctx.action.ReadValue<float>() < 0.5f)
                        {
                            ReportingLog.current.Record("Grab", "release", rightHasBlock.Value, "Right Trigger", rightHasBlock.gameObject.name);
                            rightDisplay.endTarget = null;
                            rightHasBlock = null;
                        }
                    }
                    else
                    {
                        ReportingLog.current.Record("Grab", "break", rightHasBlock.Value, "Right Trigger", rightHasBlock.gameObject.name);
                        rightDisplay.endTarget = null;
                        rightHasBlock = null;
                    }
                }
                else
                {
                    rightDisplay.endTarget = null;
                    rightDisplay.length = 10f;
                    rightDisplay.endColor = normalColor;
                }

                if(slider1 != leftHasBlock && slider1 != rightHasBlock)
                {
                    slider1.SetPosition(Mathf.Lerp(slider1.Value, 0, Time.deltaTime * Mathf.Abs(slider1.maxSpeed * 3)), "None", 1);
                }
                if (slider2 != leftHasBlock && slider2 != rightHasBlock)
                {
                    slider2.SetPosition(Mathf.Lerp(slider2.Value, 0, Time.deltaTime * Mathf.Abs(slider2.maxSpeed * 3)), "None", 1);
                }

                if (slider1.Value >= 1f && slider2.Value >= 1f)
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
