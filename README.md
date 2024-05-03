## Alex

Alex is an interactive story powered by OpenAI's GPT-4. Your actions decide the course of Alex’s life. What you do changes who he will become – a schizophrenic? A stay at home loser? A nerd at the top of his class… or even a cool kid??
It is all down to you and the inner warfare between different parts of Alex’s soul.

### OpenAI-Unity Package
This project is made using the OpenAI-Unity Package: https://github.com/srcnalt/OpenAI-Unity
In order to run the project, you need to use your API key and organization name (if applicable). To avoid exposing your API key in your Unity project, you can save it in your device's local storage.

To do this, follow these steps:

- Create a folder called .openai in your home directory (e.g. `C:User\UserName\` for Windows or `~\` for Linux or Mac)
- Create a file called `auth.json` in the `.openai` folder
- Add an api_key field and a organization field (if applicable) to the auth.json file and save it
- Here is an example of what your auth.json file should look like:

```json
{
    "api_key": "sk-...W6yi",
    "organization": "org-...L7W"
}
```