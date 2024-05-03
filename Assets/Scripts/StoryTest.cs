using UnityEngine;
using System.Collections.Generic;

namespace OpenAI
{
    public class StoryTest : MonoBehaviour
    {
        [SerializeField] int comfortSeeking = 8;
        [SerializeField] int depression = 2;
        [SerializeField] int love = 0;
        [SerializeField] int anger = 0;
        [SerializeField] int schizophrenia = 0;
        [SerializeField] string currentConcept = "";
        [SerializeField] int spicyness = 75;

        private readonly int loserThreshold = 2;
        private readonly int murderThreshold = 5;
        private readonly int spiceRandomizationCeiling = 15;

        private readonly OpenAIApiGateway apiGateway = new();
        private UIManager uiManager;

        private readonly List<string> possibleConcepts = new();
        private readonly List<string> chosenConcepts = new();
        private readonly string systemPrompt = "You are a bestselling fiction writer with expert writing skills. You write punchy dialogue and explosive character interactions following the principles of 'Show Don't Tell.' You describe specific actions and never summarize. You never use adverbs. You are writing a story about a 14 year old schoolboy in Surbiton, UK, called Alex, the kind of boy who could end up becoming a school shooter. When the user submits statistics about the main character, you interpret them describing Alex's life, showing his development in both positive or negative ways over time, depending on the statistics submitted by the user. Only respond with fiction text. Write in a direct, punchy, simple and popular style. Include recurring characters and show how Alex's relationships with them develop. Each section should develop plot threads from previous sections.\nHere is a style example:\nWHAM.An rugby ball knocked into Alex's head, sending him flying.\n'Catch, you muppet!' yelled Jordan, but Alex just scrambled to get out of the way as a mob of Year 8s charged towards him.\n";
        private readonly string conceptSystemPrompt = "Choose words or phrases that the events in this part of the story may trigger Alex to think about. For example, to represent the idea of rebelling against authortiy, you can choose 'rebel' or 'fight-the-system'. Try to choose simple words: instead of 'isolation,' choose 'lonely.' List the words separated by commas in lowercase, and if you have a phrase, connect each word with a dash. For example:\n girlfriend, bully, guitar, artist, parallel-universes.\nHere's another example. If the text was \n'WHAM. A rugby ball knocked into Alex's head, sending him flying.\n'Catch, you muppet!' yelled Jordan, but Alex just scrambled to get out of the way as a mob of Year 8s charged towards him.' we would return:\nJordan,rugby, muppet.\nMake sure the words are specific to the events in the story and send the story in an interesting direction. Most importantly - do not choose abstract nouns. The best choices are physical objects such as guitar, activities like rubgy or areas of interest like philosophy, or new identity labels like rocker, loner, genius. Make sure the words or phrases you choose are NOT included in the following list:\n";
        
        private readonly List<string> spiceDirectives = new()
        {
            "Add a new enemy",
            "Add conflict",
            "Alex meets a brutal setback",
            "include extremely mundane grounded description",
            "include unexpected changes at home",
            "Include disturbing developments",
            "Describe a sadistic teacher",
            "haircut: -500",
            "Alex-smelliness: 3",
            "someone calls Alex a gaylord",
            "Add punchy action and dialogue beats",
        };

        private readonly List<string> loveDirectives = new()
        {
            "Add a new love interest",
            "Alex's love interest cheats in an immature way",
            "Alex's love interest struggles with her own silly problem",
            "kiss",
            "Alex's love interest harshly criticises Alex",
            "conflict with love interest",
        };

        private readonly List<string> loserDirectives = new()
        {
            "No hope, no silver lining",
            "Describe brutal ostracism",
            "Alex loses a friend",
            "Describe harsh and shocking events",
            "Include disturbing developments",
            "Everything goes terribly wrong",
        };

        private readonly List<string> schizophreniaDirectives = new()
        {
            "Alex hears voices",
            "a hallucination",
            "Alex loses touch with reality",
            "Alex can't sleep",
        };

        private readonly List<string> murderDirectives = new()
        {
            "Alex thinks about hurting others",
            "Alex fantasizes about taking revenge",
            "Alex imagines murdering someone",
            "Alex makes plans to take revenge",
            "Alex puts his plans into action",
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
            apiGateway.CreateMessage(systemPrompt, "system");
        }

        private async void RequestStory(string inputText)
        {
            AddUserMessage();

            uiManager.ClearInputText();
            uiManager.SetInputAllowed(false);

            var message = await apiGateway.MakeStoryRequest();

            if (message != null)
            {
                AddMessage(message.Value);
                GetConcepts(message.Value);
            }

            uiManager.SetInputAllowed(true);
        }

        async void GetConcepts(ChatMessage message)
        {
            var conceptResponse = await apiGateway.MakeConceptRequest(message, conceptSystemPrompt + string.Join(", ", chosenConcepts));
            string[] words = SeparateWords(conceptResponse.Value.Content);

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

        private void AddUserMessage()
        {
            string messageText = FormatUserMessage();
            var message = apiGateway.CreateMessage(messageText, "user");
            uiManager.AppendMessage(message);
        }

        string FormatUserMessage()
        {
            string messageText = "comfort-seeking-ness: " + comfortSeeking
                + "/10, depression: " + depression + "/10, love: " + love + "/10, anger: " + anger + "/10";

            if (currentConcept != "")
            {
                messageText += ", Alex-slightly-thinking-about: " + currentConcept;
                chosenConcepts.Add(currentConcept);
                currentConcept = "";
            }

            if (ShouldAddSpice())
            {
                messageText += ", also: " + GetRandomSpiceDirective();
            }

            return messageText;
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
            int loserIndex = depression + comfortSeeking - love;

            var randomInt = Random.Range(0, spiceRandomizationCeiling);

            bool shouldChooseMurder = randomInt < murderIndex && anger > murderThreshold;
            bool shouldChooseSchizophrenia = randomInt < schizophrenia;
            bool shouldChooseLoser = randomInt < loserIndex && depression > loserThreshold;
            bool shouldChooseLove = randomInt < love;

            Debug.Log("randomInt: " + randomInt + ", loser index: " + loserIndex + ", murder index:" + murderIndex);

            if (shouldChooseMurder) return murderDirectives;
            else if (shouldChooseSchizophrenia) return schizophreniaDirectives;
            else if (shouldChooseLoser) return loserDirectives;
            else if (shouldChooseLove) return loveDirectives;
            else return spiceDirectives;
        }

        private bool ShouldAddSpice()
        {
            if (apiGateway.messages.Count == 1)
            {
                return false;
            }
            else
            {
                return Random.Range(0, 100) < spicyness;
            }
        }

        private void AddMessage(ChatMessage message)
        {
            apiGateway.AddMessage(message);
            uiManager.AppendMessage(message);
        }

        string[] SeparateWords(string inputText)
        {
            char[] separators = { ',', '.', ' ' };
            return inputText.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
        }
    }
}