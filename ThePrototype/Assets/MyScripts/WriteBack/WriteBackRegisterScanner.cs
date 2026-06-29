using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Physical write-back pedestal that accepts the destination register token.
/// Unlike the decode scanners, this scanner does not emit packets. It only
/// latches the chosen destination register long enough for the write-back
/// controller to validate RegDst and apply the final value.
/// </summary>
[DisallowMultipleComponent]
public class WriteBackRegisterScanner : PedestalScannerBase
{
    [SerializeField]
    Collider m_ScanZone;

    [SerializeField]
    float m_RequiredStableSeconds = 1f;

    [SerializeField]
    float m_LocalPressedOffsetY = -0.02f;

    [SerializeField]
    string m_ExpectedRegisterId = string.Empty;

    readonly HashSet<RegisterToken> m_TokensInZone = new();

    public RegisterToken AcceptedRegister { get; private set; }
    public string ExpectedRegisterId => m_ExpectedRegisterId;

    public event Action<WriteBackRegisterScanner, RegisterToken> RegisterAccepted;

    protected override float RequiredStableSeconds => m_RequiredStableSeconds;
    protected override float PressedOffsetY => m_LocalPressedOffsetY;

    protected override void Awake()
    {
        base.Awake();
        BindZoneHelper();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        BindZoneHelper();
    }

    public void SetActive(bool isActive)
    {
        SetStepActive(isActive);
    }

    /// <summary>
    /// Updates which architectural register should currently be accepted.
    /// This is driven by the authored RegDst button state.
    /// </summary>
    public void SetExpectedRegisterId(string expectedRegisterId)
    {
        m_ExpectedRegisterId = expectedRegisterId ?? string.Empty;

        if (AcceptedRegister != null &&
            !string.Equals(AcceptedRegister.RegisterId, m_ExpectedRegisterId, StringComparison.OrdinalIgnoreCase))
        {
            ResetScanner();
        }
    }

    public new void ResetScanner()
    {
        m_TokensInZone.Clear();
        AcceptedRegister = null;
        base.ResetScanner();
    }

    public void NotifyTokenEntered(RegisterToken registerToken)
    {
        if (registerToken != null)
            m_TokensInZone.Add(registerToken);
    }

    public void NotifyTokenExited(RegisterToken registerToken)
    {
        if (registerToken == null)
            return;

        m_TokensInZone.Remove(registerToken);

        if (AcceptedRegister == registerToken)
        {
            AcceptedRegister = null;
            base.ResetScanner();
        }
    }

    protected override Component GetStableCandidate()
    {
        m_TokensInZone.RemoveWhere(token => token == null);

        foreach (var registerToken in m_TokensInZone)
        {
            if (registerToken == null || registerToken.IsGrabbed)
                continue;

            return registerToken;
        }

        return null;
    }

    protected override void CacheVisualReferences()
    {
        base.CacheVisualReferences();

        if (m_ScanZone == null)
        {
            var scanZoneTransform = transform.Find("Scan Zone");
            if (scanZoneTransform != null)
                m_ScanZone = scanZoneTransform.GetComponent<Collider>();
        }
    }

    void BindZoneHelper()
    {
        if (m_ScanZone == null)
            return;

        var helper = m_ScanZone.GetComponent<WriteBackRegisterScannerZone>();
        if (helper == null)
            helper = m_ScanZone.gameObject.AddComponent<WriteBackRegisterScannerZone>();

        helper.Bind(this);
    }

    protected override void HandleScannerReset()
    {
        AcceptedRegister = null;
    }

    protected override void HandleStableCandidate(Component candidate)
    {
        var registerToken = candidate as RegisterToken;
        if (registerToken == null)
            return;

        if (!string.Equals(registerToken.RegisterId, m_ExpectedRegisterId, StringComparison.OrdinalIgnoreCase))
        {
            FlashFailure();
            return;
        }

        AcceptedRegister = registerToken;
        RegisterAccepted?.Invoke(this, registerToken);
        MarkSuccess();
    }
}
