using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TruckGame.Editor
{
    public static class PrototypeBuild
    {
        private const string OutputPath = "Builds/iOS";

        [MenuItem("Truck Game/Build iOS Prototype")]
        public static void BuildIosPrototype()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled scenes are configured in Build Settings.");
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log(
                $"iOS prototype build {summary.result}: " +
                $"{summary.totalErrors} errors, {summary.totalWarnings} warnings, " +
                $"{summary.totalSize / (1024f * 1024f):0.0} MB, {summary.totalTime}.");

            if (summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"iOS prototype build failed with {summary.totalErrors} errors.");
            }
        }
    }
}
