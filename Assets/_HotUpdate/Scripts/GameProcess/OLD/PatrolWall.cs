using UnityEngine;

public sealed class PatrolWall : MonoBehaviour
{
    [SerializeField] private Transform _pointA;
    [SerializeField] private Transform _pointB;
    [SerializeField, Min(0.01f)] private float _moveSpeed = 2f;
    [SerializeField, Min(0f)] private float _waitTime = 0.5f;

    private Transform _target;
    private float _waitTimer;

    private void Awake()
    {
        if (_pointA == null || _pointB == null)
        {
            enabled = false;
            return;
        }

        transform.position = _pointA.position;
        _target = _pointB;
    }

    private void Update()
    {
        if (_waitTimer > 0f)
        {
            _waitTimer -= Time.deltaTime;
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            _target.position,
            _moveSpeed * Time.deltaTime);

        if ((transform.position - _target.position).sqrMagnitude > 0.0001f)
            return;

        _target = _target == _pointA ? _pointB : _pointA;
        _waitTimer = _waitTime;
    }
}