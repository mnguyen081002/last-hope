using System.IO;
using UnityEditor;
using UnityEngine;

namespace LastHope.EditorTools
{
    /// <summary>
    /// Imports TextMeshPro's essential resources (TMP Settings + default font asset) — without
    /// this, any TextMeshProUGUI.Awake() throws NullReferenceException at runtime because
    /// TMP_Settings.instance is null. Normally done via Window > TextMeshPro > Import TMP
    /// Essential Resources; scripted here so batchmode setup is reproducible.
    /// </summary>
    public static class TmpSetup
    {
        /// <summary>
        /// Run WITHOUT -quit: AssetDatabase.ImportPackage is asynchronous even with
        /// interactive=false, so a -quit in the same invocation can end the process before the
        /// import actually completes. This method exits the process itself once Unity's
        /// importPackageCompleted/importPackageFailed callback fires.
        /// </summary>
        [MenuItem("Last Hope/Import TMP Essential Resources")]
        public static void ImportEssentials()
        {
            string packagePath = FindPackage("TMP Essential Resources.unitypackage");
            if (packagePath == null)
            {
                Debug.LogError("[TmpSetup] Could not find 'TMP Essential Resources.unitypackage' in PackageCache.");
                EditorApplication.Exit(1);
                return;
            }

            AssetDatabase.importPackageCompleted += OnCompleted;
            AssetDatabase.importPackageFailed += OnFailed;
            AssetDatabase.ImportPackage(packagePath, false);
        }

        private static void OnCompleted(string packageName)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"[TmpSetup] Import completed: {packageName}.");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        private static void OnFailed(string packageName, string errorMessage)
        {
            Debug.LogError($"[TmpSetup] Import failed: {packageName}: {errorMessage}");
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }

        private static string FindPackage(string fileName)
        {
            string packageCacheDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Library", "PackageCache");
            if (!Directory.Exists(packageCacheDir)) return null;

            foreach (string dir in Directory.GetDirectories(packageCacheDir, "com.unity.ugui@*"))
            {
                string candidate = Path.Combine(dir, "Package Resources", fileName);
                if (File.Exists(candidate)) return candidate;
            }
            return null;
        }
    }
}
