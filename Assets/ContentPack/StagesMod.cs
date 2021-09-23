﻿using BepInEx;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace StagesMod
{
    [BepInPlugin(ModGuid, ModIdentifier, ModVer)]
    public class StagesMod : BaseUnityPlugin, IContentPackProvider
    {
        internal const string ModVer =
#if DEBUG
            "9999." +
#endif
            "0.0.1";

        internal const string ModIdentifier = "TurboEdition";
        internal const string ModGuid = "com.Anreol." + ModIdentifier;

        public static StagesMod instance;
        public static PluginInfo pluginInfo;
        public static SerializableContentPack serializableContentPack;

        public string identifier
        {
            get
            {
                return ModGuid;
            }
        }

        public void Awake()
        {
            SMLog.logger = Logger;
#if DEBUG
            SMLog.LogW("Running TurboEdition DEBUG build. PANIC!");
#endif
            pluginInfo = Info;
            instance = this;

            ContentManager.collectContentPackProviders += (addContentPackProvider) => addContentPackProvider(this);

#if DEBUG
            //Components.MaterialControllerComponents.AttachControllerFinderToObjects(Assets.mainAssetBundle);
#endif
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            List<AssetBundle> loadedBundles = new List<AssetBundle>();
            //AssetBundles
            var bundlePaths = Assets.GetAssetBundlePaths();
            int num;
            for (int i = 0; i < bundlePaths.Length; i = num)
            {
                var bundleLoadRequest = AssetBundle.LoadFromFileAsync(bundlePaths[i]);
                while (!bundleLoadRequest.isDone)
                {
                    args.ReportProgress(Util.Remap(bundleLoadRequest.progress + i, 0f, bundlePaths.Length, 0f, 0.8f));
                    yield return null;
                }
                num = i + 1;
                loadedBundles.Add(bundleLoadRequest.assetBundle);
            }
            Assets.assetBundles = new ReadOnlyCollection<AssetBundle>(loadedBundles);

            serializableContentPack = Assets.mainAssetBundle.LoadAsset<SerializableContentPack>("ContentPack");
            Assets.Init();
            //InitPickups.Init();
            //InitBuffs.Initialize();
            GetType().Assembly.GetTypes()
                              .Where(type => typeof(EntityStates.EntityState).IsAssignableFrom(type))
                              .ToList()
                              .ForEach(state => HG.ArrayUtils.ArrayAppend(ref serializableContentPack.entityStateTypes, new EntityStates.SerializableEntityStateType(state)));

            args.ReportProgress(1f);

            if (onLoadStaticContent != null)
                yield return onGenerateContentPack;
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            //NOTE: For some reason any instructions that are put inside of here are done twice. This will cause serious errors if you do not watch out.

            ContentPack contentPack = serializableContentPack.CreateContentPack();
            contentPack.identifier = identifier;
            ContentPack.Copy(contentPack, args.output);

            args.ReportProgress(1f);
            if (onGenerateContentPack != null)
                yield return onGenerateContentPack;
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public static LoadStaticContentAsyncDelegate onLoadStaticContent { get; set; }
        public static GenerateContentPackAsyncDelegate onGenerateContentPack { get; set; }
        public static FinalizeAsyncDelegate onFinalizeAsync { get; set; }

        public delegate IEnumerator LoadStaticContentAsyncDelegate(LoadStaticContentAsyncArgs args);

        public delegate IEnumerator GenerateContentPackAsyncDelegate(GetContentPackAsyncArgs args);

        public delegate IEnumerator FinalizeAsyncDelegate(FinalizeAsyncArgs args);
    }

    /* Apparently this is all unused because Ghor didn't finish it....
     * leaving it just in case tho...
     * im sad...

    public class TurboRoR2Mod : RoR2Mod
    {
        public RoR2.ContentManagement.IContentPackProvider contentPackProvider = ???;
        public TurboRoR2Mod() : base (Mod.FromJsonFile("TurboEdition", pathToFile))
        {
        }
    }*/
}