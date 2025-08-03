using System.Collections;
using UnityEngine;

public class PlayerControllerOld : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] public float _speed = 3;
    [SerializeField] public float _acceleration = 100f;
    [SerializeField] private float _angularSpeed = 360f;

    [Header("Jump Settings")]
    [SerializeField] private AudioClip _jump;
    [SerializeField] private float _jumpForce = 30;
    [SerializeField] private float _jumpDistance = 1.2f;
    [SerializeField] private float _inAirDelay = 0.5f;

    [Header("Push Settings")]
    [SerializeField] private float _pushRadius = 2f;
    [SerializeField] private float _pushForce = 10f;
    [SerializeField] private float _pushCooldown = 1f;

    private Movement _movementHandler;
    private JumpSystem jumpSystemHandler;
    private BotPusher _botPusher;
    private Animations _animationHandler;
    
    private IInput _input;
    private Camera _mainCamera;
    private Rigidbody _rigidbody;
    
    private Coroutine _jumpAnimationCoroutine;
    private Coroutine _jumpBoostRoutine;
    private Coroutine _speedBoostRoutine;
    
    private Vector3 _moveDirectionWorld;
    private Vector3 _lastVelocityDirection;
    private LayerMask _layerMask;

    private float _originalSpeed;
    
    private bool _jumpPressedThisFrame;
    private bool _wasGroundedLastFrame;
    
    public IInput Input => _input;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
        _mainCamera = Camera.main;
        _layerMask = ~((1 << gameObject.layer) | (1 << 2));

        _movementHandler = new Movement();
        jumpSystemHandler = new JumpSystem(_jumpForce, _audioSource, _jump);
        _animationHandler = new Animations(_animator);
        _botPusher = new BotPusher(transform, _pushRadius, _pushForce, _pushCooldown);
    }

    private void Update()
    {
        UpdateController();
        
        _botPusher.Tick();
    }

    private void FixedUpdate()
    {
        bool isGrounded = jumpSystemHandler.IsGrounded(transform, _jumpDistance, _layerMask);

        _movementHandler.HandleMovement(_rigidbody, _moveDirectionWorld, isGrounded, _speed, _acceleration);

        if (!TryGetComponent(out PlayerBoostTarget boostTarget) || !boostTarget.IsBoosting)
            _movementHandler.ClampSpeed(_rigidbody, _speed);
        
        _animationHandler.UpdateAnimator(_rigidbody);

        if (isGrounded && !_wasGroundedLastFrame && _jumpAnimationCoroutine != null)
        {
            StopCoroutine(_jumpAnimationCoroutine);
            _animator.Play("StopJumpNoPos");
            _jumpAnimationCoroutine = StartCoroutine(PlayGroundedAnimation());
        }

        _wasGroundedLastFrame = isGrounded;
    }

    public void SetAnimator(Animator animator)
    {
        _animator = animator;
        _animator.Rebind();
    }

    public void SetInput(IInput input)
    {
        if (_input != null)
            _input.Jumped -= OnJumped;

        _input = input;

        if (_input != null)
            _input.Jumped += OnJumped;
    }

    public void ApplyTemporaryJumpBoost(float multiplier, float duration)
    {
        if (_jumpBoostRoutine != null)
            StopCoroutine(_jumpBoostRoutine);

        _jumpBoostRoutine = StartCoroutine(JumpBoostCoroutine(multiplier, duration));
    }
    
    public void ApplyTemporarySpeedBoost(float boostedSpeed, float duration)
    {
        if (_speedBoostRoutine != null)
            StopCoroutine(_speedBoostRoutine);

        _originalSpeed = _speed;
        _speed = boostedSpeed;
        _speedBoostRoutine = StartCoroutine(SpeedBoostCoroutine(duration));
    }
    
    private void UpdateController()
    {
        bool isGrounded = jumpSystemHandler.IsGrounded(transform, _jumpDistance, _layerMask);

        UpdateMoveDirection();

        if (_jumpPressedThisFrame && isGrounded)
        {
            jumpSystemHandler.TryJump(_rigidbody, _moveDirectionWorld, isGrounded);
            _animationHandler.OnJumpTriggered();
            
            if (_jumpAnimationCoroutine != null)
                StopCoroutine(_jumpAnimationCoroutine);
            
            _jumpAnimationCoroutine = StartCoroutine(JumpAnimationSequence());
        }

        RotateTowardsVelocity();

        _jumpPressedThisFrame = false;
    }

    private void UpdateMoveDirection()
    {
        if (_input == null)
            return;

        Vector2 raw = _input.InputDirection;
        Vector3 direction = new Vector3(raw.x, 0f, raw.y);

        if (direction.sqrMagnitude > 0.0001f)
        {
            Vector3 forward = _mainCamera.transform.TransformDirection(direction);
            _moveDirectionWorld = new Vector3(forward.x, 0f, forward.z).normalized;
        }
        else
        {
            _moveDirectionWorld = Vector3.zero;
        }
    }

    private void RotateTowardsVelocity()
    {
        Vector3 velocity = _rigidbody.velocity;
        velocity.y = 0f;
        
        if (velocity.sqrMagnitude > 0.16f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
           
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _angularSpeed * Time.deltaTime);
            
            // _lastVelocityDirection = _rigidbody.velocity;
            //
            // if (Mathf.Abs(_lastVelocityDirection.normalized.y) > 0.999f)
            //     _lastVelocityDirection.y = 0f;
            //
            // transform.rotation = Quaternion.Slerp(transform.rotation,
            //     Quaternion.LookRotation(_lastVelocityDirection.normalized), _angularSpeed * Time.deltaTime);
        }
    }

    private void OnJumped()
    {
        _jumpPressedThisFrame = true;
    }
    
    private IEnumerator JumpAnimationSequence()
    {
        _animator.Play("JumpStartNoPos");
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName("JumpStartNoPos") && stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        }

        yield return new WaitForSeconds(_inAirDelay);

        while (!jumpSystemHandler.IsGrounded(transform, _jumpDistance, _layerMask))
        {
            _animator.CrossFade("InAirNoPos", 0.2f);
            yield return null;
        }
    }

    private IEnumerator PlayGroundedAnimation()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName("StopJumpNoPos") && stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        }

        _animator.CrossFade("anim", 0.2f);
        _jumpAnimationCoroutine = null;
    }
    
    private IEnumerator JumpBoostCoroutine(float multiplier, float duration)
    {
        float originalForce = _jumpForce;
        _jumpForce *= multiplier;

        yield return new WaitForSeconds(duration);

        _jumpForce = originalForce;
        _jumpBoostRoutine = null;
    }
    
    private IEnumerator SpeedBoostCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        _speed = _originalSpeed;
        _speedBoostRoutine = null;
    }
}