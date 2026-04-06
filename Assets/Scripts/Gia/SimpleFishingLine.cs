using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SimpleFishingLine : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("The start point of the line (e.g., rod tip)")]
    public Transform lineStartPoint;

    [Tooltip("The end point of the line (e.g., the bobber)")]
    public Transform lineTarget;

    [Header("Line Settings")]
    [Tooltip("Material for the line. Defaults to pink if missing.")]
    public Material lineMaterial;

    [Tooltip("Maximum sag of the line in meters")]
    [Range(0f, 10f)]
    public float lineSag = 1.0f;

    [Tooltip("Width of the line")]
    [Range(0.01f, 0.5f)]
    public float lineWidth = 0.02f;

    [Tooltip("Number of segments for the curve (more is smoother)")]
    [Range(5, 50)]
    public int lineSegments = 20;

    private LineRenderer _lineRenderer;

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    void SetupLineRenderer()
    {
        if (lineMaterial != null)
        {
            _lineRenderer.material = lineMaterial;
        }
        else
        {
            Debug.LogWarning("[SimpleFishingLine] 'Line Material' is missing! Line will default to pink.");
        }

        _lineRenderer.positionCount = lineSegments;
        _lineRenderer.startWidth = lineWidth;
        _lineRenderer.endWidth = lineWidth;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.enabled = true;
    }

    void Update()
    {
        if (lineStartPoint == null || lineTarget == null)
        {
            if (_lineRenderer.enabled)
                _lineRenderer.enabled = false;
            return;
        }

        if (!_lineRenderer.enabled)
            _lineRenderer.enabled = true;

        UpdateLineProperties();
        DrawLine();
    }

    void UpdateLineProperties()
    {
        if (_lineRenderer.startWidth != lineWidth)
        {
            _lineRenderer.startWidth = lineWidth;
            _lineRenderer.endWidth = lineWidth;
        }

        if (_lineRenderer.positionCount != lineSegments)
        {
            _lineRenderer.positionCount = lineSegments;
        }
    }

    void DrawLine()
    {
        Vector3 startPoint = lineStartPoint.position;
        Vector3 endPoint = lineTarget.position;

        for (int i = 0; i < lineSegments; i++)
        {
            float t = (float)i / (float)(lineSegments - 1);

            Vector3 straightLinePos = Vector3.Lerp(startPoint, endPoint, t);

            float sagAmount = Mathf.Sin(t * Mathf.PI) * lineSag;

            Vector3 sagPos = straightLinePos + (Vector3.down * sagAmount);

            _lineRenderer.SetPosition(i, sagPos);
        }
    }
}