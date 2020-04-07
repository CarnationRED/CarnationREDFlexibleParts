using RealFuels.Tanks;
using UnityEngine;

namespace CarnationVariableSectionPart
{
    internal class RFAPI
    {
        internal static void RF_UpdateVolume(ModuleCarnationVariablePart cvsp, float totalVolume)
        {
            var mft = cvsp.part.FindModuleImplementing<ModuleFuelTanks>();
            if (mft == null)
            {
                Debug.LogError("[CRFP] RF's component ModuleFuelTanks is not found on the part");
                return;
            }
            mft.ChangeTotalVolume(totalVolume * CVSPConfigs.RealFuelVolumeFactor);
        }
    }
}
