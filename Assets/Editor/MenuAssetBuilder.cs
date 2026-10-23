#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using Wobble.Economy;

namespace Wobble.Editor
{
    public static class MenuAssetBuilder
    {
        [MenuItem("WOBBLE/Menu/Configure Menu Assets")]
        public static void Configure()
        {
            Directory.CreateDirectory("Assets/Localization");
            Directory.CreateDirectory("Assets/Resources/Menu");
            AssetDatabase.Refresh();
            var settings=AssetDatabase.LoadAssetAtPath<LocalizationSettings>("Assets/Localization/WobbleLocalization.asset");
            if(settings==null){settings=ScriptableObject.CreateInstance<LocalizationSettings>();AssetDatabase.CreateAsset(settings,"Assets/Localization/WobbleLocalization.asset");}
            LocalizationEditorSettings.ActiveLocalizationSettings=settings;
            var locales=new Locale[2];
            var codes=new[]{"tr","en"};
            for(var i=0;i<2;i++)
            {
                var path="Assets/Localization/"+codes[i]+".asset";
                locales[i]=AssetDatabase.LoadAssetAtPath<Locale>(path);
                if(locales[i]==null){locales[i]=Locale.CreateLocale(codes[i]);AssetDatabase.CreateAsset(locales[i],path);}
                LocalizationEditorSettings.AddLocale(locales[i]);
            }
            settings.GetStartupLocaleSelectors().Clear();
            settings.GetStartupLocaleSelectors().Add(new SpecificLocaleSelector { LocaleId=new LocaleIdentifier("tr") });
            LocalizationSettings.ProjectLocale=locales[0];
            EditorUtility.SetDirty(settings);
            var collection=LocalizationEditorSettings.GetStringTableCollection("Wobble") ??
                LocalizationEditorSettings.CreateStringTableCollection("Wobble","Assets/Localization",locales);
            var lines=File.ReadAllLines("Assets/LocalizationAuthoring/Wobble.tsv");
            foreach(var line in lines.Skip(1))
            {
                if(string.IsNullOrWhiteSpace(line))continue;
                var cells=line.Split('\t');
                if(cells.Length!=3)throw new Exception("Invalid localization row: "+line);
                for(var i=0;i<2;i++)
                {
                    var table=(StringTable)collection.GetTable(new LocaleIdentifier(codes[i]));
                    var value=cells[i+1].Replace("\\n","\n");
                    var entry=table.AddEntry(cells[0],value);entry.IsSmart=value.Contains("{");
                    EditorUtility.SetDirty(table);
                }
            }
            collection.RefreshAddressables();
            collection.SetPreloadTableFlag(true);
            EditorUtility.SetDirty(collection.SharedData);
            EditorUtility.SetDirty(collection);
            var addressables=AddressableAssetSettingsDefaultObject.Settings;
            // Reconnect persisted schemas if an earlier interrupted authoring run
            // created their files before saving the group's schema list.
            foreach(var group in addressables.groups.Where(g=>g!=null && g.Name.StartsWith("Localization-")))
            {
                foreach(var guid in AssetDatabase.FindAssets("t:AddressableAssetGroupSchema",new[]{"Assets/AddressableAssetsData/AssetGroups/Schemas"}))
                {
                    var schemaPath=AssetDatabase.GUIDToAssetPath(guid);
                    if(!Path.GetFileName(schemaPath).StartsWith(group.Name+"_"))continue;
                    var schema=AssetDatabase.LoadAssetAtPath<AddressableAssetGroupSchema>(schemaPath);
                    if(!group.HasSchema(schema.GetType()))group.AddSchema(schema);
                }
                EditorUtility.SetDirty(group);
            }
            addressables.BuildAddressablesWithPlayerBuild=AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;
            EditorUtility.SetDirty(addressables);
            BuildCatalog();
            const string uiMaterialPath="Assets/Resources/Menu/MenuUI.mat";
            if(AssetDatabase.LoadAssetAtPath<Material>(uiMaterialPath)==null)
                AssetDatabase.CreateAsset(new Material(Shader.Find("UI/Default")){name="Wobble Menu UI"},uiMaterialPath);
            AssetDatabase.SaveAssets();
            BuildFont(string.Join("",lines)+"₺0123456789× ·‹");
            AssetDatabase.SaveAssets();
            Debug.Log("WOBBLE menu assets ready: Unity Localization tr/en, Turkish default, catalog and localized font.");
        }

        private static void BuildFont(string characters)
        {
            const string path="Assets/Resources/Menu/WobbleLocalizedSans.asset";
            var existing=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            var currency=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Menu/CurrencyFallback.asset");
            if(existing!=null && existing.material!=null && currency!=null && currency.material!=null)return;
            // Recover only these generated assets after an interrupted authoring run.
            if(existing!=null)AssetDatabase.DeleteAsset(path);
            if(currency!=null && currency.material==null)AssetDatabase.DeleteAsset("Assets/Resources/Menu/CurrencyFallback.asset");
            var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
            var font=TMP_FontAsset.CreateFontAsset(source,96,12,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,2048,2048);
            font.name="Wobble Localized Sans";
            font.TryAddCharacters(new string(characters.Where(c=>!char.IsControl(c)).Distinct().ToArray()),out var missing);
            if(!string.IsNullOrEmpty(missing))
            {
                var fallback=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/Menu/CurrencyFallback.asset");
                if(fallback==null)
                {
                var fallbackSource=AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSans.ttf");
                fallback=TMP_FontAsset.CreateFontAsset(fallbackSource,96,12,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,512,512);
                fallback.name="Wobble Currency Fallback";
                fallback.TryAddCharacters(missing,out var stillMissing);
                if(!string.IsNullOrEmpty(stillMissing))throw new Exception("Missing fallback glyphs: "+stillMissing);
                fallback.atlasPopulationMode=AtlasPopulationMode.Static;
                AssetDatabase.CreateAsset(fallback,"Assets/Resources/Menu/CurrencyFallback.asset");
                AssetDatabase.AddObjectToAsset(fallback.material,fallback);
                foreach(var atlas in fallback.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,fallback);
                EditorUtility.SetDirty(fallback);
                }
                font.fallbackFontAssetTable=new List<TMP_FontAsset>{fallback};
            }
            font.atlasPopulationMode=AtlasPopulationMode.Static;
            AssetDatabase.CreateAsset(font,path);
            AssetDatabase.AddObjectToAsset(font.material,font);
            foreach(var atlas in font.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,font);
            EditorUtility.SetDirty(font);
        }

        private static void BuildCatalog()
        {
            const string path="Assets/Resources/Menu/MenuCatalog.asset";
            if(AssetDatabase.LoadAssetAtPath<MenuCatalog>(path)!=null)return;
            var catalog=ScriptableObject.CreateInstance<MenuCatalog>();
            ProductDefinition P(string id,string key,ProductSection section,string price,int gems=0,int lives=0,string skin="",bool noAds=false)=>
                new ProductDefinition{id=id,nameKey=key,section=section,placeholderPrice=price,gems=gems,lives=lives,futureSkinId=skin,removesAds=noAds};
            catalog.products=new[]{
                P("lives10","shop.lives10",ProductSection.Lives,"₺19.99",lives:10),
                P("lives50","shop.lives50",ProductSection.Lives,"₺59.99",lives:50),
                P("gems250","shop.gems250",ProductSection.Gems,"₺19.99",250),
                P("gems750","shop.gems750",ProductSection.Gems,"₺49.99",750),
                P("gems2000","shop.gems2000",ProductSection.Gems,"₺99.99",2000),
                P("starter","shop.starter_pack",ProductSection.Special,"₺39.99",500,20,"future-exclusive"),
                P("removeAds","shop.remove_ads",ProductSection.Special,"₺79.99",noAds:true)};
            var ids=new[]{"Carrara","Obsidian","AncientStone","Neon","Space"};
            var keys=new[]{"theme.carrara","theme.obsidian","theme.ancient","theme.neon","theme.space"};
            var colors=new[]{new Color(.86f,.84f,.79f),new Color(.15f,.16f,.18f),new Color(.47f,.43f,.34f),new Color(.25f,.42f,.42f),new Color(.25f,.28f,.38f)};
            catalog.themes=Enumerable.Range(0,5).Select(i=>new ThemeDefinition{id=ids[i],nameKey=keys[i],previewTint=colors[i],gemPrice=i*250,available=i==0}).ToArray();
            var material=new Material(Resources.Load<Shader>("Shaders/Carrara")){name="Carrara menu preview",color=colors[0]};
            material.mainTexture=Resources.Load<Texture2D>("Rendering/CarraraNatural");
            AssetDatabase.CreateAsset(material,"Assets/Resources/Menu/CarraraPreview.mat");
            catalog.themes[0].previewMaterial=material;
            catalog.dailyRewards=new[]{new RewardDefinition{gems=20},new RewardDefinition{lives=1},new RewardDefinition{gems=30},new RewardDefinition{lives=1},new RewardDefinition{gems=50},new RewardDefinition{lives=2},new RewardDefinition{gems=100,lives=3}};
            AssetDatabase.CreateAsset(catalog,path);
        }
    }
}
#endif
