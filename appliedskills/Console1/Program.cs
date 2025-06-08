using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.SemanticKernel.Connectors.OpenAI;

string deploymentName = Environment.GetEnvironmentVariable("deploymentname", EnvironmentVariableTarget.Machine)!;
string endpoint = Environment.GetEnvironmentVariable("endpoint", EnvironmentVariableTarget.Machine)!;
string apiKey = Environment.GetEnvironmentVariable("apiKey", EnvironmentVariableTarget.Machine)!;

var kernel = Kernel.CreateBuilder()
                      .AddAzureOpenAIChatCompletion(
                            deploymentName, 
                            endpoint,
                            apiKey)
                      .Build();

ChatHistory history = [];
List<string> choices = ["ContinueConversation", "EndConversation"];

// Create few-shot examples
List<ChatHistory> fewShotExamples = [
// TODO 2.1 Create few-shot examples
// [
//     new ChatMessageContent(AuthorRole.User, "Can you send a very quick approval to the marketing team?"),
//     new ChatMessageContent(AuthorRole.Assistant, "ContinueConversation")
// ]
    string prompt = $" Can you";
];


// Create handlebars template for intent
// TODO 2.2 Add few shot examples template and chat history template
var getIntent = kernel.CreateFunctionFromPrompt(
    new()
    {
        Template = @"
<message role=""system"">Instructions: What is the intent of this request?
Do not explain the reasoning, just reply back with the intent. If you are unsure, reply with {{choices[0]}}.
Choices: {{choices}}.</message>

{{#each fewShotExamples}}

{{/each}}

{{#each chatHistory}}

{{/each}}

<message role=""user"">{{request}}</message>",
        TemplateFormat = "handlebars"
    },
    new HandlebarsPromptTemplateFactory()
);

// TODO 2.3 Create a template for chat by including the history, request, and assistant response
string chatTemplate = @"{{$history}}
    User: {{$request}}
    Assistant: ";
var chat = ;


// Start the chat loop
while (true)
{
    Console.Write("User > ");
    var request = Console.ReadLine();

    // Invoke prompt
    var intent = await kernel.InvokeAsync(
        getIntent,
        new() {
            { "request", request },
            { "choices", choices },
            { "history", history },
            { "fewShotExamples", fewShotExamples }
        }
    );

    // End the chat if the intent is "EndConversation"
    if (intent.ToString() == "EndConversation")
    {
        break;
    }

    // Get chat response
    var chatResult = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
        chat,
        new() {
            { "request", request },
            { "history", string.Join("\n", history.Select(x => x.Role + ": " + x.Content)) }
        }
    );

    // Stream the response
    string message = "";
    await foreach (var chunk in chatResult)
    {
        if (chunk.Role.HasValue) Console.Write(chunk.Role + " > ");
        message += chunk;
        Console.Write(chunk);
    }
    Console.WriteLine();

    history.AddUserMessage(request!);
    // TODO 2.4 Append the assistant message to history
    history.;
}