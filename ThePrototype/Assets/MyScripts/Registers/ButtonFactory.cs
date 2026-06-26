using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Shared helper for building simple XR lesson buttons at runtime.
/// Keeping the construction logic in one place avoids duplicating the same
/// collider, primitive, and label setup across multiple scripts.
/// </summary>
public static class ButtonFactory
{
    public static RegisterButton CreateButton(
        Transform parent,
        string objectName,
        Vector3 localPosition,
        string label,
        string buttonId,
        RegisterButton.ButtonKind buttonKind)
    {
        var root = new GameObject(objectName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPosition;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        var collider = root.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, 0.055f, 0f);
        collider.size = new Vector3(0.18f, 0.14f, 0.18f);

        root.AddComponent<XRSimpleInteractable>();

        var baseVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseVisual.name = "Base";
        baseVisual.transform.SetParent(root.transform, false);
        baseVisual.transform.localPosition = new Vector3(0f, 0.025f, 0f);
        baseVisual.transform.localScale = new Vector3(0.18f, 0.05f, 0.18f);
        DisablePrimitiveCollider(baseVisual);

        var capVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        capVisual.name = "Cap";
        capVisual.transform.SetParent(root.transform, false);
        capVisual.transform.localPosition = new Vector3(0f, 0.075f, 0f);
        capVisual.transform.localScale = new Vector3(0.14f, 0.04f, 0.14f);
        DisablePrimitiveCollider(capVisual);

        var labelRoot = new GameObject("Label");
        labelRoot.transform.SetParent(root.transform, false);
        labelRoot.transform.localPosition = new Vector3(0f, 0.135f, 0f);
        labelRoot.transform.localRotation = Quaternion.identity;
        labelRoot.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);

        var labelText = labelRoot.AddComponent<TextMeshPro>();
        labelText.text = label;
        labelText.fontSize = 3.2f;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.white;

        var button = root.AddComponent<RegisterButton>();
        button.Initialize(
            buttonLabel: label,
            buttonId: buttonId,
            buttonKind: buttonKind,
            buttonCap: capVisual.transform,
            baseRenderer: baseVisual.GetComponent<Renderer>(),
            capRenderer: capVisual.GetComponent<Renderer>(),
            labelText: labelText);

        return button;
    }

    static void DisablePrimitiveCollider(GameObject primitive)
    {
        var primitiveCollider = primitive.GetComponent<Collider>();
        if (primitiveCollider != null)
            primitiveCollider.enabled = false;
    }
}
