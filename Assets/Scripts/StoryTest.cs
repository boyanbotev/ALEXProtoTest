using UnityEngine;
using System.Collections.Generic;
using UnityEditor.VersionControl;

namespace OpenAI
{
    public class StoryTest : MonoBehaviour
    {
        private OpenAIApi openai = new OpenAIApi();
        private UIManager uiManager;

        private List<ChatMessage> messages = new List<ChatMessage>();
        private List<string> activeConcepts = new List<string>();
        private string systemPrompt = "You are a bestselling fiction writer with expert writing skills. You write punchy dialogue and explosive character interactions following the principles of 'Show Don't Tell.' You describe specific actions and never summarize. You never use adverbs. You are writing a story about a 14 year old schoolboy in Surbiton, UK, called Alex, the kind of boy who could end up becoming a school shooter. When the user submits statistics about the main character, you interpret them describing Alex's life, showing his development in both positive or negative ways over time, depending on the statistics submitted by the user. Only respond with fiction text. Write in a direct, punchy, simple and popular style. Include recurring characters and show how Alex's relationships with them develop. Each section should develop plot threads from previous sections.\nHere is a style example:\nWHAM.An rugby ball knocked into Alex's head, sending him flying.\n'Catch, you muppet!' yelled Jordan, but Alex just scrambled to get out of the way as a mob of Year 8s charged towards him.\n";
        private string conceptSystemPrompt = "Choose words or phrases that the events in this part of the story may trigger Alex to think about. For example, to represent the idea of rebelling against authortiy, you can choose 'rebel' or 'fight-the-system'. Try to choose simple words: instead of 'isolation,' choose 'lonely.' List the words separated by commas in lowercase, and if you have a phrase, connect each word with a dash. For example:\n girlfriend, bully, guitar, artist, parallel-universes.\nHere's another example. If the text was \n'WHAM. A rugby ball knocked into Alex's head, sending him flying.\n'Catch, you muppet!' yelled Jordan, but Alex just scrambled to get out of the way as a mob of Year 8s charged towards him.' we would return:\nJordan,rugby, muppet.\nMake sure the words are specific to the events in the story and send the story in an interesting direction. Most importantly - do not choose abstract nouns. The best choices are physical objects such as guitar, activities like rubgy or areas of interest like philosophy, or new identity labels like rocker, loner, genius. Make sure the words or phrases you choose are NOT included in the following list:\n";

        private void OnEnable()
        {
            UIManager.onButtonClick += RequestStory;
        }

        private void OnDisable()
        {
            UIManager.onButtonClick -= RequestStory;
        }

        private void Start()
        {
            uiManager = GetComponent<UIManager>();

            var systemMessage = new ChatMessage()
            {
                Role = "system",
                Content = systemPrompt,
            };

            messages.Add(systemMessage);
        }

        private async void RequestStory(string inputText)
        {
            AddUserMessage(inputText);

            uiManager.ClearInputText();
            uiManager.SetInputAllowed(false);

            var message = await MakeRequest(new CreateChatCompletionRequest()
            {
                Model = "gpt-4-turbo",
                Messages = messages,
                FrequencyPenalty = 0.2f,
                PresencePenalty = 0.2f,
                MaxTokens = 750,
            });

            if (message != null)
            {
                AddMessage(message.Value);
                GetConcepts(message.Value);
            }

            uiManager.SetInputAllowed(true);
        }

        async void GetConcepts(ChatMessage message)
        {
            var activeConceptsString = activeConcepts.ToString();
            var conceptSystemMessage = new ChatMessage()
            {
                Role = "system",
                Content = conceptSystemPrompt + activeConceptsString,
            };

            List<ChatMessage> conceptMessageList = new List<ChatMessage>
            {
                conceptSystemMessage,
                message,
            };

            var conceptMessage = await MakeRequest(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo",
                Messages = conceptMessageList,
                MaxTokens = 50,
            });

            char[] separators = { ',', '.', ' ' };
            string[] words = conceptMessage.Value.Content.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                // Actually we only add if the user chooses it
                if (!activeConcepts.Contains(word))
                {
                    Debug.Log(word);
                    activeConcepts.Add(word);
                } 
                else
                {
                    Debug.Log(word + "is already in active concepts");
                }
            }
        }

        async System.Threading.Tasks.Task<ChatMessage?> MakeRequest(CreateChatCompletionRequest request)
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

        private void AddUserMessage(string inputText)
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = inputText
            };

            AddMessage(newMessage);
        }

        void AddMessage(ChatMessage message)
        {
            uiManager.AppendMessage(message);
            messages.Add(message);
        }
    }
}
// comfort-seeking-ness: 8/10, depression: 2/10, love/friendship: 1/10, anger/rebelliousness: 1/10