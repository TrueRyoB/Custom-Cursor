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
        private readonly Vector2 targetSpeed = new Vector2(80f, 50f);
        private float targetMsMultiplier = 1f;
        private bool shouldTrackCursor;

        private void Update()
        {
            // Update target pos
            if (shouldTrackCursor)
            {
                targetRect.anchoredPosition += new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * targetSpeed * (targetMsMultiplier * Time.deltaTime);
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
        /// <param name="pos"></param>
        public void SetTargetTracking(bool value, Vector2? pos = null)
        {
            shouldTrackCursor = value;
            targetRect.gameObject.SetActive(value);
            if (value)
            {
                if (targetCanvas == null)
                {
                    return;
                }
                targetRect.anchoredPosition = pos ?? new Vector2(targetCanvas.sizeDelta.x / 2f, targetCanvas.sizeDelta.y / 2f);
            }
        }
        
        private void FindTargetCanvas()
        {
            GameObject targetHolder = GameObject.FindGameObjectWithTag(GameObjectTag.CursorHolder);

            if (targetHolder == null)
            {
                Debug.Log("targetHolder was not found!");
                return;
            }
            
            targetRect.transform.SetParent(targetHolder.transform); 
            targetRect.transform.localPosition = Vector3.zero;
            targetRect.transform.localScale = Vector3.one;
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
                DontDestroyOnLoad(gameObject);
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
        }

        [SerializeField] private GameObject proxyPrefab;
        private RectTransform proxyRect;
        private Image proxyImg;
        
        private void UpdateCursorCanvas()
        {
            // Re-register itself
            _ = mSceneLoadManager.AddFunctionOnSceneLoad(UpdateCursorCanvas);

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
                SetTargetTracking(false);
                DontDestroyOnLoad(cursorObj);
            }
            
            FindTargetCanvas();
        }

        private void Start()
        {
            UpdateCursorCanvas();
        }
    }
}
