using System;
using Elementary;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.GameEngine.Animation
{
    [AddComponentMenu("GameEngine/Animation/Animator Machine")]
    public class UAnimatorMachine : MonoBehaviour
    {
        private static readonly int STATE_PARAMETER = Animator.StringToHash("State");

        public event StateDelegate OnStateEntered;

        public event StateDelegate OnStateExited;

        public event Action OnEventReceived
        {
            add { this.eventDispatcher.OnEventReceived += value; }
            remove { this.eventDispatcher.OnEventReceived -= value; }
        }

        public event Action<bool> OnBoolReceived
        {
            add { this.eventDispatcher.OnBoolReceived += value; }
            remove { this.eventDispatcher.OnBoolReceived -= value; }
        }

        public event Action<int> OnIntReceived
        {
            add { this.eventDispatcher.OnIntReceived += value; }
            remove { this.eventDispatcher.OnIntReceived -= value; }
        }

        public event Action<float> OnFloatReceived
        {
            add { this.eventDispatcher.OnFloatReceived += value; }
            remove { this.eventDispatcher.OnFloatReceived -= value; }
        }

        public event Action<string> OnStringReceived
        {
            add { this.eventDispatcher.OnStringReceived += value; }
            remove { this.eventDispatcher.OnStringReceived -= value; }
        }

        public event Action<Object> OnObjectReceived
        {
            add { this.eventDispatcher.OnObjectReceived += value; }
            remove { this.eventDispatcher.OnObjectReceived -= value; }
        }

        public event Action<AnimationClip> OnAnimationStarted
        {
            add { this.eventDispatcher.OnAnimationStarted += value; }
            remove { this.eventDispatcher.OnAnimationStarted -= value; }
        }

        public event Action<AnimationClip> OnAnimationFinished
        {
            add { this.eventDispatcher.OnAnimationEnded += value; }
            remove { this.eventDispatcher.OnAnimationEnded -= value; }
        }

        [PropertySpace]
        [PropertyOrder(-10)]
        [LabelText("Apply Root Motion")]
        [ReadOnly]
        [ShowInInspector]
        public bool IsRootMotion
        {
            get { return this.animator != null && this.animator.applyRootMotion; }
        }

        public float BaseSpeed
        {
            get { return this.baseSpeed; }
        }

        public int CurrentState
        {
            get { return this.stateId; }
        }

        [ReadOnly]
        [ShowInInspector]
        private int stateId;

        [ReadOnly]
        [ShowInInspector]
        private float baseSpeed;
        
        [Space]
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private AnimatorObservable eventDispatcher;
        
        private readonly AnimatorMultiplierComposite multiplier = new();

        [SerializeField]
        private StateHolder[] states = Array.Empty<StateHolder>();

        protected virtual void Awake()
        {
            this.stateId = this.animator.GetInteger(STATE_PARAMETER);
            this.baseSpeed = this.animator.speed;
        }

        protected virtual void OnEnable()
        {
            this.eventDispatcher.OnStateEntered += this.OnEnterState;
            this.eventDispatcher.OnStateExited += this.OnExitState;
        }

        protected virtual  void OnDisable()
        {
            this.eventDispatcher.OnStateEntered -= this.OnEnterState;
            this.eventDispatcher.OnStateExited -= this.OnExitState;
        }

        public void PlayAnimation(string animationName, string layerName, float normalizedTime = 0)
        {
            var id = Animator.StringToHash(animationName);
            this.PlayAnimation(id, layerName, normalizedTime);
        }

        public void PlayAnimation(int hash, string layerName, float normalizedTime = 0)
        {
            var index = this.animator.GetLayerIndex(layerName);
            this.PlayAnimation(hash, index, normalizedTime);
        }

        public void SetLayerWeight(int layer, float weight)
        {
            this.animator.SetLayerWeight(layer, weight);
        }

        public void PlayAnimation(int hash, int layer, float normalizedTime = 0)
        {
            this.animator.Play(hash, layer, normalizedTime);
        }

        public void ChangeState(int stateId)
        {
            if (this.stateId == stateId)
            {
                return;
            }

            this.stateId = stateId;
            this.animator.SetInteger(STATE_PARAMETER, this.stateId);
        }

        public void AddSpeedMultiplier(IAnimatorMultiplier multiplier)
        {
            this.multiplier.Add(multiplier);
            this.UpdateAnimatorSpeed();
        }

        public void RemoveSpeedMultiplier(IAnimatorMultiplier multiplier)
        {
            this.multiplier.Remove(multiplier);
            this.UpdateAnimatorSpeed();
        }

        public void SetBaseSpeed(float speed)
        {
            if (Mathf.Approximately(speed, this.baseSpeed))
            {
                return;
            }

            this.baseSpeed = speed;
            this.UpdateAnimatorSpeed();
        }

        public void ApplyRootMotion()
        {
            this.animator.applyRootMotion = true;
        }

        public void ResetRootMotion(bool resetPosition = true, bool resetRotation = true)
        {
            this.animator.applyRootMotion = false;
            if (resetPosition)
            {
                this.animator.transform.localPosition = Vector3.zero;
            }

            if (resetRotation)
            {
                this.animator.transform.localRotation = Quaternion.identity;
            }
        }

        private void OnEnterState(AnimatorStateInfo state, int stateId, int layerindex)
        {
            if (this.FindState(stateId, out var fsmState))
            {
                fsmState.Enter();
            }

            this.OnStateEntered?.Invoke(state, stateId, layerindex);
        }

        private void OnExitState(AnimatorStateInfo state, int stateId, int layerindex)
        {
            if (this.FindState(stateId, out var fsmState))
            {
                fsmState.Exit();
            }

            this.OnStateExited?.Invoke(state, stateId, layerindex);
        }

        private void UpdateAnimatorSpeed()
        {
            this.animator.speed = this.baseSpeed * this.multiplier.GetValue();
        }

        private bool FindState(int id, out MonoState state)
        {
            for (int i = 0, count = this.states.Length; i < count; i++)
            {
                StateHolder holder = this.states[i];
                if (holder.id == id)
                {
                    state = holder.state;
                    return true;
                }
            }

            state = default;
            return false;
        }

        [Serializable]
        private struct StateHolder
        {
            [SerializeField]
            public int id;

            [SerializeField]
            public MonoState state;
        }
    }
}