using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Overlays;

public static class SceneScreenshot
{
    [MenuItem("Tools/Capture Clean Scene View")]
    public static void Capture()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;

        if (sceneView == null)
        {
            Debug.LogError("No active Scene View.");
            return;
        }

        // Remember which overlays were visible
        var overlayStates = new List<(Overlay overlay, bool visible)>();

        foreach (Overlay overlay in sceneView.overlayCanvas.overlays)
        {
            overlayStates.Add((overlay, overlay.displayed));
            overlay.displayed = false;
        }

        sceneView.Repaint();

        // Give Unity one frame to actually hide them
        EditorApplication.delayCall += () =>
        {
            Rect viewport = sceneView.cameraViewport;

            Vector2 screenPosition =
                sceneView.position.position +
                new Vector2(viewport.x, viewport.y);

            int width = Mathf.RoundToInt(viewport.width);
            int height = Mathf.RoundToInt(viewport.height);

            Color[] pixels = InternalEditorUtility.ReadScreenPixel(
                screenPosition,
                width,
                height
            );

            Texture2D screenshot =
                new Texture2D(width, height, TextureFormat.RGB24, false);

            screenshot.SetPixels(pixels);
            screenshot.Apply();

            string folder = Path.Combine(
                Directory.GetParent(Application.dataPath).FullName,
                "Screenshots"
            );

            Directory.CreateDirectory(folder);

            string path = Path.Combine(
                folder,
                $"Scene_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png"
            );

            File.WriteAllBytes(path, screenshot.EncodeToPNG());

            UnityEngine.Object.DestroyImmediate(screenshot);

            // Put your editor back exactly how it was
            foreach (var state in overlayStates)
                state.overlay.displayed = state.visible;

            sceneView.Repaint();

            Debug.Log($"Clean Scene screenshot saved:\n{path}");
            EditorUtility.RevealInFinder(path);
        };
    }
}