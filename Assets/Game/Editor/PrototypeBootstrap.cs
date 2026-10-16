using System.Collections.Generic;
using TruckGame.CameraSystem;
using TruckGame.Core;
using TruckGame.Driving;
using TruckGame.Input;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace TruckGame.Editor
{
    public static class PrototypeBootstrap
    {
        private const string ScenePath = "Assets/Game/Scenes/Prototype.unity";
        private const string TuningPath = "Assets/Game/Settings/PrototypeTruckTuning.asset";
        private const string PipelinePath = "Assets/Game/Settings/MobileURP.asset";
        private const string RendererPath = "Assets/Game/Settings/MobileRenderer.asset";

        [MenuItem("Truck Game/Create Prototype Foundation")]
        public static void CreateProjectFoundation()
        {
            EnsureFolder("Assets/Game/Scenes");
            EnsureFolder("Assets/Game/Settings");
            EnsureFolder("Assets/Game/Materials");

            var pipeline = EnsureRenderPipeline();
            var tuning = EnsureTuning();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateRuntimeRoot();
            CreateLighting();
            CreateRoad();
            var truck = CreateTruck(tuning);
            CreateCamera(truck.transform);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            PlayerSettings.companyName = "Nikita Protsenko";
            PlayerSettings.productName = "Truck Game Prototype";
            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.iOS,
                "com.nprotsenko.truckgame.prototype");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Truck Game prototype foundation created at {ScenePath}");
        }

        private static UniversalRenderPipelineAsset EnsureRenderPipeline()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline != null)
            {
                return pipeline;
            }

            var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, RendererPath);
            pipeline = UniversalRenderPipelineAsset.Create(renderer);
            pipeline.name = "MobileURP";
            pipeline.msaaSampleCount = 4;
            pipeline.renderScale = 1f;
            pipeline.shadowDistance = 45f;
            AssetDatabase.CreateAsset(pipeline, PipelinePath);
            return pipeline;
        }

        private static TruckTuning EnsureTuning()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<TruckTuning>(TuningPath);
            if (tuning != null)
            {
                return tuning;
            }

            tuning = ScriptableObject.CreateInstance<TruckTuning>();
            AssetDatabase.CreateAsset(tuning, TuningPath);
            return tuning;
        }

        private static void CreateRuntimeRoot()
        {
            var root = new GameObject("Prototype Runtime");
            root.AddComponent<PrototypeRuntime>();
        }

        private static void CreateLighting()
        {
            var lightObject = new GameObject("Sun");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.94f, 0.82f);
            lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.45f, 0.58f, 0.72f);
            RenderSettings.ambientEquatorColor = new Color(0.3f, 0.34f, 0.35f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.14f, 0.12f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.68f, 0.78f, 0.84f);
            RenderSettings.fogStartDistance = 55f;
            RenderSettings.fogEndDistance = 190f;
        }

        private static void CreateRoad()
        {
            var roadMaterial = EnsureMaterial(
                "Assets/Game/Materials/Road.mat",
                new Color(0.105f, 0.125f, 0.15f),
                0.12f);
            var grassMaterial = EnsureMaterial(
                "Assets/Game/Materials/Ground.mat",
                new Color(0.16f, 0.31f, 0.14f),
                0.05f);
            var lineMaterial = EnsureMaterial(
                "Assets/Game/Materials/RoadLine.mat",
                new Color(1f, 0.72f, 0.08f),
                0.25f);
            var edgeLineMaterial = EnsureMaterial(
                "Assets/Game/Materials/EdgeLine.mat",
                new Color(0.92f, 0.94f, 0.92f),
                0.2f);
            var shoulderMaterial = EnsureMaterial(
                "Assets/Game/Materials/Shoulder.mat",
                new Color(0.34f, 0.29f, 0.23f),
                0.05f);
            var markerMaterial = EnsureMaterial(
                "Assets/Game/Materials/RoadMarker.mat",
                new Color(0.96f, 0.94f, 0.86f),
                0.18f);

            CreateBox("Road", new Vector3(0f, -0.3f, 55f), new Vector3(14f, 0.6f, 120f), roadMaterial);
            CreateBox("Left Shoulder", new Vector3(-7.6f, -0.18f, 55f), new Vector3(1.2f, 0.2f, 120f), shoulderMaterial);
            CreateBox("Right Shoulder", new Vector3(7.6f, -0.18f, 55f), new Vector3(1.2f, 0.2f, 120f), shoulderMaterial);
            CreateBox("Ground Left", new Vector3(-35.7f, -0.55f, 55f), new Vector3(55f, 0.5f, 120f), grassMaterial);
            CreateBox("Ground Right", new Vector3(35.7f, -0.55f, 55f), new Vector3(55f, 0.5f, 120f), grassMaterial);

            CreateVisualBox("Left Edge Line", new Vector3(-6.55f, 0.025f, 55f), new Vector3(0.16f, 0.05f, 116f), edgeLineMaterial);
            CreateVisualBox("Right Edge Line", new Vector3(6.55f, 0.025f, 55f), new Vector3(0.16f, 0.05f, 116f), edgeLineMaterial);

            for (var z = 4f; z < 112f; z += 8f)
            {
                CreateVisualBox("Centre Line", new Vector3(0f, 0.03f, z), new Vector3(0.18f, 0.06f, 4f), lineMaterial);
            }

            for (var z = 8f; z < 116f; z += 18f)
            {
                CreateRoadsideMarker(new Vector3(-8.35f, 0.55f, z), markerMaterial, lineMaterial);
                CreateRoadsideMarker(new Vector3(8.35f, 0.55f, z), markerMaterial, lineMaterial);
            }
        }

        private static GameObject CreateTruck(TruckTuning tuning)
        {
            var truck = new GameObject("Prototype Truck");
            truck.transform.position = new Vector3(-2.5f, 1.1f, 0f);

            var body = truck.AddComponent<Rigidbody>();
            body.mass = 4500f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var input = truck.AddComponent<SteeringInput>();
            var controller = truck.AddComponent<ArcadeTruckController>();
            truck.AddComponent<TruckRecovery>();
            var visualSync = truck.AddComponent<WheelVisualSync>();

            var bodyMaterial = EnsureMaterial(
                "Assets/Game/Materials/TruckBody.mat",
                new Color(0.03f, 0.34f, 0.78f),
                0.42f);
            var cargoMaterial = EnsureMaterial(
                "Assets/Game/Materials/TruckCargo.mat",
                new Color(0.82f, 0.86f, 0.88f),
                0.25f);
            var wheelMaterial = EnsureMaterial(
                "Assets/Game/Materials/Tire.mat",
                new Color(0.018f, 0.022f, 0.028f),
                0.18f);
            var darkMaterial = EnsureMaterial(
                "Assets/Game/Materials/TruckDark.mat",
                new Color(0.035f, 0.055f, 0.075f),
                0.38f);
            var glassMaterial = EnsureMaterial(
                "Assets/Game/Materials/TruckGlass.mat",
                new Color(0.24f, 0.65f, 0.82f),
                0.72f);
            var metalMaterial = EnsureMaterial(
                "Assets/Game/Materials/TruckMetal.mat",
                new Color(0.52f, 0.58f, 0.62f),
                0.7f,
                0.65f);
            var lightMaterial = EnsureMaterial(
                "Assets/Game/Materials/TruckLight.mat",
                new Color(1f, 0.85f, 0.42f),
                0.55f);

            var cab = CreateBox("Cab", Vector3.zero, new Vector3(2.5f, 2.25f, 2.1f), bodyMaterial);
            cab.transform.SetParent(truck.transform, false);
            cab.transform.localPosition = new Vector3(0f, 1.15f, 1.35f);

            var cargo = CreateBox("Cargo Box", Vector3.zero, new Vector3(2.45f, 2.65f, 3.15f), cargoMaterial);
            cargo.transform.SetParent(truck.transform, false);
            cargo.transform.localPosition = new Vector3(0f, 1.35f, -1.25f);

            var chassis = CreateBox("Chassis", Vector3.zero, new Vector3(2.35f, 0.42f, 5.6f), darkMaterial);
            chassis.transform.SetParent(truck.transform, false);
            chassis.transform.localPosition = new Vector3(0f, 0.42f, -0.25f);

            CreateTruckDetail(truck.transform, "Windshield", new Vector3(0f, 1.48f, 2.415f), new Vector3(1.85f, 0.62f, 0.045f), glassMaterial);
            CreateTruckDetail(truck.transform, "Front Grille", new Vector3(0f, 0.72f, 2.42f), new Vector3(1.45f, 0.34f, 0.05f), darkMaterial);
            CreateTruckDetail(truck.transform, "Front Bumper", new Vector3(0f, 0.3f, 2.49f), new Vector3(2.7f, 0.2f, 0.22f), metalMaterial);
            CreateTruckDetail(truck.transform, "Left Headlight", new Vector3(-0.86f, 0.86f, 2.45f), new Vector3(0.34f, 0.28f, 0.07f), lightMaterial);
            CreateTruckDetail(truck.transform, "Right Headlight", new Vector3(0.86f, 0.86f, 2.45f), new Vector3(0.34f, 0.28f, 0.07f), lightMaterial);
            CreateTruckDetail(truck.transform, "Cargo Accent", new Vector3(0f, 1.25f, -2.84f), new Vector3(1.95f, 0.18f, 0.045f), bodyMaterial);

            var front = new WheelCollider[2];
            var rear = new WheelCollider[2];
            var bindings = new List<WheelVisualSync.WheelBinding>();
            front[0] = CreateWheel(truck.transform, "Front Left", new Vector3(-1.15f, 0.45f, 1.65f), wheelMaterial, bindings);
            front[1] = CreateWheel(truck.transform, "Front Right", new Vector3(1.15f, 0.45f, 1.65f), wheelMaterial, bindings);
            rear[0] = CreateWheel(truck.transform, "Rear Left", new Vector3(-1.15f, 0.45f, -1.9f), wheelMaterial, bindings);
            rear[1] = CreateWheel(truck.transform, "Rear Right", new Vector3(1.15f, 0.45f, -1.9f), wheelMaterial, bindings);

            controller.Configure(body, input, tuning, front, rear);
            visualSync.Configure(bindings.ToArray());
            return truck;
        }

        private static void CreateTruckDetail(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            var detail = CreateVisualBox(name, Vector3.zero, localScale, material);
            detail.transform.SetParent(parent, false);
            detail.transform.localPosition = localPosition;
        }

        private static void CreateRoadsideMarker(Vector3 position, Material postMaterial, Material reflectorMaterial)
        {
            CreateVisualBox("Roadside Marker", position, new Vector3(0.16f, 1.1f, 0.16f), postMaterial);
            CreateVisualBox("Roadside Reflector", position + new Vector3(0f, 0.28f, 0f), new Vector3(0.21f, 0.22f, 0.2f), reflectorMaterial);
        }

        private static WheelCollider CreateWheel(
            Transform parent,
            string name,
            Vector3 localPosition,
            Material material,
            ICollection<WheelVisualSync.WheelBinding> bindings)
        {
            var colliderObject = new GameObject($"{name} Collider");
            colliderObject.transform.SetParent(parent, false);
            colliderObject.transform.localPosition = localPosition;
            var wheel = colliderObject.AddComponent<WheelCollider>();
            wheel.mass = 45f;
            wheel.radius = 0.55f;
            wheel.suspensionDistance = 0.35f;
            wheel.forceAppPointDistance = 0.15f;
            wheel.suspensionSpring = new JointSpring
            {
                spring = 42000f,
                damper = 5200f,
                targetPosition = 0.5f
            };

            var sideways = wheel.sidewaysFriction;
            sideways.stiffness = 1.45f;
            wheel.sidewaysFriction = sideways;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = $"{name} Visual";
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            visual.transform.localScale = new Vector3(0.55f, 0.24f, 0.55f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<Renderer>().sharedMaterial = material;

            bindings.Add(new WheelVisualSync.WheelBinding { collider = wheel, visual = visual.transform });
            return wheel;
        }

        private static void CreateCamera(Transform target)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 62f;
            camera.nearClipPlane = 0.2f;
            camera.farClipPlane = 500f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = target.TransformPoint(new Vector3(0f, 6.5f, -10.5f));
            cameraObject.AddComponent<SimpleChaseCamera>().Configure(target);
        }

        private static GameObject CreateBox(string name, Vector3 position, Vector3 scale, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.position = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
            return box;
        }

        private static GameObject CreateVisualBox(string name, Vector3 position, Vector3 scale, Material material)
        {
            var box = CreateBox(name, position, scale, material);
            Object.DestroyImmediate(box.GetComponent<Collider>());
            return box;
        }

        private static Material EnsureMaterial(string path, Color color, float smoothness, float metallic = 0f)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.color = color;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            var segments = path.Split('/');
            var current = segments[0];
            for (var index = 1; index < segments.Length; index++)
            {
                var next = $"{current}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }
    }
}
