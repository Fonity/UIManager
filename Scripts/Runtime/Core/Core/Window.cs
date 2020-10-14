using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    [RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
    public partial class Window : MonoBehaviour
    {
        [SerializeField]
        private bool cacheInterfacesInstance = true;

        [SerializeField] 
        private bool disableInteractionWhileTransitioning = true;
        
        private RectTransform cachedRectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (cachedRectTransform == null)
                    cachedRectTransform = GetComponent<RectTransform>();
                return cachedRectTransform;
            }
        }

        private CanvasGroup cachedCanvasGroup;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (cachedCanvasGroup == null)
                    cachedCanvasGroup = GetComponent<CanvasGroup>();
                return cachedCanvasGroup;
            }
        }

        
        private GraphicRaycaster cachedGraphicRaycaster;
        public GraphicRaycaster GraphicRaycaster
        {
            get
            {
                if (cachedGraphicRaycaster == null)
                    cachedGraphicRaycaster = this.GetOrAddComponent<GraphicRaycaster>();
                return cachedGraphicRaycaster;
            }
        }

        private WindowsManager windowsManager;
        public WindowsManager WindowsManager => windowsManager;

        private WindowID windowID;
        public WindowID WindowID => windowID;

        private bool isOpen;
        public bool IsOpen => isOpen;
        
        private bool initialized;
        public bool Initialized => initialized;

        private Coroutine closeRoutine;
        private Coroutine openRoutine;
        

        public void Initialize(WindowsManager targetWindowsManager, WindowID targetWindowID)
        {
            windowsManager = targetWindowsManager;
            windowID = targetWindowID;
            DispatchWindowInitialized();
            initialized = true;
        }

        private IEnumerator OpenEnumerator(Action<Window> callback)
        {
            if (disableInteractionWhileTransitioning)
                GraphicRaycaster.enabled = false;

            DispatchOnBeforeWindowOpen();
            gameObject.SetActive(true);

            yield return ExecuteTransitionEnumerator(TransitionType.TransitionIn);
            DispatchOnAfterWindowOpen();

            GraphicRaycaster.enabled = true;
            callback?.Invoke(this);
        }
       
        private IEnumerator CloseEnumerator(Action<Window> callback)
        {
            if (disableInteractionWhileTransitioning)
                GraphicRaycaster.enabled = false;
            
            DispatchOnBeforeWindowClose();

            yield return ExecuteTransitionEnumerator(TransitionType.TransitionOut);
            gameObject.SetActive(false);
            DispatchOnAfterWindowClose();
            
            GraphicRaycaster.enabled = true;
            callback?.Invoke(this);
        }
        
        private IEnumerator ExecuteTransitionEnumerator(TransitionType transitionType)
        {
            if(!windowID.TryGetTransition(transitionType, out AnimatedTransition animatedTransition, out bool playBackwards))
                yield break;

            animatedTransition.BeforeTransition(this);
            yield return animatedTransition.ExecuteEnumerator(this, transitionType, playBackwards);
        }


        public void Close(Action<Window> callback = null)
        {
            if (!isOpen)
                return;
            
            StopTransitionCoroutines();
            
            isOpen = false;
            closeRoutine = windowsManager.StartCoroutine(CloseEnumerator(callback));
        }

        public void Open(Action<Window> callback = null)
        {
            if (isOpen)
                return;

            StopTransitionCoroutines();
            
            isOpen = true;
            openRoutine = windowsManager.StartCoroutine(OpenEnumerator(callback));
        }

        public virtual void OnGainFocus()
        {
            DispatchOnGainFocus();
        }

        public void OnLostFocus()
        {
            DispatchOnLostFocus();
        }

        private void OnDestroy()
        {
            StopTransitionCoroutines();
        }

        private void StopTransitionCoroutines()
        {
            if (closeRoutine != null)
                StopCoroutine(closeRoutine);

            if (openRoutine != null)
                StopCoroutine(openRoutine);
            
            closeRoutine = null;
            openRoutine = null;
        }
    }
}