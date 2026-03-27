using UnityEngine;

namespace Aula0.Runtime
{
    public sealed class ProjectileSpawner : MonoBehaviour
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform playerTarget;
        [SerializeField] private Vector2 arenaSize = new(18f, 10f);
        [SerializeField] private float spawnOffset = 2f;
        [SerializeField] private float initialSpawnInterval = 1.1f;
        [SerializeField] private float minimumSpawnInterval = 0.28f;
        [SerializeField] private float difficultyRampPerSecond = 0.03f;

        private float _elapsed;
        private float _nextSpawnAt;
        private bool _isRunning = true;

        public Vector2 ArenaSize => arenaSize;

        private void Update()
        {
            if (!_isRunning || projectilePrefab == null || playerTarget == null)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            if (Time.time < _nextSpawnAt)
            {
                return;
            }

            SpawnProjectile();

            var interval = Mathf.Max(minimumSpawnInterval, initialSpawnInterval - (_elapsed * difficultyRampPerSecond));
            _nextSpawnAt = Time.time + interval;
        }

        public void Begin()
        {
            _isRunning = true;
            _elapsed = 0f;
            _nextSpawnAt = Time.time + initialSpawnInterval;
        }

        public void Stop()
        {
            _isRunning = false;
        }

        private void SpawnProjectile()
        {
            var half = arenaSize * 0.5f;
            var side = Random.Range(0, 4);
            Vector2 spawnPosition;

            switch (side)
            {
                case 0:
                    spawnPosition = new Vector2(Random.Range(-half.x, half.x), half.y + spawnOffset);
                    break;
                case 1:
                    spawnPosition = new Vector2(Random.Range(-half.x, half.x), -half.y - spawnOffset);
                    break;
                case 2:
                    spawnPosition = new Vector2(-half.x - spawnOffset, Random.Range(-half.y, half.y));
                    break;
                default:
                    spawnPosition = new Vector2(half.x + spawnOffset, Random.Range(-half.y, half.y));
                    break;
            }

            var projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            var direction = (Vector2)playerTarget.position - spawnPosition;
            projectile.Launch(direction);
        }
    }
}
