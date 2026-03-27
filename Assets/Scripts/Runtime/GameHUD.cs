using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aula0.Runtime
{
    public sealed class GameHUD : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Image[] lifeBoxes;
        [SerializeField] private Text timerText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            ResolveUiReferences();
            ResolvePlayerHealth();
        }

        private void OnEnable()
        {
            ResolveUiReferences();
            ConnectPlayerHealth(playerHealth);
        }

        private void Start()
        {
            ResolveUiReferences();
            ConnectPlayerHealth(playerHealth);
        }

        private void OnDisable()
        {
            if (playerHealth == null)
            {
                return;
            }

            playerHealth.LivesChanged -= OnLivesChanged;
        }

        public void Bind(GameManager gameManager)
        {
            ResolveUiReferences();

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(gameManager.RestartGame);
            }
        }

        public void ConnectPlayerHealth(PlayerHealth health)
        {
            if (playerHealth != null)
            {
                playerHealth.LivesChanged -= OnLivesChanged;
            }

            playerHealth = health != null ? health : FindObjectOfType<PlayerHealth>();
            if (playerHealth == null)
            {
                return;
            }

            playerHealth.LivesChanged -= OnLivesChanged;
            playerHealth.LivesChanged += OnLivesChanged;
            SetLives(playerHealth.CurrentLives, playerHealth.MaxLives);
        }

        public void SetLives(int currentLives, int maxLives)
        {
            ResolveUiReferences();
            if (lifeBoxes == null || lifeBoxes.Length == 0)
            {
                return;
            }

            for (var i = 0; i < lifeBoxes.Length; i++)
            {
                if (lifeBoxes[i] == null)
                {
                    continue;
                }

                lifeBoxes[i].enabled = i < currentLives && i < maxLives;
                lifeBoxes[i].color = new Color(0.9f, 0.16f, 0.16f, 1f);
            }
        }

        public void SetTime(float elapsedSeconds)
        {
            if (timerText == null)
            {
                timerText = FindChildComponent<Text>("TopBar/TimerText");
            }

            if (timerText != null)
            {
                timerText.text = $"TEMPO {elapsedSeconds:0.0}s";
            }
        }

        public void ShowGameOver(bool isVisible)
        {
            ResolveUiReferences();
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(isVisible);
            }
        }

        private void OnLivesChanged(int currentLives, int maxLives)
        {
            SetLives(currentLives, maxLives);
        }

        private void ResolvePlayerHealth()
        {
            if (playerHealth == null)
            {
                playerHealth = FindObjectOfType<PlayerHealth>();
            }
        }

        private void ResolveUiReferences()
        {
            if (timerText == null)
            {
                timerText = FindChildComponent<Text>("TopBar/TimerText");
            }

            if (gameOverPanel == null)
            {
                gameOverPanel = FindChildGameObject("GameOverPanel");
            }

            if (restartButton == null)
            {
                restartButton = FindChildComponent<Button>("GameOverPanel/RestartButton");
            }

            if (lifeBoxes == null || lifeBoxes.Length == 0 || HasMissingLifeBoxReference())
            {
                var container = transform.Find("TopBar/LifeBoxes");
                if (container == null)
                {
                    return;
                }

                var images = new List<Image>();
                for (var i = 0; i < container.childCount; i++)
                {
                    var image = container.GetChild(i).GetComponent<Image>();
                    if (image != null)
                    {
                        images.Add(image);
                    }
                }

                lifeBoxes = images.ToArray();
            }
        }

        private bool HasMissingLifeBoxReference()
        {
            if (lifeBoxes == null || lifeBoxes.Length == 0)
            {
                return true;
            }

            foreach (var box in lifeBoxes)
            {
                if (box == null)
                {
                    return true;
                }
            }

            return false;
        }

        private T FindChildComponent<T>(string relativePath) where T : Component
        {
            var child = transform.Find(relativePath);
            return child != null ? child.GetComponent<T>() : null;
        }

        private GameObject FindChildGameObject(string relativePath)
        {
            var child = transform.Find(relativePath);
            return child != null ? child.gameObject : null;
        }
    }
}
