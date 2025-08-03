using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [SerializeField] private List<Waypoint> _nextWaypoints;
    [SerializeField] private bool _requiresJump = false;
    [SerializeField] private float _activationRadius = 0.5f;

    public List<Waypoint> NextWaypoints => _nextWaypoints;
    public bool RequiresJump => _requiresJump;
    public float ActivationRadius => _activationRadius;

    private void OnDrawGizmos()
    {
        Gizmos.color = _requiresJump ? Color.yellow : Color.green;
        Gizmos.DrawSphere(transform.position, _activationRadius);
        Gizmos.color = Color.white;
        
        foreach (var nw in _nextWaypoints)
            if (nw != null)
                Gizmos.DrawLine(transform.position, nw.transform.position);
    }
}