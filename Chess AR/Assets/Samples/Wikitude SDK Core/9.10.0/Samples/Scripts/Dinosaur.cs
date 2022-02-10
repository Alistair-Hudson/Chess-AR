using UnityEngine;
using System.Collections;

/* Controls a dinosaur in the Multiple Targets sample. */
public class Dinosaur : MonoBehaviour
{
    /* Parameter defined in the Assets/Wikitude/Samples/Animations/Dinosaur.controller. */
    private static readonly int WalkingSpeedParameterHash = Animator.StringToHash("Walking Speed");
    private static readonly int HitParameterHash = Animator.StringToHash("Hit");
    private static readonly int AttackParameterHash = Animator.StringToHash("Attack");
    private static readonly int CelebrateParameterHash = Animator.StringToHash("Celebrate");

    /* When the angle between the dinosaur and the desired target is less than this threshold, we stop rotating it. */
    private const float AngleThreshold = 1.0f;
    /* When the distance between the dinosaur and the desired target is less than this threshold, we stop moving it. */
    public const float DistanceThreshold = 0.06f;
    private const float WalkingSpeed = 1.0f;
    /* Time it takes to transition to full walking speed. */
    private const float ToWalkingTransitionTime = 0.5f;
    private bool _alignDinosaur = false;

    /* All the states in which the dinosaur can be in. */
    private enum State {
        Idle,
        RotateToTarget,
        MoveToTarget,
        WaitingForAttacker,
        Fight,
        Defeated,
        Celebrate,
        MoveToOrigin
    }

    /* The walking speed coroutine is started when the walking speed needs to gradually change towards a target speed. */
    private Coroutine _walkingSpeedCoroutine;
    /* The sequence coroutine is started when when two dinosaurs are tracked and one of them moves next to the other and initiates an attack. */
    private Coroutine _sequenceCoroutine;
    private Coroutine _nestedCoroutine;

    /* The target dinosaur that this dinosaur is supposed to attack. */
    public Dinosaur TargetDinosaur {
        get;
        private set;
    }

    /* The attacking dinosaur from which this dinosaur is supposed to defend from. */
    public Dinosaur AttackingDinosaur {
        get;
        private set;
    }

    private Animator _animator;

    public float RotationSpeed = 140.0f;
    public float MovementSpeed = 0.5f;

    public bool InBattle {
        get;
        private set;
    }

    private State CurrentState { get; set; }

    /* When two dinosaurs are tracked, the attack sequence is started. */
    public void Attack(Dinosaur targetDinosaur) {
        if (CurrentState != State.Idle) {
            return;
        }

        if (targetDinosaur.CurrentState != State.Idle) {
            return;
        }

        TargetDinosaur = targetDinosaur;
        /* The target dinosaur will start its defense sequence. */
        TargetDinosaur.DefendFrom(this);
        InBattle = true;
        _sequenceCoroutine = StartCoroutine(StartAttackSequence());
    }

    public void StartWalkCoroutine(Transform target) {
        if ((target.position - this.gameObject.transform.position).magnitude < 0.01f) {
            StopIfWalking();
            return;
        }
        if (_nestedCoroutine != null) {
            StopCoroutine(_nestedCoroutine);
        }
        if (_walkingSpeedCoroutine != null) {
            StopCoroutine(_walkingSpeedCoroutine);
        }
        
        _nestedCoroutine = StartCoroutine(StartWalkSequence(target));
    }

    private IEnumerator StartWalkSequence(Transform target) {
        CurrentState = State.RotateToTarget;
        yield return RotateTowards(target);
        CurrentState = State.MoveToTarget;
        yield return MoveTowards(target);
        CurrentState = State.Idle;
        yield return SetWalkingSpeedCoroutine(0f, 0f);
    }

    private IEnumerator StartAttackSequence() {
        _nestedCoroutine = StartCoroutine(StartWalkSequence(TargetDinosaur.transform));
        yield return _nestedCoroutine;

        while (TargetDinosaur.CurrentState != State.WaitingForAttacker) {
            yield return null;
        }

        Attack();

        while (TargetDinosaur.CurrentState != State.Defeated) {
            yield return null;
        }

        Celebrate();

        /* Wait until the celebration is done and the dinosaur can move back to its original location */
        while (CurrentState != State.MoveToOrigin) {
            yield return null;
        }

        yield return StartCoroutine(WalkBackSequence());
    }

    private IEnumerator WalkBackSequence() {
        yield return RotateTowards(transform.parent);
        yield return MoveTowards(transform.parent, 0.01f);
        SetWalkingSpeed(0.0f, 0.2f);
        CurrentState = State.Idle;
        InBattle = false;
    }

    private void DefendFrom(Dinosaur attackingDinosaur) {
        AttackingDinosaur = attackingDinosaur;
        InBattle = true;
        _sequenceCoroutine = StartCoroutine(StartDefendSequence());
    }
    public void SetAlignDinosaur(bool value) {
        _alignDinosaur = value;
    }
    private void StopCoroutines() {
        if (_sequenceCoroutine != null) {
            StopCoroutine(_sequenceCoroutine);
        }

        if (_nestedCoroutine != null) {
            StopCoroutine(_nestedCoroutine);
        }

        if (_walkingSpeedCoroutine != null) {
            StopCoroutine(_walkingSpeedCoroutine);
            SetWalkingSpeed(0f,0f);
        }
    }

    public void OnAttackerDisappeared() {
        /* If the attacking dinosaur disappears, because its target was lost, revert to idle, if we weren't already defeated. */
        StopCoroutines();
        if (CurrentState != State.Defeated) {
            CurrentState = State.Idle;
            InBattle = false;
        }
    }

    public void OnTargetDisappeared() {
        /* If the target dinosaur disappears, because its target was lost, move back to the original position. */
        StopCoroutines();

        StartCoroutine(WalkBackSequence());
    }

    private IEnumerator StartDefendSequence() {
        CurrentState = State.RotateToTarget;
        yield return RotateTowards(AttackingDinosaur.transform);
        /* Stop walking. */
        SetWalkingSpeed(0.0f, 0.5f);
        CurrentState = State.WaitingForAttacker;
        while (CurrentState != State.Fight) {
            yield return null;
        }
        /* As soon as we are hit, play the hit animation. */
        _animator.SetTrigger(HitParameterHash);
    }

    private void Attack() {
        _animator.SetTrigger(AttackParameterHash);
        CurrentState = State.Fight;
    }

    private void Hit() {
        CurrentState = State.Fight;
    }

    private void Celebrate() {
        _animator.SetTrigger(CelebrateParameterHash);
        CurrentState = State.Celebrate;
    }

    private void OnAttackAnimationEvent() {
        TargetDinosaur.Hit();
    }

    private void OnDefeatedAnimationEvent() {
        CurrentState = State.Defeated;
    }

    private void OnCelebrateEndAnimationEvent() {
        CurrentState = State.MoveToOrigin;
    }

    public void StopIfWalking(float time = ToWalkingTransitionTime) {
        if (CurrentState != State.Idle) {
            StopCoroutines();
            CurrentState = State.Idle;
            SetWalkingSpeed(0.0f, time);
        }
    }

    private IEnumerator RotateTowards(Transform rotationTarget) {
        /* Gradually rotate towards a target, until the AngleThreshold is hit. */
        Vector3 heightAdjusted = rotationTarget.position;
        Vector3 upVector = rotationTarget.up;
        if (_alignDinosaur) {
            heightAdjusted.y = transform.position.y;
            upVector = Vector3.up;
        }
        var targetRotation = Quaternion.LookRotation((heightAdjusted - transform.position).normalized, upVector);
        float angleToTarget = Quaternion.Angle(targetRotation, transform.rotation);

        if (angleToTarget > AngleThreshold) {
            SetWalkingSpeed(1.0f, ToWalkingTransitionTime);

            while (angleToTarget > AngleThreshold && rotationTarget != null) {
                float maxAngle = RotationSpeed * Time.deltaTime;
                float maxT = Mathf.Min(1.0f, maxAngle / angleToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, maxT);
                heightAdjusted.y = transform.position.y;
                targetRotation = Quaternion.LookRotation((heightAdjusted - transform.position).normalized, upVector);
                angleToTarget = Quaternion.Angle(targetRotation, transform.rotation);

                yield return null;
            }
        }
    }

    public float GetDistanceToTarget(Transform target) {
        return (transform.position - target.position).magnitude;
    }

    private IEnumerator MoveTowards(Transform moveTarget, float distanceThreshold = DistanceThreshold) {
        /* Gradually move towards a target, until the DistanceThreshold is hit. */
        
        float distanceToTarget = GetDistanceToTarget(moveTarget);
        Vector3 heightAdjusted = moveTarget.position;
        Vector3 upVector = moveTarget.up;
        if (_alignDinosaur) {
            heightAdjusted.y = transform.position.y;
            upVector = Vector3.up;
        }
        if (distanceToTarget > DistanceThreshold) {
            SetWalkingSpeed(1.0f, ToWalkingTransitionTime);
            while (distanceToTarget > distanceThreshold && moveTarget != null) {
                if (_alignDinosaur) {
                    heightAdjusted.y = transform.position.y;
                }
                transform.LookAt(heightAdjusted, upVector);

                Vector3 direction = (moveTarget.position - transform.position).normalized;
                transform.position += direction * MovementSpeed * Time.deltaTime;
                distanceToTarget = GetDistanceToTarget(moveTarget);

                yield return null;
            }
        }
    }

    private void SetWalkingSpeed(float walkingSpeed, float transitionTime) {
        _walkingSpeedCoroutine = StartCoroutine(SetWalkingSpeedCoroutine(walkingSpeed, transitionTime));
    }

    private IEnumerator SetWalkingSpeedCoroutine(float walkingSpeed, float transitionTime) {
        /* Gradually change the walking speed. */
        float startingSpeed = _animator.GetFloat(WalkingSpeedParameterHash);
        float currentTime = 0.0f;
        while (currentTime < transitionTime) {
            _animator.SetFloat(WalkingSpeedParameterHash, Mathf.Lerp(startingSpeed, walkingSpeed, currentTime / transitionTime));
            currentTime += Time.deltaTime;
            yield return null;
        }
        _animator.SetFloat(WalkingSpeedParameterHash, walkingSpeed);
    }

    private void Awake() {
        _animator = GetComponent<Animator>();
        CurrentState = State.Idle;
    }
}
