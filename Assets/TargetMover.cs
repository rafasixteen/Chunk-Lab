using UnityEngine;

namespace Rafasixteen
{
    public class TargetMover : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _moveInterval = 2f;
        [SerializeField] private float _sphereRadius = 10f;
        [SerializeField] private bool _lockYPosition = true;

        private Vector3 _targetPosition;
        private float _timeSinceLastMove;

        private void Start()
        {
            SetRandomTargetPosition();
        }

        private void Update()
        {
            MoveTowardsTarget();
            CheckTargetChange();
        }

        private void MoveTowardsTarget()
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime);
        }

        private void CheckTargetChange()
        {
            _timeSinceLastMove += Time.deltaTime;
            if (_timeSinceLastMove >= _moveInterval || IsCloseToTarget())
            {
                SetRandomTargetPosition();
                _timeSinceLastMove = 0f;
            }
        }

        private bool IsCloseToTarget()
        {
            return Vector3.Distance(transform.position, _targetPosition) < 0.1f;
        }

        private void SetRandomTargetPosition()
        {
            _targetPosition = Random.insideUnitSphere * _sphereRadius;

            if (_lockYPosition)
                _targetPosition.y = transform.position.y;
        }
    }
}