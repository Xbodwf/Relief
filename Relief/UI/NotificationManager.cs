using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Relief.UI
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        private void Start()
        {
            bool flag = NotificationManager.Instance == null;
            if (flag)
            {
                NotificationManager.Instance = this;
                UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
            }
            else
            {
                UnityEngine.Object.Destroy(base.gameObject);
            }
        }

        public void ShowNotification(string message, float dur = 1f,float size = 32f)
        {
            bool flag = false ;
            if (!flag)
            {
                bool flag2 = this._canvas == null;
                if (flag2)
                {
                    GameObject gameObject = new GameObject("UI Canvas");
                    this._canvas = gameObject.AddComponent<Canvas>();
                    this._canvas.renderMode = 0;
                }
                this._canvas.sortingOrder = int.MaxValue;
                bool flag3 = this._notificationText == null;
                if (flag3)
                {
                    this._notificationText = new GameObject("Notification Text").AddComponent<TextMeshProUGUI>();
                    this._notificationText.transform.SetParent(this._canvas.transform, false);
                    try
                    {
                        this._notificationText.font = RDString.fontData.fontTMP;
                        this._notificationText.fontStyle = 0;
                    }
                    catch
                    {
                    }
                    this._notificationText.color = Color.white;
                    this._notificationText.alignment = TextAlignmentOptions.TopLeft;
                    this._notificationText.rectTransform.anchorMin = new Vector2(0f, 1f);
                    this._notificationText.rectTransform.anchorMax = new Vector2(0f, 1f);
                    this._notificationText.rectTransform.pivot = new Vector2(0f, 1f);
                    this._notificationText.rectTransform.anchoredPosition = new Vector2(10f, -10f);
                    this._notificationText.fontMaterial.EnableKeyword("UNDERLAY_ON");
                    this._notificationText.fontMaterial.SetFloat("_UnderlayOffsetX", 0.5f);
                    this._notificationText.fontMaterial.SetFloat("_UnderlayOffsetY", -0.5f);
                    this._notificationText.fontMaterial.SetFloat("_UnderlayDilate", 0.5f);
                    this._notificationText.fontMaterial.SetFloat("_UnderlaySoftness", 0.5f);
                    this._notificationText.enabled = false;
                }
                bool flag4 = this._currentFadeOutCoroutine != null;
                if (flag4)
                {
                    base.StopCoroutine(this._currentFadeOutCoroutine);
                    this._notificationText.enabled = false;
                }
                this._notificationText.text = message;
                this._notificationText.enabled = true;
                this._notificationText.fontSize = size;
                this._notificationText.enableWordWrapping = true;
                this._notificationText.rectTransform.sizeDelta = new Vector2((float)(Screen.width / 2 + 1), -1f);
                this._notificationText.color = new Color(this._notificationText.color.r, this._notificationText.color.g, this._notificationText.color.b, 1f);
                this._currentFadeOutCoroutine = base.StartCoroutine(this.FadeOutText(dur));
            }
        }

        private IEnumerator FadeOutText(float dur)
        {
            float elapsedTime = 0f;
            Color textColor = this._notificationText.color;
            while (elapsedTime < dur + this.fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float alpha = 1f;
                bool flag = elapsedTime < dur;
                if (flag)
                {
                    alpha = 1f;
                }
                else
                {
                    alpha = 1f - (elapsedTime - dur) / this.fadeDuration;
                }
                textColor.a = alpha;
                this._notificationText.color = textColor;
                yield return null;
            }
            this._notificationText.enabled = false;
            yield break;
        }

        private TextMeshProUGUI _notificationText;

        private Canvas _canvas;

        public float fadeDuration = 1f;

        private Coroutine _currentFadeOutCoroutine;
    }
}
