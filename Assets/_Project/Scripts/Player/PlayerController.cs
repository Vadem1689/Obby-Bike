using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public event Action<float> SpeedBoostStarted;
    public event Action SpeedBoostEnded;
    public event Action<float> JumpBoostStarted;
    public event Action JumpBoostEnded;
    
    [SerializeField] private PlayerConfig _playerConfig;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Transform _visualRootTransform;
    
    private JumpSystem _jumpSystemHandler;
    private BotPusher _botPusher;
    private Animations _animationHandler;
    private BikeMovementLogic _bikeMovementLogic;
    private BoostManager _boostManager;

    private IInput _input;
    private Rigidbody _rigidbody;
    private LayerMask _layerMask;

    private Coroutine _jumpAnimationCoroutine;
    
    private bool _jumpPressedThisFrame;
    private bool _wasGroundedLastFrame;

    public float BaseSpeed => _playerConfig != null ? _playerConfig.MaxSpeed : 0f;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        
        _layerMask = ~((1 << gameObject.layer) | (1 << 2));
        
        _jumpSystemHandler = new JumpSystem(_playerConfig.JumpForce, _audioSource, _playerConfig.JumpClip);
        _bikeMovementLogic = new BikeMovementLogic(_rigidbody, transform, _playerConfig, _jumpSystemHandler, _layerMask, _visualRootTransform);
        _animationHandler = new Animations(_animator);
        _botPusher = new BotPusher(transform, _playerConfig.PushRadius, _playerConfig.PushForce, _playerConfig.PushCooldown);
        _boostManager = new BoostManager(_playerConfig, _jumpSystemHandler, this);
        
        _boostManager.SpeedBoostStarted += f => SpeedBoostStarted?.Invoke(f);
        _boostManager.SpeedBoostEnded += () => SpeedBoostEnded?.Invoke();
        _boostManager.JumpBoostStarted += f => JumpBoostStarted?.Invoke(f);
        _boostManager.JumpBoostEnded += () => JumpBoostEnded?.Invoke();
    }

    private void Update()
    {
        UpdateController();
    }
    
    private void FixedUpdate()
    {
        _bikeMovementLogic.UpdatePhysics();
        _animationHandler.UpdateAnimator(_rigidbody);

        if (_bikeMovementLogic.IsGrounded && !_wasGroundedLastFrame && _jumpAnimationCoroutine != null)
        {
            StopCoroutine(_jumpAnimationCoroutine);
            
            _animator.Play("StopJumpNoPos");
            _jumpAnimationCoroutine = StartCoroutine(PlayGroundedAnimation());
        }

        _wasGroundedLastFrame = _bikeMovementLogic.IsGrounded;
    }

    private void OnDisable()
    {
        _boostManager.Reset();
    }

    public void SetAnimator(Animator animator)
    {
        _animator = animator;
        _animator.Rebind();
    }

    public void SetInput(IInput input)
    {
        if (_input != null)
        {
            _input.Jumped -= OnJumped;
            _input.Pushed -= OnManualPush;
        }

        _input = input;

        if (_input != null)
        {
            _input.Jumped += OnJumped;
            _input.Pushed += OnManualPush;
        }
        
        _bikeMovementLogic.ForceForwardMovement = _input is MobileInput;
    }

    public void ApplyTemporaryJumpBoost(float newJumpForce, float duration) => _boostManager.ApplyJumpBoost(newJumpForce, duration);
    
    public void ApplyTemporarySpeedBoost(float newSpeed, float duration) => _boostManager.ApplySpeedBoost(newSpeed, duration);
    
    private void UpdateController()
    {
        if (_input != null)
            _bikeMovementLogic.UpdateInput(_input);

        _bikeMovementLogic.TryJump(_jumpPressedThisFrame);
        _botPusher.Tick();

        if (_jumpPressedThisFrame && _bikeMovementLogic.IsGrounded)
        {
            _animationHandler.OnJumpTriggered();
            
            if (_jumpAnimationCoroutine != null)
                StopCoroutine(_jumpAnimationCoroutine);

            _jumpAnimationCoroutine = StartCoroutine(JumpAnimationSequence());
        }

        _jumpPressedThisFrame = false;
    }

    private void OnJumped() => _jumpPressedThisFrame = true;

    private void OnManualPush()
    {
        bool pushed = _botPusher.TryPush();
        
        if (pushed)
        {
            if (_playerConfig != null && _playerConfig.PushClip != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_playerConfig.PushClip);
            }
        }
    }
    
    private IEnumerator JumpAnimationSequence()
    {
        _animator.Play("JumpStartNoPos");
        var state = _animator.GetCurrentAnimatorStateInfo(0);
        
        while (state.IsName("JumpStartNoPos") && state.normalizedTime < 1f)
        {
            yield return null;
            
            state = _animator.GetCurrentAnimatorStateInfo(0);
        }
        
        yield return new WaitForSeconds(_playerConfig.InAirDelay);
        
        while (!_jumpSystemHandler.IsGrounded(transform, _playerConfig.JumpDistance, _layerMask))
        {
            _animator.CrossFade("InAirNoPos", 0.2f);
            
            yield return null;
        }
    }

    private IEnumerator PlayGroundedAnimation()
    {
        var state = _animator.GetCurrentAnimatorStateInfo(0);
        
        while (state.IsName("StopJumpNoPos") && state.normalizedTime < 1f)
        {
            yield return null;
            state = _animator.GetCurrentAnimatorStateInfo(0);
        }

        _animator.CrossFade("anim", 0.2f);
        _jumpAnimationCoroutine = null;
    }
}