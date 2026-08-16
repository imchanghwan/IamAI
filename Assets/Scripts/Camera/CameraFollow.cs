using UnityEngine;

namespace Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private float  smoothSpeed = 8f;
        [SerializeField] private Vector3 offset     = new(0f, 0f, -10f);

        private Transform _target;

        public void SetTarget(Transform target) => _target = target;

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desired = _target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        }
    }
}