using System.Collections.Generic;

namespace CarnationVariableSectionPart
{
    internal class TankTypeDefinition
    {
        internal string abbrName;
        internal string partTitle;
        internal List<string> resources;
        internal List<float> resourceRatio;
        internal float dryMassCalcCoeff;
        internal float dryMassPerArea;
        internal float dryMassPerVolume;
        internal float dryCostPerMass;
    }
}