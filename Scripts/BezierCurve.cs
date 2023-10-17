using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
public class BezierCurve : MonoBehaviour
{
    Vector3[] points;
    Vector3[] stepPoints;
    public float lerpAmount = 0.5f;
    public float height = 1.5f;
    public int lineSteps = 10;
    [SerializeField]
    LineRenderer lineRenderer;
    public bool isDrawing { get; protected set; }

    Vector3 GetPoint(float t)
    {
        return transform.TransformPoint(Bezier.GetPoint(points[0], points[1], points[2], points[3], t));
    }

    Vector3 GetVelocity(float t)
    {
        return transform.TransformPoint(Bezier.GetFirstDerivative(points[0], points[1], points[2], points[3], t)) - transform.position;
    }

    Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }

    void SetEndPoint(Vector3 start, Vector3 end)
    {
        //start point should always 0
        var a = Vector3.zero;
        var b = Vector3.Lerp(start, end, lerpAmount)-start;
        b.y = height;
        var c = Vector3.Lerp(end, start, lerpAmount)-start;
        c.y = height;
        var d = end-start;
        points = new Vector3[]
        {
            a,b,c,d
        };
        ProcessStepPoints();
        ApplyToLineRenderer();
    }

    public void Draw(Vector3 start, Vector3 end)
    {
        transform.position = start;
        SetEndPoint(start, end);
        if (isDrawing == false)
            isDrawing = true;
    }

    public void Clear()
    {
        if (isDrawing == false) return;
        lineRenderer.enabled = false;
        isDrawing = false;
    }

    void ProcessStepPoints()
    {
        Vector3 point = GetPoint(0f);
        stepPoints = new Vector3[lineSteps + 1];
        stepPoints[0] = point;
        for (int i = 1; i <= lineSteps; i++)
        {
            point = GetPoint(i / (float)lineSteps);
            stepPoints[i] = point;
        }
    }

    void ApplyToLineRenderer()
    {
        if (lineRenderer == null)
        {
            Debug.LogError($"Done have LineRenderer reference");
            return;
        }
        if (lineRenderer.useWorldSpace == false)
            lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = stepPoints.Length;
        lineRenderer.SetPositions(stepPoints);
        if (lineRenderer.enabled == false)
            lineRenderer.enabled = true;
    }
#if ODIN_INSPECTOR
    [Button("TestDraw")]
#endif
    void TestDraw(Transform target)
    {
        var a = transform.position;
        if (target == null)
        {
            a.x += 4f;
        }
        else
        {
            a = target.position;
        }
        SetEndPoint(transform.position, a);
        for (int i = 1; i < stepPoints.Length; i++)
        {
            Debug.DrawLine(stepPoints[i - 1], stepPoints[i], Color.yellow, 3f);
        }
    }
#if ODIN_INSPECTOR
    [Button("TestClear")]
#endif
    void TestClear()
    {
        Clear();
    }
}