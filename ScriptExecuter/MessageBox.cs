using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ScriptExecuter
{
    public class MessageBox
    {
        public static void Show(string title, string message, Action onConfirm = null, Action onCancel = null)
        {
            // Create Canvas
            GameObject canvasGameObject = new GameObject("MessageBoxCanvas");
            Canvas canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasGameObject.AddComponent<GraphicRaycaster>();

            // Ensure an EventSystem exists in the scene
            if (EventSystem.current == null)
            {
                GameObject eventSystemGameObject = new GameObject("EventSystem");
                eventSystemGameObject.AddComponent<EventSystem>();
                eventSystemGameObject.AddComponent<StandaloneInputModule>();
            }

            // Create Panel
            GameObject panelGameObject = new GameObject("MessageBoxPanel");
            RectTransform panelRect = panelGameObject.AddComponent<RectTransform>();
            panelRect.SetParent(canvas.transform, false);
            panelRect.sizeDelta = new Vector2(400, 200);
            panelRect.anchoredPosition = Vector2.zero;
            Image panelImage = panelGameObject.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark semi-transparent background

            // Create Title Text
            GameObject titleGameObject = new GameObject("TitleText");
            RectTransform titleRect = titleGameObject.AddComponent<RectTransform>();
            titleRect.SetParent(panelRect, false);
            titleRect.sizeDelta = new Vector2(380, 40);
            titleRect.anchoredPosition = new Vector2(0, 70);
            Text titleText = titleGameObject.AddComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 24;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.text = title;

            // Create Message Text
            GameObject messageGameObject = new GameObject("MessageText");
            RectTransform messageRect = messageGameObject.AddComponent<RectTransform>();
            messageRect.SetParent(panelRect, false);
            messageRect.sizeDelta = new Vector2(380, 80);
            messageRect.anchoredPosition = new Vector2(0, 0);
            Text messageText = messageGameObject.AddComponent<Text>();
            messageText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            messageText.fontSize = 16;
            messageText.color = Color.white;
            messageText.alignment = TextAnchor.MiddleCenter;
            messageText.text = message;

            // Create Confirm Button
            GameObject confirmButtonGameObject = new GameObject("ConfirmButton");
            RectTransform confirmButtonRect = confirmButtonGameObject.AddComponent<RectTransform>();
            confirmButtonRect.SetParent(panelRect, false);
            confirmButtonRect.sizeDelta = new Vector2(100, 30);
            confirmButtonRect.anchoredPosition = new Vector2(60, -70);
            Image confirmButtonImage = confirmButtonGameObject.AddComponent<Image>();
            confirmButtonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green button
            Button confirmButton = confirmButtonGameObject.AddComponent<Button>();
            confirmButton.onClick.AddListener(() =>
            {
                onConfirm?.Invoke();
                UnityEngine.Object.Destroy(canvasGameObject);
            });

            GameObject confirmTextGameObject = new GameObject("ConfirmText");
            RectTransform confirmTextRect = confirmTextGameObject.AddComponent<RectTransform>();
            confirmTextRect.SetParent(confirmButtonRect, false);
            confirmTextRect.sizeDelta = Vector2.zero;
            confirmTextRect.anchorMin = Vector2.zero;
            confirmTextRect.anchorMax = Vector2.one;
            Text confirmButtonText = confirmTextGameObject.AddComponent<Text>();
            confirmButtonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            confirmButtonText.fontSize = 18;
            confirmButtonText.color = Color.white;
            confirmButtonText.alignment = TextAnchor.MiddleCenter;
            confirmButtonText.text = "Confirm";

            // Create Cancel Button (if onCancel is provided)
            if (onCancel != null)
            {
                GameObject cancelButtonGameObject = new GameObject("CancelButton");
                RectTransform cancelButtonRect = cancelButtonGameObject.AddComponent<RectTransform>();
                cancelButtonRect.SetParent(panelRect, false);
                cancelButtonRect.sizeDelta = new Vector2(100, 30);
                cancelButtonRect.anchoredPosition = new Vector2(-60, -70);
                Image cancelButtonImage = cancelButtonGameObject.AddComponent<Image>();
                cancelButtonImage.color = new Color(0.6f, 0.2f, 0.2f, 1f); // Red button
                Button cancelButton = cancelButtonGameObject.AddComponent<Button>();
                cancelButton.onClick.AddListener(() =>
                {
                    onCancel?.Invoke();
                    UnityEngine.Object.Destroy(canvasGameObject);
                });

                GameObject cancelTextGameObject = new GameObject("CancelText");
                RectTransform cancelTextRect = cancelTextGameObject.AddComponent<RectTransform>();
                cancelTextRect.SetParent(cancelButtonRect, false);
                cancelTextRect.sizeDelta = Vector2.zero;
                cancelTextRect.anchorMin = Vector2.zero;
                cancelTextRect.anchorMax = Vector2.one;
                Text cancelButtonText = cancelTextGameObject.AddComponent<Text>();
                cancelButtonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                cancelButtonText.fontSize = 18;
                cancelButtonText.color = Color.white;
                cancelButtonText.alignment = TextAnchor.MiddleCenter;
                cancelButtonText.text = "Cancel";
            }
        }
    }
}