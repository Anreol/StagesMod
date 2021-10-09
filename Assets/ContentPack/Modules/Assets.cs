using StagesMod.ScriptableObjects;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;
using Path = System.IO.Path;

namespace StagesMod
{
    public static class Assets
    {
        public static AssetBundle mainAssetBundle
        {
            get
            {
                return assetBundles[0];
            }
        }

        internal static string assemblyDir
        {
            get
            {
                return Path.GetDirectoryName(StagesMod.pluginInfo.Location);
            }
        }

        private const string assetBundleFolderName = "assetbundles";
        internal static string mainAssetBundleName = "assetstages";
        public static ReadOnlyCollection<AssetBundle> assetBundles;

        [RoR2.SystemInitializer] //See the same thing on TE
        public static void Init()
        {
            var gameMaterials = Resources.FindObjectsOfTypeAll<Material>();
            foreach (var assetBundle in assetBundles)
            {
                SMLog.LogW("Changing " + assetBundle.name + " materials. Is Streamed: " + assetBundle.isStreamedSceneAssetBundle);
                MapMaterials(assetBundle, gameMaterials);
            }
        }

        public static string[] GetAssetBundlePaths()
        {
            return Directory.GetFiles(Path.Combine(assemblyDir, assetBundleFolderName))
               .Where(filePath => !filePath.EndsWith(".manifest")).Where(filePath => !filePath.EndsWith(".ros")) //FIXME: Stages cannot be added by RoS and here else it would be duped. The async load cannot take care of "streamed scene assetS" or smth. also a || didn't work.
               .OrderByDescending(path => Path.GetFileName(path).Equals(mainAssetBundleName))
               .ToArray();
        }

        //This one is the one for actually loading effects into the contentpack
        internal static void LoadEffects()
        {
            var effectHolders = mainAssetBundle.LoadAllAssets<EffectDefHolder>();
            foreach (var effectHolder in effectHolders)
                foreach (var effectPrefab in effectHolder.effectPrefabs)
                    HG.ArrayUtils.ArrayAppend(ref StagesMod.serializableContentPack.effectDefs, EffectDefHolder.ToEffectDef(effectPrefab));
        }

        //useless, didnt do what i wanted it to do
        private static readonly System.Collections.Generic.List<Material> loadedHGMaterials = new System.Collections.Generic.List<Material>();
        internal static void MapMaterials(AssetBundle assetBundle, Material[] gameMaterials)
        {
            if (assetBundle.isStreamedSceneAssetBundle)
                return;
            //SMLog.LogW("Mapping " + assetBundle.name + " materials");
            var cloudMat = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/LightningStrikeOrbEffect").transform.Find("Ring").GetComponent<ParticleSystemRenderer>().material;

            Material[] assetBundleMaterials = assetBundle.LoadAllAssets<Material>();
            //SMLog.LogW("Materials loaded length " + assetBundleMaterials.Length);
            for (int i = 0; i < assetBundleMaterials.Length; i++)
            {
                var material = assetBundleMaterials[i];
                // If it's stubbed, just switch out the shader unless it's fucking cloudremap
                if (material.shader.name.StartsWith("StubbedShader"))
                {
                    //SMLog.LogW(material.shader);
                    material.shader = Resources.Load<Shader>("shaders" + material.shader.name.Substring(13));
                    //SMLog.LogW(material.shader);
                    if (material.shader.name.Contains("Cloud Remap"))
                    {
                        var cockSucker = new RuntimeCloudMaterialMapper(material);
                        material.CopyPropertiesFromMaterial(cloudMat);
                        cockSucker.SetMaterialValues(ref material);
                    }
                }

                //If it's this shader it searches for a material with the same name and copies the properties
                if (material.shader.name.Equals("CopyFromRoR2"))
                {
                    foreach (var gameMaterial in gameMaterials)
                        if (material.name.Equals(gameMaterial.name))
                        {
                            material.shader = gameMaterial.shader;
                            material.CopyPropertiesFromMaterial(gameMaterial);
                            break;
                        }
                }
                assetBundleMaterials[i] = material;
                loadedHGMaterials.Add(material);
            }
        }
    }
}