using Fusion;
using Camera;
using Input;
using UnityEngine;

namespace Player
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("속도")]
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float dashSpeed = 4.2f;

        [Header("스태미나")]
        [SerializeField] private float maxStamina      = 100f;
        [SerializeField] private float dashDrain       = 40f;   // /sec
        [SerializeField] private float staminaRecovery = 25f;   // /sec
        [SerializeField] private float exhaustDuration = 2f;

        [Networked] public  float Stamina        { get; private set; }
        [Networked] private bool  IsExhausted    { get; set; }
        [Networked] private float ExhaustedTimer { get; set; }

        private Rigidbody2D _rb;

        public override void Spawned()
        {
            _rb = GetComponent<Rigidbody2D>();
            Stamina = maxStamina;

            if (HasInputAuthority)
                UnityEngine.Camera.main?.GetComponent<CameraFollow>()?.SetTarget(transform);
        }

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out NetworkInputData input)) return;

            float dt = Runner.DeltaTime;
            UpdateStamina(input.IsDashing, dt);
            Move(input.MoveDirection, input.IsDashing, dt);
        }

        private void UpdateStamina(bool dashInput, float dt)
        {
            if (IsExhausted)
            {
                ExhaustedTimer -= dt;
                if (ExhaustedTimer <= 0f) IsExhausted = false;
                return;
            }

            if (!dashInput)
                Stamina = Mathf.Min(maxStamina, Stamina + staminaRecovery * dt);
        }

        private void Move(Vector2 direction, bool dashInput, float dt)
        {
            bool isDashing = dashInput && !IsExhausted && Stamina > 0f;

            if (isDashing)
            {
                Stamina -= dashDrain * dt;
                if (Stamina <= 0f)
                {
                    Stamina        = 0f;
                    IsExhausted    = true;
                    ExhaustedTimer = exhaustDuration;
                    isDashing      = false;
                }
            }

            if (direction.sqrMagnitude > 0f)
            {
                float targetSpeed = isDashing ? dashSpeed : moveSpeed;
                _rb.linearVelocity = direction.normalized * targetSpeed;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            // 입력 없을 때는 Linear Drag가 자연스럽게 감속 처리
        }
    }
}