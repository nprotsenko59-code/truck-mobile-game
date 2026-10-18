using System.Collections.Generic;
using TruckGame.CameraSystem;
using TruckGame.Cargo;
using TruckGame.Core;
using TruckGame.Delivery;
using TruckGame.Driving;
using TruckGame.Economy;
using TruckGame.Fuel;
using TruckGame.Input;
using TruckGame.Traffic;
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
            EnsureFolder("Assets/Game/Fuel");
            EnsureFolder("Assets/Game/Delivery");
            EnsureFolder("Assets/Game/Cargo");
            EnsureFolder("Assets/Game/Economy");

            var pipeline = EnsureRenderPipeline();
            var tuning = EnsureTuning();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateRuntimeRoot();
            CreateLighting();
            var route = CreateRoad();
            var truck = CreateTruck(tuning);
            CreateTraffic(route);
            CreateFuelSystem(truck, route);
            CreateDeliverySystem(truck, route);
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

        private static TrafficRoute CreateRoad()
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
            var barrierMaterial = EnsureMaterial(
                "Assets/Game/Materials/Barrier.mat",
                new Color(0.62f, 0.67f, 0.69f),
                0.6f,
                0.45f);

            // A single flat collision surface keeps the greybox drive smooth while the
            // visual road tiles provide the route shape above it.
            CreateBox("Terrain", new Vector3(0f, -0.3f, 450f), new Vector3(500f, 0.6f, 1300f), grassMaterial);

            const float segmentLength = 12f;
            const float roadWidth = 14f;
            const float edgeOffset = 6.55f;
            const float shoulderOffset = 7.6f;
            const float markerOffset = 8.45f;

            var yaws = new[]
            {
                0f, 0f, 0f, 4f, 8f, 12f, 16f, 20f, 20f, 16f, 12f, 8f, 4f,
                0f, -4f, -8f, -12f, -16f, -20f, -20f, -16f, -12f, -8f, -4f,
                0f, 0f, 0f, -4f, -8f, -12f, -16f, -12f, -8f, -4f, 0f, 4f,
                8f, 12f, 16f, 20f, 24f, 24f, 20f, 16f, 12f, 8f, 4f, 0f, -4f,
                -8f, -12f, -16f, -20f, -24f, -24f, -20f, -16f, -12f, -8f, -4f,
                0f, 0f, 0f, 4f, 8f, 12f, 16f, 20f, 20f, 16f, 12f, 8f, 4f, 0f,
                -4f, -8f, -12f, -16f, -20f, -20f, -16f, -12f, -8f, -4f, 0f, 0f
            };

            var routeObject = new GameObject("Traffic Route");
            var route = routeObject.AddComponent<TrafficRoute>();
            var routePoints = new List<Vector3>();
            var cursor = new Vector3(0f, 0f, -18f);
            routePoints.Add(cursor);
            for (var index = 0; index < yaws.Length; index++)
            {
                var rotation = Quaternion.Euler(0f, yaws[index], 0f);
                var forward = rotation * Vector3.forward;
                var right = rotation * Vector3.right;
                var centre = cursor + forward * (segmentLength * 0.5f);

                CreateVisualRouteBox(
                    $"Road {index + 1:00}",
                    centre + Vector3.up * 0.025f,
                    new Vector3(roadWidth, 0.05f, segmentLength + 1.1f),
                    roadMaterial,
                    rotation);
                CreateVisualRouteBox(
                    $"Left Shoulder {index + 1:00}",
                    centre - right * shoulderOffset + Vector3.up * 0.018f,
                    new Vector3(1.35f, 0.035f, segmentLength + 1.1f),
                    shoulderMaterial,
                    rotation);
                CreateVisualRouteBox(
                    $"Right Shoulder {index + 1:00}",
                    centre + right * shoulderOffset + Vector3.up * 0.018f,
                    new Vector3(1.35f, 0.035f, segmentLength + 1.1f),
                    shoulderMaterial,
                    rotation);
                CreateVisualRouteBox(
                    $"Left Edge Line {index + 1:00}",
                    centre - right * edgeOffset + Vector3.up * 0.06f,
                    new Vector3(0.16f, 0.04f, segmentLength + 0.5f),
                    edgeLineMaterial,
                    rotation);
                CreateVisualRouteBox(
                    $"Right Edge Line {index + 1:00}",
                    centre + right * edgeOffset + Vector3.up * 0.06f,
                    new Vector3(0.16f, 0.04f, segmentLength + 0.5f),
                    edgeLineMaterial,
                    rotation);
                CreateVisualRouteBox(
                    $"Centre Line {index + 1:00}",
                    centre + Vector3.up * 0.065f,
                    new Vector3(0.18f, 0.045f, 5.2f),
                    lineMaterial,
                    rotation);

                if (index % 2 == 0)
                {
                    CreateRoadsideMarker(centre - right * markerOffset, rotation, markerMaterial, lineMaterial);
                    CreateRoadsideMarker(centre + right * markerOffset, rotation, markerMaterial, lineMaterial);
                }

                if (index is >= 6 and <= 9)
                {
                    CreateRotatedBox(
                        "Outer Curve Barrier",
                        centre - right * 6.95f + Vector3.up * 0.35f,
                        new Vector3(0.22f, 0.7f, segmentLength + 0.8f),
                        barrierMaterial,
                        rotation);
                }
                else if (index is >= 16 and <= 19)
                {
                    CreateRotatedBox(
                        "Outer Curve Barrier",
                        centre + right * 6.95f + Vector3.up * 0.35f,
                        new Vector3(0.22f, 0.7f, segmentLength + 0.8f),
                        barrierMaterial,
                        rotation);
                }

                cursor += forward * segmentLength;
                routePoints.Add(cursor);
            }

            route.Configure(routePoints.ToArray());
            return route;
        }

        private static void CreateTraffic(TrafficRoute route)
        {
            var red = EnsureMaterial(
                "Assets/Game/Materials/TrafficRed.mat",
                new Color(0.78f, 0.08f, 0.06f),
                0.4f);
            var yellow = EnsureMaterial(
                "Assets/Game/Materials/TrafficYellow.mat",
                new Color(0.95f, 0.55f, 0.04f),
                0.4f);
            var teal = EnsureMaterial(
                "Assets/Game/Materials/TrafficTeal.mat",
                new Color(0.02f, 0.52f, 0.54f),
                0.4f);
            var white = EnsureMaterial(
                "Assets/Game/Materials/TrafficWhite.mat",
                new Color(0.78f, 0.82f, 0.84f),
                0.35f);
            var dark = EnsureMaterial(
                "Assets/Game/Materials/TruckDark.mat",
                new Color(0.035f, 0.055f, 0.075f),
                0.38f);
            var glass = EnsureMaterial(
                "Assets/Game/Materials/TruckGlass.mat",
                new Color(0.24f, 0.65f, 0.82f),
                0.72f);
            var tire = EnsureMaterial(
                "Assets/Game/Materials/Tire.mat",
                new Color(0.018f, 0.022f, 0.028f),
                0.18f);

            var root = new GameObject("Traffic");
            CreateTrafficVehicle(root.transform, "Slow Red Car", route, red, dark, glass, tire, 2.6f, 9.5f, false, 5);
            CreateTrafficVehicle(root.transform, "Yellow Van", route, yellow, dark, glass, tire, 2.6f, 11f, false, 15);
            CreateTrafficVehicle(root.transform, "Oncoming Teal Car", route, teal, dark, glass, tire, -2.6f, 14f, true, 9);
            CreateTrafficVehicle(root.transform, "Oncoming White Car", route, white, dark, glass, tire, -2.6f, 13f, true, 18);
            CreateTrafficVehicle(root.transform, "Oncoming Red Car", route, red, dark, glass, tire, -2.6f, 15f, true, 24);
        }

        private static void CreateTrafficVehicle(
            Transform parent,
            string name,
            TrafficRoute route,
            Material bodyMaterial,
            Material darkMaterial,
            Material glassMaterial,
            Material tireMaterial,
            float laneOffset,
            float speed,
            bool oncoming,
            int startPoint)
        {
            var vehicle = new GameObject(name);
            vehicle.transform.SetParent(parent);

            var collider = vehicle.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.62f, 0f);
            collider.size = new Vector3(1.9f, 1.25f, 3.8f);

            var body = vehicle.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            CreateVehicleDetail(vehicle.transform, "Lower Body", new Vector3(0f, 0.55f, 0f), new Vector3(1.85f, 0.65f, 3.7f), bodyMaterial);
            CreateVehicleDetail(vehicle.transform, "Cabin", new Vector3(0f, 1.08f, -0.15f), new Vector3(1.55f, 0.7f, 1.8f), bodyMaterial);
            CreateVehicleDetail(vehicle.transform, "Windshield", new Vector3(0f, 1.16f, 0.775f), new Vector3(1.3f, 0.42f, 0.055f), glassMaterial);
            CreateVehicleDetail(vehicle.transform, "Front Grille", new Vector3(0f, 0.52f, 1.88f), new Vector3(1.2f, 0.28f, 0.07f), darkMaterial);
            CreateTrafficWheel(vehicle.transform, new Vector3(-0.93f, 0.38f, 1.15f), tireMaterial);
            CreateTrafficWheel(vehicle.transform, new Vector3(0.93f, 0.38f, 1.15f), tireMaterial);
            CreateTrafficWheel(vehicle.transform, new Vector3(-0.93f, 0.38f, -1.15f), tireMaterial);
            CreateTrafficWheel(vehicle.transform, new Vector3(0.93f, 0.38f, -1.15f), tireMaterial);

            vehicle.AddComponent<TrafficVehicle>().Configure(route, laneOffset, speed, oncoming, startPoint);
        }

        private static void CreateVehicleDetail(
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

        private static void CreateTrafficWheel(Transform parent, Vector3 localPosition, Material material)
        {
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "Wheel";
            wheel.transform.SetParent(parent, false);
            wheel.transform.localPosition = localPosition;
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            wheel.transform.localScale = new Vector3(0.38f, 0.18f, 0.38f);
            Object.DestroyImmediate(wheel.GetComponent<Collider>());
            wheel.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static GameObject CreateTruck(TruckTuning tuning)
        {
            var truck = new GameObject("Prototype Truck");
            truck.transform.position = new Vector3(2.5f, 1.1f, 0f);

            var body = truck.AddComponent<Rigidbody>();
            body.mass = 4500f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var input = truck.AddComponent<SteeringInput>();
            var controller = truck.AddComponent<ArcadeTruckController>();
            var fuelTank = truck.AddComponent<FuelTank>();
            fuelTank.Configure(100f, 52f, 0.65f, 0.35f, 0.15f);
            var cargoTracker = truck.AddComponent<CargoTracker>();
            cargoTracker.Configure("Hotel Supplies", 4f, 5f, 22f, 2.5f);
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
            controller.ConfigureFuelTank(fuelTank);
            visualSync.Configure(bindings.ToArray());
            return truck;
        }

        private static void CreateFuelSystem(GameObject truck, TrafficRoute route)
        {
            var laneMaterial = EnsureMaterial(
                "Assets/Game/Materials/FuelLane.mat",
                new Color(0.075f, 0.11f, 0.14f),
                0.18f);
            var fuelBlue = EnsureMaterial(
                "Assets/Game/Materials/FuelBlue.mat",
                new Color(0.03f, 0.72f, 0.88f),
                0.55f);
            var fuelOrange = EnsureMaterial(
                "Assets/Game/Materials/FuelOrange.mat",
                new Color(1f, 0.42f, 0.04f),
                0.42f);
            var stationWhite = EnsureMaterial(
                "Assets/Game/Materials/FuelStationWhite.mat",
                new Color(0.88f, 0.91f, 0.9f),
                0.32f);
            var stationDark = EnsureMaterial(
                "Assets/Game/Materials/FuelStationDark.mat",
                new Color(0.045f, 0.065f, 0.08f),
                0.25f);

            var stationRoot = new GameObject("Right-Side Fuel Station");
            var offsets = new[] { 6.8f, 8.5f, 10f, 10f, 8.5f, 6.8f };
            for (var index = 0; index < offsets.Length; index++)
            {
                var routeIndex = 9 + index;
                var forward = route.GetForward(routeIndex);
                var rotation = Quaternion.LookRotation(forward, Vector3.up);
                var centre = route.GetLanePoint(routeIndex, offsets[index]) + forward * 5.8f;
                var lane = CreateVisualRouteBox(
                    $"Station Lane {index + 1:00}",
                    centre + Vector3.up * 0.08f,
                    new Vector3(4.8f, 0.08f, 13f),
                    laneMaterial,
                    rotation);
                lane.transform.SetParent(stationRoot.transform);

                var right = Vector3.Cross(Vector3.up, forward).normalized;
                var outerLine = CreateVisualRouteBox(
                    "Station Lane Guide",
                    centre + right * 2.25f + Vector3.up * 0.14f,
                    new Vector3(0.18f, 0.045f, 10f),
                    fuelBlue,
                    rotation);
                outerLine.transform.SetParent(stationRoot.transform);
            }

            const int stationIndex = 12;
            const float stationOffset = 10f;
            var stationForward = route.GetForward(stationIndex);
            var stationRight = Vector3.Cross(Vector3.up, stationForward).normalized;
            var stationRotation = Quaternion.LookRotation(stationForward, Vector3.up);
            var stationCentre = route.GetLanePoint(stationIndex, stationOffset);

            var canopy = CreateVisualRouteBox(
                "Fuel Canopy",
                stationCentre + stationRight * 4.5f + Vector3.up * 3.9f,
                new Vector3(8f, 0.45f, 8f),
                stationWhite,
                stationRotation);
            canopy.transform.SetParent(stationRoot.transform);
            CreateStationPost(stationRoot.transform, stationCentre + stationRight * 2.4f, stationRotation, fuelBlue);
            CreateStationPost(stationRoot.transform, stationCentre + stationRight * 6.6f, stationRotation, fuelBlue);

            var building = CreateVisualRouteBox(
                "Fuel Station Building",
                stationCentre + stationRight * 9f + Vector3.up * 1.45f,
                new Vector3(4.4f, 2.9f, 7f),
                stationDark,
                stationRotation);
            building.transform.SetParent(stationRoot.transform);

            var archLeft = CreateVisualRouteBox(
                "Fuel Arch Left",
                stationCentre - stationRight * 2.3f + Vector3.up * 2.1f,
                new Vector3(0.34f, 4.2f, 0.34f),
                fuelBlue,
                stationRotation);
            archLeft.transform.SetParent(stationRoot.transform);
            var archRight = CreateVisualRouteBox(
                "Fuel Arch Right",
                stationCentre + stationRight * 2.3f + Vector3.up * 2.1f,
                new Vector3(0.34f, 4.2f, 0.34f),
                fuelBlue,
                stationRotation);
            archRight.transform.SetParent(stationRoot.transform);
            var archTop = CreateVisualRouteBox(
                "Fuel Pickup Marker",
                stationCentre + Vector3.up * 4.05f,
                new Vector3(5f, 0.75f, 0.55f),
                fuelOrange,
                stationRotation);
            archTop.transform.SetParent(stationRoot.transform);

            var sign = new GameObject("FUEL Sign");
            sign.transform.SetParent(stationRoot.transform);
            sign.transform.position = stationCentre - stationForward * 0.32f + Vector3.up * 4.08f;
            sign.transform.rotation = stationRotation;
            var text = sign.AddComponent<TextMesh>();
            text.text = "FUEL";
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.42f;
            text.fontSize = 72;
            text.color = Color.white;

            var triggerObject = new GameObject("Fuel Collection Zone");
            triggerObject.transform.SetParent(stationRoot.transform);
            triggerObject.transform.position = stationCentre + Vector3.up * 1.1f;
            triggerObject.transform.rotation = stationRotation;
            var trigger = triggerObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(5.2f, 2.8f, 9f);
            triggerObject.AddComponent<AudioSource>();
            triggerObject.AddComponent<FuelStation>().Configure(archTop.transform);

            var hud = new GameObject("Fuel HUD");
            hud.AddComponent<FuelHud>().Configure(truck.GetComponent<FuelTank>());
        }

        private static void CreateDeliverySystem(GameObject truck, TrafficRoute route)
        {
            var gold = EnsureMaterial(
                "Assets/Game/Materials/DestinationGold.mat",
                new Color(1f, 0.65f, 0.08f),
                0.48f);
            var green = EnsureMaterial(
                "Assets/Game/Materials/DestinationGreen.mat",
                new Color(0.08f, 0.72f, 0.45f),
                0.42f);
            var warmWhite = EnsureMaterial(
                "Assets/Game/Materials/HotelWarm.mat",
                new Color(0.88f, 0.79f, 0.65f),
                0.24f);
            var dark = EnsureMaterial(
                "Assets/Game/Materials/TruckDark.mat",
                new Color(0.035f, 0.055f, 0.075f),
                0.38f);

            var root = new GameObject("Delivery Landmarks");
            CreateWarehouse(root.transform, route, dark, gold);

            var destinationIndex = route.PointCount - 1;
            var forward = route.GetForward(destinationIndex);
            var right = Vector3.Cross(Vector3.up, forward).normalized;
            var rotation = Quaternion.LookRotation(forward, Vector3.up);
            var centre = route.GetLanePoint(destinationIndex, 2.6f) - forward * 5f;

            var arrivalLane = CreateVisualRouteBox(
                "Hotel Arrival Zone",
                centre + Vector3.up * 0.08f,
                new Vector3(6.2f, 0.08f, 15f),
                green,
                rotation);
            arrivalLane.transform.SetParent(root.transform);

            var archLeft = CreateVisualRouteBox(
                "Destination Arch Left",
                centre - right * 3.25f + Vector3.up * 2.45f,
                new Vector3(0.42f, 4.9f, 0.42f),
                gold,
                rotation);
            archLeft.transform.SetParent(root.transform);
            var archRight = CreateVisualRouteBox(
                "Destination Arch Right",
                centre + right * 3.25f + Vector3.up * 2.45f,
                new Vector3(0.42f, 4.9f, 0.42f),
                gold,
                rotation);
            archRight.transform.SetParent(root.transform);
            var archTop = CreateVisualRouteBox(
                "Destination Arch Top",
                centre + Vector3.up * 4.72f,
                new Vector3(7f, 0.75f, 0.6f),
                green,
                rotation);
            archTop.transform.SetParent(root.transform);
            CreateWorldText(root.transform, "HOTEL Sign", "HOTEL", centre - forward * 0.34f + Vector3.up * 4.75f, rotation, 0.42f);

            var hotel = CreateVisualRouteBox(
                "Destination Hotel",
                centre + right * 12f - forward * 2f + Vector3.up * 3.3f,
                new Vector3(12f, 6.6f, 18f),
                warmWhite,
                rotation);
            hotel.transform.SetParent(root.transform);
            var hotelDoor = CreateVisualRouteBox(
                "Hotel Entrance",
                centre + right * 6.05f - forward * 2f + Vector3.up * 1.45f,
                new Vector3(0.25f, 2.9f, 3.2f),
                dark,
                rotation);
            hotelDoor.transform.SetParent(root.transform);

            var triggerObject = new GameObject("Hotel Delivery Zone");
            triggerObject.transform.SetParent(root.transform);
            triggerObject.transform.position = centre + Vector3.up * 1.5f;
            triggerObject.transform.rotation = rotation;
            var trigger = triggerObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(7f, 3.5f, 9f);

            var deliveryRun = truck.AddComponent<DeliveryRunController>();
            deliveryRun.Configure(
                truck.transform,
                truck.GetComponent<FuelTank>(),
                truck.GetComponent<CargoTracker>(),
                route,
                triggerObject.transform,
                truck.GetComponent<ArcadeTruckController>(),
                1250);
            triggerObject.AddComponent<DestinationZone>().Configure(deliveryRun);

            var jobs = new[]
            {
                new DeliveryJobDefinition("standard", "STANDARD RUN", "EASY", "LOW", "SAFE", 1250, 56f, 4f, 2f),
                new DeliveryJobDefinition("priority", "PRIORITY RUN", "MEDIUM", "MEDIUM", "MODERATE", 1550, 52f, 5f, 2.5f),
                new DeliveryJobDefinition("premium", "PREMIUM RUN", "HARD", "HIGH", "TIGHT", 1800, 48f, 6.5f, 3.25f)
            };
            var economyObject = new GameObject("Economy & Job Flow");
            var economy = economyObject.AddComponent<EconomyGameController>();
            economy.Configure(
                deliveryRun,
                truck.GetComponent<FuelTank>(),
                truck.GetComponent<CargoTracker>(),
                truck.GetComponent<ArcadeTruckController>(),
                jobs);

            var hud = new GameObject("Delivery HUD");
            hud.AddComponent<DeliveryHud>().Configure(deliveryRun, economy);

            var jobBoard = new GameObject("Job Board HUD");
            jobBoard.AddComponent<JobBoardHud>().Configure(economy);

            var cargoHud = new GameObject("Cargo HUD");
            cargoHud.AddComponent<CargoHud>().Configure(truck.GetComponent<CargoTracker>());
        }

        private static void CreateWarehouse(Transform parent, TrafficRoute route, Material buildingMaterial, Material accentMaterial)
        {
            const int originIndex = 1;
            var forward = route.GetForward(originIndex);
            var right = Vector3.Cross(Vector3.up, forward).normalized;
            var rotation = Quaternion.LookRotation(forward, Vector3.up);
            var centre = route.GetPoint(originIndex) - right * 22f;

            var warehouse = CreateVisualRouteBox(
                "Origin Warehouse",
                centre + Vector3.up * 2.6f,
                new Vector3(13f, 5.2f, 15f),
                buildingMaterial,
                rotation);
            warehouse.transform.SetParent(parent);
            var loadingDoor = CreateVisualRouteBox(
                "Warehouse Loading Door",
                centre + right * 6.55f + Vector3.up * 1.55f,
                new Vector3(0.22f, 3.1f, 4.8f),
                accentMaterial,
                rotation);
            loadingDoor.transform.SetParent(parent);
        }

        private static void CreateWorldText(
            Transform parent,
            string name,
            string content,
            Vector3 position,
            Quaternion rotation,
            float characterSize)
        {
            var sign = new GameObject(name);
            sign.transform.SetParent(parent);
            sign.transform.position = position;
            sign.transform.rotation = rotation;
            var text = sign.AddComponent<TextMesh>();
            text.text = content;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = characterSize;
            text.fontSize = 72;
            text.color = Color.white;
        }

        private static void CreateStationPost(
            Transform parent,
            Vector3 groundPosition,
            Quaternion rotation,
            Material material)
        {
            var post = CreateVisualRouteBox(
                "Fuel Canopy Post",
                groundPosition + Vector3.up * 1.9f,
                new Vector3(0.35f, 3.8f, 0.35f),
                material,
                rotation);
            post.transform.SetParent(parent);
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

        private static void CreateRoadsideMarker(
            Vector3 position,
            Quaternion rotation,
            Material postMaterial,
            Material reflectorMaterial)
        {
            CreateVisualRouteBox(
                "Roadside Marker",
                position + Vector3.up * 0.55f,
                new Vector3(0.16f, 1.1f, 0.16f),
                postMaterial,
                rotation);
            CreateVisualRouteBox(
                "Roadside Reflector",
                position + Vector3.up * 0.83f,
                new Vector3(0.21f, 0.22f, 0.2f),
                reflectorMaterial,
                rotation);
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

        private static GameObject CreateRotatedBox(
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            Quaternion rotation)
        {
            var box = CreateBox(name, position, scale, material);
            box.transform.rotation = rotation;
            return box;
        }

        private static GameObject CreateVisualRouteBox(
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            Quaternion rotation)
        {
            var box = CreateRotatedBox(name, position, scale, material, rotation);
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
