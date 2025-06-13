using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using Plugins;
// TODO 3.4: Add using directive for Filters


#pragma warning disable SKEXP0050

// Create kernel
var builder = Kernel.CreateBuilder();

string deploymentName = Environment.GetEnvironmentVariable("deploymentname", EnvironmentVariableTarget.Machine)!;
string endpoint = Environment.GetEnvironmentVariable("endpoint", EnvironmentVariableTarget.Machine)!;
string apiKey = Environment.GetEnvironmentVariable("apiKey", EnvironmentVariableTarget.Machine)!;

builder.Services.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
// TODO 3.2: register native plugin with the kernel
builder.Plugins.AddFromType<>();

var kernel = builder.Build();
// TODO 3.5: register LogFilter with kernel



// Create chat history
ChatHistory history = [];

// Get chat completion service
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Start the conversation
while (true)
{
    // Get user input
    Console.Write("User > ");
    history.AddUserMessage(Console.ReadLine()!);

    // TODO 3.3 enable auto function calling
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = 
    };

    // Get the response from the AI
    var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Stream the results
    string fullMessage = "";
    var first = true;
    await foreach (var content in result)
    {
        if (content.Role.HasValue && first)
        {
            Console.Write("Assistant > ");
            first = false;
        }
        Console.Write(content.Content);
        fullMessage += content.Content;
    }
    Console.WriteLine();

    // Add the message from the agent to the chat history
    history.AddAssistantMessage(fullMessage);
}