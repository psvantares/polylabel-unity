using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.PolyLabel
{
    public class FpsStats : MonoBehaviour
    {
        private enum DisplayMode
        {
            FPS,
            MS
        }

        [SerializeField]
        private Text statsText;

        [SerializeField]
        private DisplayMode displayMode = DisplayMode.FPS;

        [SerializeField, Range(0.1f, 2f)]
        private float sampleDuration = 1f;

        private bool isActive;
        private int frames;
        private float duration;
        private float bestDuration = float.MaxValue;
        private float worstDuration;
        private readonly StringBuilder sb = new(300);

        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            var frameDuration = Time.unscaledDeltaTime;
            frames += 1;
            duration += frameDuration;

            if (frameDuration < bestDuration)
            {
                bestDuration = frameDuration;
            }

            if (frameDuration > worstDuration)
            {
                worstDuration = frameDuration;
            }

            if (duration >= sampleDuration)
            {
                float best;
                float average;
                float worst;
                string header;

                if (displayMode == DisplayMode.FPS)
                {
                    // FPS
                    average = (int)(frames / duration);
                    best = (int)(1f / bestDuration);
                    worst = (int)(1f / worstDuration);
                    header = "FPS";
                }
                else
                {
                    // MS
                    average = 1000f * duration / frames;
                    best = 1000f * bestDuration;
                    worst = 1000f * worstDuration;
                    header = "MS";
                }

                sb.AppendLine($"<color=green>{header} COUNTER</color>");
                sb.AppendLine("<color=orange>AVG:</color> " + average);
                sb.AppendLine("<color=orange>MAX:</color> " + best);
                sb.AppendLine("<color=orange>MIN:</color> " + worst);

                statsText.text = sb.ToString();
                sb.Clear();

                frames = 0;
                duration = 0f;
                bestDuration = float.MaxValue;
                worstDuration = 0f;
            }
        }

        public void Run()
        {
            sb.Clear();

            frames = 0;
            duration = 0f;
            bestDuration = float.MaxValue;
            worstDuration = 0f;
            isActive = true;
        }

        public void Stop()
        {
            isActive = false;
        }
    }
}