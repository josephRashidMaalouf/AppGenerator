using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using TeacherAssistant.GptApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var kvUrl = builder.Configuration["AzureKeyVault"];
var secretClient = new SecretClient(new Uri(kvUrl), new DefaultAzureCredential());
var gptKey = secretClient.GetSecret("GptKey").Value.Value;

    builder.Services.AddHttpClient("GptApi", options =>
{
    options.BaseAddress = new Uri("https://api.openai.com");
}).ConfigureHttpClient(client =>
{
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", gptKey);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5500") 
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("frontend");

app.UseHttpsRedirection();


app.MapPost("/api/gpt", async (IHttpClientFactory _httpClientFactory, GptMessageDto gptMessageDto) =>
{
    //responses.Headers.Add("Access-Control-Allow-Origin", "http://localhost:5500");
    var httpClient = _httpClientFactory.CreateClient("GptApi");

    ResponseDto responseDto = new();
    GptRequestDto gptRequestDto = new();

    GptMessageDto instructionMessage = new GptMessageDto()
    {
        Role = "system",
        Content =
            "You are an expert web developer. Your task is to generate the HTML, inline CSS, and JavaScript code based on a user's description of an app. Please follow these instructions:\n\nBody-Only Output: Provide only the code that goes inside the <body> tags. Do not include the <html>, <head>, or <body> tags themselves. The code should be structured to function correctly within the body of an HTML document.\n\nInline CSS: All CSS should be included inline, directly within the relevant HTML tags, using the style attribute. Ensure that the styles are correctly applied to match the user's description.\n\nJavaScript: Include the necessary JavaScript directly within <script> tags. Place these tags within the code provided, ensuring that the JavaScript functions correctly with the HTML structure.\n\nGeneral Guidelines:\n\nEnsure that the HTML is semantically correct and accessible.\nThe code should be optimized for functionality and performance.\nThe JavaScript should avoid using external libraries unless specifically requested by the user.\nOutput Format:\n\nProvide only the code that belongs inside the <body> tags, including inline CSS and embedded JavaScript.\nRespond only with the code, no explanations or additional comments."
    };
    gptRequestDto.Messages = new[] {instructionMessage, gptMessageDto };

    try
    {
        var response = await httpClient.PostAsJsonAsync<GptRequestDto>("/v1/chat/completions", gptRequestDto);

        response.EnsureSuccessStatusCode();


        var result = await response.Content.ReadFromJsonAsync<GptResponseDto>();
        var message = result.choices[0].message;

        responseDto.Result = message;

        return Results.Ok(responseDto);
    }
    catch (Exception ex)
    {
        responseDto.IsSuccess = false;
        responseDto.Message = ex.Message;
        responseDto.Result = null;
        return Results.BadRequest(responseDto);
    }
});

app.Run();

