using UnityEngine;
using UnityEngine.UI;
using System;

namespace OpenAI
{
    public class UIManager : MonoBehaviour
    {
        public static event Action<string> onButtonClick;

        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;

        [SerializeField] private RectTransform sentPrefab;
        [SerializeField] private RectTransform receivedPrefab;

        private float height;

        private void Start()
        {
            button.onClick.AddListener(MakeRequest);
        }

        private void MakeRequest()
        {
            onButtonClick?.Invoke(inputField.text);
        }

        public void AppendMessage(ChatMessage? message = null)
        {
            PreRebuildLayout();

            var item = Instantiate(message != null && message?.Role == "user" ? sentPrefab : receivedPrefab, scroll.content);

            if (message != null)
            {
                item.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = message.Value.Content;
            }

            RebuildLayout(item);

            Debug.Log(message.Value.Content);
        }

        private void PreRebuildLayout()
        {
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }

        private void RebuildLayout(RectTransform item)
        {
            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;
        }

        public RectTransform GetLatestMessageUI()
        {
            return (RectTransform)scroll.content.GetChild(scroll.content.childCount - 1);
        }

        public void SetInputAllowed(bool isAllowed)
        {
            button.enabled = isAllowed;
            inputField.enabled = isAllowed;
        }

        public void ClearInputText()
        {
            inputField.text = "";
        }
    }
}
