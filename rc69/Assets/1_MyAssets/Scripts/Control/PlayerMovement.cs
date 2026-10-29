using Raccoon.InputCtr;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Raccoon.Player
{

    public class PlayerMovement : MonoBehaviour
    {
        float multiSpeedRun = 0f;
        public float MultiSpeedRun => multiSpeedRun + 1;

        float multiJumpForce = 0f;

        [FormerlySerializedAs("m_JumpPower")]
        public float jumpPower = 12f;
        [FormerlySerializedAs("m_MovingTurnSpeed")]
        public float movingTurnSpeed = 360;
        [FormerlySerializedAs("m_StationaryTurnSpeed")]
        public float stationaryTurnSpeed = 180;
        [Tooltip("Player is on ground.")]
        public bool isGrounded;
        public LayerMask layerCheckGround;
        [FormerlySerializedAs("m_GravityMultiplier")]
        [Range(1f, 4f)] public float gravityMultiplier = 2f;

        [FormerlySerializedAs("m_GroundCheckDistance")]
        public float groundCheckDistance = 0.1f;
        [FormerlySerializedAs("m_ClimbSpeed")]
        public float climbSpeed = 1f;
        public float capsuleRadius = 1;

        [Header("World Limits")]
        public bool limitBoundsEnabled;
        public Bounds limitBounds;


        public float timeDelayJump = 0.1f;
        float currentDelayJump = 0;

        Rigidbody rb;
        PlayerInput input;
        Vector3 moveDir, camForward;

        Vector3 dirForward;
        Vector3 dirRight;
        bool isMoving, isRunning;


        float turnAmount;
        float forwardAmount;
        float origGroundCheckDistance;

        [SerializeField]UnityEvent<float, float, bool> actionAnimMove;
        [SerializeField]UnityEvent<float> actionMultiSpeedRun;
        [SerializeField]UnityEvent jumpedAction;
        [SerializeField]UnityEvent jumpAction;

        public bool canMove = true;
        private void Start()
        {
            canMove = true;
            input = new PlayerInput();
            origGroundCheckDistance = groundCheckDistance;

        }

        public void Init(Rigidbody rg, CapsuleCollider capsule)
        {
            this.rb = rg;
            capsuleRadius = capsule.radius * transform.lossyScale.x;
        }

        public void SetMultiSpeedRun(float set)
        {
            multiSpeedRun = set;
        }

        public void SetMultiJumpForce(float set)
        {
            multiJumpForce = set;
        }
        public void SetCanMove(bool canmove)
        {
            canMove = canmove;
        }
        public void SetInput(PlayerInput input)
        {
            this.input = input;

        }
        private void Update()
        {
            actionAnimMove?.Invoke(forwardAmount, turnAmount, isGrounded);
        }
        private void FixedUpdate()
        {
            CountCurrentJump();
            FixedUpdateImpl();
        }
        public void SetDirFoward(Vector3 dirForward)
        {
            this.dirForward = dirForward;
        }
        public void SetDirRight(Vector3 dirRight)
        {
            this.dirRight = dirRight;
        }
        protected virtual void FixedUpdateImpl()
        {
            float h = input.horizontalAxis;
            float v = input.verticalAxis;
            if (!canMove)
            {
                h = v = 0;
                input.isJump = false;
                forwardAmount = 0;
                turnAmount = 0;
                rb.linearVelocity = Vector3.zero;
                return;
            }
            camForward = Vector3.Scale(dirForward, new Vector3(1, 0, 1)).normalized;
            moveDir = v * camForward + h * dirRight;


            if (moveDir.sqrMagnitude > 0)
            {
                moveDir.Normalize();
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }
            isRunning = input.isRunning;

            float speed = 1;
            if (isMoving)
            {

                if (isRunning)
                {
                    speed += multiSpeedRun / 10f;
                }

                //if (isGrounded)
                //{
                //    Vector3 dir = moveDir.normalized;
                //    var apply = dir * climbSpeed * speed;
                //    apply.y = rb.linearVelocity.y;
                //    rb.linearVelocity = apply;
                //}
                actionMultiSpeedRun?.Invoke(Mathf.Clamp(speed/2,0.75f,speed));
                Vector3 dir = moveDir.normalized;
                var apply = dir * climbSpeed * speed;
                apply.y = rb.linearVelocity.y;
                rb.linearVelocity = apply;
            }
            else
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
            // pass all parameters to the character control script
            Move(moveDir);
            Jump();
        }
        void Jump()
        {
            if (isGrounded && currentDelayJump <= 0)
            {
                HandleGroundedMovement(input.isJump);
            }
            else
            {
                HandleAirborneMovement();
            }
            input.isJump = false;
        }

        void CountCurrentJump()
        {
            if (isGrounded && currentDelayJump > 0) currentDelayJump -= Time.fixedDeltaTime;
        }

        public virtual void Move(Vector3 move)
        {
            if (move.magnitude > 1f)
                move.Normalize();
            move = transform.InverseTransformDirection(move);
            CheckGroundStatus();


            move = Vector3.ProjectOnPlane(move, Vector3.up);
            turnAmount = Mathf.Atan2(move.x, move.z);
            forwardAmount = move.z;


            Vector3 curPos = rb.position;

            ApplyExtraTurnRotation();


            // Check limits
            if (limitBoundsEnabled && !limitBounds.Contains(rb.position))
            {
                rb.position = curPos;
            }
        }

        protected virtual void CheckGroundStatus()
        {
            if (GroundCheck())
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
        }
        protected virtual bool GroundCheck()
        {
            // 0.1f is a small offset to start the ray from inside the character
            // it is also good to note that the transform position in the sample assets is at the base of the character
            Vector3 pos = transform.position + (Vector3.up * 0.1f * transform.lossyScale.y);
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, groundCheckDistance, layerCheckGround))
            {
                return true;
            }
            pos.x += capsuleRadius;
            pos.z += capsuleRadius;
#if UNITY_EDITOR
            // heelper to visualise the ground check ray in the scne view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, groundCheckDistance, layerCheckGround))
                return true;
            pos.z -= capsuleRadius * 2f;
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, groundCheckDistance, layerCheckGround))
                return true;
            pos.x -= capsuleRadius * 2f;
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, groundCheckDistance, layerCheckGround))
                return true;
            pos.z += capsuleRadius * 2f;
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, groundCheckDistance, layerCheckGround))
                return true;

            return false;
        }

        protected virtual void ApplyExtraTurnRotation()
        {
            float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
            transform.Rotate(0, turnAmount * turnSpeed * climbSpeed * Time.deltaTime, 0);
        }

        protected virtual void HandleGroundedMovement(bool jump)
        {
            if (jump)
            {
                currentDelayJump = timeDelayJump;
                float scaleJump = 1;
                if (isRunning)
                {
                    scaleJump += multiJumpForce / 10f;
                }
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpPower * scaleJump, rb.linearVelocity.z);
                isGrounded = false;

                groundCheckDistance = 0.1f;
                jumpAction?.Invoke();
            }
        }
        protected virtual void HandleAirborneMovement()
        {
            // apply extra gravity from multiplier:


            //Vector3 extraGravityForce = (Physics.gravity * gravityMultiplier) - Physics.gravity;
            //rb.AddForce(extraGravityForce);

            rb.linearVelocity += Vector3.up *
                    Physics.gravity.y *
                    (gravityMultiplier) *
                    Time.fixedDeltaTime;
            groundCheckDistance = rb.linearVelocity.y < 0 ? origGroundCheckDistance : 0.1f;
        }
    }
}