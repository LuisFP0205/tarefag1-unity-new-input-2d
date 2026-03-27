using System.IO;
using Aula0.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Aula0.Editor
{
    public static class DodgePrototypeSetup
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string ProjectilePrefabPath = "Assets/Prefabs/Projectile.prefab";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string PlayerSpritesFolder = "Assets/Art/Player";

        [MenuItem("Tools/Dodge Prototype/Create Or Refresh")]
        public static void CreateOrRefresh()
        {
            EnsureSpritesAreImported();

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                Debug.LogError("Input action asset nao encontrado.");
                return;
            }

            if (inputActions.FindAction("Player/Move", true) == null)
            {
                Debug.LogError("Action Player/Move nao encontrada.");
                return;
            }

            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                mainCamera = cameraObject.AddComponent<Camera>();
            }

            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 2.45f;
            mainCamera.aspect = 16f / 9f;
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);

            _ = GetOrAddComponent<FollowCamera2D>(mainCamera.gameObject);
            var player = SetupPlayer(inputActions, mainCamera);

            var projectilePrefab = CreateOrRefreshProjectilePrefab();
            var systems = FindOrCreateRoot("GameSystems");
            var spawner = GetOrAddComponent<ProjectileSpawner>(systems);
            var gameManager = GetOrAddComponent<GameManager>(systems);

            SetSerializedObjectField(spawner, "projectilePrefab", projectilePrefab);
            SetSerializedObjectField(spawner, "playerTarget", player.transform);

            var playerHealth = player.GetComponent<PlayerHealth>();
            CreateOrRefreshBounds(mainCamera, player.GetComponent<CircleCollider2D>());
            var hud = CreateOrRefreshHud(inputActions);

            SetSerializedObjectField(gameManager, "playerHealth", playerHealth);
            SetSerializedObjectField(gameManager, "projectileSpawner", spawner);
            SetSerializedObjectField(gameManager, "gameHud", hud);
            SetSerializedObjectField(hud, "playerHealth", playerHealth);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Prototipo criado/atualizado. Abra SampleScene e aperte Play.");
        }

        private static GameObject SetupPlayer(InputActionAsset inputActions, Camera mainCamera)
        {
            var player = FindOrCreateRoot("Player");
            player.transform.position = Vector3.zero;
            player.transform.localScale = Vector3.one;

            var renderer = GetOrAddComponent<SpriteRenderer>(player);
            var body = GetOrAddComponent<Rigidbody2D>(player);
            var collider = GetOrAddComponent<CircleCollider2D>(player);
            var controller = GetOrAddComponent<PlayerController2D>(player);
            _ = GetOrAddComponent<PlayerHealth>(player);

            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            collider.radius = 0.5f;

            SetSerializedObjectField(controller, "inputActions", inputActions);
            SetSerializedObjectField(controller, "worldCamera", mainCamera);

            var sprites = LoadOrderedSprites();
            if (sprites.Length >= 9 && sprites[8] != null)
            {
                renderer.sprite = sprites[8];
                SetSerializedObjectField(controller, "idleSprite", sprites[8]);
                SetSerializedArrayField(controller, "runFrames", new Object[]
                {
                    sprites[3], sprites[4], sprites[5], sprites[6], sprites[7], sprites[8],
                });
            }

            return player;
        }

        private static void EnsureSpritesAreImported()
        {
            if (!Directory.Exists(PlayerSpritesFolder))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(PlayerSpritesFolder, "*.png"))
            {
                var importer = AssetImporter.GetAtPath(file) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 44f;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static Sprite[] LoadOrderedSprites()
        {
            var sprites = new Sprite[9];
            for (var i = 0; i < 9; i++)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{PlayerSpritesFolder}/Tyer{i + 1}.png");
            }

            return sprites;
        }

        private static Projectile CreateOrRefreshProjectilePrefab()
        {
            var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
            var root = prefabRoot == null
                ? new GameObject("Projectile")
                : PrefabUtility.LoadPrefabContents(ProjectilePrefabPath);

            var renderer = GetOrAddComponent<SpriteRenderer>(root);
            var body = GetOrAddComponent<Rigidbody2D>(root);
            var collider = GetOrAddComponent<CircleCollider2D>(root);
            _ = GetOrAddComponent<Projectile>(root);

            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            renderer.color = new Color(1f, 0.3f, 0.2f, 1f);
            root.transform.localScale = Vector3.one * 0.65f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            collider.isTrigger = true;
            collider.radius = 0.5f;

            PrefabUtility.SaveAsPrefabAsset(root, ProjectilePrefabPath);
            if (prefabRoot == null)
            {
                Object.DestroyImmediate(root);
            }
            else
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            var prefabGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
            return prefabGameObject != null ? prefabGameObject.GetComponent<Projectile>() : null;
        }

        private static void CreateOrRefreshBounds(Camera mainCamera, CircleCollider2D playerCollider)
        {
            var halfHeight = mainCamera.orthographicSize;
            var halfWidth = halfHeight * mainCamera.aspect;
            var colliderRadius = playerCollider != null ? playerCollider.radius : 0.5f;
            const float wallThickness = 1f;

            var boundsRoot = FindOrCreateRoot("ArenaBounds");
            CreateWall(boundsRoot.transform, "Top", new Vector2(0f, halfHeight + (wallThickness * 0.5f) - colliderRadius), new Vector2((halfWidth * 2f) + 2f, wallThickness));
            CreateWall(boundsRoot.transform, "Bottom", new Vector2(0f, -halfHeight - (wallThickness * 0.5f) + colliderRadius), new Vector2((halfWidth * 2f) + 2f, wallThickness));
            CreateWall(boundsRoot.transform, "Left", new Vector2(-halfWidth - (wallThickness * 0.5f) + colliderRadius, 0f), new Vector2(wallThickness, (halfHeight * 2f) + 2f));
            CreateWall(boundsRoot.transform, "Right", new Vector2(halfWidth + (wallThickness * 0.5f) - colliderRadius, 0f), new Vector2(wallThickness, (halfHeight * 2f) + 2f));
        }

        private static void CreateWall(Transform root, string wallName, Vector2 position, Vector2 size)
        {
            var wall = FindOrCreateWorldChild(root, wallName);
            wall.transform.localPosition = position;
            var collider = GetOrAddComponent<BoxCollider2D>(wall);
            collider.size = size;
        }

        private static GameHUD CreateOrRefreshHud(InputActionAsset actions)
        {
            var canvasObject = FindOrCreateRoot("GameCanvas");
            var canvas = GetOrAddComponent<Canvas>(canvasObject);
            var scaler = GetOrAddComponent<CanvasScaler>(canvasObject);
            _ = GetOrAddComponent<GraphicRaycaster>(canvasObject);
            ClearChildren(canvasObject.transform);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvas.pixelPerfect = true;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var eventSystem = Object.FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            }

            var inputModule = GetOrAddComponent<InputSystemUIInputModule>(eventSystem.gameObject);
            inputModule.actionsAsset = actions;

            var topBar = FindOrCreateUiChild(canvasObject.transform, "TopBar");
            var topBarImage = GetOrAddComponent<Image>(topBar);
            topBarImage.color = new Color(0f, 0f, 0f, 0.55f);
            var topBarRect = topBar.GetComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0f, 1f);
            topBarRect.anchorMax = new Vector2(1f, 1f);
            topBarRect.pivot = new Vector2(0.5f, 1f);
            topBarRect.anchoredPosition = Vector2.zero;
            topBarRect.sizeDelta = new Vector2(0f, 90f);

            var hud = GetOrAddComponent<GameHUD>(canvasObject);
            var lifeBoxes = CreateLifeBoxes(topBar.transform);
            var timerText = CreateAnchorText("TimerText", topBar.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(260f, 0f), new Vector2(360f, 48f), "TEMPO 0.0s", 30, TextAnchor.MiddleLeft);

            var gameOverPanel = CreateCenteredPanel("GameOverPanel", canvasObject.transform, "Você morreu", new Vector2(640f, 320f));
            var restartButton = CreateAnchorButton("RestartButton", gameOverPanel.transform, "Reiniciar", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(240f, 60f), 24);
            gameOverPanel.SetActive(false);

            SetSerializedArrayField(hud, "lifeBoxes", lifeBoxes);
            SetSerializedObjectField(hud, "timerText", timerText);
            SetSerializedObjectField(hud, "gameOverPanel", gameOverPanel);
            SetSerializedObjectField(hud, "restartButton", restartButton);
            return hud;
        }

        private static Object[] CreateLifeBoxes(Transform parent)
        {
            var container = FindOrCreateUiChild(parent, "LifeBoxes");
            var rect = container.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(32f, 0f);
            rect.sizeDelta = new Vector2(200f, 48f);

            var images = new Object[5];
            for (var i = 0; i < 5; i++)
            {
                var box = FindOrCreateUiChild(container.transform, $"LifeBox{i + 1}");
                var image = GetOrAddComponent<Image>(box);
                image.color = new Color(0.9f, 0.16f, 0.16f, 1f);

                var boxRect = box.GetComponent<RectTransform>();
                boxRect.anchorMin = new Vector2(0f, 0.5f);
                boxRect.anchorMax = new Vector2(0f, 0.5f);
                boxRect.pivot = new Vector2(0f, 0.5f);
                boxRect.anchoredPosition = new Vector2(i * 36f, 0f);
                boxRect.sizeDelta = new Vector2(28f, 28f);
                images[i] = image;
            }

            return images;
        }

        private static GameObject CreateCenteredPanel(string name, Transform parent, string title, Vector2 size)
        {
            var panel = FindOrCreateUiChild(parent, name);
            var image = GetOrAddComponent<Image>(panel);
            image.color = new Color(0f, 0f, 0f, 0.78f);

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;

            CreateAnchorText($"{name}Title", panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 86f), new Vector2(size.x - 40f, 50f), title, 32, TextAnchor.MiddleCenter);
            return panel;
        }

        private static Button CreateAnchorButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size, int fontSize)
        {
            var buttonObject = FindOrCreateUiChild(parent, name);
            var image = GetOrAddComponent<Image>(buttonObject);
            var button = GetOrAddComponent<Button>(buttonObject);
            image.color = new Color(0.16f, 0.16f, 0.16f, 0.95f);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            CreateAnchorText($"{name}Label", buttonObject.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size, label, fontSize, TextAnchor.MiddleCenter);
            return button;
        }

        private static Text CreateAnchorText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size, string value, int fontSize, TextAnchor alignment)
        {
            var textObject = FindOrCreateUiChild(parent, name);
            var text = GetOrAddComponent<Text>(textObject);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return text;
        }

        private static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static GameObject FindOrCreateRoot(string name)
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == name)
                {
                    return root;
                }
            }

            return new GameObject(name);
        }

        private static GameObject FindOrCreateUiChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child.gameObject;
            }

            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static GameObject FindOrCreateWorldChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child.gameObject;
            }

            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static void SetSerializedObjectField(Object target, string fieldName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetSerializedArrayField(Object target, string fieldName, Object[] values)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property == null || !property.isArray)
            {
                return;
            }

            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
