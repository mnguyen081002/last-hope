using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LastHope.EditorTools
{
    /// <summary>
    /// Creates and assigns the URP asset per technical-specification.md mục 4/24:
    /// Universal Render Pipeline 2D Renderer, Linear color space. Sprint 1 (BL-P1-01);
    /// switched to Renderer2DData for the 2026-07-25 2D isometric migration (Light2D/Shadow
    /// Caster 2D support for rain/night mood — the 3D UniversalRendererData has no 2D lighting).
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

            var rendererData = AssetDatabase.LoadAssetAtPath<Renderer2DData>(RendererDataPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<Renderer2DData>();
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
