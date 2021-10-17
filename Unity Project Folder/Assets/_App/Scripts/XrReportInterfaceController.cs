using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PhilResearch
{
    public class XrReportInterfaceController : MonoBehaviour
    {
        [SerializeField]
        private UserReportingScript report;
        [SerializeField]
        private int nextSceneIndex;

        public UnityEvent createReportRequest;

        private void Start()
        {
            report.UserReportSubmitted.AddListener(HandleReportSubmitted);
        }

        private void Update()
        {
            if(report.State == UserReportingState.Idle && ReportingLog.current.rightPrimaryPressAction.IsActivated && ReportingLog.current.leftPrimaryPressAction.IsActivated)
            {
                createReportRequest?.Invoke();
            }

            if (ReportingLog.current.rightTriggerAction.IsActivated)
            {
                Ray ray = new Ray(ReportingLog.current.rightPoint.position, ReportingLog.current.rightPoint.forward);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 100))
                {
                    if (hitInfo.collider != null)
                    {
                        var slider = hitInfo.collider.GetComponentInParent<XrSliderInterface>();
                        if (slider != null)
                            slider.SetPosition(hitInfo.point);
                        else
                        {
                            var button = hitInfo.collider.GetComponentInParent<XrClickButtonInterface>();
                            if (button != null)
                            {
                                button.Click?.Invoke();
                            }
                        }
                    }
                }
            }

            if (ReportingLog.current.leftTriggerAction.IsActivated)
            {
                Ray ray = new Ray(ReportingLog.current.leftPoint.position, ReportingLog.current.leftPoint.forward);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 100))
                {
                    if (hitInfo.collider != null)
                    {
                        var slider = hitInfo.collider.GetComponentInParent<XrSliderInterface>();
                        if (slider != null)
                            slider.SetPosition(hitInfo.point);
                        else
                        {
                            var button = hitInfo.collider.GetComponentInParent<XrClickButtonInterface>();
                            if (button != null)
                            {
                                button.Click?.Invoke();
                            }
                        }
                    }
                }
            }
        }

        private void HandleReportSubmitted()
        {
            if (nextSceneIndex > -1)
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
            else
                Application.Quit();
        }

        public void ExitRequest()
        {
            ReportingLog.current.Record("Exit", "Request", 0);
            report.UserReportSubmitted.RemoveListener(HandleReportSubmitted);

            report.IsInSilentMode = true;
            report.CreateUserReport();

            report.UserReportSubmitted.AddListener(() =>
            {
                Application.Quit();
            });
        }

        public void NextRequest()
        {
            ReportingLog.current.Record("Next", "Request", 0);
            report.IsInSilentMode = true;
            report.CreateUserReport();
        }
    }
}
