using UnityEngine;

public class BounceTrampoline
{
    public void TryBounce(PlayerBoostTarget target, BoostZonePreset preset, Collision collision, ref float lastBounceTime)
    {
        if (Time.time - lastBounceTime < preset.BounceCooldown)
            return;

        Rigidbody rigidbody = target.Rigidbody;

        if (rigidbody == null)
            return;

        Vector3 normal;
        
        if (preset.UseSurfaceNormal)
            normal = collision.GetContact(0).normal;
        else if (preset.CustomBounceDirection.sqrMagnitude > 0.001f)
            normal = preset.CustomBounceDirection.normalized;
        else
            normal = Vector3.up;
        
        if (Vector3.Dot(normal, Vector3.up) < 0f)
            normal = -normal;
        
        Vector3 bounceVel = normal * preset.BounceForce;

        if (preset.KeepHorizontalVelocity)
        {
            Vector3 horizontal = Vector3.ProjectOnPlane(rigidbody.velocity, normal);
            bounceVel += horizontal;
        }
        
        target.BoostArc(bounceVel);

        lastBounceTime = Time.time;
    }
}