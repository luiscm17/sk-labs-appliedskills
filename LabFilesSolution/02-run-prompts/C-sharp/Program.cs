using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

string filePath = Path.GetFullPath("appsettings.json");
var config = new ConfigurationBuilder()
    .AddJsonFile(filePath)
    .Build();

// Set your values in appsettings.json
string apikey = config["OpenAI:apikey"]!;
string endpoint = config["OpenAI:endpoint"]!;
string model = config["OpenAI:model"]!;

// Create a kernel with Azure OpenAI chat completion
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    model ?? throw new ArgumentNullException(nameof(model), "Model deployment name cannot be null"),
    endpoint ?? throw new ArgumentNullException(nameof(endpoint), "Endpoint cannot be null"),
    apikey ?? throw new ArgumentNullException(nameof(apikey), "API key cannot be null"));
var kernel = builder.Build();

// Create the chat history
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();

// Create a semantic kernel prompt template
var skTemplateFactory = new KernelPromptTemplateFactory();
var skPromptTemplate = skTemplateFactory.Create(new PromptTemplateConfig(
    """
    You are a helpful career advisor. Based on the users's skills and interest, suggest up to 5 suitable roles.
    Return the output as JSON in the following format:
    "Role Recomendation":
    {
    "recomendedRoles": [],
    "industries": [],
    "estimatedSalaryRange": ""
    }

    My skills are: {{$skills}}. My interests are: {{$interests}}. What are some roles that would be suitable for me?
    """
));

// Render the prompt with arguments
var skRenderedPrompt = await skPromptTemplate.RenderAsync(
    kernel,
    new KernelArguments
    {
        ["skills"] = "Software Engineering, C#, Python, Drawing, Guitar, Dance",
        ["intersts"] = "Eduaction, Psychology, Programming, Helping Others"
    }
);

// Add the Semanitc Kernel prompt to the chat history and get the reply
chatHistory.AddUserMessage(skRenderedPrompt);
await GetReply();

// Create a handlebars template
var hbTemplateFactory = new HandlebarsPromptTemplateFactory();
var hbPromptTemplate = hbTemplateFactory.Create(new PromptTemplateConfig()
{
    TemplateFormat = "handlebars",
    Name = "MissingSkillsPrompt",
    Template = """
    <mesage role="system">
    Instructions: You are a career advisor. Analyze the skill gap between 
    the user's current skills and the requirements of the target role.
    </message>
    <message role="user">Target Role:{{targetRole}}</message>
    <message role="user">Current Skills:{{currentSkills}}</message>

    <message role="assistant">
    "Skill Gap Analysis":
    {
        "missingSkills": [],
        "coursesToTake": [],
        "certificationSuggestions": []
    }
    </message>
    
    """
}
);

// Render the Handlebars prompt with arguments
var hbRenderPrompt = await hbPromptTemplate.RenderAsync(
    kernel,
    new KernelArguments
    {
        ["targetRole"] = "Game Developer",
        ["currentSkills"] = "Software Engineering, C#, Python, Drawing, Guitar, Dance"
    }
);


// Add the Handlebars prompt to the chat history and get the reply
chatHistory.AddUserMessage(hbRenderPrompt);
await GetReply();

// Get a follow-up prompt from the user
Console.WriteLine("Assistant: How can I help you?");
Console.Write("User: ");
string input = Console.ReadLine()!;

// Add the user input to the chat history and get the reply
chatHistory.AddUserMessage(input);
await GetReply();

async Task GetReply()
{
    // Get the reply from the chat completion service
    ChatMessageContent reply = await chatCompletionService.GetChatMessageContentAsync(
    chatHistory,
    kernel: kernel
);
    Console.WriteLine("Assistant: " + reply.ToString());
    chatHistory.AddAssistantMessage(reply.ToString());
}