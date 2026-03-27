using System;
using UnityEngine;

namespace Aula0.Runtime
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxLives = 5;

        [SerializeField] private float hitDetectionRadius = 0.55f;

        private int _currentLives;

        public event Action<int, int> LivesChanged;
        public event Action Died;

        public int CurrentLives => _currentLives;
        public int MaxLives => maxLives;

        private void Awake()
        {
            _currentLives = maxLives;
        }

        public void ResetLives()
        {
            _currentLives = maxLives;
            LivesChanged?.Invoke(_currentLives, maxLives);
        }

        public bool TakeHit()
        {
            if (_currentLives <= 0)
            {
                return false;
            }

            _currentLives = Mathf.Max(0, _currentLives - 1);
            LivesChanged?.Invoke(_currentLives, maxLives);

            if (_currentLives == 0)
            {
                Died?.Invoke();
            }

            return true;
        }

        private void FixedUpdate()
        {
            if (_currentLives <= 0)
            {
                return;
            }

            var hits = Physics2D.OverlapCircleAll(transform.position, hitDetectionRadius);
            foreach (var hit in hits)
            {
                if (hit == null || hit.gameObject == gameObject)
                {
                    continue;
                }

                var projectile = hit.GetComponent<Projectile>() ?? hit.GetComponentInParent<Projectile>();
                if (projectile == null)
                {
                    continue;
                }

                if (TakeHit())
                {
                    Destroy(projectile.gameObject);
                }

                break;
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            TryHandleProjectile(other.gameObject);
        }

        private void TryHandleProjectile(GameObject otherObject)
        {
            var projectile = otherObject.GetComponent<Projectile>();
            if (projectile == null)
            {
                return;
            }

            if (TakeHit())
            {
                Destroy(projectile.gameObject);
            }
        }
    }
}
