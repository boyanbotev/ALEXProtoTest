using UnityEngine;
using System.Collections.Generic;

namespace OpenAI
{
    public class OpenAIApiGateway 
    { 
        private readonly OpenAIApi openai = new();
        public List<ChatMessage> messages = new();

        public async System.Threading.Tasks.Task<ChatMessage?> MakeStoryRequest()
        {
            var message = await MakeRequest(new CreateChatCompletionRequest()
            {
                Model = "gpt-4-turbo",
                Messages = messages,
                FrequencyPenalty = 0.1f,
                PresencePenalty = 0.1f,
                MaxTokens = 750,
            });

            return message;
        }

        public async System.Threading.Tasks.Task<ChatMessage?> MakeConceptRequest(ChatMessage messageToConceptualize, string systemPrompt)
        {
            var conceptSystemMessage = new ChatMessage()
            {
                Role = "system",
                Content = systemPrompt,
            };

            Debug.Log(conceptSystemMessage.Content);

            List<ChatMessage> conceptMessageList = new List<ChatMessage>
            {
                conceptSystemMessage,
                messageToConceptualize,
            };

            var conceptResponse = await MakeRequest(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo",
                Messages = conceptMessageList,
                MaxTokens = 10,
            });

            return conceptResponse;
        }

        private async System.Threading.Tasks.Task<ChatMessage?> MakeRequest(CreateChatCompletionRequest request)
        {
            var completionResponse = await openai.CreateChatCompletion(request);

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();

                return message;
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
                return null;
            }
        }

        public ChatMessage CreateMessage(string text, string role)
        {
            var message = new ChatMessage()
            {
                Role = role,
                Content = text,
            };

            messages.Add(message);

            return message;
        }

        public void AddMessage(ChatMessage message)
        {
            messages.Add(message);
        }
    }
}