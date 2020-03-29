﻿using CarnationVariableSectionPart.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarnationVariableSectionPart
{
    internal class CVSPConfigs
    {
        private static List<TankTypeDefinition> tankDefinitions;
        private static List<TextureDefinition> textureDefinitions;
        private static Dictionary<string, float> fuelAmountPerVolume;
        private static string CRFPConfigPath;
        private static bool realFuel = false;
        private static bool RFChecked = false;
        private static bool far = false;
        private static bool FARChecked = false;
        internal static double RealFuelVolumeFactor = 765.056629974;
        private static MethodInfo LoadFromStringArray;

        public static bool RealFuel
        {
            get
            {
                if (RFChecked)
                    return realFuel;
                else
                {
                    RFChecked = true;
                    try
                    {
                        AssemblyLoader.LoadedAssembly rfAss = AssemblyLoader.loadedAssemblies.FirstOrDefault(q => q.assembly.GetName().Name == "RealFuels");
                        if (rfAss != null)
                            realFuel = true;
                    }
                    finally
                    {
                    }
                    return realFuel;
                }
            }
        }
        public static bool FAR
        {
            get
            {
                if (FARChecked)
                    return far;
                else
                {
                    FARChecked = true;
                    try
                    {
                        AssemblyLoader.LoadedAssembly farAss = AssemblyLoader.loadedAssemblies.FirstOrDefault(q => q.assembly.GetName().Name == "FerramAerospaceResearch");
                        if (farAss != null)
                            far = true;
                    }
                    finally
                    {
                    }
                    return far;
                }
            }
        }
        static CVSPConfigs()
        {
            CRFPConfigPath = Assembly.GetAssembly(typeof(CVSPConfigs)).Location;
            CRFPConfigPath = CRFPConfigPath.Remove(CRFPConfigPath.LastIndexOf("Plugins"));
            LoadCRFPSettings();
            LoadFromStringArray = typeof(ConfigNode).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(q => q.Name == "LoadFromStringArray" && q.GetParameters().Length < 2);
        }

        internal static List<TankTypeDefinition> TankDefinitions
        {
            get
            {
                if (tankDefinitions == null)
                {
                    tankDefinitions = LoadFuelTankDefinitions();
                    if (tankDefinitions == null)
                        tankDefinitions = new List<TankTypeDefinition>();
                }

                return tankDefinitions;
            }
        }
        internal static List<TextureDefinition> TextureDefinitions
        {
            get
            {
                if (textureDefinitions == null)
                {
                    textureDefinitions = LoadTextureDefinitions();
                    if (textureDefinitions == null)
                        textureDefinitions = new List<TextureDefinition>();
                }

                return textureDefinitions;
            }
        }
        internal static Dictionary<string, float> FuelAmountPerVolume
        {
            get
            {
                if (fuelAmountPerVolume == null)
                {
                    LoadAmountPerVolumeDefinitions();
                    if (fuelAmountPerVolume == null)
                        fuelAmountPerVolume = new Dictionary<string, float>();
                }

                return fuelAmountPerVolume;
            }
        }


        internal static void Reload()
        {
            List<ConfigNode> l = new List<ConfigNode>();
            UrlDir urlDir = new UrlDir(new UrlDir.ConfigDirectory[] { new UrlDir.ConfigDirectory("", "GameData", UrlDir.DirectoryType.GameData) },
                                       new UrlDir.ConfigFileType[] { new UrlDir.ConfigFileType(UrlDir.FileType.Config) });
            using IEnumerator<UrlDir.UrlConfig> enu = urlDir.GetConfigs("CRFPTankTypeDefinition").GetEnumerator();
            if (enu == null) return;
            while (enu.MoveNext())
            {
                UrlDir.UrlConfig current = enu.Current;
                l.Add(current.config);
            }
            enu.Dispose();
            tankDefinitions = LoadFuelTankDefinitions(l.ToArray());
            if (tankDefinitions == null)
                tankDefinitions = new List<TankTypeDefinition>();
            var tankTypeAbbrNames = new string[TankDefinitions.Count];
            for (int i = 0; i < TankDefinitions.Count; i++)
                tankTypeAbbrNames[i] = TankDefinitions[i].abbrName;
            CVSPUIManager.Instance.resources.Instance.RefreshItems(tankTypeAbbrNames);

            l.Clear();
            using IEnumerator<UrlDir.UrlConfig> enu3 = urlDir.GetConfigs("CRFPTextureDefinition").GetEnumerator();
            while (enu3.MoveNext())
            {
                UrlDir.UrlConfig current = enu3.Current;
                l.Add(current.config);
            }
            enu3.Dispose();
            textureDefinitions = LoadTextureDefinitions(l.ToArray());
            if (textureDefinitions != null && textureDefinitions.Count > 0)
            {
                TextureDefinitionSwitcher.DefaultItem = (textureDefinitions[0]);
                CVSPUIManager.Instance.sideTextures.RefreshItems(textureDefinitions.ToArray());
                CVSPUIManager.Instance.endsTextures.RefreshItems(textureDefinitions.ToArray());
            }

            l.Clear();
            using IEnumerator<UrlDir.UrlConfig> enu1 = urlDir.GetConfigs("CRFPFuelAmountPerVolumeDefinition").GetEnumerator();
            if (enu1 == null) return;
            while (enu1.MoveNext())
            {
                UrlDir.UrlConfig current = enu1.Current;
                l.Add(current.config);
            }
            enu1.Dispose();
            LoadAmountPerVolumeDefinitions(l.ToArray()[0]);
            if (fuelAmountPerVolume == null)
                fuelAmountPerVolume = new Dictionary<string, float>();

            l.Clear();
            using IEnumerator<UrlDir.UrlConfig> enu2 = urlDir.GetConfigs("CRFPSettings").GetEnumerator();
            if (enu2 == null) return;
            while (enu2.MoveNext())
            {
                UrlDir.UrlConfig current = enu2.Current;
                l.Add(current.config);
            }
            enu2.Dispose();
            LoadCRFPSettings(l.ToArray()[0]);
        }
        /// <summary>
        /// Not useable, confignode created by "LoadFromStringArray" or "Load" has always only one node,
        /// </summary>
        internal static void ReloadFast()
        {

            if (File.Exists(CRFPConfigPath + "CRFPTankTypeDefinition.cfg"))
            {
                StreamReader sr = new StreamReader(CRFPConfigPath + "CRFPTankTypeDefinition.cfg");
                if (sr != null)
                {
                    List<string> l = new List<string>();
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        l.Add(line);
                        // l.Add(string.Empty);
                    }

                    sr.Dispose();
                    if (l.Count > 1)
                    {
                        ConfigNode cfg = LoadFromStringArray.Invoke(null, new object[] { l.ToArray() }) as ConfigNode;
                        if (cfg != null)
                        {
                            tankDefinitions = LoadFuelTankDefinitions(new ConfigNode[] { cfg });
                            if (tankDefinitions == null)
                                tankDefinitions = new List<TankTypeDefinition>();
                            var tankTypeAbbrNames = new string[TankDefinitions.Count];
                            for (int i = 0; i < TankDefinitions.Count; i++)
                                tankTypeAbbrNames[i] = TankDefinitions[i].abbrName;
                            CVSPUIManager.Instance.resources.Instance.RefreshItems( tankTypeAbbrNames);
                        }
                    }
                }
            }
            if (File.Exists(CRFPConfigPath + "CRFPFuelAmountPerVolumeDefinition.cfg"))
            {
                ConfigNode cfg = ConfigNode.Load(CRFPConfigPath + "CRFPFuelAmountPerVolumeDefinition.cfg", false);
                if (cfg != null)
                {
                    LoadAmountPerVolumeDefinitions(cfg);
                    if (fuelAmountPerVolume == null)
                        fuelAmountPerVolume = new Dictionary<string, float>();
                }
            }
            if (File.Exists(CRFPConfigPath + "CRFPSettings.cfg"))
            {
                ConfigNode cfg = ConfigNode.Load(CRFPConfigPath + "CRFPSettings.cfg", false);
                if (cfg != null)
                    LoadCRFPSettings(cfg);
            }
        }

        private static List<TankTypeDefinition> LoadFuelTankDefinitions(ConfigNode[] cfg = null)
        {
            if (cfg == null)
                cfg = GameDatabase.Instance.GetConfigNodes("CRFPTankTypeDefinition");
            List<TankTypeDefinition> result = new List<TankTypeDefinition>();
            foreach (var config in cfg)
            {
                foreach (var tankType in config.GetNodes("TankType"))
                    if (tankType != null)
                    {
                        var name = tankType.GetValue("abbrName");
                        if (name == null) return null;
                        var title = tankType.GetValue("partTitle");
                        if (title == null) return null;
                        var resources = new List<string>();
                        var resourceRatio = new List<float>();
                        foreach (var res in tankType.GetNodes("Resource"))
                        {
                            var item = res.GetValue("type");
                            if (item != null)
                                resources.Add(item);
                            else return null;
                            if (ParseFloat(res, "ratio", out float pct))
                                resourceRatio.Add(pct);
                            else return null;
                        }
                        if (!ParseBool(tankType, "dryMassCalcByArea", out var dryMassCalcByArea)) return null;
                        if (!ParseFloat(tankType, "dryMassPerArea", out var dryMassPerArea)) return null;
                        if (!ParseFloat(tankType, "dryMassPerVolume", out var dryMassPerVolume)) return null;
                        if (!ParseFloat(tankType, "dryCostPerMass", out var dryCostPerMass)) return null;
                        result.Add(new TankTypeDefinition()
                        {
                            abbrName = name,
                            partTitle = title,
                            resources = resources,
                            resourceRatio = resourceRatio,
                            dryMassCalcByArea = dryMassCalcByArea,
                            dryMassPerArea = dryMassPerArea,
                            dryMassPerVolume = dryMassPerVolume,
                            dryCostPerMass = dryCostPerMass
                        });
                    }
            }
            return result;
        }

        private static void LoadAmountPerVolumeDefinitions(ConfigNode cfg = null)
        {
            if (cfg == null)
                cfg = GameDatabase.Instance.GetConfigNodes("CRFPFuelAmountPerVolumeDefinition")[0];
            if (cfg != null)
            {
                fuelAmountPerVolume = new Dictionary<string, float>();
                foreach (var item in cfg.GetNodes("Resource"))
                {
                    var name = item.GetValue("name");
                    if (!ParseFloat(item, "amountPerVolume", out var amountPerVol)) break;
                    fuelAmountPerVolume.Add(name, amountPerVol);
                }
            }
        }

        private static void LoadCRFPSettings(ConfigNode cfg = null)
        {
            if (cfg == null)
                cfg = GameDatabase.Instance.GetConfigNodes("CRFPSettings")[0];
            if (cfg == null)
                Debug.LogError("[CRFP] Can't load CRFPSetting.cfg");
            var keyStr = cfg.GetValue("EditorToolToggle");
            if (!Enum.TryParse<KeyCode>(keyStr, out var key))
            {
                Debug.LogError("[CRFP] Can't load EditorToolToggle from CRFPSetting.cfg");
                return;
            }
            CVSPEditorTool.ToggleKey = key;

            var createdType = cfg.GetValue("CreatorDefaultTankType");
            if (createdType == null)
            {
                Debug.LogError("[CRFP] Can't load CreatorDefaultTankType from CRFPSetting.cfg");
                return;
            }
            CVSPEditorTool.CreatorDefaultTankType = createdType;

            var factor = cfg.GetValue("RealFuelVolumeFactor");
            if (factor == null || (!double.TryParse(factor, out RealFuelVolumeFactor)))
            {
                Debug.LogError("[CRFP] Can't load RealFuelVolumeFactor from CRFPSetting.cfg");
                return;
            }

            var fullundo = cfg.GetValue("FullUndoAndRedo");
            if (fullundo == null || (!bool.TryParse(fullundo, out ModuleCarnationVariablePart.FullUndoAndRedo)))
            {
                ModuleCarnationVariablePart.FullUndoAndRedo = true;
                Debug.LogError("[CRFP] Can't load FullUndoAndRedo from CRFPSetting.cfg");
                return;
            }
        }

        private static List<TextureDefinition> LoadTextureDefinitions(ConfigNode[] cfg = null)
        {
            if (cfg == null)
                cfg = GameDatabase.Instance.GetConfigNodes("CRFPTextureDefinition");
            List<TextureDefinition> result = new List<TextureDefinition>();
            foreach (var config in cfg)
                foreach (var texDefNode in config.GetNodes("TextureDefinition"))
                    if (texDefNode != null)
                    {
                        var texDef = new TextureDefinition();
                        if (!texDefNode.TryGetValue("name", ref texDef.name) ||
                            !texDefNode.TryGetValue("directory", ref texDef.directory) ||
                            !texDefNode.TryGetValue("diffuse", ref texDef.diffuse) ||
                            !texDefNode.TryGetValue("normals", ref texDef.normals) ||
                            !texDefNode.TryGetValue("specular", ref texDef.specular)
                            )
                        {
                            Debug.LogError($"[CRFP] Can't load TextureDefinition {texDefNode.name}");
                            continue;
                        }

                        GameDatabase.TextureInfo tex = LoadTextureFromDatabase(texDef.directory, texDef.diffuse, asNormal: false);
                        if (tex == null)
                        {
                            Debug.LogError($"[CRFP] Texture {texDef.diffuse} isn't load by the game");
                            continue;
                        }
                        texDef.diff = tex.texture;

                        tex = LoadTextureFromDatabase(texDef.directory, texDef.normals, asNormal: true);
                        if (tex == null)
                        {
                            Debug.LogError($"[CRFP] Texture {texDef.normals} isn't load by the game");
                            continue;
                        }
                        texDef.norm = tex.texture;

                        tex = LoadTextureFromDatabase(texDef.directory, texDef.specular, asNormal: false);
                        if (tex == null)
                        {
                            Debug.LogError($"[CRFP] Texture {texDef.specular} isn't load by the game");
                            continue;
                        }
                        texDef.spec = tex.texture;

                        result.Add(texDef);
                    }

            return result;
        }

        private static GameDatabase.TextureInfo LoadTextureFromDatabase(string directory, string texname, bool asNormal)
        {
            string path = "CarnationREDFlexiblePart/Texture/" + (directory.Length > 0 ? (directory + '/') : "") + Path.GetFileNameWithoutExtension(texname);
            string extension = Path.GetExtension(texname).Substring(1);
            var tex = GameDatabase.Instance.databaseTexture
                .FirstOrDefault(q=> q.name.IndexOf(path) >= 0 && q.file.fileExtension.Equals(extension));
            if (tex != null)
            {
                // Idk y but isNormalMap is always false, and I can't stand conversion for every normalmaps, so I don't convert
                if (!tex.isNormalMap && asNormal)
                {
                    // Debug.LogError($"[CRFP] Please convert {texname} to DXT5_NRM");
                    Texture2D n = null;
                    n = tex.texture;
                    // Idk y but isNormalMap is always false, and I won't allow conversion of every normalmaps, so I don't convert
                    //if (tex.file.fileExtension == ".dds")
                    //    n = CVSPUIManager.ConvertToNormalMap_DDS(tex.texture);
                    //else if (tex.file.fileExtension == ".png")
                    //    n = CVSPUIManager.ConvertToNormalMap_PNG(tex.texture);
                    if (n)
                    {
                        tex.texture = n;
                        tex.isNormalMap = true;
                    }
                }
            }
            return tex;
        }

        private static bool ParseFloat(ConfigNode cfg, string s, out float result)
        {
            var r = cfg.GetValue(s);
            if (r == null)
            {
                result = 0;
                Debug.LogError($"\"{s}\" not found in {cfg.name}");
                return false;
            }
            if (float.TryParse(r, out result))
                return true;
            else
            {
                Debug.LogError($"Can't parse {s} in {cfg.name}");
                return false;
            }
        }
        private static bool ParseBool(ConfigNode cfg, string s, out bool result)
        {
            var r = cfg.GetValue(s);
            if (r == null)
            {
                result = false;
                Debug.LogError($"\"{s}\" not found in {cfg.name}");
                return false;
            }
            if (bool.TryParse(r, out result))
                return true;
            else
            {
                Debug.LogError($"Can't parse {s} in {cfg.name}");
                return false;
            }
        }
    }

    internal class TankTypeDefinition
    {
        internal string abbrName;
        internal string partTitle;
        internal List<string> resources;
        internal List<float> resourceRatio;
        internal bool dryMassCalcByArea;
        internal float dryMassPerArea;
        internal float dryMassPerVolume;
        internal float dryCostPerMass;
    }
}