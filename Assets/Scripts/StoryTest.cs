using UnityEngine;
using System.Collections.Generic;

namespace OpenAI
{
    public class StoryTest : MonoBehaviour
    {
        [SerializeField] int comfortSeekingNess = 8;
        [SerializeField] int depression = 2;
        [SerializeField] int love = 0;
        [SerializeField] int anger = 0;
        [SerializeField] string currentConcept = "";
        [SerializeField] int spicyness = 75;

        private OpenAIApi openai = new OpenAIApi();
        private UIManager uiManager;

        private List<ChatMessage> messages = new List<ChatMessage>();
        private List<string> possibleConcepts = new List<string>();
        private List<string> chosenConcepts = new List<string>();
        private string systemPrompt = "You are a bestselling fiction writer with expert writing skills. You write punchy dialogue and explosive character interactions following the principles of 'Show Don't Tell.' You describe specific actions and never summarize. You never use adverbs. You are writing a story about a 14 year old schoolboy in Surbiton, UK, called Alex, the kind of boy who could end up becoming a school shooter. When the user submits statistics about the main character, you interpret them describing Alex's life, showing his development in both positive or negative ways over time, depending on the statistics submitted by the user. Only respond with fiction text. Write in a direct, punchy, simple and popular style. Include recurring characters and show how Alex's relationships with them develop. Each section should develop plot threads from previous sections.\nHere is a style example:\nWHAM.An rugby ball knocked into Alex's head, sending him flying.\n'Catch, you muppet!' yelled Jordan, but Alex just scrambled to get out of the way as a mob of Year 8s charged towards him.\n";
        private string conceptSystemPrompt = "Choose words or phrases that the events in this part of the story may trigger Alex to think about. For example, to represent the idea of rebelling against authortiy, you can choose 'rebel' or 'fight-the-system'. Try to choose simple words: instead of 'isolation,' choose 'lonely.' List the words separated by commas in lowercase, and if you have a phrase, connect each word with a dash. For example:\n girlfriend, bully, guitar, artist, parallel-universes.\nHere's another example. If the text was \n'WHAM. A rugby ball knocked into Alex's head, sending him flying.\n'Catch, you muppet!' yelled Jordan, but Alex just scrambled to get out of the way as a mob of Year 8s charged towards him.' we would return:\nJordan,rugby, muppet.\nMake sure the words are specific to the events in the story and send the story in an interesting direction. Most importantly - do not choose abstract nouns. The best choices are physical objects such as guitar, activities like rubgy or areas of interest like philosophy, or new identity labels like rocker, loner, genius. Make sure the words or phrases you choose are NOT included in the following list:\n";
        
        private List<string> spiceDirectives = new List<string>
        {
            "Add a new love interest", // should only be called if love is greater than 1?
            "Add a new enemy",
            "Add conflict",
            "Alex meets a brutal setback",
            "include extremely mundane grounded description",
            "Include unexpected dangers",
            "include unexpected changes at home",
            "Include disturbing developments",
            "The story takes a surprising turn",
            "Describe a sadistic teacher",
            "Describe a controlling teacher",
            "haircut: -500",
            "smelliness: 3",
            "someone calls Alex a gaylord",
            "Add punchy action and dialogue beats",
        };

        private List<string> loserDirectives = new List<string>
        {
            "No hope, no silver lining", // need depression to be above certain values
            "Describe brutal ostracism", // need depression or comfort to be above certain values
            "Alex loses a friend", // need depression or comfort to be above certain values
            "Describe harsh and shocking events",
            "Include disturbing developments",
            "Everything goes terribly wrong",
        };

        private List<string> schizophreniaDirectives = new List<string>
        {
            "Alex hears voices",
            "a hallucination",
            "a sudden impulse",
        };

        private List<string> murderDirectives = new List<string>
        {
            "Alex thinks about hurting others",
            "Alex fantasizes about taking revenge",
            "Alex imagines murdering someone",
            "Alex makes plans to take revenge",
            "Alex puts his plans into action",
            "Alex does the brutal act",
        };

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
            AddUserMessage();

            uiManager.ClearInputText();
            uiManager.SetInputAllowed(false);

            var message = await MakeRequest(new CreateChatCompletionRequest()
            {
                Model = "gpt-4-turbo",
                Messages = messages,
                FrequencyPenalty = 0.1f,
                PresencePenalty = 0.1f,
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
            var chosenConceptsString = string.Join(", ", chosenConcepts);
            var conceptSystemMessage = new ChatMessage()
            {
                Role = "system",
                Content = conceptSystemPrompt + chosenConceptsString,
            };

            Debug.Log(conceptSystemMessage.Content);

            List<ChatMessage> conceptMessageList = new List<ChatMessage>
            {
                conceptSystemMessage,
                message,
            };

            var conceptResponse = await MakeRequest(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo",
                Messages = conceptMessageList,
                MaxTokens = 10,
            });

            char[] separators = { ',', '.', ' ' };
            string[] words = conceptResponse.Value.Content.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);

            possibleConcepts.Clear();

            foreach (var word in words)
            {
                if (!chosenConcepts.Contains(word))
                {
                    Debug.Log(word);
                    possibleConcepts.Add(word);
                } 
                else
                {
                    Debug.Log(word + "is already in chosen concepts");
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

        private void AddUserMessage()
        {
            string messageText = "comfort-seeking-ness: " + comfortSeekingNess 
                + ", depression: " + depression + ", love: " + love + ", anger: " + anger;

            if (currentConcept != "")
            {
                messageText += ", Alex-slightly-thinking-about: " + currentConcept;
                chosenConcepts.Add(currentConcept);
                currentConcept = "";
            }

            if (ShouldAddSpice()) {
                messageText += ", also: " + GetRandomSpiceDirective();
            }

            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = messageText, 
            };

            AddMessage(newMessage);
        }

        private string GetRandomSpiceDirective()
        {
            var directivePool = GetDirectivePool();

            var randomInt = Random.Range(0, directivePool.Count);
            return directivePool.ToArray()[randomInt];
        }

        List<string> GetDirectivePool()
        {
            int murderIndex = depression + anger - love;
            int loserIndex = depression + comfortSeekingNess - love;

            var randomInt = Random.Range(0, 15);
            Debug.Log(randomInt);

            bool shouldChooseMurder = randomInt < murderIndex && anger > 5;
            bool shouldChooseLoser = randomInt < loserIndex && depression > 2;

            Debug.Log("loser index" + loserIndex + shouldChooseLoser);

            if (shouldChooseMurder) return murderDirectives;
            else if (shouldChooseLoser) return loserDirectives;
            else return spiceDirectives;
        }

        private bool ShouldAddSpice()
        {
            if (messages.Count == 1)
            {
                return false;
            }
            else
            {
                return Random.Range(0, 100) < spicyness;
            }
        }

        void AddMessage(ChatMessage message)
        {
            uiManager.AppendMessage(message);
            messages.Add(message);
        }
    }
}