using UnityEngine;
using System.Collections;
using Fujin.System;
using Fujin.Constants;
using UnityEngine.UI;

namespace Fujin.UI
{
    /// <summary>
    /// public methods as an instance: SetCursorTracking() ChangeCursorSensitivity() GetPosOnCanvas() GetPosOnScreen()
    /// </summary>
    public class CursorManager : MonoBehaviour
    {
        public enum CursorSpriteType
        {
            Invalid,
            Default,
        }
        
        [SerializeField] private SceneLoadManager mSceneLoadManager;
        private static CursorManager _instance;
        public static CursorManager Instance => _instance;

        private RectTransform targetCanvas;
        
        [SerializeField] private GameObject targetPrefab;
        private RectTransform targetRect;

        [SerializeField] private Sprite defaultCursor;
        
        // For target
        private readonly Vector2 targetSpeed = new Vector2(4f, 2.5f);
        private float targetMsMultiplier = 1f;
        private bool shouldTrackCursor;

        private void Update()
        {
            // Update target pos
            if (shouldTrackCursor)
            {
                targetRect.anchoredPosition += new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * targetSpeed * targetMsMultiplier;
            }

            // Adjust proxy pos
            if (Cursor.lockState == CursorLockMode.Confined)
            {
                proxyRect.anchoredPosition = new Vector2(Input.mousePosition.x / Screen.width * 1920f, Input.mousePosition.y / Screen.height * 1080f);
            }
        }

        /// <summary>
        /// Is distinct to GetPosOnCanvas (assumes the parent panel is set exactly on camera)
        /// </summary>
        /// <returns></returns>
        public Vector2 GetTargetPosOnScreen() => new Vector2(targetRect.anchoredPosition.x * Screen.width / targetCanvas.sizeDelta.x, 
            targetRect.anchoredPosition.y * Screen.height / targetCanvas.sizeDelta.y);
        

        
        private Coroutine changeMsCoroutine;

        /// <summary>
        /// Change cursor sensitivity
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="over"></param>
        public void ChangeTargetSensitivity(float from, float to, float over)
        {
            if (changeMsCoroutine != null)
            {
                StopCoroutine(changeMsCoroutine);
            }
            changeMsCoroutine = StartCoroutine(ChangeMsMultiplier(from, to, over));
        }

        private IEnumerator ChangeMsMultiplier(float firstValue, float endValue, float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime <= duration)
            {
                targetMsMultiplier = Mathf.Lerp(firstValue, endValue, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            targetMsMultiplier = endValue;
            changeMsCoroutine = null;
        }

        /// <summary>
        /// Make sure to assign pos when the value is true
        /// </summary>
        /// <param name="value"></param>
        /// <param name="spriteType"></param>
        /// <param name="pos"></param>
        public void SetTargetTracking(bool value, Vector2? pos = null)
        {
            shouldTrackCursor = value;
            targetRect.gameObject.SetActive(value);
            if (value)
            {
                if (targetCanvas == null)
                {
                    FindTargetCanvas();
                }
                targetRect.anchoredPosition = pos ?? new Vector2(targetCanvas.sizeDelta.x / 2f, targetCanvas.sizeDelta.y / 2f);
            }
        }
        
        private void FindTargetCanvas()
        {
            GameObject targetHolder = GameObject.FindGameObjectWithTag(GameObjectTag.CursorHolder);

            if (targetHolder == null)
            {
                Debug.LogError("No target holder is present in the scene.");
                return;
            }
            
            targetRect.transform.SetParent(targetHolder.transform); 
            targetCanvas = targetHolder.GetComponent<RectTransform>();
        }

        private CursorSpriteType currentSpriteType = CursorSpriteType.Invalid;
        private readonly Vector2 defaultSize = new Vector2(20f, 20f);

        private void SetCursorSprite(CursorSpriteType spriteType)
        {
            currentSpriteType = spriteType;
            switch (spriteType)
            {
                case CursorSpriteType.Default:
                    proxyImg.sprite = defaultCursor;
                    proxyRect.sizeDelta = defaultSize;
                    break;
            }
        }
        
        public void SetCursorVisibility(bool isVisible, CursorSpriteType spriteType = CursorSpriteType.Default)
        {
            Cursor.lockState = isVisible ? CursorLockMode.Confined : CursorLockMode.Locked;
            Cursor.visible = false;
            proxyRect.gameObject.SetActive(isVisible);

            SetCursorSprite(spriteType);
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this);
                ApplyTransparentTexture();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void ApplyTransparentTexture()
        {
            Cursor.visible = false;
            // Texture2D transparentTexture = new Texture2D(1, 1);
            // transparentTexture.SetPixel(0, 0, new Color(0, 0, 0, 0));
            // transparentTexture.Apply();
            //
            // Cursor.SetCursor(transparentTexture, Vector2.zero, CursorMode.Auto);
        }

        [SerializeField] private GameObject proxyPrefab;
        private RectTransform proxyRect;
        private Image proxyImg;
        
        private void UpdateCursorCanvas()
        {
            // Re-register itself
            mSceneLoadManager.AddFunctionOnSceneLoad(UpdateCursorCanvas);

            if (proxyRect == null)
            {
                GameObject cursorCanvas = Instantiate(proxyPrefab);
                GameObject cursorObj = cursorCanvas.transform.GetChild(0).gameObject;
                proxyRect = cursorObj.GetComponent<RectTransform>();
                proxyImg = cursorObj.GetComponent<Image>();
                DontDestroyOnLoad(cursorCanvas);
                
                SetCursorVisibility(false);
            }

            if (targetRect == null)
            {
                GameObject cursorObj = Instantiate(targetPrefab);
                targetRect = cursorObj.GetComponent<RectTransform>();
                DontDestroyOnLoad(cursorObj);
            }
        }

        private void Start()
        {
            UpdateCursorCanvas();
        }
    }
}