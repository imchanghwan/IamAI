using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float  _smoothSpeed = 8f;
    [SerializeField] private Vector3 _offset     = new(0f, 0f, -10f);

    private Transform _target;

    public void SetTarget(Transform target) => _target = target;

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 desired = _target.position + _offset;
        transform.position = Vector3.Lerp(transform.position, desired, _smoothSpeed * Time.deltaTime);
    }
}