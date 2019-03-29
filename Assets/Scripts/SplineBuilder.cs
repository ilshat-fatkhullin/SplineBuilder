using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SplineBuilder : MonoBehaviour
{
    public float Accurency;

    public GameObject NodePrefab;

    [SerializeField]
    public List<Transform> Points;    

    LineRenderer lineRenderer;

    Vector3[,] Coefficients;

    List<Vector3> spline;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        spline = new List<Vector3>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject node = Instantiate(NodePrefab, Vector3.zero, Quaternion.identity);
            Points.Add(node.transform);
        }

        UpdateSpline();
    }

    void UpdateSpline()
    {
        Vector3[] points = new Vector3[Points.Count];

        for (int i = 0; i < Points.Count; i++)
        {
            points[i] = Points[i].transform.position;
        }

        CalculateCoefficients(points);

        spline.Clear();

        float step = 1f / Accurency;
        float t = 0;

        for (int i = 0; i < Points.Count - 1; i++)
        {
            while (t < i + 1)
            {
                AddPointIntoSpline(GetSplineAtTime(t));
                t += step;
            }
            AddPointIntoSpline(Points[i + 1].position);
        }

        lineRenderer.positionCount = spline.Count;
        lineRenderer.SetPositions(spline.ToArray());
    }

    void AddPointIntoSpline(Vector3 point)
    {
        spline.Add(point);
        Debug.DrawLine(point, point + Vector3.down);
    }

    Vector3 GetSplineAtTime(float t)
    {
        int splineIndex = Mathf.FloorToInt(t);        
        return Coefficients[splineIndex, 0] + Coefficients[splineIndex, 1] * (t - splineIndex) + Coefficients[splineIndex, 2] * (t - splineIndex) * (t - splineIndex) + Coefficients[splineIndex, 3] * (t - splineIndex) * (t - splineIndex) * (t - splineIndex);        
    }

    Vector3[,] CalculateCoefficients(Vector3[] points)
    {
        Coefficients = new Vector3[points.Length, 4];

        Vector3[] z = new Vector3[points.Length];
        Vector3[] a = new Vector3[points.Length];

        float[] l = new float[points.Length];
        float[] u = new float[points.Length];        

        int n = points.Length - 1;

        for (int i = 1; i <= n - 1; i++)
        {
            a[i] = 3 * (points[i + 1] - 2 * points[i] + points[i - 1]);
        }

        Coefficients[n, 2] = Vector3.zero;
        z[0] = Vector3.zero;
        z[n] = Vector3.zero;
        l[0] = l[n] = 1;
        u[0] = 0;        

        for (int i = 1; i <= n - 1; i++)
        {
            l[i] = 4 - u[i - 1];
            u[i] = 1 / l[i];
            z[i] = (a[i] - z[i - 1]) / l[i];
        }

        for (int i = 0; i < points.Length; i++)
        {
            Coefficients[i, 0] = points[i];
        }

        for (int j = n - 1; j >= 0; j--)
        {
            Coefficients[j, 2] = z[j] - u[j] * Coefficients[j + 1, 2];
            Coefficients[j, 3] = (Coefficients[j + 1, 2] - Coefficients[j, 2]) / 3.0f;
            Coefficients[j, 1] = points[j + 1] - points[j] - Coefficients[j, 2] - Coefficients[j, 3];
        }

        return Coefficients;
    }
}
