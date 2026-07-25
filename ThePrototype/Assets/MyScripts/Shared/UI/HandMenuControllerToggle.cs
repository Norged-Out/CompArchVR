using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.UI.BodyUI;

/// <summary>
/// Adds a controller button toggle on top of Unity's existing HandMenu system.
/// When the chosen action is pressed in controller mode, the menu opens on the
/// chosen hand and can optionally relax the strict sample pose checks so the
/// menu behaves more like a conventional settings panel.
/// </summary>
[DisallowMultipleComponent]
public sealed class HandMenuControllerToggle : MonoBehaviour
{
    static readonly FieldInfo s_ControllerFollowPresetField =
        typeof(HandMenu).GetField("m_ControllerFollowPreset", BindingFlags.Instance | BindingFlags.NonPublic);

    static readonly PropertyInfo s_DatumValueProperty =
        s_ControllerFollowPresetField?.FieldType.GetProperty("Value");

    [SerializeField]
    HandMenu m_HandMenu;

    [SerializeField]
    InputActionReference m_ToggleAction;

    [SerializeField]
    HandMenu.MenuHandedness m_ControllerOpenHandedness = HandMenu.MenuHandedness.Left;

    [SerializeField]
    bool m_StartOpenInControllerMode = false;

    [SerializeField]
    bool m_RelaxControllerPoseChecks = true;

    [SerializeField]
    bool m_DisableGazeRequirementWhileOpen = true;

    HandMenu.MenuHandedness m_DefaultHandedness;
    bool m_DefaultHideMenuWhenGazeDiverges;
    bool m_ControllerMenuOpen;
    bool m_EnabledToggleAction;
    bool m_HasControllerPreset;
    bool m_HasWarnedAboutPresetAccess;

    FollowPreset m_DefaultControllerPreset;
    FollowPreset m_ControllerOpenPreset;

    XRInputModalityManager.InputMode m_LastInputMode = XRInputModalityManager.InputMode.None;

    void Awake()
    {
        if (m_HandMenu == null)
            m_HandMenu = GetComponent<HandMenu>();

        if (m_HandMenu == null)
        {
            Debug.LogError("HandMenuControllerToggle requires a HandMenu reference.", this);
            enabled = false;
            return;
        }

        if (m_ToggleAction == null || m_ToggleAction.action == null)
        {
            Debug.LogError("HandMenuControllerToggle requires an InputActionReference.", this);
            enabled = false;
            return;
        }

        m_DefaultHandedness = m_HandMenu.menuHandedness;
        m_DefaultHideMenuWhenGazeDiverges = m_HandMenu.hideMenuWhenGazeDiverges;
        m_ControllerMenuOpen = m_StartOpenInControllerMode;

        CacheControllerPresetCopies();
        ApplyModeState(XRInputModalityManager.currentInputMode.Value);
    }

    void OnEnable()
    {
        var action = m_ToggleAction != null ? m_ToggleAction.action : null;
        if (action != null && !action.enabled)
        {
            action.Enable();
            m_EnabledToggleAction = true;
        }

        ApplyModeState(XRInputModalityManager.currentInputMode.Value);
    }

    void OnDisable()
    {
        RestoreDefaultState();

        var action = m_ToggleAction != null ? m_ToggleAction.action : null;
        if (m_EnabledToggleAction && action != null)
        {
            action.Disable();
            m_EnabledToggleAction = false;
        }
    }

    void Update()
    {
        var currentInputMode = XRInputModalityManager.currentInputMode.Value;
        if (currentInputMode != m_LastInputMode)
        {
            ApplyModeState(currentInputMode);
            return;
        }

        if (currentInputMode != XRInputModalityManager.InputMode.MotionController)
            return;

        var action = m_ToggleAction != null ? m_ToggleAction.action : null;
        if (action == null || !action.WasPressedThisFrame())
            return;

        m_ControllerMenuOpen = !m_ControllerMenuOpen;
        ApplyControllerModeState();
    }

    void ApplyModeState(XRInputModalityManager.InputMode currentInputMode)
    {
        m_LastInputMode = currentInputMode;

        if (currentInputMode == XRInputModalityManager.InputMode.MotionController)
        {
            ApplyControllerModeState();
            return;
        }

        RestoreDefaultState();
    }

    void ApplyControllerModeState()
    {
        if (m_RelaxControllerPoseChecks && m_HasControllerPreset)
        {
            // Swap between the authored preset and a relaxed runtime clone so
            // controller opening can feel conventional without permanently
            // mutating the sample asset data.
            var targetPreset = m_ControllerMenuOpen
                ? m_ControllerOpenPreset
                : m_DefaultControllerPreset;

            SetControllerFollowPreset(targetPreset);
        }

        if (m_DisableGazeRequirementWhileOpen)
            m_HandMenu.hideMenuWhenGazeDiverges = !m_ControllerMenuOpen && m_DefaultHideMenuWhenGazeDiverges;

        m_HandMenu.menuHandedness = m_ControllerMenuOpen
            ? m_ControllerOpenHandedness
            : HandMenu.MenuHandedness.None;
    }

    void RestoreDefaultState()
    {
        if (m_HandMenu == null)
            return;

        if (m_HasControllerPreset)
            SetControllerFollowPreset(m_DefaultControllerPreset);

        m_HandMenu.hideMenuWhenGazeDiverges = m_DefaultHideMenuWhenGazeDiverges;
        m_HandMenu.menuHandedness = m_DefaultHandedness;
    }

    void CacheControllerPresetCopies()
    {
        if (!TryGetControllerFollowPreset(out var currentPreset))
            return;

        // Work from clones so any runtime pose relaxation remains local to
        // this helper and never dirties the package/sample preset itself.
        m_DefaultControllerPreset = ClonePreset(currentPreset);
        m_DefaultControllerPreset.ComputeDotProductThresholds();

        m_ControllerOpenPreset = ClonePreset(m_DefaultControllerPreset);
        if (m_RelaxControllerPoseChecks)
        {
            m_ControllerOpenPreset.requirePalmFacingUser = false;
            m_ControllerOpenPreset.requirePalmFacingUp = false;
        }

        m_ControllerOpenPreset.ComputeDotProductThresholds();

        m_HasControllerPreset = true;
    }

    bool TryGetControllerFollowPreset(out FollowPreset controllerPreset)
    {
        controllerPreset = null;

        if (s_ControllerFollowPresetField == null || s_DatumValueProperty == null)
        {
            WarnAboutPresetAccess();
            return false;
        }

        var controllerPresetProperty = s_ControllerFollowPresetField.GetValue(m_HandMenu);
        if (controllerPresetProperty == null)
        {
            WarnAboutPresetAccess();
            return false;
        }

        controllerPreset = s_DatumValueProperty.GetValue(controllerPresetProperty) as FollowPreset;
        if (controllerPreset == null)
            WarnAboutPresetAccess();

        return controllerPreset != null;
    }

    void SetControllerFollowPreset(FollowPreset preset)
    {
        if (preset == null || s_ControllerFollowPresetField == null || s_DatumValueProperty == null)
            return;

        var controllerPresetProperty = s_ControllerFollowPresetField.GetValue(m_HandMenu);
        if (controllerPresetProperty == null)
            return;

        try
        {
            // Assign a fresh copy so runtime changes stay local to this helper
            // and do not keep mutating the same preset instance frame after
            // frame.
            s_DatumValueProperty.SetValue(controllerPresetProperty, ClonePreset(preset));
        }
        catch
        {
            if (!m_HasWarnedAboutPresetAccess)
            {
                Debug.LogWarning(
                    "HandMenuControllerToggle could not override the controller follow preset at runtime. " +
                    "The menu toggle will still work with the currently authored preset values.",
                    this);
                m_HasWarnedAboutPresetAccess = true;
            }
        }
    }

    void WarnAboutPresetAccess()
    {
        if (m_HasWarnedAboutPresetAccess)
            return;

        Debug.LogWarning(
            "HandMenuControllerToggle could not access HandMenu's controller follow preset. " +
            "The menu button toggle will still work, but the controller pose may remain strict.",
            this);

        m_HasWarnedAboutPresetAccess = true;
    }

    static FollowPreset ClonePreset(FollowPreset source)
    {
        if (source == null)
            return null;

        return new FollowPreset
        {
            rightHandLocalPosition = source.rightHandLocalPosition,
            leftHandLocalPosition = source.leftHandLocalPosition,
            rightHandLocalRotation = source.rightHandLocalRotation,
            leftHandLocalRotation = source.leftHandLocalRotation,
            palmReferenceAxis = source.palmReferenceAxis,
            invertAxisForRightHand = source.invertAxisForRightHand,
            requirePalmFacingUser = source.requirePalmFacingUser,
            palmFacingUserDegreeAngleThreshold = source.palmFacingUserDegreeAngleThreshold,
            requirePalmFacingUp = source.requirePalmFacingUp,
            palmFacingUpDegreeAngleThreshold = source.palmFacingUpDegreeAngleThreshold,
            hideDelaySeconds = source.hideDelaySeconds,
            PalmFacingUserHideMenuAngleThresholdDelta = source.PalmFacingUserHideMenuAngleThresholdDelta,
            PalmFacingUpHideMenuAngleThresholdDelta = source.PalmFacingUpHideMenuAngleThresholdDelta,
            snapToGaze = source.snapToGaze,
            snapToGazeAngleThreshold = source.snapToGazeAngleThreshold,
            allowSmoothing = source.allowSmoothing,
            followLowerSmoothingValue = source.followLowerSmoothingValue,
            followUpperSmoothingValue = source.followUpperSmoothingValue,
        };
    }
}
