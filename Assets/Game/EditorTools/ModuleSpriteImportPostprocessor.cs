using UnityEditor;
using UnityEngine;

namespace LastHope.EditorTools
{
    /// <summary>Giữ import settings của art Module nhất quán sau mỗi lần regenerate/copy.</summary>
    public class ModuleSpriteImportPostprocessor : AssetPostprocessor
    {
        const string ModuleSpriteRoot = "Assets/Resources/Art/ShelterModulesP3/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ModuleSpriteRoot, System.StringComparison.Ordinal)) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 256f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(0.5f, 0f);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
        }
    }
}
