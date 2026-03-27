using UnityEngine;
using UnityEngine.SceneManagement;

namespace Aula0.Runtime
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private ProjectileSpawner projectileSpawner;
        [SerializeField] private GameHUD gameHud;

        private float _survivalTime;
        private bool _isGameOver;
        private int _lastSyncedLives = -1;
        private int _lastSyncedMaxLives = -1;

        private void Awake()
        {
            Time.timeScale = 1f;
            ResolveReferences();
            ConfigureHud();
            SyncLives(force: true);
            gameHud?.SetTime(0f);
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeToPlayerHealth();
            SyncLives(force: true);
        }

        private void OnDisable()
        {
            UnsubscribeFromPlayerHealth();
        }

        private void Start()
        {
            ResolveReferences();
            ConfigureHud();
            SubscribeToPlayerHealth();

            if (playerHealth != null)
            {
                playerHealth.ResetLives();
            }

            SyncLives(force: true);
            projectileSpawner?.Begin();
        }

        private void Update()
        {
            if (_isGameOver)
            {
                return;
            }

            _survivalTime += Time.deltaTime;
            gameHud?.SetTime(_survivalTime);
            SyncLives();
        }

        private void OnDestroy()
        {
            UnsubscribeFromPlayerHealth();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnLivesChanged(int currentLives, int maxLives)
        {
            gameHud.SetLives(currentLives, maxLives);
            _lastSyncedLives = currentLives;
            _lastSyncedMaxLives = maxLives;
        }

        private void OnPlayerDied()
        {
            _isGameOver = true;
            if (projectileSpawner != null)
            {
                projectileSpawner.Stop();
            
            }

            Debug.Log($"[GameManager] OnPlayerDied - gameHud={gameHud}, isGameOver={_isGameOver}");
            Time.timeScale = 0f;
            if (gameHud != null)
            {
                gameHud.ShowGameOver(true);
            }
        }

        private void ResolveReferences()
        {
            if (playerHealth == null)
            {
                playerHealth = FindObjectOfType<PlayerHealth>();
            }

            if (projectileSpawner == null)
            {
                projectileSpawner = FindObjectOfType<ProjectileSpawner>();
            }

            if (gameHud == null)
            {
                gameHud = FindObjectOfType<GameHUD>();
            }
        }

        private void ConfigureHud()
        {
            if (gameHud == null)
            {
                return;
            }

            gameHud.Bind(this);
            gameHud.ConnectPlayerHealth(playerHealth);
            gameHud.ShowGameOver(false);
        }

        private void SubscribeToPlayerHealth()
        {
            if (playerHealth == null)
            {
                return;
            }

            playerHealth.LivesChanged -= OnLivesChanged;
            playerHealth.Died -= OnPlayerDied;
            playerHealth.LivesChanged += OnLivesChanged;
            playerHealth.Died += OnPlayerDied;
        }

        private void UnsubscribeFromPlayerHealth()
        {
            if (playerHealth == null)
            {
                return;
            }

            playerHealth.LivesChanged -= OnLivesChanged;
            playerHealth.Died -= OnPlayerDied;
        }

        private void SyncLives(bool force = false)
        {
            if (playerHealth == null || gameHud == null)
            {
                return;
            }

            var currentLives = playerHealth.CurrentLives;
            var maxLives = playerHealth.MaxLives;

            if (!force && currentLives == _lastSyncedLives && maxLives == _lastSyncedMaxLives)
            {
                return;
            }

            gameHud.SetLives(currentLives, maxLives);
            _lastSyncedLives = currentLives;
            _lastSyncedMaxLives = maxLives;
        }
    }
}
