using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Sandbox.PolyLabel
{
    public class DebugView : MonoBehaviour
    {
        [Header("STATS"), SerializeField]
        private FpsStats fpsStats;

        [Header("BUTTONS"), SerializeField]
        private Button generateButton;

        [SerializeField]
        private Button zoomButton;

        [Header("TEXTS"), SerializeField]
        private Text infoText;

        public event Action OnGenerate;
        public event Action OnZoom;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            OnDemandRendering.renderFrameInterval = 1;
        }

        private void OnEnable()
        {
            generateButton.onClick.AddListener(OnGenerateEvent);
            zoomButton.onClick.AddListener(OnZoomEvent);

            fpsStats.Run();
        }

        private void OnDisable()
        {
            generateButton.onClick.RemoveAllListeners();
            zoomButton.onClick.RemoveAllListeners();

            fpsStats.Stop();
        }

        public void SetInfoText(string text)
        {
            infoText.text = text;
        }

        // Events

        private void OnGenerateEvent()
        {
            OnGenerate?.Invoke();
        }

        private void OnZoomEvent()
        {
            OnZoom?.Invoke();
        }
    }
}