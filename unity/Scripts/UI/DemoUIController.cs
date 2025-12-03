using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Very small UI helper for the investor demo.
/// Use it to switch between Student/Instructor views and run a short tour.
/// Wire Buttons from the Canvas to the public methods.
/// </summary>
public class DemoUIController : MonoBehaviour
{
    public Transform playerTransform;
    public Transform studentViewPoint;
    public Transform instructorViewPoint;
    public float travelTime = 1.0f;
    public WhiteboardInteractable whiteboard;

    public Button toStudentBtn;
    public Button toInstructorBtn;
    public Button startTourBtn;
    public Button clearBoardBtn;

    void Start()
    {
        if (toStudentBtn) toStudentBtn.onClick.AddListener(() => MoveTo(studentViewPoint));
        if (toInstructorBtn) toInstructorBtn.onClick.AddListener(() => MoveTo(instructorViewPoint));
        if (startTourBtn) startTourBtn.onClick.AddListener(() => StartCoroutine(RunTour()));
        if (clearBoardBtn && whiteboard) clearBoardBtn.onClick.AddListener(() => whiteboard.ClearBoard());
    }

    public void MoveTo(Transform target)
    {
        if (playerTransform == null || target == null) return;
        StopAllCoroutines();
        StartCoroutine(LerpToPoint(playerTransform, target.position, target.rotation, travelTime));
    }

    IEnumerator RunTour()
    {
        if (studentViewPoint == null || instructorViewPoint == null || playerTransform == null) yield break;
        yield return LerpToPoint(playerTransform, studentViewPoint.position, studentViewPoint.rotation, travelTime);
        yield return new WaitForSeconds(0.8f);
        yield return LerpToPoint(playerTransform, instructorViewPoint.position, instructorViewPoint.rotation, travelTime);
        // animate a quick pointer to the whiteboard (optional)
    }

    IEnumerator LerpToPoint(Transform t, Vector3 pos, Quaternion rot, float duration)
    {
        Vector3 startPos = t.position;
        Quaternion startRot = t.rotation;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            t.position = Vector3.Lerp(startPos, pos, p);
            t.rotation = Quaternion.Slerp(startRot, rot, p);
            yield return null;
        }
        t.position = pos;
        t.rotation = rot;
    }
}
