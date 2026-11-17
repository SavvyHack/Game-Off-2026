using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Renders fixed views of the pod to PNG so layout changes can be reviewed without entering Play mode.
    /// Output goes to Temp/PodShots (ignored by git), or to the folder in the GAMEOFF_SHOT_DIR environment variable.
    /// </summary>
    public static class PodScreenshots
    {
        private const int Width = 1280;
        private const int Height = 720;

        [MenuItem("Tools/Game Off/Capture Pod Screenshots")]
        public static void Capture()
        {
            EditorSceneManager.OpenScene(PodLevelBuilder.ScenePath, OpenSceneMode.Single);

            string folder = System.Environment.GetEnvironmentVariable("GAMEOFF_SHOT_DIR");
            if (string.IsNullOrEmpty(folder))
            {
                folder = "Temp/PodShots";
            }

            Directory.CreateDirectory(folder);

            var cameraObject = new GameObject("ShotCamera") { hideFlags = HideFlags.HideAndDontSave };
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 70f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 1500f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.004f, 0.005f, 0.012f);
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = target;

            try
            {
                Shot(camera, target, folder, "01_spawn_forward", new Vector3(0.3f, 1.65f, -3.9f), new Vector3(0f, 1.3f, 6f));
                Shot(camera, target, folder, "02_cockpit", new Vector3(0f, 1.65f, 4.0f), new Vector3(0f, 1.2f, 6.8f));
                Shot(camera, target, folder, "03_lounge_from_cockpit", new Vector3(0f, 1.65f, 4.9f), new Vector3(0f, 1.1f, 0f));
                Shot(camera, target, folder, "04_lab", new Vector3(0.1f, 1.65f, 0.9f), new Vector3(0f, 1.1f, -3f));
                Shot(camera, target, folder, "05_quarters", new Vector3(0.2f, 1.65f, -2.4f), new Vector3(-0.4f, 0.9f, -5.5f));

                foreach (Station station in Object.FindObjectsByType<Station>())
                {
                    Vector3 eye = station.StandPoint.position + Vector3.up * 1.65f;
                    Shot(camera, target, folder, "station_" + station.Type, eye, station.FocusPoint.position);
                }

                // Plan view: orthographic from above, with the near plane below the ceiling so the roof is cut away.
                camera.orthographic = true;
                camera.orthographicSize = 3.9f;
                camera.transform.SetPositionAndRotation(new Vector3(0f, 12f, 0.5f), Quaternion.Euler(90f, 90f, 0f));
                camera.nearClipPlane = 12f - 2.55f;
                camera.farClipPlane = 13f;
                Save(camera, target, folder, "00_plan");
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = null;
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(cameraObject);
            }

            Debug.Log("[PodScreenshots] Wrote screenshots to " + Path.GetFullPath(folder));
        }

        private static void Shot(Camera camera, RenderTexture target, string folder, string name, Vector3 eye, Vector3 lookAt)
        {
            camera.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(lookAt - eye));
            Save(camera, target, folder, name);
        }

        private static void Save(Camera camera, RenderTexture target, string folder, string name)
        {
            camera.Render();

            RenderTexture.active = target;
            var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            image.Apply();
            File.WriteAllBytes(Path.Combine(folder, name + ".png"), image.EncodeToPNG());
            Object.DestroyImmediate(image);
        }
    }
}
