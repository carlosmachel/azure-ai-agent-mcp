using Microsoft.AspNetCore.Mvc;

namespace AzureAiAgentMcp.Application;

public static class Module
{
    public static void Register(this IEndpointRouteBuilder app)
    {
        //Instructions: You are a helpful agent that can use MCP tools to assist users. Use the available MCP tools to answer questions and perform tasks.
        //Name: my-mcp-agent
        app.MapPost("/ai-agent", async (
                [FromServices] Service service,
                [FromQuery] string name, [FromQuery] string instructions) =>
            {
                var agentId = await service.CreateAgentAsync(name, instructions);
                return Results.Ok(agentId);
            })
            .WithTags("Ai Agents");
        
        app.MapGet("/ai-agent/create-thread", async (
                [FromServices] Service service) =>
            {
                var agentId = await service.CreateThreadAsync();
                return Results.Ok(agentId);
            })
            .WithTags("Ai Agents");
        
        app.MapGet("/ai-agent/run", async (
                [FromServices] Service service,
                [FromQuery] string agentId,
                [FromQuery] string threadId,
                [FromQuery] string userInput) =>
            {
                var result = await service.RunAsync(agentId, threadId, userInput);
                return Results.Ok(result);
            })
            .WithTags("Ai Agents");
    }
}