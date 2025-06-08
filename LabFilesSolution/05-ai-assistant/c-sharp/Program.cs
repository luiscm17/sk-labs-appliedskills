using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.Diagnostics.Contracts;
using System.Reflection;

string filePath = Path.GetFullPath("appsettings.json");
var config = new ConfigurationBuilder()
.AddJsonFile(filePath)
.Build();

// Set your values in appsettings.json
var apikey = config["OpenAI:apikey"]!;
var endpoint = config["OpenAI:endpoint"]!;
var model = config["OpenAI:model"]!;

// Create a kernel builder with Azure OpenAI chat completion
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    model ?? throw new ArgumentNullException(nameof(model), "Model deployment name cannot be null"),
    endpoint ?? throw new ArgumentNullException(nameof(endpoint), "Endpoint cannot be null"),
    apikey ?? throw new ArgumentNullException(nameof(apikey), "API key cannot be null")
);
var kernel = builder.Build();

// Import plugins to the kernel
kernel.ImportPluginFromType<DevopsPlugin>();

// Create prompt execution settings
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Create chat history
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
ChatHistory chatHistory = [];

// Create a kernel function to deploy the staging environment
var deployStageFunction = kernel.CreateFunctionFromPrompt(
promptTemplate: @"This is the most recent build log:
{{DevopsPlugin.ReadLogFile}}

If there are errors, do not deploy the stage environment. Otherwise, invoke the stage deployment function",
functionName: "DeployStageEnvironment",
description: "Deploy the staging environment"
);
kernel.Plugins.AddFromFunctions("DeployStageEnvironment", [deployStageFunction]);

// Create a handlebars prompt
string hbprompt = """
    <message role="system">Instructions: Before creating a new branch for a user,
    request the new branch name and base branch name</message>
    <message role="user">Can you create a new branch?</message>
    <message role="assistant">Sure, what would you like to name your branch? And which
    base branch would you like to use?</message>
    <message role="user">{{input}}</message>
    <message role="assistant">
    """;

// Create the prompt template config using handlebars format
var templateFactory = new HandlebarsPromptTemplateFactory();
var promptTemplateConfig = new PromptTemplateConfig()
{
    Template = hbprompt,
    TemplateFormat = "handlebars",
    Name = "CreateBranch"
};

// Create a plugin function from the prompt
var promptFunction = kernel.CreateFunctionFromPrompt(
    promptTemplateConfig, templateFactory
);
var branchPlugin = kernel.CreatePluginFromFunctions(
    "BranchPlugin",
    [promptFunction]
);
kernel.Plugins.Add(branchPlugin);

// Add filters to the kernel
kernel.FunctionInvocationFilters.Add(new PermissionFilter());

Console.WriteLine("Press enter to exit");
Console.WriteLine("Assistant: How may I help you?");
Console.Write("User: ");

string input = Console.ReadLine()!;

// User interaction logic

while (input != "")
{
    chatHistory.AddUserMessage(input);
    await GetReply();
    input = GetInput();
}

string GetInput()
{
    Console.Write("User: ");
    string input = Console.ReadLine()!;
    chatHistory.AddUserMessage(input);
    return input;
}

async Task GetReply()
{
    ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(
        chatHistory,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel
    );
    Console.WriteLine("Assistant: " + reply.ToString());
    chatHistory.AddAssistantMessage(reply.ToString());
}


class DevopsPlugin
{
    // Create a kernel function to build the stage environment => done
    [KernelFunction("BuildStageEnvironment")]
    public string BuildStageEnvironment()
    {
        return "Stage build completed.";
    }

    [KernelFunction("DeployToStage")]
    public string DeployToStage()
    {
        return "Staging site deployed successfully.";
    }

    [KernelFunction("DeployToProd")]
    public string DeployToProd()
    {
        return "Production site deployed successfully.";
    }

    [KernelFunction("CreateNewBranch")]
    public string CreateNewBranch(string branchName, string baseBranch)
    {
        return $"Created new branch `{branchName}` from `{baseBranch}`";
    }

    [KernelFunction("ReadLogFile")]
    public string ReadLogFile()
    {
        string content = File.ReadAllText($"Files/build.log");
        return content;
    }
}

// Create a function filter
class PermissionFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        // Check the plugin and function names
        if ((context.Function.PluginName == "DevopsPlugin" && context.Function.Name == "DeployToProd"))
        {
            // Request user approval
            Console.WriteLine("System Message: The assistant requires an approval to complete this operation. Do you approve (Y/N)?");
            Console.Write("User: ");
            string shouldProceed = Console.ReadLine()!;

            // Proceed if approved
            if (shouldProceed != "Y")
            {
                context.Result = new FunctionResult(context.Result, "The operation was not approved by the user");
                return;
            }
        }

        await next(context);
    }
}
