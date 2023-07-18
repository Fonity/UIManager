using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
    public partial class Window : MonoBehaviour
    {
        [SerializeField]
        private bool cacheInterfacesInstance = true;

        [SerializeField] 
        private bool disableInteractionWhileTransitioning = true;

        [SerializeField]
        private WindowIDIndirectReference windowID;
        public WindowID WindowID => windowID.Ref;


        private bool hasCachedRectTransform;
        private RectTransform cachedRectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (!hasCachedRectTransform)
                {
                    cachedRectTransform = transform as RectTransform;
                    hasCachedRectTransform = true;
                }
                return cachedRectTransform;
            }
        }

        private bool hasCachedCanvasGroup;
        private CanvasGroup cachedCanvasGroup;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (!hasCachedCanvasGroup)
                {
                    cachedCanvasGroup = GetComponent<CanvasGroup>();
                    hasCachedCanvasGroup = cachedCanvasGroup != null;
                }
                return cachedCanvasGroup;
            }
        }

        
        private bool hasCachedGraphicRaycaster;
        private GraphicRaycaster cachedGraphicRaycaster;
        public GraphicRaycaster GraphicRaycaster
        {
            get
            {
                if (!hasCachedGraphicRaycaster)
                {
                    cachedGraphicRaycaster = GetComponent<GraphicRaycaster>();
                    hasCachedGraphicRaycaster = cachedGraphicRaycaster != null;
                }
                return cachedGraphicRaycaster;
            }
        }

        protected WindowsManager windowsManager;
        public WindowsManager WindowsManager => windowsManager;

        private bool isOpen;
        public bool IsOpen => isOpen;
        
        private bool initialized;
        public bool Initialized => initialized;

        private Coroutine closeRoutine;
        private Coroutine openRoutine;
        

        internal void Initialize(WindowsManager targetWindowsManager, WindowID targetWindowID)
        {
            windowsManager = targetWindowsManager;
            windowID = new WindowIDIndirectReference(targetWindowID);
            initialized = true;
            DispatchWindowInitialized();
        }

        internal IEnumerator OpenEnumerator(Action<Window> callback = null)
        {
            if (isOpen)
                yield break;

            if (closeRoutine != null)
                StopCoroutine(closeRoutine);

            isOpen = true;

            if (disableInteractionWhileTransitioning)
            {
                if (hasCachedGraphicRaycaster)
                    GraphicRaycaster.enabled = false;
            }

            OnBeforeOpen();

            yield return TransiteInEnumerator();
            
            if (hasCachedGraphicRaycaster)
                GraphicRaycaster.enabled = true;
            
            callback?.Invoke(this);
            OnAfterOpen();
        }

        protected virtual void OnBeforeOpen()
        {
            DispatchOnBeforeWindowOpen();
        }
        
        private void OnAfterOpen()
        {
            DispatchOnAfterWindowOpen();
        }

        internal IEnumerator CloseEnumerator()
        {
            if (!isOpen)
                yield break;

            isOpen = false;

            if (disableInteractionWhileTransitioning)
            {
                if (hasCachedGraphicRaycaster)
                    GraphicRaycaster.enabled = false;
            }
            
            OnBeforeClose();

            if (openRoutine != null)
                StopCoroutine(openRoutine);

            yield return TransiteOutEnumerator();

            OnAfterClose();
        }

        protected virtual void OnBeforeClose()
        {
            DispatchOnBeforeWindowClose();
        }
        
        protected virtual void OnAfterClose()
        {
            DispatchOnAfterWindowClose();
        }
        
        protected virtual IEnumerator TransiteInEnumerator()
        {
            gameObject.SetActive(true);
            yield return null;
        }
        
        protected virtual IEnumerator TransiteOutEnumerator()
        {
            gameObject.SetActive(false);
            yield return null;
        }

        internal virtual void OnGainFocus()
        {
            DispatchOnGainFocus();
        }

        internal virtual void OnLostFocus()
        {
            DispatchOnLostFocus();
        }

        private void StopTransitionsCoroutines()
        {
            if (closeRoutine != null)
                StopCoroutine(closeRoutine);

            if (openRoutine != null)
                StopCoroutine(openRoutine);
            
            closeRoutine = null;
            openRoutine = null;
        }
        
        protected virtual void OnDestroy()
        {
            StopTransitionsCoroutines();
        }
    }
}
