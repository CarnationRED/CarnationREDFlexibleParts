using CarnationVariableSectionPart.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CarnationVariableSectionPart
{
    internal class CVSPConfigs
    {
        private static List<TankTypeDefinition> tankDefinitions;
        private static List<TextureSetDefinition> textureSets;
        private static Dictionary<string, float> fuelAmountPerVolume;
        private static readonly string CRFPConfigPath;
        private static bool realFuel = false;
        private static bool RFChecked = false;
        private static bool far = false;
        private static bool FARChecked = false;
        internal static double RealFuelVolumeFactor = 765.056629974;
        private static readonly MethodInfo LoadFromStringArray;
        private static List<SectionCorner> sectionCornerTypes;
        private static List<TechLimit> techLimits;
        private static float maxSize = -1;
        private static float maxLength = -1;

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

        public static float MaxSize
        {
            get
            {
                if (maxSize < 0f)
                {
                    if (techLimits == null)
                    {
                        techLimits = LoadTechLimits();
                        var arr = techLimits.ToArray();
                        Array.Sort(arr);
                        techLimits = new List<TechLimit>(arr);
                    }
                    if (ResearchAndDevelopment.Instance == null) new Thread(WaitForRnDInit).Start();
                    int i = 0;
                    for (; i < techLimits.Count; i++)
                    {
                        TechLimit t = techLimits[i];
                        RDTech.State state = ResearchAndDevelopment.GetTechnologyState(t.level);
                        if (state != RDTech.State.Available)
                        {
                            TechLimit techLimit = techLimits[Mathf.Max(i - 1, 0)];
                            maxSize = techLimit.maxSize;
                            maxLength = techLimit.maxLength;

                            if (ModuleCarnationVariablePart.partInfo != null) ModuleCarnationVariablePart.partInfo.TechRequired = techLimit.level;
                            else ModuleCarnationVariablePart.TechRequired = techLimit.level;
                            foreach (var p in from p in PartLoader.Instance.loadedParts
                                              where p.name.StartsWith("CarnationREDFlexible")
                                              select p)
                                p.TechRequired = techLimit.level;

                            break;
                        }
                    }
                    if (i == techLimits.Count)
                    {
                        TechLimit techLimit = techLimits[Mathf.Max(i - 1, 0)];
                        maxSize = techLimit.maxSize;
                        maxLength = techLimit.maxLength;
                        if (ModuleCarnationVariablePart.partInfo != null) ModuleCarnationVariablePart.partInfo.TechRequired = techLimit.level;
                    }
                    maxSize = Mathf.Max(0.625f, maxSize);
                }
                return maxSize;
            }
        }
        //internal static event OnTechLimitLoadedHandler OnTechLimitLoaded;
        //internal delegate void OnTechLimitLoadedHandler();
        private static void WaitForRnDInit()
        {
            while (ResearchAndDevelopment.Instance == null)
                Thread.Sleep(250);
            maxSize = -1;
            _ = MaxSize;
            //if (HighLogic.LoadedSceneIsEditor && OnTechLimitLoaded != null)
            //{
            //    OnTechLimitLoaded.Invoke();
            //}
        }

        public static float MaxLength
        {
            get
            {
                if (maxLength < 0f)
                {
                    _ = MaxSize;
                    maxLength = Mathf.Max(1.25f, maxLength);
                }
                return maxLength;
            }
        }
        static CVSPConfigs()
        {
            CRFPConfigPath = Assembly.GetAssembly(typeof(CVSPConfigs)).Location;
            CRFPConfigPath = CRFPConfigPath.Remove(CRFPConfigPath.LastIndexOf("Plugins"));
            LoadCRFPSettings();
            //LoadFromStringArray = typeof(ConfigNode).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(q => q.Name == "LoadFromStringArray" && q.GetParameters().Length < 2);
            GameEvents.onGameSceneSwitchRequested.Add((GameEvents.FromToAction<GameScenes, GameScenes> data) =>
                {
                    //OnTechLimitLoaded = null;
                    maxSize = -1;
                    _ = MaxSize;
                });
        }
        internal static List<TankTypeDefinition> TankDefinitions
            => tankDefinitions ?? (tankDefinitions = LoadFuelTankDefinitions() ?? new List<TankTypeDefinition>());
        internal static List<TextureSetDefinition> TextureSets
            => textureSets ?? (textureSets = LoadTextureSetsDefinitions() ?? new List<TextureSetDefinition>());
        internal static List<SectionCorner> SectionCornerDefinitions
            => sectionCornerTypes ?? (sectionCornerTypes = LoadSectionCornerDefinitions() ?? new List<SectionCorner>());
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
            UrlDir urlDir = new UrlDir(new UrlDir.ConfigDirectory[] { new UrlDir.ConfigDirectory("", "GameData", UrlDir.DirectoryType.GameData) },
                                       new UrlDir.ConfigFileType[] { new UrlDir.ConfigFileType(UrlDir.FileType.Config) });

            tankDefinitions = LoadFuelTankDefinitions(GetNodes(urlDir, "CRFPTankTypeDefinition").ToArray());
            if (tankDefinitions == null)
                tankDefinitions = new List<TankTypeDefinition>();
            var tankTypeAbbrNames = new string[TankDefinitions.Count];
            for (int i = 0; i < TankDefinitions.Count; i++)
                tankTypeAbbrNames[i] = TankDefinitions[i].abbrName;
            CVSPUIManager.Instance.resources.Instance.RefreshItems(tankTypeAbbrNames);

            textureSets = LoadTextureSetsDefinitions(GetNodes(urlDir, "CRFPTextureDefinition").ToArray());
            if (textureSets != null && textureSets.Count > 0)
            {
                TextureDefinitionSwitcher.DefaultItem = (textureSets[0]);
                CVSPUIManager.Instance.sideTextures.RefreshItems(textureSets.ToArray());
                CVSPUIManager.Instance.endsTextures.RefreshItems(textureSets.ToArray());
            }


            LoadAmountPerVolumeDefinitions(GetNodes(urlDir, "CRFPFuelAmountPerVolumeDefinition").ToArray()[0]);
            if (fuelAmountPerVolume == null)
                fuelAmountPerVolume = new Dictionary<string, float>();

            LoadCRFPSettings(GetNodes(urlDir, "CRFPSettings").ToArray()[0]);

            LoadSectionCornerDefinitions(GetNodes(urlDir, "CRFPSectionCornerDefinitions").ToArray());

            techLimits = LoadTechLimits(GetNodes(urlDir, "CRFPTechLimits").ToArray());
        }

        private static List<ConfigNode> GetNodes(UrlDir urlDir, string TypeName)
        {
            List<ConfigNode> l = new List<ConfigNode>();
            using (IEnumerator<UrlDir.UrlConfig> enu = urlDir.GetConfigs(TypeName).GetEnumerator())
            {
                if (enu != null)
                    while (enu.MoveNext())
                    {
                        UrlDir.UrlConfig current = enu.Current;
                        l.Add(current.config);
                    }
            };
            return l;
        }

        /// <summary>
        /// Not useable, confignodes created by "LoadFromStringArray" or "Load" has always only one node,
        /// </summary>
        internal static void ReloadFast()
        {

            if (File.Exists(CRFPConfigPath + "Config" + Path.DirectorySeparatorChar + "CRFPTankTypeDefinition.cfg"))
            {
                StreamReader sr = new StreamReader(CRFPConfigPath + "Config" + Path.DirectorySeparatorChar + "CRFPTankTypeDefinition.cfg");
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
                        if (LoadFromStringArray.Invoke(null, new object[] { l.ToArray() }) is ConfigNode cfg)
                        {
                            tankDefinitions = LoadFuelTankDefinitions(new ConfigNode[] { cfg });
                            if (tankDefinitions == null)
                                tankDefinitions = new List<TankTypeDefinition>();
                            var tankTypeAbbrNames = new string[TankDefinitions.Count];
                            for (int i = 0; i < TankDefinitions.Count; i++)
                                tankTypeAbbrNames[i] = TankDefinitions[i].abbrName;
                            CVSPUIManager.Instance.resources.Instance.RefreshItems(tankTypeAbbrNames);
                        }
                    }
                }
            }
            if (File.Exists(CRFPConfigPath + "Config" + Path.DirectorySeparatorChar + "CRFPFuelAmountPerVolumeDefinition.cfg"))
            {
                ConfigNode cfg = ConfigNode.Load(CRFPConfigPath + "Config" + Path.DirectorySeparatorChar + "CRFPFuelAmountPerVolumeDefinition.cfg", false);
                if (cfg != null)
                {
                    LoadAmountPerVolumeDefinitions(cfg);
                    if (fuelAmountPerVolume == null)
                        fuelAmountPerVolume = new Dictionary<string, float>();
                }
            }
            if (File.Exists(CRFPConfigPath + "Config" + Path.DirectorySeparatorChar + "CRFPSettings.cfg"))
            {
                ConfigNode cfg = ConfigNode.Load(CRFPConfigPath + "Config" + Path.DirectorySeparatorChar + "CRFPSettings.cfg", false);
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
                        if (!ParseFloat(tankType, "dryMassCalcCoeff", out var dryMassCalcCoeff)) return null;
                        if (!ParseFloat(tankType, "dryMassPerArea", out var dryMassPerArea)) return null;
                        if (!ParseFloat(tankType, "dryMassPerVolume", out var dryMassPerVolume)) return null;
                        if (!ParseFloat(tankType, "dryCostPerMass", out var dryCostPerMass)) return null;
                        result.Add(new TankTypeDefinition()
                        {
                            abbrName = name,
                            partTitle = title,
                            resources = resources,
                            resourceRatio = resourceRatio,
                            dryMassCalcCoeff = dryMassCalcCoeff,
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

        private static List<TextureSetDefinition> LoadTextureSetsDefinitions(ConfigNode[] cfg = null)
        {
            if (cfg == null)
                cfg = GameDatabase.Instance.GetConfigNodes("CRFPTextureDefinition");
            List<TextureSetDefinition> result = new List<TextureSetDefinition>();
            foreach (var config in cfg)
                foreach (var texDefNode in config.GetNodes("TextureDefinition"))
                    if (texDefNode != null)
                    {
                        var texDef = new TextureSetDefinition();
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

        private static List<SectionCorner> LoadSectionCornerDefinitions(ConfigNode[] cfg = null)
        {
            if (cfg == null)
                cfg = GameDatabase.Instance.GetConfigNodes("CRFPSectionCornerDefinitions");
            List<SectionCorner> result = new List<SectionCorner>();
            foreach (var config in cfg)
                foreach (var secDefNode in config.GetNodes("SectionCorner"))
                    if (secDefNode != null)
                    {
                        var secDef = new SectionCorner
                        {
                            vertices = new Vector2[7]
                        };
                        if (!secDefNode.TryGetValue("name", ref secDef.name) ||
                            !secDefNode.TryGetValue("vertex1", ref secDef.vertices[0]) ||
                            !secDefNode.TryGetValue("vertex2", ref secDef.vertices[1]) ||
                            !secDefNode.TryGetValue("vertex3", ref secDef.vertices[2]) ||
                            !secDefNode.TryGetValue("vertex4", ref secDef.vertices[3]) ||
                            !secDefNode.TryGetValue("vertex5", ref secDef.vertices[4]) ||
                            !secDefNode.TryGetValue("vertex6", ref secDef.vertices[5]) ||
                            !secDefNode.TryGetValue("vertex7", ref secDef.vertices[6]) ||
                            !secDefNode.TryGetValue("cornerPerimeter", ref secDef.cornerPerimeter) ||
                            !secDefNode.TryGetValue("cornerArea", ref secDef.cornerArea)
                            )
                        {
                            Debug.LogError($"[CRFP] Can't load SectionCornerDefinition {secDefNode.name}");
                            continue;
                        }
                        result.Add(secDef);
                    }

            return result;
        }

        private static List<TechLimit> LoadTechLimits(ConfigNode[] cfg = null)
        {
            if (cfg == null)
                cfg = GameDatabase.Instance.GetConfigNodes("CRFPTechLimits");
            List<TechLimit> result = new List<TechLimit>();
            foreach (var config in cfg)
                foreach (var techNode in config.GetNodes("TechLimit"))
                    if (techNode != null)
                    {
                        var tech = new TechLimit();
                        if (!techNode.TryGetValue("level", ref tech.level) ||
                            !techNode.TryGetValue("maxSize", ref tech.maxSize) ||
                            !techNode.TryGetValue("maxLength", ref tech.maxLength)
                            )
                        {
                            Debug.LogError($"[CRFP] Can't load TechLimits {techNode.name}");
                            continue;
                        }
                        result.Add(tech);
                    }

            return result;
        }

        private static GameDatabase.TextureInfo LoadTextureFromDatabase(string directory, string texname, bool asNormal)
        {
            string path = "CarnationREDFlexiblePart/Texture/" + (directory.Length > 0 ? (directory + '/') : "") + Path.GetFileNameWithoutExtension(texname);
            string extension = Path.GetExtension(texname).Substring(1);
            var tex = GameDatabase.Instance.databaseTexture
                .FirstOrDefault(q => q.name.IndexOf(path) >= 0 && q.file.fileExtension.Equals(extension));
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
    internal struct TechLimit : IComparable
    {
        public string level;
        public float maxSize;
        public float maxLength;

        public int CompareTo(object obj) => maxSize.CompareTo(((TechLimit)obj).maxSize);
    }
}