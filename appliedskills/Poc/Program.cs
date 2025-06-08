using System.Text;
using Microsoft.SemanticKernel;

string yourDeploymentName = "gpt35";
string yourEndpoint = "https://openai51831849.openai.azure.com/";
string yourKey = "82c1eb7f810d4aa2991dfd2645107e16";

//TODO 1.2 - Create a Kernel builder by using the CreateBuilder method of the Kernel object
var builder = Kernel.CreateBuilder();
//TODO 1.3 - Configure access to gpt35 using the AddAzureOpenAIChatCompletion method of the builder's services object
builder.AddAzureOpenAIChatCompletion(
    yourDeploymentName,
    yourEndpoint,
    yourKey
);

var kernel = builder.Build();

string input;

do {
    Console.WriteLine("Do you have a question?");
    //TODO 1.4 - Gather user input
    input = @"Give me a list of number 1 to 5 in english";
    
    //TODO 1.5 - Provide response based on the user input
    if (!string.IsNullOrWhiteSpace(input))    
    {
        var result = await kernel.InvokePromptAsync(
            "Give me a list of number 1 to 5 in english"
        );
        Console.WriteLine(result);
    }
}
while (!string.IsNullOrWhiteSpace(input));
