using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LastHope.EditorTools
{
    /// <summary>Build Windows player từ batchmode. Dùng cho verification cuối mỗi sprint.</summary>
    public static class BuildScript
    {
        const string OutputPath = "Builds/Windows/LastHope.exe";

        [MenuItem("Last Hope/Build Windows (Development)")]
        public static void BuildWindowsDevelopment() => Build(development: true);

        public static void BuildWindowsRelease() => Build(development: false);

        static void Build(bool development)
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Fail("Build Settings không có scene nào. Chạy 'Last Hope/Build Sprint 1 Scenes' trước.");
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = development
                    ? BuildOptions.Development | BuildOptions.AllowDebugging
                    : BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] OK — {summary.totalSize} bytes, {summary.totalTime}.");
                if (Application.isBatchMode) EditorApplication.Exit(0);
            }
            else
            {
                Fail($"Build {summary.result}, {summary.totalErrors} lỗi.");
            }
        }

        static void Fail(string message)
        {
            Debug.LogError($"[BuildScript] {message}");
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }
}
