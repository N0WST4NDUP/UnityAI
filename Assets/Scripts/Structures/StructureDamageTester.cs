using UnityEngine;

namespace UnityAI.Structures
{
    public class StructureDamageTester : MonoBehaviour
    {
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(1)) return;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;

            hit.collider.TryGetComponent<Structure>(out var structure);

            structure.TakeDamage(1);
            Debug.Log("데미지!");
        }
    }
}
