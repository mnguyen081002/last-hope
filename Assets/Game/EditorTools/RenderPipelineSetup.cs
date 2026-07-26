using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LastHope.EditorTools
{
    /// <summary>
    /// Creates and assigns the URP asset per technical-specification.md mục 4/24:
    /// Universal Render Pipeline, Linear color space. Sprint 1 (BL-P1-01).
    /// 2026-07-25: URP in Unity 6 doesn't separate Renderer2DData from UniversalRendererData — both
    /// 2D and 3D use the same UniversalRendererData, just configured differently (Light2D vs Light3D).
    /// Kept on UniversalRendererData; Light2D support is automatic.
    /// </summary>
    public static class RenderPipelineSetup
    {
        private const string SettingsFolder = "Assets/Settings";
        private const string RendererDataPath = SettingsFolder + "/LastHope_Renderer.asset";
        private const string PipelineAssetPath = SettingsFolder + "/LastHope_URP.asset";

        [MenuItem("Last Hope/Setup URP Pipeline")]
        public static void Setup()
        {
            if (!AssetDatabase.IsValidFolder(SettingsFolder))
                AssetDatabase.CreateFolder("Assets", "Settings");

            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, RendererDataPath);
            }

            var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
            if (pipelineAsset == null)
            {
                pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipelineAsset, PipelineAssetPath);
            }

            GraphicsSettings.defaultRenderPipeline = pipelineAsset;

            int previousQualityLevel = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, applyExpensiveChanges: false);
                QualitySettings.renderPipeline = pipelineAsset;
            }
            QualitySettings.SetQualityLevel(previousQualityLevel, applyExpensiveChanges: false);

            PlayerSettings.colorSpace = ColorSpace.Linear;

            AssetDatabase.SaveAssets();
            Debug.Log("[RenderPipelineSetup] URP asset created and assigned (Graphics + all Quality levels). Color space set to Linear.");
        }
    }
}
