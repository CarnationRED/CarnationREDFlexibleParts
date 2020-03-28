using UnityEngine;

namespace ProceduralArmor
{
    public class Drag : MonoBehaviour
    {
        internal bool mouseOver;
        private void OnMouseDrag()
        {
            Plane plane = new Plane(transform.forward, transform.position);

            Ray ray = ShapeControl.EditorCamera.ScreenPointToRay(Input.mousePosition);

            Debug.DrawRay(ray.origin, ray.direction, Color.red);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                transform.position = hitPoint;
                ShapeControl.editingHex.regenMesh = true;
            }
        }
        private void OnMouseOver() => mouseOver = true;
        private void OnMouseExit() => mouseOver = false;
        private void OnMouseEnter() => mouseOver = true;
    }
}
