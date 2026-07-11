using UnityEngine;

/// <summary>
/// Scene-side gate wrapper.
/// The authored transform position is treated as the closed pose, and the gate
/// opens by moving along its local Y axis by the configured offset.
/// </summary>
[DisallowMultipleComponent]
public sealed class LessonGate : MonoBehaviour
{
    [SerializeField]
    GateId m_GateId;

    [SerializeField]
    Transform m_MovableRoot;

    [SerializeField]
    float m_OpenYOffset = -2f;

    [SerializeField]
    float m_MoveDurationSeconds = 0.5f;

    Vector3 m_ClosedLocalPosition;
    Vector3 m_OpenLocalPosition;
    Coroutine m_MoveRoutine;
    bool m_IsInitialized;

    /// <summary>
     /// Stable identifier used by the lesson-level gate controller.
     /// </summary>
    public GateId Id => m_GateId;

    /// <summary>
    /// Opens or closes the gate by animating between its authored closed pose
    /// and an opened pose offset along local Y.
     /// </summary>
    public void SetOpen(bool isOpen)
    {
        EnsureInitialized();

        var targetPosition = isOpen ? m_OpenLocalPosition : m_ClosedLocalPosition;

        if (!Application.isPlaying)
        {
            m_MovableRoot.localPosition = targetPosition;
            return;
        }

        if (m_MoveRoutine != null)
            StopCoroutine(m_MoveRoutine);

        m_MoveRoutine = StartCoroutine(MoveTo(targetPosition));
    }

    /// <summary>
    /// Caches the authored closed pose and derives the corresponding open pose.
    /// </summary>
    void EnsureInitialized()
    {
        if (m_IsInitialized)
            return;

        if (m_MovableRoot == null)
            m_MovableRoot = transform;

        m_ClosedLocalPosition = m_MovableRoot.localPosition;
        m_OpenLocalPosition = m_ClosedLocalPosition + (Vector3.up * m_OpenYOffset);
        m_IsInitialized = true;
    }

    /// <summary>
    /// Smoothly moves the gate toward the requested local pose over the configured duration.
    /// </summary>
    System.Collections.IEnumerator MoveTo(Vector3 targetPosition)
    {
        var startPosition = m_MovableRoot.localPosition;
        var duration = Mathf.Max(0f, m_MoveDurationSeconds);

        if (duration <= 0f)
        {
            m_MovableRoot.localPosition = targetPosition;
            m_MoveRoutine = null;
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsed / duration);
            m_MovableRoot.localPosition = Vector3.Lerp(startPosition, targetPosition, normalizedTime);
            yield return null;
        }

        m_MovableRoot.localPosition = targetPosition;
        m_MoveRoutine = null;
    }
}

/// <summary>
/// Identifies the authored navigation gates used by the lesson route.
/// </summary>
public enum GateId
{
    Fetch,
    Decode,
    Execute,
    WriteBackShortcut,
    PcUpdate,
    Exit,
}
