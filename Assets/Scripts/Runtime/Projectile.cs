using UnityEngine;

namespace Aula0.Runtime
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 7f;
        [SerializeField] private float maxLifetime = 8f;

        private Rigidbody2D _body;
        private float _despawnAt;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _despawnAt = Time.time + maxLifetime;
        }

        private void Update()
        {
            if (Time.time >= _despawnAt)
            {
                Destroy(gameObject);
            }
        }

        public void Launch(Vector2 direction)
        {
            _body.linearVelocity = direction.normalized * speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDealDamage(other.gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDealDamage(other.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            TryDealDamage(other.gameObject);
        }

        private void TryDealDamage(GameObject target)
        {
            var health = target.GetComponent<PlayerHealth>() ?? target.GetComponentInParent<PlayerHealth>();
            if (health == null)
            {
                return;
            }

            if (health.TakeHit())
            {
                Destroy(gameObject);
            }
        }
    }
}
