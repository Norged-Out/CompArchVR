using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Small bridge component for the authored data-packet reset button.
/// It only snaps free packets back to their original spawn poses and does not
/// alter packet values, sign-extension state, or lesson progress.
/// </summary>
[DisallowMultipleComponent]
public class DataPacketResetButton : MonoBehaviour
{
    [SerializeField]
    XRSimpleInteractable m_Interactable;

    void Awake()
    {
        CacheReferences();
    }

    void OnEnable()
    {
        CacheReferences();

        if (m_Interactable == null)
            return;

        m_Interactable.firstSelectEntered.AddListener(OnFirstSelectEntered);
    }

    void OnDisable()
    {
        if (m_Interactable == null)
            return;

        m_Interactable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
    }

    void OnFirstSelectEntered(SelectEnterEventArgs _)
    {
        foreach (var dataPacket in DataPacketToken.ActiveTokens)
        {
            if (dataPacket == null || dataPacket.IsLatched)
                continue;

            dataPacket.ResetToSpawnPose();
        }
    }

    void CacheReferences()
    {
        m_Interactable ??= GetComponent<XRSimpleInteractable>();
    }
}
