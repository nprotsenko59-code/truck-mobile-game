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
        }

        private static void CreateRoad()
        {
            var roadMaterial = EnsureMaterial(
                "Assets/Game/Materials/Road.mat",
                new Color(0.12f, 0.14f, 0.16f));
            var grassMaterial = EnsureMaterial(
                "Assets/Game/Materials/Ground.mat",
                new Color(0.18f, 0.33f, 0.14f));
            var lineMaterial = EnsureMaterial(
                "Assets/Game/Materials/RoadLine.mat",
                new Color(0.95f, 0.78f, 0.18f));

            CreateBox("Road", new Vector3(0f, -0.3f, 55f), new Vector3(12f, 0.6f, 120f), roadMaterial);
            CreateBox("Ground Left", new Vector3(-31f, -0.55f, 55f), new Vector3(50f, 0.5f, 120f), grassMaterial);
            CreateBox("Ground Right", new Vector3(31f, -0.55f, 55f), new Vector3(50f, 0.5f, 120f), grassMaterial);

            for (var z = 4f; z < 112f; z += 8f)
            {
                CreateBox("Centre Line", new Vector3(0f, 0.025f, z), new Vector3(0.16f, 0.05f, 4f), lineMaterial);
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
                new Color(0.08f, 0.44f, 0.82f));
            var wheelMaterial = EnsureMaterial(
                "Assets/Game/Materials/Tire.mat",
                new Color(0.025f, 0.025f, 0.03f));

            var cab = CreateBox("Cab", Vector3.zero, new Vector3(2.5f, 2.3f, 3f), bodyMaterial);
            cab.transform.SetParent(truck.transform, false);
            cab.transform.localPosition = new Vector3(0f, 1.15f, 1.2f);

            var chassis = CreateBox("Chassis", Vector3.zero, new Vector3(2.35f, 0.5f, 5.8f), bodyMaterial);
            chassis.transform.SetParent(truck.transform, false);
            chassis.transform.localPosition = new Vector3(0f, 0.45f, -0.3f);

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

        private static Material EnsureMaterial(string path, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(material, path);
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
