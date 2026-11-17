using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Generates the greybox drone pod: scene, materials, meshes, lights, player and HUD.
    /// The scene is an output of this file. Change the layout here and rebuild rather than editing the scene.
    ///
    /// Coordinates: the pod is a cylinder along Z. +Z is the front (cockpit), +X is the right-hand wall when
    /// facing forward, the deck is at y = 0 and the cylinder's axis is at y = AxisHeight.
    /// </summary>
    public static class PodLevelBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/DronePod.unity";

        private const string Root = "Assets/_Project";
        private const string MaterialFolder = Root + "/Materials";
        private const string MeshFolder = Root + "/Meshes";
        private const string SettingsFolder = Root + "/Settings";

        // Pod dimensions (metres).
        public const float InnerRadius = 1.9f;
        public const float AxisHeight = 1.3f;
        public const float AftZ = -6f;
        public const float FrontZ = 7f;
        private const float HullThickness = 0.15f;
        private const float Length = FrontZ - AftZ;
        private const float MidZ = (FrontZ + AftZ) * 0.5f;

        private static Palette palette;

        [MenuItem("Tools/Game Off/Rebuild Drone Pod")]
        public static void Rebuild()
        {
            EnsureFolders();
            ConfigureProject();
            palette = new Palette();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConfigureEnvironment();

            Transform pod = new GameObject("DronePod").transform;
            BuildHull(Child(pod, "Hull"));
            BuildCockpit(Child(pod, "Cockpit"));
            BuildLounge(Child(pod, "Lounge"));
            BuildLab(Child(pod, "Lab"));
            BuildQuarters(Child(pod, "Quarters"));
            BuildExterior(Child(pod, "Exterior"));
            BuildLights(Child(pod, "Lights"));
            BuildPostProcessing();

            InteractionPromptUI hud = BuildHud();
            BuildPlayer(hud);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();

            Debug.Log("[PodLevelBuilder] Rebuilt " + ScenePath);
        }

        // ------------------------------------------------------------------ project / environment

        private static void EnsureFolders()
        {
            foreach (string folder in new[] { Root + "/Scenes", MaterialFolder, MeshFolder, SettingsFolder })
            {
                Directory.CreateDirectory(folder);
            }

            AssetDatabase.Refresh();
        }

        private static void ConfigureProject()
        {
            PlayerSettings.productName = "Game Off 2026";
            PlayerSettings.companyName = "SavvyHack";
            PlayerSettings.colorSpace = ColorSpace.Linear;

            if (GraphicsSettings.currentRenderPipeline == null)
            {
                Debug.LogError("[PodLevelBuilder] No render pipeline asset is active. Assign Assets/Settings/PC_RPAsset in Project Settings > Graphics.");
            }
        }

        private static void ConfigureEnvironment()
        {
            // Sealed interior in deep space: no sky, no sun, a little flat fill so shadows are not pure black.
            RenderSettings.skybox = null;
            RenderSettings.sun = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.055f, 0.065f, 0.085f);
            RenderSettings.fog = false;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.reflectionIntensity = 0f;
        }

        // ------------------------------------------------------------------ hull

        private static void BuildHull(Transform parent)
        {
            Vector3 axisMid = new Vector3(0f, AxisHeight, MidZ);

            Mesh shell = SaveMesh(PodMeshFactory.Tube("HullShell", InnerRadius, InnerRadius + HullThickness, Length, 48));
            MeshObject("HullShell", parent, shell, palette.Hull, axisMid, true);

            Box("Deck", parent, new Vector3(0f, -0.05f, MidZ), new Vector3(2.9f, 0.1f, Length), palette.Deck);

            // Glow strips where the deck meets the wall, and a spine of ceiling lights.
            foreach (float side in new[] { -1f, 1f })
            {
                Box("DeckStrip", parent, new Vector3(side * 1.33f, 0.015f, MidZ), new Vector3(0.04f, 0.03f, Length - 0.4f), palette.StripCyan, collider: false);
            }

            // Light ribs every so often so the wall does not read as one flat tube.
            Mesh rib = SaveMesh(PodMeshFactory.Tube("HullRib", InnerRadius - 0.07f, InnerRadius + 0.01f, 0.12f, 48));
            foreach (float z in new[] { -5.2f, -4.1f, -1.9f, -0.7f, 1.6f, 2.7f, 3.6f, 5.6f })
            {
                MeshObject("Rib", parent, rib, palette.Trim, new Vector3(0f, AxisHeight, z), false);
            }

            // Heavy bulkheads: cockpit/lounge and lab/quarters. The opening is a doorway about 1.3 m wide at deck level.
            Mesh bulkhead = SaveMesh(PodMeshFactory.Tube("Bulkhead", 1.45f, InnerRadius + 0.01f, 0.3f, 48));
            Mesh bulkheadAccent = SaveMesh(PodMeshFactory.Tube("BulkheadAccent", 1.44f, 1.5f, 0.1f, 48));
            foreach (float z in new[] { 4.5f, -3f })
            {
                MeshObject("Bulkhead", parent, bulkhead, palette.Bulkhead, new Vector3(0f, AxisHeight, z), true);
                MeshObject("BulkheadAccent", parent, bulkheadAccent, palette.StripAmber, new Vector3(0f, AxisHeight, z), false);
            }

            // Lighter divider between lounge and lab.
            Mesh divider = SaveMesh(PodMeshFactory.Tube("Divider", 1.62f, InnerRadius + 0.01f, 0.14f, 48));
            MeshObject("Divider", parent, divider, palette.Bulkhead, new Vector3(0f, AxisHeight, 0.5f), true);
        }

        // ------------------------------------------------------------------ cockpit (z 4.5 .. 7)

        private static void BuildCockpit(Transform parent)
        {
            // Front wall with a round viewport.
            Mesh frame = SaveMesh(PodMeshFactory.Tube("ViewportFrame", 1.15f, InnerRadius + 0.01f, 0.16f, 48));
            MeshObject("ViewportFrame", parent, frame, palette.Bulkhead, new Vector3(0f, AxisHeight, FrontZ - 0.08f), true);
            Disc("ViewportGlass", parent, new Vector3(0f, AxisHeight, FrontZ - 0.08f), 2.32f, 0.04f, palette.Glass);
            Mesh viewportRing = SaveMesh(PodMeshFactory.Tube("ViewportRing", 1.12f, 1.2f, 0.2f, 48));
            MeshObject("ViewportRing", parent, viewportRing, palette.StripCyan, new Vector3(0f, AxisHeight, FrontZ - 0.08f), false);

            Transform station = Child(parent, "Station_PilotSeat");
            var highlights = new List<Renderer>();

            // Dash under the viewport.
            Box("Dash", station, new Vector3(0f, 0.4f, 6.4f), new Vector3(2.3f, 0.8f, 0.7f), palette.Panel);
            Box("DashTop", station, new Vector3(0f, 0.86f, 6.3f), new Vector3(2.1f, 0.06f, 0.75f), palette.Trim, new Vector3(-14f, 0f, 0f));
            highlights.Add(Screen("DashKeys", station, new Vector3(0f, 0.9f, 6.22f), new Vector3(1.2f, 0.02f, 0.3f), palette.ScreenAmber, new Vector3(-14f, 0f, 0f)));

            // Wraparound screens.
            highlights.Add(Screen("ScreenCentre", station, new Vector3(0f, 1.18f, 6.62f), new Vector3(0.72f, 0.36f, 0.04f), palette.ScreenCyan, new Vector3(-10f, 0f, 0f)));
            highlights.Add(Screen("ScreenLeft", station, new Vector3(-0.76f, 1.18f, 6.5f), new Vector3(0.72f, 0.36f, 0.04f), palette.ScreenCyan, new Vector3(-10f, -24f, 0f)));
            highlights.Add(Screen("ScreenRight", station, new Vector3(0.76f, 1.18f, 6.5f), new Vector3(0.72f, 0.36f, 0.04f), palette.ScreenCyan, new Vector3(-10f, 24f, 0f)));
            Screen("WallPanelLeft", parent, new Vector3(-1.72f, 1.45f, 5.55f), new Vector3(0.05f, 0.55f, 1.0f), palette.ScreenBlue, new Vector3(0f, 0f, -6f));
            Screen("WallPanelRight", parent, new Vector3(1.72f, 1.45f, 5.55f), new Vector3(0.05f, 0.55f, 1.0f), palette.ScreenBlue, new Vector3(0f, 0f, 6f));
            Box("SideConsoleLeft", parent, new Vector3(-1.2f, 0.35f, 5.55f), new Vector3(0.35f, 0.7f, 1.1f), palette.Panel);
            Box("SideConsoleRight", parent, new Vector3(1.2f, 0.35f, 5.55f), new Vector3(0.35f, 0.7f, 1.1f), palette.Panel);

            // Pilot seat, facing the viewport.
            const float seatZ = 5.45f;
            Cylinder("SeatPost", station, new Vector3(0f, 0.2f, seatZ), 0.22f, 0.4f, palette.Trim);
            Box("SeatBase", station, new Vector3(0f, 0.46f, seatZ), new Vector3(0.56f, 0.14f, 0.56f), palette.Seat);
            Box("SeatBack", station, new Vector3(0f, 0.95f, seatZ - 0.3f), new Vector3(0.56f, 0.9f, 0.13f), palette.Seat, new Vector3(-8f, 0f, 0f));
            Box("SeatHeadrest", station, new Vector3(0f, 1.48f, seatZ - 0.37f), new Vector3(0.32f, 0.2f, 0.12f), palette.Seat, new Vector3(-8f, 0f, 0f));
            Box("ArmrestLeft", station, new Vector3(-0.33f, 0.66f, seatZ + 0.02f), new Vector3(0.08f, 0.06f, 0.45f), palette.Trim);
            Box("ArmrestRight", station, new Vector3(0.33f, 0.66f, seatZ + 0.02f), new Vector3(0.08f, 0.06f, 0.45f), palette.Trim);

            FinishStation(station, StationType.PilotSeat, "Pilot Seat", "Sit in",
                "Navigation offline. Travel is not available yet.",
                stand: new Vector3(0.62f, 0f, 5.05f), focus: new Vector3(0f, 0.95f, seatZ - 0.2f), highlights);
        }

        // ------------------------------------------------------------------ lounge / ops (z 0.5 .. 4.5)

        private static void BuildLounge(Transform parent)
        {
            // Sofa along the left wall.
            Transform sofa = Child(parent, "Sofa");
            Box("SofaBase", sofa, new Vector3(-0.98f, 0.2f, 2.9f), new Vector3(0.74f, 0.4f, 2.0f), palette.Panel);
            Box("SofaSeatA", sofa, new Vector3(-0.95f, 0.47f, 2.42f), new Vector3(0.7f, 0.14f, 0.94f), palette.Sofa);
            Box("SofaSeatB", sofa, new Vector3(-0.95f, 0.47f, 3.38f), new Vector3(0.7f, 0.14f, 0.94f), palette.Sofa);
            Box("SofaBack", sofa, new Vector3(-1.27f, 0.75f, 2.9f), new Vector3(0.2f, 0.56f, 2.0f), palette.Sofa, new Vector3(0f, 0f, 8f));
            Box("SofaArmAft", sofa, new Vector3(-0.98f, 0.38f, 1.84f), new Vector3(0.74f, 0.66f, 0.14f), palette.Sofa);
            Box("SofaArmFront", sofa, new Vector3(-0.98f, 0.38f, 3.96f), new Vector3(0.74f, 0.66f, 0.14f), palette.Sofa);

            // Wall monitor above the sofa and a side table with a lamp.
            Screen("WallMonitor", parent, new Vector3(-1.66f, 1.85f, 2.9f), new Vector3(0.05f, 0.6f, 1.5f), palette.ScreenBlue, new Vector3(0f, 0f, -14f));
            Box("SideTable", parent, new Vector3(-1.0f, 0.25f, 1.25f), new Vector3(0.5f, 0.5f, 0.5f), palette.Trim);
            Cylinder("LampStem", parent, new Vector3(-1.05f, 0.62f, 1.25f), 0.04f, 0.24f, palette.Trim, collider: false);
            Sphere("LampShade", parent, new Vector3(-1.05f, 0.8f, 1.25f), 0.2f, palette.LampWarm);

            // Drone dispatch console along the right wall.
            Transform station = Child(parent, "Station_DroneDispatch");
            var highlights = new List<Renderer>();
            Box("Desk", station, new Vector3(1.06f, 0.4f, 2.6f), new Vector3(0.6f, 0.8f, 1.9f), palette.Panel);
            Box("DeskTop", station, new Vector3(1.0f, 0.83f, 2.6f), new Vector3(0.78f, 0.06f, 2.0f), palette.Trim);
            highlights.Add(Screen("MainScreen", station, new Vector3(1.52f, 1.5f, 2.6f), new Vector3(0.05f, 0.72f, 1.3f), palette.ScreenCyan, new Vector3(0f, 0f, 10f)));
            highlights.Add(Screen("SideScreenAft", station, new Vector3(1.4f, 1.3f, 1.72f), new Vector3(0.05f, 0.46f, 0.5f), palette.ScreenCyan, new Vector3(0f, 28f, 8f)));
            highlights.Add(Screen("SideScreenFront", station, new Vector3(1.4f, 1.3f, 3.48f), new Vector3(0.05f, 0.46f, 0.5f), palette.ScreenCyan, new Vector3(0f, -28f, 8f)));
            highlights.Add(Screen("ControlSlab", station, new Vector3(0.92f, 0.87f, 2.6f), new Vector3(0.36f, 0.02f, 0.9f), palette.ScreenAmber));

            // Drone rack: four launch ports next to the console.
            Box("DroneRack", station, new Vector3(1.12f, 0.75f, 4.02f), new Vector3(0.5f, 1.5f, 0.42f), palette.Bulkhead);
            for (int i = 0; i < 4; i++)
            {
                float y = 0.5f + 0.52f * (i / 2);
                float z = 3.92f + 0.2f * (i % 2);
                Sphere("DronePort" + (i + 1), station, new Vector3(0.88f, y, z), 0.13f, palette.StripAmber, collider: false);
            }

            FinishStation(station, StationType.DroneDispatch, "Drone Dispatch Console", "Use",
                "Drone bay offline. Sample dispatch is not available yet.",
                stand: new Vector3(0.1f, 0f, 2.6f), focus: new Vector3(1.5f, 1.5f, 2.6f), highlights);
        }

        // ------------------------------------------------------------------ lab (z -3 .. 0.5)

        private static void BuildLab(Transform parent)
        {
            // Glass sample tube on the left.
            Transform tube = Child(parent, "SampleTube");
            Vector3 tubeBase = new Vector3(-0.85f, 0f, -1.3f);
            Cylinder("TubeBase", tube, tubeBase + Vector3.up * 0.14f, 1.0f, 0.28f, palette.Bulkhead);
            Cylinder("TubeBaseGlow", tube, tubeBase + Vector3.up * 0.3f, 0.9f, 0.04f, palette.GlowGreen, collider: false);
            Cylinder("TubeGlass", tube, tubeBase + Vector3.up * 1.22f, 0.86f, 1.84f, palette.Glass);
            Cylinder("TubeCore", tube, tubeBase + Vector3.up * 1.22f, 0.07f, 1.8f, palette.GlowGreen, collider: false);
            Sphere("TubeSample", tube, tubeBase + Vector3.up * 1.25f, 0.3f, palette.GlowGreen, collider: false);
            Cylinder("TubeCap", tube, tubeBase + Vector3.up * 2.27f, 1.0f, 0.26f, palette.Bulkhead);

            // Storage either side of the tube.
            Box("Locker", parent, new Vector3(-1.12f, 0.95f, -0.12f), new Vector3(0.5f, 1.9f, 0.7f), palette.Panel);
            Box("LockerStrip", parent, new Vector3(-0.86f, 1.3f, -0.12f), new Vector3(0.02f, 0.5f, 0.05f), palette.StripCyan, collider: false);

            Transform shelf = Child(parent, "SampleShelf");
            const float shelfZ = -2.42f;
            Box("ShelfBack", shelf, new Vector3(-1.3f, 0.9f, shelfZ), new Vector3(0.05f, 1.8f, 0.72f), palette.Panel);
            Box("ShelfSideA", shelf, new Vector3(-1.1f, 0.9f, shelfZ - 0.36f), new Vector3(0.45f, 1.8f, 0.04f), palette.Panel);
            Box("ShelfSideB", shelf, new Vector3(-1.1f, 0.9f, shelfZ + 0.36f), new Vector3(0.45f, 1.8f, 0.04f), palette.Panel);
            Material[] canisterColours = { palette.StripAmber, palette.GlowGreen, palette.StripCyan };
            for (int level = 0; level < 4; level++)
            {
                float y = 0.3f + level * 0.48f;
                Box("ShelfBoard" + level, shelf, new Vector3(-1.1f, y, shelfZ), new Vector3(0.45f, 0.04f, 0.72f), palette.Trim);
                for (int c = 0; c < 3; c++)
                {
                    Cylinder("Canister", shelf, new Vector3(-1.08f, y + 0.14f, shelfZ - 0.2f + c * 0.2f), 0.11f, 0.22f,
                        canisterColours[(level + c) % canisterColours.Length], collider: false);
                }
            }

            // Analysis bench along the right wall.
            Transform station = Child(parent, "Station_AnalysisBench");
            var highlights = new List<Renderer>();
            const float benchZ = -1.3f;
            Box("Bench", station, new Vector3(1.06f, 0.44f, benchZ), new Vector3(0.6f, 0.88f, 2.2f), palette.Panel);
            Box("BenchTop", station, new Vector3(1.0f, 0.91f, benchZ), new Vector3(0.78f, 0.06f, 2.3f), palette.BenchTop);
            highlights.Add(Screen("ResultsScreen", station, new Vector3(1.52f, 1.5f, benchZ), new Vector3(0.05f, 0.62f, 1.1f), palette.ScreenGreen, new Vector3(0f, 0f, 10f)));

            // Instruments: scope, centrifuge, spectrometer, vial rack, rulebook.
            Box("ScopeBase", station, new Vector3(1.05f, 0.98f, benchZ + 0.8f), new Vector3(0.26f, 0.06f, 0.22f), palette.Trim);
            Box("ScopeArm", station, new Vector3(1.14f, 1.16f, benchZ + 0.8f), new Vector3(0.06f, 0.34f, 0.08f), palette.Trim);
            Cylinder("ScopeTube", station, new Vector3(1.03f, 1.24f, benchZ + 0.8f), 0.08f, 0.26f, palette.Bulkhead, new Vector3(0f, 0f, 20f), collider: false);
            Cylinder("Centrifuge", station, new Vector3(1.08f, 1.05f, benchZ + 0.3f), 0.36f, 0.22f, palette.Bulkhead);
            highlights.Add(Screen("CentrifugeLid", station, new Vector3(1.08f, 1.17f, benchZ + 0.3f), new Vector3(0.14f, 0.02f, 0.14f), palette.ScreenGreen));
            Box("Spectrometer", station, new Vector3(1.12f, 1.1f, benchZ - 0.35f), new Vector3(0.4f, 0.32f, 0.5f), palette.Panel);
            highlights.Add(Screen("SpectrometerSlot", station, new Vector3(0.915f, 1.12f, benchZ - 0.35f), new Vector3(0.02f, 0.06f, 0.34f), palette.ScreenGreen));
            Box("VialRack", station, new Vector3(0.9f, 0.97f, benchZ - 0.88f), new Vector3(0.14f, 0.05f, 0.5f), palette.Trim);
            Material[] vialColours = { palette.GlowGreen, palette.StripAmber, palette.StripCyan, palette.LampWarm, palette.GlowGreen };
            for (int i = 0; i < 5; i++)
            {
                Cylinder("Vial", station, new Vector3(0.9f, 1.06f, benchZ - 1.06f + i * 0.09f), 0.045f, 0.14f, vialColours[i], collider: false);
            }

            Box("Rulebook", station, new Vector3(0.82f, 0.955f, benchZ + 0.42f), new Vector3(0.24f, 0.03f, 0.32f), palette.Book, new Vector3(0f, 12f, 0f));

            FinishStation(station, StationType.AnalysisBench, "Analysis Bench", "Use",
                "Analysis bench offline. Chemical analysis is not available yet.",
                stand: new Vector3(0.1f, 0f, benchZ), focus: new Vector3(1.5f, 1.5f, benchZ), highlights);
        }

        // ------------------------------------------------------------------ quarters (z -6 .. -3)

        private static void BuildQuarters(Transform parent)
        {
            // Bunk along the left wall.
            Transform station = Child(parent, "Station_Bunk");
            var highlights = new List<Renderer>();
            const float bunkZ = -4.6f;
            Box("BunkFrame", station, new Vector3(-0.9f, 0.28f, bunkZ), new Vector3(0.92f, 0.56f, 2.1f), palette.Panel);
            Box("Mattress", station, new Vector3(-0.9f, 0.63f, bunkZ), new Vector3(0.86f, 0.14f, 2.0f), palette.Bedding);
            Box("Blanket", station, new Vector3(-0.9f, 0.72f, bunkZ + 0.4f), new Vector3(0.88f, 0.05f, 1.1f), palette.BeddingDark);
            Box("Pillow", station, new Vector3(-0.9f, 0.75f, bunkZ - 0.75f), new Vector3(0.6f, 0.1f, 0.36f), palette.Pillow);
            Box("OverheadCabinet", station, new Vector3(-1.22f, 2.05f, bunkZ), new Vector3(0.5f, 0.42f, 2.0f), palette.Panel);
            highlights.Add(Screen("ReadingLight", station, new Vector3(-1.15f, 1.82f, bunkZ - 0.6f), new Vector3(0.3f, 0.03f, 0.5f), palette.LampWarm));

            FinishStation(station, StationType.Bunk, "Bunk", "Rest in",
                "You are not tired yet.",
                stand: new Vector3(-0.05f, 0f, bunkZ), focus: new Vector3(-0.9f, 0.7f, bunkZ), highlights);

            // Right side: locker and a small desk.
            Box("Locker", parent, new Vector3(1.12f, 0.95f, -5.35f), new Vector3(0.5f, 1.9f, 0.9f), palette.Panel);
            Box("LockerStrip", parent, new Vector3(0.86f, 1.3f, -5.35f), new Vector3(0.02f, 0.5f, 0.05f), palette.StripAmber, collider: false);
            Box("SmallDesk", parent, new Vector3(1.1f, 0.4f, -4.1f), new Vector3(0.5f, 0.8f, 0.8f), palette.Panel);
            Screen("DeskMonitor", parent, new Vector3(1.24f, 1.02f, -4.1f), new Vector3(0.04f, 0.3f, 0.44f), palette.ScreenBlue, new Vector3(0f, 0f, 8f));

            // Aft wall with a sealed service hatch.
            Disc("AftWall", parent, new Vector3(0f, AxisHeight, AftZ + 0.06f), InnerRadius * 2f + 0.1f, 0.12f, palette.Bulkhead);
            Vector3 hatchCentre = new Vector3(0f, 1.15f, AftZ + 0.14f);
            Mesh hatchFrame = SaveMesh(PodMeshFactory.Tube("HatchFrame", 0.56f, 0.72f, 0.1f, 32));
            MeshObject("HatchFrame", parent, hatchFrame, palette.Trim, hatchCentre, false);
            Disc("HatchDoor", parent, hatchCentre, 1.14f, 0.06f, palette.Panel, collider: false);
            Box("HatchBar", parent, hatchCentre + new Vector3(0f, 0f, 0.05f), new Vector3(0.9f, 0.1f, 0.05f), palette.Trim, collider: false);
            Box("HatchStatus", parent, hatchCentre + new Vector3(0f, 0.9f, 0f), new Vector3(0.4f, 0.07f, 0.04f), palette.StripRed, collider: false);
        }

        // ------------------------------------------------------------------ exterior

        private static void BuildExterior(Transform parent)
        {
            float outer = InnerRadius + HullThickness;

            // Coupling ring around the viewport.
            Mesh coupling = SaveMesh(PodMeshFactory.Tube("CouplingRing", outer - 0.05f, outer + 0.45f, 0.55f, 48));
            MeshObject("CouplingRing", parent, coupling, palette.Bulkhead, new Vector3(0f, AxisHeight, FrontZ - 0.1f), false);

            // Hull bands over the bulkheads.
            Mesh band = SaveMesh(PodMeshFactory.Tube("HullBand", outer - 0.02f, outer + 0.14f, 0.5f, 48));
            foreach (float z in new[] { 4.5f, -3f })
            {
                MeshObject("HullBand", parent, band, palette.Bulkhead, new Vector3(0f, AxisHeight, z), false);
            }

            // Thruster block.
            Disc("ThrusterBlock", parent, new Vector3(0f, AxisHeight, AftZ - 0.3f), outer * 2f - 0.5f, 0.6f, palette.Bulkhead, collider: false);
            foreach (Vector2 offset in new[] { new Vector2(-0.75f, 0.75f), new Vector2(0.75f, 0.75f), new Vector2(-0.75f, -0.75f), new Vector2(0.75f, -0.75f) })
            {
                Vector3 centre = new Vector3(offset.x, AxisHeight + offset.y, AftZ - 0.85f);
                Disc("ThrusterNozzle", parent, centre, 0.9f, 0.6f, palette.Trim, collider: false);
                Disc("ThrusterGlow", parent, centre + Vector3.back * 0.31f, 0.7f, 0.02f, palette.StripCyan, collider: false);
            }

            // Stars, far enough out that walking around the pod gives no parallax.
            Mesh stars = SaveMesh(PodMeshFactory.StarField("StarField", 900, 600f, 2026));
            GameObject starObject = MeshObject("StarField", parent, stars, palette.Stars, new Vector3(0f, AxisHeight, 0f), false);
            var starRenderer = starObject.GetComponent<MeshRenderer>();
            starRenderer.shadowCastingMode = ShadowCastingMode.Off;
            starRenderer.receiveShadows = false;
        }

        // ------------------------------------------------------------------ lights

        private static void BuildLights(Transform parent)
        {
            // Ceiling fittings (visible) with a point light under each (the actual illumination).
            CeilingLight(parent, "Cockpit", 5.3f, new Color(0.45f, 0.85f, 1f), 6f, 5f, palette.StripCyan, shadows: false);
            CeilingLight(parent, "Lounge", 2.6f, new Color(1f, 0.82f, 0.62f), 9f, 6.5f, palette.LampWarm, shadows: true);
            CeilingLight(parent, "Lab", -1.3f, new Color(0.8f, 0.92f, 1f), 7f, 6.5f, palette.LampCool, shadows: true);
            CeilingLight(parent, "Quarters", -4.5f, new Color(1f, 0.75f, 0.55f), 7f, 5.5f, palette.LampWarm, shadows: false);

            PointLight(parent, "CockpitScreenGlow", new Vector3(0f, 1.3f, 6.0f), new Color(0.3f, 0.85f, 1f), 3.5f, 3.5f, false);
            PointLight(parent, "DispatchScreenGlow", new Vector3(1.1f, 1.45f, 2.6f), new Color(0.3f, 0.85f, 1f), 2.5f, 3f, false);
            PointLight(parent, "AnalysisScreenGlow", new Vector3(1.1f, 1.45f, -1.3f), new Color(0.4f, 1f, 0.75f), 2.5f, 3f, false);
            PointLight(parent, "SampleTubeGlow", new Vector3(-0.3f, 1.3f, -1.3f), new Color(0.4f, 1f, 0.6f), 1.6f, 3f, false);
            PointLight(parent, "SofaLamp", new Vector3(-0.9f, 1.0f, 1.3f), new Color(1f, 0.72f, 0.45f), 2.5f, 3f, false);
            PointLight(parent, "BunkReadingLight", new Vector3(-1.0f, 1.6f, -5.2f), new Color(1f, 0.75f, 0.5f), 2f, 2.5f, false);
        }

        private static void CeilingLight(Transform parent, string zone, float z, Color colour, float intensity, float range, Material fitting, bool shadows)
        {
            float ceiling = AxisHeight + InnerRadius;
            Box(zone + "CeilingFitting", parent, new Vector3(0f, ceiling - 0.09f, z), new Vector3(0.28f, 0.05f, 1.3f), fitting, collider: false);
            PointLight(parent, zone + "CeilingLight", new Vector3(0f, ceiling - 0.55f, z), colour, intensity, range, shadows);
        }

        private static void PointLight(Transform parent, string name, Vector3 position, Color colour, float intensity, float range, bool shadows)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.position = position;

            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = colour;
            light.intensity = intensity;
            light.range = range;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
        }

        private static void BuildPostProcessing()
        {
            string path = SettingsFolder + "/PodVolumeProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);

                var bloom = profile.Add<Bloom>();
                bloom.intensity.Override(0.9f);
                bloom.threshold.Override(1.0f);
                bloom.scatter.Override(0.65f);
                AssetDatabase.AddObjectToAsset(bloom, profile);

                var tonemapping = profile.Add<Tonemapping>();
                tonemapping.mode.Override(TonemappingMode.ACES);
                AssetDatabase.AddObjectToAsset(tonemapping, profile);

                var vignette = profile.Add<Vignette>();
                vignette.intensity.Override(0.28f);
                vignette.smoothness.Override(0.4f);
                AssetDatabase.AddObjectToAsset(vignette, profile);

                EditorUtility.SetDirty(profile);
            }

            var volumeObject = new GameObject("PostProcessing");
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        // ------------------------------------------------------------------ player and HUD

        private static void BuildPlayer(InteractionPromptUI hud)
        {
            var player = new GameObject("Player") { tag = "Player", layer = LayerMask.NameToLayer("Ignore Raycast") };
            // Start in the quarters looking forward, so the first view runs the length of the pod.
            player.transform.SetPositionAndRotation(new Vector3(0.3f, 0.05f, -3.9f), Quaternion.identity);

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.75f;
            controller.radius = 0.28f;
            controller.center = new Vector3(0f, 0.875f, 0f);
            controller.stepOffset = 0.25f;

            var pivot = new GameObject("CameraPivot");
            pivot.transform.SetParent(player.transform, false);
            pivot.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            var cameraObject = new GameObject("PlayerCamera") { tag = "MainCamera" };
            cameraObject.transform.SetParent(pivot.transform, false);
            var camera = cameraObject.AddComponent<Camera>();
            ConfigureCamera(camera);
            cameraObject.AddComponent<AudioListener>();

            player.AddComponent<FirstPersonController>().Configure(pivot.transform);
            player.AddComponent<PlayerInteractor>().Configure(cameraObject.transform, hud);
        }

        private static void ConfigureCamera(Camera camera)
        {
            camera.fieldOfView = 70f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 1500f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.004f, 0.005f, 0.012f);
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        }

        private static InteractionPromptUI BuildHud()
        {
            var canvasObject = new GameObject("HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var crosshair = new GameObject("Crosshair").AddComponent<Image>();
            crosshair.transform.SetParent(canvasObject.transform, false);
            crosshair.color = new Color(1f, 1f, 1f, 0.75f);
            crosshair.raycastTarget = false;
            crosshair.rectTransform.sizeDelta = new Vector2(5f, 5f);

            Text prompt = HudText(canvasObject.transform, "Prompt", new Vector2(0f, -70f), 30, new Color(0.85f, 0.97f, 1f));
            Text message = HudText(canvasObject.transform, "Message", new Vector2(0f, -150f), 26, new Color(1f, 0.86f, 0.6f));

            var hud = canvasObject.AddComponent<InteractionPromptUI>();
            hud.Configure(prompt, message);
            return hud;
        }

        private static Text HudText(Transform parent, string name, Vector2 offsetFromCentre, int size, Color colour)
        {
            var text = new GameObject(name).AddComponent<Text>();
            text.transform.SetParent(parent, false);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = colour;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.rectTransform.anchoredPosition = offsetFromCentre;
            text.rectTransform.sizeDelta = new Vector2(1100f, 60f);

            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            return text;
        }

        // ------------------------------------------------------------------ stations

        private static void FinishStation(Transform root, StationType type, string displayName, string verb, string placeholder,
            Vector3 stand, Vector3 focus, List<Renderer> highlights)
        {
            Transform standPoint = Child(root, "StandPoint");
            standPoint.position = stand;
            Transform focusPoint = Child(root, "FocusPoint");
            focusPoint.position = focus;

            // Face the stand point towards the station so it doubles as a "use" pose later.
            Vector3 flat = focus - stand;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.0001f)
            {
                standPoint.rotation = Quaternion.LookRotation(flat);
            }

            root.gameObject.AddComponent<Station>().Configure(type, displayName, verb, placeholder, standPoint, focusPoint, highlights.ToArray());
        }

        // ------------------------------------------------------------------ primitives

        private static Transform Child(Transform parent, string name)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static GameObject Box(string name, Transform parent, Vector3 position, Vector3 size, Material material,
            Vector3 euler = default, bool collider = true)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Place(box, name, parent, position, size, euler, material);
            if (!collider)
            {
                Object.DestroyImmediate(box.GetComponent<Collider>());
            }

            return box;
        }

        /// <summary>An emissive panel. Same as a box, returned as a renderer so stations can highlight it.</summary>
        private static Renderer Screen(string name, Transform parent, Vector3 position, Vector3 size, Material material, Vector3 euler = default)
        {
            GameObject screen = Box(name, parent, position, size, material, euler);
            var renderer = screen.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return renderer;
        }

        /// <summary>An upright cylinder (axis along Y before <paramref name="euler"/> is applied).</summary>
        private static GameObject Cylinder(string name, Transform parent, Vector3 position, float diameter, float height, Material material,
            Vector3 euler = default, bool collider = true)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            // Unity's cylinder is 2 units tall and 1 wide.
            Place(cylinder, name, parent, position, new Vector3(diameter, height * 0.5f, diameter), euler, material);

            // The default capsule collider is wrong for squat cylinders; a box is close enough for greybox.
            Object.DestroyImmediate(cylinder.GetComponent<Collider>());
            if (collider)
            {
                cylinder.AddComponent<BoxCollider>().size = new Vector3(1f, 2f, 1f);
            }

            return cylinder;
        }

        /// <summary>A flat cylinder whose faces point along Z (walls, hatches, nozzles).</summary>
        private static GameObject Disc(string name, Transform parent, Vector3 position, float diameter, float thickness, Material material, bool collider = true)
        {
            return Cylinder(name, parent, position, diameter, thickness, material, new Vector3(90f, 0f, 0f), collider);
        }

        private static GameObject Sphere(string name, Transform parent, Vector3 position, float diameter, Material material, bool collider = true)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Place(sphere, name, parent, position, Vector3.one * diameter, default, material);
            if (!collider)
            {
                Object.DestroyImmediate(sphere.GetComponent<Collider>());
            }

            return sphere;
        }

        private static GameObject MeshObject(string name, Transform parent, Mesh mesh, Material material, Vector3 position, bool collider)
        {
            var meshObject = new GameObject(name);
            meshObject.transform.SetParent(parent, false);
            meshObject.transform.position = position;
            meshObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            meshObject.AddComponent<MeshRenderer>().sharedMaterial = material;
            if (collider)
            {
                meshObject.AddComponent<MeshCollider>().sharedMesh = mesh;
            }

            return meshObject;
        }

        private static void Place(GameObject target, string name, Transform parent, Vector3 position, Vector3 scale, Vector3 euler, Material material)
        {
            target.name = name;
            target.transform.SetParent(parent, false);
            target.transform.SetPositionAndRotation(position, Quaternion.Euler(euler));
            target.transform.localScale = scale;
            target.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        // ------------------------------------------------------------------ assets

        /// <summary>Writes the mesh to the Meshes folder, reusing the existing asset so its GUID stays stable across rebuilds.</summary>
        private static Mesh SaveMesh(Mesh mesh)
        {
            string path = MeshFolder + "/" + mesh.name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }

            existing.Clear();
            EditorUtility.CopySerialized(mesh, existing);
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(mesh);
            return existing;
        }

        private sealed class Palette
        {
            public readonly Material Hull = Lit("Hull", new Color(0.16f, 0.18f, 0.21f), 0.35f, 0.4f);
            public readonly Material Deck = Lit("Deck", new Color(0.11f, 0.12f, 0.14f), 0.2f, 0.3f);
            public readonly Material Bulkhead = Lit("Bulkhead", new Color(0.09f, 0.1f, 0.12f), 0.45f, 0.45f);
            public readonly Material Trim = Lit("Trim", new Color(0.3f, 0.32f, 0.36f), 0.4f, 0.5f);
            public readonly Material Panel = Lit("Panel", new Color(0.22f, 0.24f, 0.28f), 0.2f, 0.35f);
            public readonly Material BenchTop = Lit("BenchTop", new Color(0.55f, 0.57f, 0.6f), 0.1f, 0.6f);
            public readonly Material Seat = Lit("Seat", new Color(0.13f, 0.14f, 0.17f), 0f, 0.25f);
            public readonly Material Sofa = Lit("Sofa", new Color(0.14f, 0.27f, 0.6f), 0f, 0.2f);
            public readonly Material Bedding = Lit("Bedding", new Color(0.3f, 0.62f, 0.3f), 0f, 0.15f);
            public readonly Material BeddingDark = Lit("BeddingDark", new Color(0.16f, 0.4f, 0.22f), 0f, 0.15f);
            public readonly Material Pillow = Lit("Pillow", new Color(0.75f, 0.78f, 0.74f), 0f, 0.15f);
            public readonly Material Book = Lit("Book", new Color(0.55f, 0.2f, 0.12f), 0f, 0.2f);

            public readonly Material ScreenCyan = Emissive("ScreenCyan", new Color(0.15f, 0.8f, 1f), 0.95f);
            public readonly Material ScreenBlue = Emissive("ScreenBlue", new Color(0.2f, 0.45f, 1f), 0.9f);
            public readonly Material ScreenGreen = Emissive("ScreenGreen", new Color(0.25f, 1f, 0.6f), 0.9f);
            public readonly Material ScreenAmber = Emissive("ScreenAmber", new Color(1f, 0.6f, 0.15f), 0.85f);
            public readonly Material StripCyan = Emissive("StripCyan", new Color(0.2f, 0.85f, 1f), 0.9f);
            public readonly Material StripAmber = Emissive("StripAmber", new Color(1f, 0.5f, 0.1f), 1.0f);
            public readonly Material StripRed = Emissive("StripRed", new Color(1f, 0.12f, 0.08f), 1.4f);
            public readonly Material GlowGreen = Emissive("GlowGreen", new Color(0.35f, 1f, 0.5f), 1.1f);
            public readonly Material LampWarm = Emissive("LampWarm", new Color(1f, 0.78f, 0.5f), 1.8f);
            public readonly Material LampCool = Emissive("LampCool", new Color(0.8f, 0.92f, 1f), 1.8f);

            public readonly Material Glass = Transparent("Glass", new Color(0.55f, 0.85f, 0.95f, 0.14f));
            public readonly Material Stars = Unlit("Stars", new Color(2.2f, 2.3f, 2.6f));
        }

        private static Material LoadOrCreate(string name, string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                throw new System.InvalidOperationException("Shader not found: " + shaderName + ". Is URP installed?");
            }

            string path = MaterialFolder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material Lit(string name, Color colour, float metallic, float smoothness)
        {
            Material material = LoadOrCreate(name, "Universal Render Pipeline/Lit");
            material.SetColor("_BaseColor", colour);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            return material;
        }

        private static Material Emissive(string name, Color colour, float intensity)
        {
            Material material = Lit(name, colour * 0.12f, 0f, 0.6f);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            material.SetColor("_EmissionColor", colour * intensity);
            return material;
        }

        private static Material Transparent(string name, Color colour)
        {
            Material material = Lit(name, colour, 0f, 0.9f);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetShaderPassEnabled("ShadowCaster", false);
            material.SetShaderPassEnabled("DepthOnly", false);
            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        private static Material Unlit(string name, Color colour)
        {
            Material material = LoadOrCreate(name, "Universal Render Pipeline/Unlit");
            material.SetColor("_BaseColor", colour);
            return material;
        }
    }
}
