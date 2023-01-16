using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierTest : MonoBehaviour
{
    public GameObject startP, controlP, endP;
    [SerializeField]
    private int segment = 5;

    public LineRenderer lr;

    // Start is called before the first frame update
    void Start()
    {
        lr.positionCount = segment;
    }

    // Update is called once per frame
    void Update()
    {

        DrawQuadraticBezierCurve(startP.transform.position, controlP.transform.position, endP.transform.position, segment);

    }

    void DrawQuadraticBezierCurve(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, int numSegments)
    {
        // Calculate the step size based on the number of segments
        float stepSize = 1.0f / numSegments;

        // Set the starting position to the start point
        Vector3 currentPos = startPoint;

        // Loop through the number of segments
        for (int i = 0; i < numSegments; i++)
        {
            // Calculate the next position on the curve
            Vector3 nextPos = CalculateQuadraticBezierPoint(startPoint, controlPoint, endPoint, stepSize * i);

            // Draw a line from the current position to the next position
            Debug.DrawLine(currentPos, nextPos, Color.red);

            //Draw line renderer in real-time
            lr.SetPosition(i, currentPos);

            // Set the current position to the next position
            currentPos = nextPos;
        }
    }

    Vector3 CalculateQuadraticBezierPoint(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint, float t)
    {
        // Calculate the bezier curve using the quadratic formula
        Vector3 bezierPoint = (1 - t) * (1 - t) * startPoint + 2 * (1 - t) * t * controlPoint + t * t * endPoint;

        // Return the calculated point
        return bezierPoint;
    }
}
