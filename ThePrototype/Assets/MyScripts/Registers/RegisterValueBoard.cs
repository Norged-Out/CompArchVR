using TMPro;
using UnityEngine;

/// <summary>
/// Lightweight UI helper that mirrors the current logical register bank values
/// into a fixed list of scene-authored text fields.
/// </summary>
[DisallowMultipleComponent]
public class RegisterValueBoard : MonoBehaviour
{
    static readonly string[] k_RegisterOrder =
    {
        "zero", "at", "v0", "v1",
        "a0", "a1", "a2", "a3",
        "t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7",
        "s0", "s1", "s2", "s3", "s4", "s5", "s6", "s7",
        "t8", "t9", "k0", "k1",
        "gp", "sp", "fp", "ra"
    };

    [Header("Scene References")]
    [SerializeField]
    RegisterBank m_RegisterBank;

    [SerializeField]
    Transform m_TextRoot;

    TMP_Text[] m_RegisterTexts;

    void Awake()
    {
        CacheRegisterTexts();
        RefreshBoard();
    }

    void OnEnable()
    {
        CacheRegisterTexts();
        RefreshBoard();
    }

    void LateUpdate()
    {
        RefreshBoard();
    }

    void CacheRegisterTexts()
    {
        if (m_TextRoot == null)
        {
            m_RegisterTexts = null;
            return;
        }

        var childCount = m_TextRoot.childCount;
        m_RegisterTexts = new TMP_Text[childCount];

        for (var i = 0; i < childCount; i++)
        {
            var child = m_TextRoot.GetChild(i);
            m_RegisterTexts[i] = child.GetComponent<TMP_Text>();

            if (m_RegisterTexts[i] == null)
                m_RegisterTexts[i] = child.GetComponentInChildren<TMP_Text>(true);
        }
    }

    void RefreshBoard()
    {
        if (m_RegisterBank == null || m_RegisterTexts == null)
            return;

        var count = Mathf.Min(k_RegisterOrder.Length, m_RegisterTexts.Length);

        for (var i = 0; i < count; i++)
        {
            var registerText = m_RegisterTexts[i];
            if (registerText == null)
                continue;

            var registerId = k_RegisterOrder[i];
            var registerLabel = m_RegisterBank.GetRegisterDisplayLabel(registerId);
            var registerValue = m_RegisterBank.GetRegisterValue(registerId);
            registerText.text = $"{registerLabel}: {registerValue}";
        }
    }
}
