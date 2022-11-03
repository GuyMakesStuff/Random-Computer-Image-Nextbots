using Experiments.EndlessHallway.Managers;
using Experiments.Global.Audio;
using UnityEngine;

namespace EasyPlayerController
{
    [RequireComponent(typeof(CharacterController))]
    public class CCPlayerController : PlayerController
    {
        CharacterController CC;
        Rigidbody Body;
        Vector3 PrevVelocity;
        float Damp;
        bool IsDamping = false;
        bool IsLanded = true;
        bool IsJumped = false;
        [HideInInspector]
        public bool PlaySFX;

        public override void MoveComponentInit()
        {
            base.MoveComponentInit();
            CC = GetComponent<CharacterController>();
            Body = gameObject.AddComponent<Rigidbody>();
            Body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Body.isKinematic = true;
        }

        public override void RequestMove(Vector3 MoveVect, Vector3 GravVect)
        {
            base.RequestMove(MoveVect, GravVect);
            if(CanMove)
            {
                CC.Move(MoveVect * Time.deltaTime);
                CC.Move(GravVect * Time.deltaTime);
                PrevVelocity = MoveVect + GravVect;

                if(AudioManager.IsInstanced && PlaySFX)
                {
                    bool PlayMoveSFX = MoveVect.magnitude > 0.1f && IsGrounded;
                    SoundEffectBehaviour behaviour = (PlayMoveSFX) ? SoundEffectBehaviour.Play : SoundEffectBehaviour.Stop;
                    string MoveSFXName = (IsSprinting) ? "Running" : "Walking";
                    string OtherMoveSFXName = (!IsSprinting) ? "Running" : "Walking";
                    AudioManager.Instance.InteractWithSFXOneShot(MoveSFXName, behaviour);
                    AudioManager.Instance.InteractWithSFXOneShot(OtherMoveSFXName, SoundEffectBehaviour.Stop);
                }
            }
            else
            {
                if(AudioManager.IsInstanced)
                {
                    AudioManager.Instance.InteractWithSFX("Walking", SoundEffectBehaviour.Stop);
                    AudioManager.Instance.InteractWithSFX("Running", SoundEffectBehaviour.Stop);
                }
            }

            if(IsDamping)
            {
                Damp += Time.deltaTime;
                CC.Move(Vector3.Lerp(PrevVelocity, Vector3.zero, Damp) * Time.deltaTime);

                if(Damp >= 1f)
                {
                    IsDamping = false;
                    Damp = 0f;
                }
            }
        }

        public void DampVelocity()
        {
            IsDamping = true;
        }

        public override void OnGrounded()
        {
            base.OnGrounded();
            IsJumped = false;
            if(!IsLanded)
            {
                IsLanded = true;
                if(AudioManager.Instance && PlaySFX)
                { 
                    AudioManager.Instance.InteractWithSFX("Land", SoundEffectBehaviour.Play);
                } 
            }
        }
        public override void OnNotGrounded()
        {
            base.OnNotGrounded();
            IsLanded = false;
            if(GravVel.y > 0f && !IsJumped)
            {
                IsJumped = true;
                if(AudioManager.IsInstanced && PlaySFX)
                {
                    AudioManager.Instance.InteractWithSFX("Jump", SoundEffectBehaviour.Play);
                }
            }
        }
        public override void Teleport(Vector3 Location, bool Instant)
        {
            if(!Instant)
            {
                Vector3 Gap = Location - transform.position;
                CC.Move(Gap);
            }
            else
            {
                CC.enabled = false;
                transform.position = Location;
                CC.enabled = true;
            }
        }

        public void Ragdoll(Vector3 Direction)
        {
            CC.enabled = false;
            CanMove = false;
            
            Body.isKinematic = false;
            float Force = Random.Range(5f, 15f + float.Epsilon) * 500f;
            Body.AddForce(Direction * Force);
            Body.AddTorque((Random.insideUnitSphere * Force) * 360f);
        }

        public void ResetOrientation(bool MakeMovable)
        {
            CanMove = MakeMovable;
            CC.enabled = true;
            Body.isKinematic = true;
            transform.rotation = Quaternion.identity;
            XRig = 0f;
            GravVel.y = 0f;
        }
    }
}