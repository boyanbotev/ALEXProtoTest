using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using Newtonsoft.Json.Linq;
using System.Threading;
using System;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;
using UnityEditor.VersionControl;
using static UnityEditor.Progress;
using UnityEditor.MPE;

namespace OpenAI
{
    public class StoryTest : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;

        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        private float height;
        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string systemPrompt = "You are a bestselling fiction writer with expert writing skills. You write punchy dialogue and explosive character interactions following the principles of 'Show Don't Tell.' You describe specific actions and never summarize. You never use adverbs. You are writing a story about a 14 year old schoolboy in Surbiton, UK, called Alex. When the user submits statistics about the main character, you interpret them describing Alex's life, showing his development in both positive or negative ways over time, depending on the statistics submitted by the user. Only respond with fiction text. Write in a direct, punchy, simple and popular style. Include recurring characters and show how Alex's relationships with them.\n\nHere is a style example:\nWHAM.An rugby ball knocked into Alex's head, sending him flying.\n'Catch, you muppet!' yelled Jordan, but Alex just scrambled to get out of the way as a mob of Year 8s charged towards him.\n";

        private void Start()
        {
            button.onClick.AddListener(MakeMessageRequest);

            var systemMessage = new ChatMessage()
            {
                Role = "system",
                Content = systemPrompt,
            };

            messages.Add(systemMessage);
        }

        private async void MakeMessageRequest()
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = inputField.text
            };

            AppendMessage(newMessage);

            messages.Add(newMessage);

            inputField.text = "";
            SetInputAllowed(false);

            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo",
                Messages = messages,
                FrequencyPenalty = 0.2f,
                PresencePenalty = 0.2f,
                MaxTokens = 750,
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();

                messages.Add(message);
                AppendMessage(message);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }

        private void AppendMessage(ChatMessage? message = null)
        {
            PreRebuildLayout();

            var item = Instantiate(message != null && message?.Role == "user" ? sent : received, scroll.content);

            if (message != null)
            {
                item.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Text>().text = message.Value.Content;
            }

            RebuildLayout(item);

            Debug.Log(message);
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
            return (RectTransform) scroll.content.GetChild(scroll.content.childCount - 1);
        }

        void SetInputAllowed(bool isAllowed)
        {
            button.enabled = isAllowed;
            inputField.enabled = isAllowed;
        }
    }

    // stats: Love, Anger/Rebelliousness, Depression, Comfort-seeking-ness
}
