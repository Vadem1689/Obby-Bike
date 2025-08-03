using UnityEngine;

public class JumpSystem
{
    private readonly AudioSource _audioSource;
    private readonly AudioClip _jumpClip;
    
    private float _jumpForce;
    
    //public event Action Jumped;
    
    public JumpSystem(float jumpForce, AudioSource audioSource, AudioClip jumpClip)
    {
        _jumpForce = jumpForce;
        _audioSource = audioSource;
        _jumpClip = jumpClip;
    }

    public JumpSystem(float jumpForce)
    {
        _jumpForce = jumpForce;
    }
    
    public void SetJumpForce(float jumpForce)
    {
        _jumpForce = jumpForce;
    }

    public bool IsGrounded(Transform transform, float checkDistance, LayerMask layerMask)
    {
        return Physics.Raycast(transform.position, Vector3.down, out _, checkDistance, layerMask);
    }

    public void TryJump(Rigidbody rigidbody, Vector3 moveDirection, bool isGrounded)
    {
        if (!isGrounded)
            return;
        
        if (moveDirection.sqrMagnitude > 0.0001f)
            rigidbody.AddForce(moveDirection * rigidbody.mass * _jumpForce);

        Vector3 verticalImpulse = rigidbody.velocity;
        verticalImpulse.y = _jumpForce;
        rigidbody.velocity = verticalImpulse;

        _audioSource?.PlayOneShot(_jumpClip);
        
        //Jumped?.Invoke();
    }
}