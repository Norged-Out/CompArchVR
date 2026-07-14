using UnityEngine;

/// <summary>
/// Starts an object at a random authored local-space offset, then eases it
/// back to the authored placement over a fixed duration.
/// Attach this to scene-authored geometry such as walls, ceilings, or
/// platforms when a simple opening settle-in effect is desired.
/// </summary>
[DisallowMultipleComponent]
public sealed class AuthoredOffsetLerp : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField]
    bool m_EnableOffsetEffect = false;

    [Header("Local Offset Range")]
    [SerializeField]
    Vector3 m_MinLocalOffset = new(-1f, 0f, -1f);

    [SerializeField]
    Vector3 m_MaxLocalOffset = new(1f, 0f, 1f);

    [SerializeField]
    float m_MinimumOffsetDistance = 5f;

    [Header("Timing")]
    [SerializeField]
    float m_ReturnSeconds = 10f;

    [Header("Debug")]
    [SerializeField]
    float m_CurrentDistanceFromDestination;

    Vector3 m_AuthoredLocalPosition;
    Vector3 m_StartLocalPosition;
    float m_ElapsedSeconds;
    bool m_IsReturning;

    void Awake()
    {
        if (!m_EnableOffsetEffect)
        {
            // Default to a dormant component so the effect can be pre-attached
            // to scene pieces without changing the authored layout until needed.
            enabled = false;
            return;
        }

        m_AuthoredLocalPosition = transform.localPosition;

        var sampledOffset = SampleOffset();
        m_StartLocalPosition = m_AuthoredLocalPosition + sampledOffset;
        transform.localPosition = m_StartLocalPosition;
        RefreshCurrentDistance();

        if (m_ReturnSeconds <= 0f || sampledOffset.sqrMagnitude <= Mathf.Epsilon)
        {
            transform.localPosition = m_AuthoredLocalPosition;
            RefreshCurrentDistance();
            enabled = false;
            return;
        }

        m_IsReturning = true;
    }

    void Update()
    {
        if (!m_IsReturning)
            return;

        m_ElapsedSeconds += Time.deltaTime;

        var duration = Mathf.Max(0.0001f, m_ReturnSeconds);
        var t = Mathf.Clamp01(m_ElapsedSeconds / duration);
        transform.localPosition = Vector3.Lerp(m_StartLocalPosition, m_AuthoredLocalPosition, t);
        RefreshCurrentDistance();

        if (t < 1f)
            return;

        transform.localPosition = m_AuthoredLocalPosition;
        RefreshCurrentDistance();
        m_IsReturning = false;
        enabled = false;
    }

    void OnValidate()
    {
        if (m_MinimumOffsetDistance < 0f)
            m_MinimumOffsetDistance = 0f;

        if (m_ReturnSeconds < 0f)
            m_ReturnSeconds = 0f;
    }

    Vector3 SampleOffset()
    {
        if (m_MinimumOffsetDistance <= 0f)
            return SampleBoxOffset();

        var minimumDistanceSquared = m_MinimumOffsetDistance * m_MinimumOffsetDistance;
        var bestOffset = SampleBoxOffset();
        var bestDistanceSquared = bestOffset.sqrMagnitude;

        if (bestDistanceSquared >= minimumDistanceSquared)
            return bestOffset;

        const int sampleAttempts = 24;
        for (var attempt = 0; attempt < sampleAttempts; attempt++)
        {
            var candidate = SampleBoxOffset();
            var candidateDistanceSquared = candidate.sqrMagnitude;

            if (candidateDistanceSquared > bestDistanceSquared)
            {
                bestOffset = candidate;
                bestDistanceSquared = candidateDistanceSquared;
            }

            if (candidateDistanceSquared >= minimumDistanceSquared)
                return candidate;
        }

        // If the authored range box is too small to satisfy the minimum
        // distance, fall back to the furthest reachable offset rather than
        // silently returning something near zero.
        var furthestCornerOffset = GetFurthestCornerOffset();
        return furthestCornerOffset.sqrMagnitude > bestDistanceSquared
            ? furthestCornerOffset
            : bestOffset;
    }

    Vector3 SampleBoxOffset()
    {
        return new Vector3(
            SampleRange(m_MinLocalOffset.x, m_MaxLocalOffset.x),
            SampleRange(m_MinLocalOffset.y, m_MaxLocalOffset.y),
            SampleRange(m_MinLocalOffset.z, m_MaxLocalOffset.z));
    }

    Vector3 GetFurthestCornerOffset()
    {
        return new Vector3(
            SelectFurthestAxis(m_MinLocalOffset.x, m_MaxLocalOffset.x),
            SelectFurthestAxis(m_MinLocalOffset.y, m_MaxLocalOffset.y),
            SelectFurthestAxis(m_MinLocalOffset.z, m_MaxLocalOffset.z));
    }

    static float SampleRange(float a, float b)
    {
        var min = Mathf.Min(a, b);
        var max = Mathf.Max(a, b);
        return Random.Range(min, max);
    }

    static float SelectFurthestAxis(float a, float b)
    {
        return Mathf.Abs(a) >= Mathf.Abs(b) ? a : b;
    }

    void RefreshCurrentDistance()
    {
        m_CurrentDistanceFromDestination = Vector3.Distance(transform.localPosition, m_AuthoredLocalPosition);
    }
}
