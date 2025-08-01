using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Options;

namespace AzureAiAgentMcp.Application;

public class Service(IOptions<AzureAiSettings> options)
{
    const string mcpServerUrl = "https://gitmcp.io/Azure/azure-rest-api-specs";
    const string mcpServerLabel = "github";
    const string searchApiCode = "search_azure_rest_api_code";
    
    public async Task<string> CreateAgentAsync(string agentName, string instructions)
    {
        var mcpToolDefinition = new MCPToolDefinition(mcpServerLabel, mcpServerUrl);
        //mcpToolDefinition.AllowedTools.Add(searchApiCode);
        
        var agentClient = new PersistentAgentsClient(options.Value.Uri, new DefaultAzureCredential());
        PersistentAgent agent = await agentClient.Administration.CreateAgentAsync(options.Value.Model,
            name: agentName,
            instructions: instructions,
            tools: [mcpToolDefinition]);
        return agent.Id;
    }
    
    public async Task<string> CreateThreadAsync()
    {
        var projectClient = new AIProjectClient(new Uri(options.Value.Uri), new DefaultAzureCredential());
        var agentClient = projectClient.GetPersistentAgentsClient();

        PersistentAgentThread thread = await agentClient.Threads.CreateThreadAsync();
        return thread.Id;
    }
    
    public async Task<IEnumerable<string>> RunAsync(string assistantId, string threadId, string userInput)
    {
        MCPToolResource mcpToolResource = new(mcpServerLabel);
        mcpToolResource.UpdateHeader("SuperSecret", "123456");
        mcpToolResource.RequireApproval =  MCPToolResourceRequireApproval.Always;
        
        var toolResources = mcpToolResource.ToToolResources();
        
        var projectClient = new AIProjectClient(new Uri(options.Value.Uri), new DefaultAzureCredential());
        var agentClient = projectClient.GetPersistentAgentsClient();
        
        PersistentAgent agent = await agentClient.Administration.GetAgentAsync(assistantId);
        PersistentAgentThread thread = await agentClient.Threads.GetThreadAsync(threadId);
        
        await agentClient.Messages.CreateMessageAsync( threadId, role: MessageRole.User, content: userInput);
        
        ThreadRun run = await agentClient.Runs.CreateRunAsync(thread, agent, toolResources);

        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            run = await agentClient.Runs.GetRunAsync(thread.Id, run.Id);
            
            if (run.Status == RunStatus.RequiresAction && run.RequiredAction is SubmitToolApprovalAction toolApprovalAction)
            {
                var toolApprovals = new List<ToolApproval>();
                foreach (var toolCall in toolApprovalAction.SubmitToolApproval.ToolCalls)
                {
                    if (toolCall is RequiredMcpToolCall mcpToolCall)
                    {
                        Console.WriteLine($"Approving MCP tool call: {mcpToolCall.Name}");
                        toolApprovals.Add(new ToolApproval(mcpToolCall.Id, approve: true)
                        {
                            Headers = { ["SuperSecret"] = "1234" }
                        });
                    }
                }

                if (toolApprovals.Count > 0)
                {
                    run = await agentClient.Runs.SubmitToolOutputsToRunAsync(thread.Id, run.Id, toolApprovals: toolApprovals);
                }
            }
        }
        while (run.Status == RunStatus.Queued
               || run.Status == RunStatus.InProgress
               || run.Status == RunStatus.RequiresAction);
        
        var messagesResponse = agentClient.Messages.GetMessagesAsync(thread.Id, order: ListSortOrder.Descending, limit:1);
        var result = new List<string>();

        await foreach (var threadMessage in messagesResponse)
        {
            if (threadMessage.Role == MessageRole.Agent &&
                threadMessage.ContentItems.FirstOrDefault() is MessageTextContent messageTextContent)
            {
                result.Add(messageTextContent.Text);
            }
        }

        return result;
    }
}