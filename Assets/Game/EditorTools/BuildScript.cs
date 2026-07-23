using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace LastHope.EditorTools
{
    /// <summary>
    /// First PC build per mvp-implementation-plan.md mục 6 (Milestone 0 deliverable) and
    /// technical-specification.md mục 22 (Development build config: Mono, dev build enabled).
    /// </summary>
    public static class BuildScript
    {
        private const string OutputPath = "Builds/Windows/LastHope.exe";

        [MenuItem("Last Hope/Build Windows Development Player")]
        public static void BuildWindowsDevelopment()
        {
            PlayerSettings.SetScriptingBackend(
                UnityEditor.Build.NamedBuildTarget.Standalone,
                ScriptingImplementation.Mono2x);

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath) ?? "Builds/Windows");

            var scenes = System.Array.ConvertAll(
                System.Array.FindAll(EditorBuildSettings.scenes, s => s.enabled),
                s => s.path);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            UnityEngine.Debug.Log(
                $"[BuildScript] Result: {summary.result}, Errors: {summary.totalErrors}, " +
                $"Warnings: {summary.totalWarnings}, Size: {summary.totalSize} bytes, " +
                $"Time: {summary.totalTime}");

            if (summary.result != BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }
    }
}
