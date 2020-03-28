using FerramAerospaceResearch.FARGUI.FAREditorGUI;
using FerramAerospaceResearch.FARPartGeometry;
using System.Reflection;

namespace CarnationVariableSectionPart
{
    internal class FARAPI
    {
        private static MethodInfo RebuildAllMeshData;
        internal static void FAR_UpdateCollider(ModuleCarnationVariablePart cvsp)
        {
            //EditorGUI.RequestUpdateVoxel();
            var far =  cvsp.part.FindModuleImplementing<GeometryPartModule>();
            if (!far) return;
            if (RebuildAllMeshData == null)
                RebuildAllMeshData = typeof(GeometryPartModule).GetMethod("RebuildAllMeshData", BindingFlags.Instance | BindingFlags.NonPublic);
            RebuildAllMeshData.Invoke(far, null);
        }
    }
}
