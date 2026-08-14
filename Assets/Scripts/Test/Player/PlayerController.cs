using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D _rb;

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (HasInputAuthority)
            Camera.main?.GetComponent<CameraFollow>()?.SetTarget(transform);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            _rb.MovePosition(_rb.position + input.MoveDirection * speed * Runner.DeltaTime);
        }
    }
}