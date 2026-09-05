using UnityEngine;
using UnityEngine.Events;

namespace Raccoon.Player
{

    public class PlayerAnim : MonoBehaviour
    {

        public Animator animator;
        public Rigidbody rb;
        PlayerHandleAnimEvent handleAnimEvent;

        [SerializeField] float timeAttack;
        [SerializeField] float currentTimeAttack;
        [SerializeField] float attackSpeed = 1;
        [SerializeField] bool attacking;
        [SerializeField] bool isHoldBrainrot;
        public bool inited = false;

        public UnityAction ActionDoneAttack;
        public void Init(Rigidbody rb)
        {
            inited = true;
            this.rb = rb;

            attacking = false;
            isHoldBrainrot = false;
            attackSpeed = 1;


        }

        private void Update()
        {
            if (attacking)
            {
                currentTimeAttack += Time.deltaTime;
                if (currentTimeAttack >= timeAttack / attackSpeed)
                {
                    DoneAnimAttack();
                }
            }
        }
        private void SetFloatAnimator(string id, params float[] param)
        {
            if (param.Length == 1)
            {
                animator.SetFloat(id, param[0]);
                animator.SetFloat(id, param[0]);
            }
            if (param.Length == 3)
            {
                animator.SetFloat(id, param[0], param[1], param[2]);
                animator.SetFloat(id, param[0], param[1], param[2]);
            }
        }

        public void SetBoolAnimator(string id, bool param)
        {
            animator.SetBool(id, param);
        }

        public virtual void UpdateAnimatorMove(float forwardAmount, float turnAmount, bool isGrounded)
        {
            if (!inited) return;
            if (animator == null) return;
            // update the animator parameters
            SetFloatAnimator("Forward", forwardAmount, 0.1f, Time.deltaTime);
            SetFloatAnimator("Turn", turnAmount, 0.1f, Time.deltaTime);
            if (!isGrounded)
            {
                if (rb != null) SetFloatAnimator("Jump", rb.linearVelocity.y);
            }
            SetBoolAnimator("OnGround", isGrounded);
        }

        public void PlayerAnimJump()
        {
            for (int i = 0; i < animator.layerCount; i++)
            {
                animator.Play("Jump", i, 0);
            }
        }
        public void SetAnimationKnockback()
        {
            if (animator != null)
            {
                SetBoolAnimator("KnockbackFall", true);
                for (int i = 0; i < animator.layerCount; i++)
                {
                    animator.Play("KnockbackFall", i, 0);
                }
            }
        }
        public void DoneKnockBack()
        {
            SetBoolAnimator("KnockbackFall", false);
        }

        public void SetAnimationIdle()
        {
            if (animator != null)
            {
                SetFloatAnimator("Forward", 0, 0.1f, Time.deltaTime);

            }
        }
        public void PlayerAnimAttack(float attackSpeed)
        {
            if (isHoldBrainrot) return;
            SetFloatAnimator("AttackSpeed", attackSpeed);
            SetBoolAnimator("Attacking", true);
            currentTimeAttack = 0;
            this.attackSpeed = attackSpeed;
            animator.Play("Attack", 1, 0);
            attacking = true;
        }
        public void DoneAnimAttack()
        {
            ActionDoneAttack?.Invoke();
            attacking = false;
            SetBoolAnimator("Attacking", false);
        }

        public void SetApplyRootMotion(bool applyRootMotion)
        {
            animator.applyRootMotion = applyRootMotion;
        }

        public void ChangedAnimator(Animator anim, UnityAction ActionAttack)
        {
            if (anim == null) return;
            this.animator = anim;
            handleAnimEvent = animator.GetComponent<PlayerHandleAnimEvent>();
            if (handleAnimEvent == null) handleAnimEvent = animator.gameObject.AddComponent<PlayerHandleAnimEvent>();
            handleAnimEvent.AddEventAttack(ActionAttack);
            timeAttack = GetAnimationLengthByName("attack");
        }
        public Transform GetHolderItem()
        {
            if (handleAnimEvent == null) return null;
            return handleAnimEvent.holderItem;
        }

        public float GetAnimationLengthByName(string clipName)
        {
            RuntimeAnimatorController controller = animator.runtimeAnimatorController;

            foreach (AnimationClip clip in controller.animationClips)
            {
                if (clip.name == clipName)
                    return clip.length;
            }

            Debug.LogWarning($"Không tìm thấy animation: {clipName}");
            return 0f;
        }
        public void SetMoveSpeed(float speed)
        {
            SetFloatAnimator("MoveSpeed", speed);
        }
        public void SetWeightAnimator(int layer,float weight)
        {
            animator.SetLayerWeight(1, 0);
            animator.SetLayerWeight(2, 0);
            animator.SetLayerWeight(layer, weight);

        }
    }

}