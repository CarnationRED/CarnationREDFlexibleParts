using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarnationVariableSectionPart
{
    internal class CVSPTankTypeDefinitionList
    {
        private static List<CVSPTankTypeDefinition> tankDefinitions;

        internal static List<CVSPTankTypeDefinition> TankDefinitions
        {
            get
            {
                if (tankDefinitions == null)
                {
                    tankDefinitions = LoadDefinitions();
                    if (tankDefinitions == null)
                        tankDefinitions = new List<CVSPTankTypeDefinition>();
                }

                return tankDefinitions;
            }
        }
        internal static void Reload()
        {
            tankDefinitions = LoadDefinitions();
            if (tankDefinitions == null)
                tankDefinitions = new List<CVSPTankTypeDefinition>();
        }

        private static List<CVSPTankTypeDefinition> LoadDefinitions()
        {
            List<CVSPTankTypeDefinition> result = new List<CVSPTankTypeDefinition>();
            foreach (var config in GameDatabase.Instance.GetConfigNodes("CVSPTankTypeDefinition"))
            {
                var tankType = config.GetNode("TankType");
                if (tankType != null)
                {
                    var name = tankType.GetValue("name");
                    var resources = new List<string>();
                    var resourcePct = new List<float>();
                    foreach (var res in tankType.GetNodes("Resource"))
                    {
                        var item = res.GetValue("type");
                        if (item != null)
                            resources.Add(item);
                        else return null;
                        if (ParseFloat(res, "percentage", out float pct))
                            resourcePct.Add(pct);
                        else return null;
                    }
                    if (!ParseBool(tankType, "dryMassCalcByArea", out var dryMassCalcByArea)) return null;
                    if (!ParseFloat(tankType, "dryMassPerArea", out var dryMassPerArea)) return null;
                    if (!ParseFloat(tankType, "dryMassPerVolume", out var dryMassPerVolume)) return null;
                    if (!ParseFloat(tankType, "dryCostPerMass", out var dryCostPerMass)) return null;
                    result.Add(new CVSPTankTypeDefinition()
                    {
                        abbrName = name,
                        resources = resources,
                        resourcePct = resourcePct,
                        dryMassCalcByArea = dryMassCalcByArea,
                        dryMassPerArea = dryMassPerArea,
                        dryMassPerVolume = dryMassPerVolume,
                        dryCostPerMass = dryCostPerMass
                    });
                }
            }
            return result;
        }

        private static bool ParseFloat(ConfigNode cfg, string s, out float result)
        {
            var r = cfg.GetValue(s);
            if (r == null)
            {
                result = 0;
                Debug.LogError($"Can't parse {s} in {cfg.name}");
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
                Debug.LogError($"Can't parse {s} in {cfg.name}");
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

    internal class CVSPTankTypeDefinition
    {
        internal string abbrName;
        internal List<string> resources;
        internal List<float> resourcePct;
        internal bool dryMassCalcByArea;
        internal float dryMassPerArea;
        internal float dryMassPerVolume;
        internal float dryCostPerMass;
    }
}