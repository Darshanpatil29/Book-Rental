# BOOKIFY AI AGENT & MCP SERVER IMPLEMENTATION PLAN

**Project Type:** AI Agent Integration with Microsoft Agentic Framework & MCP Server  
**Technology Stack:** Microsoft Agentic Framework, Model Context Protocol (MCP), LLM APIs (OpenAI/Azure), .NET 10  
**Architecture:** Agent-based with MCP Server for tool management  
**Date:** May 17, 2026

---

## 1. EXECUTIVE SUMMARY

This document outlines the integration of **AI Agent capabilities** into the Bookify platform using **Microsoft Agentic Framework** and **Model Context Protocol (MCP) Server**. The AI agent will enhance user experience by providing intelligent features such as:

- **Smart Apartment Recommendations** based on user preferences and history
- **Natural Language Search** (e.g., "Find me a cozy apartment near the beach with WiFi")
- **Booking Assistance** (automated booking confirmation, rescheduling, cancellation)
- **Review Generation** (AI-assisted review writing based on booking experience)
- **Personalized Notifications** (intelligent alerts about price drops, new listings)
- **Customer Support** (chatbot for common questions, FAQ automation)
- **Price Optimization** (dynamic pricing suggestions for owners)

**Key Components:**
- ✅ Microsoft Agentic Framework integration for LLM interaction
- ✅ MCP Server for standardized tool access
- ✅ Custom tools for Bookify business operations
- ✅ Agent orchestration for multi-step workflows
- ✅ Prompt templates and optimization
- ✅ Context management and memory
- ✅ Safety guardrails and rate limiting

---

## 2. ARCHITECTURE OVERVIEW

### 2.1 AI Agent Architecture Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                    Client Applications                        │
│  (Web UI, Mobile App, API Gateway)                            │
└──────────────────────┬───────────────────────────────────────┘
         ▼
┌──────────────────────────────────────────────────────────────┐
│              Bookify.API (Presentation Layer)                 │
│  AgentController → Routes requests to AI Agent               │
└──────────────────────┬───────────────────────────────────────┘
         ▼
┌──────────────────────────────────────────────────────────────┐
│          AI Agent Orchestration Layer (New)                   │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ Microsoft Agentic Framework                            │  │
│  │ ├─ Agent runtime (core orchestrator)                   │  │
│  │ ├─ Plugins / tools                                     │  │
│  │ ├─ Memory (short-term & long-term)                     │  │
│  │ ├─ Functions (native & semantic)                       │  │
│  │ └─ Model connectors (LLM endpoints)                    │  │
│  └────────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────┐  │
│  │ MCP Server                                             │  │
│  │ ├─ Request Router                                      │  │
│  │ ├─ Tool Registry                                       │  │
│  │ ├─ Resource Management                                 │  │
│  │ └─ Session Handler                                     │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────┬───────────────────────────────────────┘
         ▼
┌──────────────────────────────────────────────────────────────┐
│            MCP Tools & Business Operations                    │
│  ┌──────────────────┐  ┌──────────────────┐                  │
│  │ Apartment Tools  │  │ Booking Tools    │                  │
│  │ - Search         │  │ - Reserve        │                  │
│  │ - GetDetails     │  │ - Confirm        │                  │
│  │ - Recommend      │  │ - Cancel         │                  │
│  │ - GetPricing     │  │ - GetHistory     │                  │
│  └──────────────────┘  └──────────────────┘                  │
│  ┌──────────────────┐  ┌──────────────────┐                  │
│  │ Review Tools     │  │ User Tools       │                  │
│  │ - GenerateReview │  │ - GetProfile     │                  │
│  │ - GetReviews     │  │ - UpdateProfile  │                  │
│  │ - RateApartment  │  │ - GetFavorites   │                  │
│  └──────────────────┘  └──────────────────┘                  │
└──────────────────────┬───────────────────────────────────────┘
         ▼
┌──────────────────────────────────────────────────────────────┐
│         Bookify.Application Layer (Existing)                  │
│  Commands, Queries, CQRS Handlers                            │
└──────────────────────┬───────────────────────────────────────┘
         ▼
┌──────────────────────────────────────────────────────────────┐
│    Bookify.Infrastructure (Data Access, External Services)   │
│  PostgreSQL, Keycloak, Redis, Email Service                  │
└──────────────────────────────────────────────────────────────┘
```

### 2.2 Agentic Framework Integration Flow

```
User Input (Natural Language)
    ↓
┌─────────────────────────────────────┐
│  Microsoft Agentic Framework Runtime │
│  ├─ Parse user intent               │
│  └─ Route to appropriate agent      │
└────────────┬────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│  Agent Logic (Planner/Loop)         │
│  ├─ Analyze task requirements       │
│  ├─ Identify needed tools           │
│  └─ Generate execution plan         │
└────────────┬────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│  MCP Server (Tool Execution)        │
│  ├─ Route tool requests             │
│  ├─ Execute MCP tools               │
│  └─ Return results                  │
└────────────┬────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│  LLM (OpenAI/Azure)                 │
│  ├─ Process tool results            │
│  ├─ Generate response               │
│  └─ Plan next steps                 │
└────────────┬────────────────────────┘
    ↓
Response to User
```

### 2.3 MCP Server Architecture

```
┌────────────────────────────────────────────────────┐
│           MCP Server (stdio/HTTP)                   │
├────────────────────────────────────────────────────┤
│                                                    │
│  ┌──────────────────────────────────────────────┐ │
│  │ Request Router                               │ │
│  │ - Parse incoming MCP messages                │ │
│  │ - Route to appropriate handler               │ │
│  │ - Manage protocol versioning                 │ │
│  └──────────────────────────────────────────────┘ │
│                                                    │
│  ┌──────────────────────────────────────────────┐ │
│  │ Tool Registry (IoC Container)                │ │
│  │ - Register all available tools               │ │
│  │ - Maintain tool metadata                     │ │
│  │ - Handle tool discovery                      │ │
│  └──────────────────────────────────────────────┘ │
│                                                    │
│  ┌──────────────────────────────────────────────┐ │
│  │ Tool Executors (Adapters)                    │ │
│  │ ├─ ApartmentToolExecutor                     │ │
│  │ ├─ BookingToolExecutor                       │ │
│  │ ├─ ReviewToolExecutor                        │ │
│  │ ├─ UserToolExecutor                          │ │
│  │ └─ PricingToolExecutor                       │ │
│  └──────────────────────────────────────────────┘ │
│                                                    │
│  ┌──────────────────────────────────────────────┐ │
│  │ Resource Manager                             │ │
│  │ - Manage sessions/contexts                   │ │
│  │ - Handle concurrency                         │ │
│  │ - Track tool usage/quotas                    │ │
│  └──────────────────────────────────────────────┘ │
│                                                    │
│  ┌──────────────────────────────────────────────┐ │
│  │ Response Formatter                           │ │
│  │ - Convert internal results to MCP format     │ │
│  │ - Handle errors gracefully                   │ │
│  │ - Validate response structure                │ │
│  └──────────────────────────────────────────────┘ │
│                                                    │
└────────────────────────────────────────────────────┘
```

---

## 3. MICROSOFT AGENTIC FRAMEWORK IMPLEMENTATION

### 3.1 Core Components

#### 3.1.1 Agentic Runtime Configuration

```csharp
// File: Bookify.AI/Configuration/AgenticFrameworkConfiguration.cs

// Microsoft Agentic Framework runtime imports (SDK-specific namespaces to be finalized)
using Bookify.AI.Plugins;
using Bookify.AI.Memory;

public static class AgenticFrameworkConfiguration
{
    public static IServiceCollection AddAgenticFrameworkServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var openAiApiKey = configuration["OpenAI:ApiKey"];
        var openAiModelId = configuration["OpenAI:ModelId"] ?? "gpt-4-turbo";
        var openAiEmbeddingModel = configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";

        // Add Microsoft Agentic Framework runtime
        var agentBuilder = AgentRuntime.CreateBuilder();

        // Configure LLM connector (OpenAI)
        agentBuilder.AddOpenAIChatCompletion(
            modelId: openAiModelId,
            apiKey: openAiApiKey,
            httpClient: new HttpClient());

        // Add memory/embeddings
        agentBuilder.AddOpenAITextEmbeddingGeneration(
            modelId: openAiEmbeddingModel,
            apiKey: openAiApiKey,
            httpClient: new HttpClient());

        var agentRuntime = agentBuilder.Build();

        // Register plugins
        agentRuntime.Plugins.AddFromType<ApartmentPlugin>("apartment");
        agentRuntime.Plugins.AddFromType<BookingPlugin>("booking");
        agentRuntime.Plugins.AddFromType<ReviewPlugin>("review");
        agentRuntime.Plugins.AddFromType<UserPlugin>("user");
        agentRuntime.Plugins.AddFromType<PricingPlugin>("pricing");

        // Register memory
        services.AddSingleton<IAgentTextMemory>(serviceProvider =>
        {
            var embeddings = serviceProvider.GetRequiredService<IAgentTextEmbeddingService>();
            return new AgentTextMemory(
                new AgentMemoryStore(),
                embeddings);
        });

        // Register agent runtime
        services.AddSingleton(agentRuntime);

        // Register agent service
        services.AddScoped<IAIAgentService, AIAgentService>();
        services.AddScoped<IMCPServerService, MCPServerService>();

        return services;
    }
}
```

#### 3.1.2 Agent Tool Modules

```csharp
// File: Bookify.AI/Plugins/ApartmentPlugin.cs

// Microsoft Agentic Framework runtime imports (SDK-specific namespaces to be finalized)
using System.ComponentModel;
using Bookify.Application.Apartments.Queries;
using MediatR;
using System.Text.Json;

public class ApartmentPlugin
{
    private readonly IMediator _mediator;
    private readonly ILogger<ApartmentPlugin> _logger;

    public ApartmentPlugin(IMediator mediator, ILogger<ApartmentPlugin> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [AgentFunction("search_apartments")]
    [Description("Search for available apartments based on criteria")]
    public async Task<string> SearchApartments(
        [Description("Check-in date (YYYY-MM-DD)")] string startDate,
        [Description("Check-out date (YYYY-MM-DD)")] string endDate,
        [Description("Maximum price per night")] decimal? maxPrice = null,
        [Description("Minimum price per night")] decimal? minPrice = null,
        [Description("Desired amenities (comma-separated)")] string? amenities = null,
        [Description("City or location")] string? location = null,
        [Description("Page number")] int pageNumber = 1)
    {
        try
        {
            var query = new SearchApartmentsQuery(
                startDate: DateOnly.ParseExact(startDate, "yyyy-MM-dd"),
                endDate: DateOnly.ParseExact(endDate, "yyyy-MM-dd"),
                minPrice: minPrice,
                maxPrice: maxPrice,
                amenities: amenities?.Split(','),
                location: location,
                pageNumber: pageNumber,
                pageSize: 10);

            var result = await _mediator.Send(query);
            
            return result.IsSuccess
                ? JsonSerializer.Serialize(result.Value)
                : $"Search failed: {result.Error}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching apartments");
            return $"Error: {ex.Message}";
        }
    }

    [AgentFunction("get_apartment_details")]
    [Description("Get detailed information about a specific apartment")]
    public async Task<string> GetApartmentDetails(
        [Description("Apartment ID (GUID)")] string apartmentId)
    {
        try
        {
            var query = new GetApartmentDetailsQuery(Guid.Parse(apartmentId));
            var result = await _mediator.Send(query);
            
            return result.IsSuccess
                ? JsonSerializer.Serialize(result.Value)
                : $"Failed to fetch details: {result.Error}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting apartment details");
            return $"Error: {ex.Message}";
        }
    }

    [AgentFunction("recommend_apartments")]
    [Description("Get personalized apartment recommendations based on user preferences")]
    public async Task<string> RecommendApartments(
        [Description("User ID")] string userId,
        [Description("Budget (min-max price)")] string? budget = null,
        [Description("Preferred amenities")] string? preferredAmenities = null,
        [Description("Location preference")] string? location = null)
    {
        try
        {
            // This would call a custom recommendation algorithm
            // Combining user history, preferences, and collaborative filtering
            var recommendationService = // Injected service
            var recommendations = await recommendationService.GetPersonalizedRecommendations(
                userId: Guid.Parse(userId),
                budget: budget,
                amenities: preferredAmenities?.Split(','),
                location: location);

            return JsonSerializer.Serialize(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendations");
            return $"Error: {ex.Message}";
        }
    }
}
```

#### 3.1.3 Booking Plugin

```csharp
// File: Bookify.AI/Plugins/BookingPlugin.cs

// Microsoft Agentic Framework runtime imports (SDK-specific namespaces to be finalized)
using System.ComponentModel;
using Bookify.Application.Bookings.Commands;
using MediatR;

public class BookingPlugin
{
    private readonly IMediator _mediator;
    private readonly ILogger<BookingPlugin> _logger;

    public BookingPlugin(IMediator mediator, ILogger<BookingPlugin> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [AgentFunction("reserve_apartment")]
    [Description("Reserve an apartment for specified dates")]
    public async Task<string> ReserveApartment(
        [Description("Apartment ID")] string apartmentId,
        [Description("User ID")] string userId,
        [Description("Check-in date (YYYY-MM-DD)")] string startDate,
        [Description("Check-out date (YYYY-MM-DD)")] string endDate)
    {
        try
        {
            var command = new ReserveBookingCommand(
                apartmentId: Guid.Parse(apartmentId),
                userId: Guid.Parse(userId),
                startDate: DateOnly.ParseExact(startDate, "yyyy-MM-dd"),
                endDate: DateOnly.ParseExact(endDate, "yyyy-MM-dd"));

            var result = await _mediator.Send(command);
            
            return result.IsSuccess
                ? $"Booking reserved successfully. ID: {result.Value}"
                : $"Reservation failed: {result.Error}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving apartment");
            return $"Error: {ex.Message}";
        }
    }

    [AgentFunction("confirm_booking")]
    [Description("Confirm a reserved booking")]
    public async Task<string> ConfirmBooking(
        [Description("Booking ID")] string bookingId)
    {
        try
        {
            var command = new ConfirmBookingCommand(Guid.Parse(bookingId));
            var result = await _mediator.Send(command);
            
            return result.IsSuccess
                ? "Booking confirmed successfully"
                : $"Confirmation failed: {result.Error}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming booking");
            return $"Error: {ex.Message}";
        }
    }

    [AgentFunction("get_user_bookings")]
    [Description("Get all bookings for a user")]
    public async Task<string> GetUserBookings(
        [Description("User ID")] string userId,
        [Description("Status filter (All/Upcoming/Past)")] string? status = "Upcoming")
    {
        try
        {
            var query = new GetUserBookingsQuery(
                userId: Guid.Parse(userId),
                status: status ?? "Upcoming");

            var result = await _mediator.Send(query);
            
            return result.IsSuccess
                ? JsonSerializer.Serialize(result.Value)
                : $"Failed to fetch bookings: {result.Error}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookings");
            return $"Error: {ex.Message}";
        }
    }
}
```

#### 3.1.4 Memory Management

```csharp
// File: Bookify.AI/Memory/AgentMemoryManager.cs

// Microsoft Agentic Framework memory abstractions (SDK-specific namespaces to be finalized)
using Bookify.Domain.Entities.Users;

public interface IAgentMemoryManager
{
    Task StoreUserPreferencesAsync(User user, string preferences);
    Task<string?> RetrieveUserPreferencesAsync(Guid userId);
    Task StoreBookingHistoryAsync(Guid userId, string history);
    Task<string?> RetrieveBookingHistoryAsync(Guid userId);
    Task SaveConversationContextAsync(string sessionId, string context);
    Task<string?> GetConversationContextAsync(string sessionId);
}

public class AgentMemoryManager : IAgentMemoryManager
{
    private readonly IAgentTextMemory _memory;
    private const string UserPreferencesCollection = "user_preferences";
    private const string BookingHistoryCollection = "booking_history";
    private const string ConversationCollection = "conversations";

    public AgentMemoryManager(IAgentTextMemory memory)
    {
        _memory = memory;
    }

    public async Task StoreUserPreferencesAsync(User user, string preferences)
    {
        await _memory.SaveInformationAsync(
            collection: UserPreferencesCollection,
            id: user.Id.ToString(),
            text: preferences,
            description: $"Preferences for user {user.Email}");
    }

    public async Task<string?> RetrieveUserPreferencesAsync(Guid userId)
    {
        try
        {
            var memory = await _memory.GetAsync(
                collection: UserPreferencesCollection,
                key: userId.ToString());

            return memory?.Metadata.Text;
        }
        catch
        {
            return null;
        }
    }

    public async Task StoreBookingHistoryAsync(Guid userId, string history)
    {
        await _memory.SaveInformationAsync(
            collection: BookingHistoryCollection,
            id: userId.ToString(),
            text: history,
            description: $"Booking history for user {userId}");
    }

    public async Task<string?> RetrieveBookingHistoryAsync(Guid userId)
    {
        try
        {
            var memory = await _memory.GetAsync(
                collection: BookingHistoryCollection,
                key: userId.ToString());

            return memory?.Metadata.Text;
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveConversationContextAsync(string sessionId, string context)
    {
        await _memory.SaveInformationAsync(
            collection: ConversationCollection,
            id: sessionId,
            text: context);
    }

    public async Task<string?> GetConversationContextAsync(string sessionId)
    {
        try
        {
            var memory = await _memory.GetAsync(
                collection: ConversationCollection,
                key: sessionId);

            return memory?.Metadata.Text;
        }
        catch
        {
            return null;
        }
    }
}
```

### 3.2 Prompt Templates

```yaml
# File: Bookify.AI/Prompts/ApartmentRecommendation.yaml

name: ApartmentRecommendation
description: Generate personalized apartment recommendations
template: |
  You are a helpful apartment booking assistant for the Bookify platform.
  
  Based on the user's profile and preferences, recommend suitable apartments.
  
  User Profile:
  {{$userProfile}}
  
  Available Apartments:
  {{$availableApartments}}
  
  User Preferences:
  {{$preferences}}
  
  Provide 3-5 recommendations with brief explanations for each.
  Format the response as a JSON array with {id, reason, matchScore}.
  
parameters:
  - name: userProfile
    description: User demographic and account information
  - name: availableApartments
    description: List of available apartments in JSON format
  - name: preferences
    description: User's stated preferences and constraints
```

---

## 4. MCP SERVER IMPLEMENTATION

### 4.1 MCP Server Core

```csharp
// File: Bookify.AI/MCP/MCPServer.cs

using System.Text.Json;
using Bookify.AI.MCP.Models;
using Bookify.AI.MCP.Tools;

public interface IMCPServer
{
    Task<MCPResponse> HandleRequestAsync(MCPRequest request);
    Task RegisterToolAsync(ITool tool);
    IReadOnlyList<ToolDefinition> GetAvailableTools();
}

public class MCPServer : IMCPServer
{
    private readonly IToolRegistry _toolRegistry;
    private readonly ILogger<MCPServer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MCPServer(
        IToolRegistry toolRegistry,
        ILogger<MCPServer> logger,
        IServiceProvider serviceProvider)
    {
        _toolRegistry = toolRegistry;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<MCPResponse> HandleRequestAsync(MCPRequest request)
    {
        try
        {
            _logger.LogInformation($"Processing MCP request: {request.Method}");

            return request.Method switch
            {
                "tools/list" => HandleToolsListRequest(request),
                "tools/call" => await HandleToolCallRequest(request),
                "resources/list" => HandleResourcesListRequest(request),
                "resources/read" => HandleResourcesReadRequest(request),
                "initialize" => HandleInitializeRequest(request),
                _ => new MCPResponse
                {
                    IsSuccess = false,
                    Error = $"Unknown method: {request.Method}"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling MCP request: {request.Method}");
            return new MCPResponse
            {
                IsSuccess = false,
                Error = ex.Message
            };
        }
    }

    private MCPResponse HandleToolsListRequest(MCPRequest request)
    {
        var tools = _toolRegistry.GetAllTools();
        var toolDefinitions = tools
            .Select(t => new ToolDefinition
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = t.InputSchema
            })
            .ToList();

        return new MCPResponse
        {
            IsSuccess = true,
            Data = new { tools = toolDefinitions }
        };
    }

    private async Task<MCPResponse> HandleToolCallRequest(MCPRequest request)
    {
        var toolName = request.Params?["name"]?.ToString();
        var arguments = request.Params?.Value.GetProperty("arguments") ?? default;

        if (string.IsNullOrEmpty(toolName))
        {
            return new MCPResponse
            {
                IsSuccess = false,
                Error = "Tool name is required"
            };
        }

        var tool = _toolRegistry.GetTool(toolName);
        if (tool == null)
        {
            return new MCPResponse
            {
                IsSuccess = false,
                Error = $"Tool not found: {toolName}"
            };
        }

        try
        {
            var result = await tool.ExecuteAsync(arguments.GetRawText() ?? "{}");
            return new MCPResponse
            {
                IsSuccess = true,
                Data = new { toolResult = result }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error executing tool: {toolName}");
            return new MCPResponse
            {
                IsSuccess = false,
                Error = ex.Message
            };
        }
    }

    private MCPResponse HandleResourcesListRequest(MCPRequest request)
    {
        // Returns available resources (read-only data)
        var resources = new object[] { };
        return new MCPResponse
        {
            IsSuccess = true,
            Data = new { resources }
        };
    }

    private MCPResponse HandleResourcesReadRequest(MCPRequest request)
    {
        // Reads specific resource data
        return new MCPResponse
        {
            IsSuccess = false,
            Error = "Not implemented"
        };
    }

    private MCPResponse HandleInitializeRequest(MCPRequest request)
    {
        return new MCPResponse
        {
            IsSuccess = true,
            Data = new
            {
                protocolVersion = "1.0",
                capabilities = new
                {
                    tools = true,
                    resources = true
                },
                serverInfo = new
                {
                    name = "Bookify MCP Server",
                    version = "1.0.0"
                }
            }
        };
    }

    public async Task RegisterToolAsync(ITool tool)
    {
        _toolRegistry.RegisterTool(tool);
        _logger.LogInformation($"Tool registered: {tool.Name}");
    }

    public IReadOnlyList<ToolDefinition> GetAvailableTools()
    {
        return _toolRegistry.GetAllTools()
            .Select(t => new ToolDefinition
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = t.InputSchema
            })
            .ToList();
    }
}
```

### 4.2 Tool Registry & Definition

```csharp
// File: Bookify.AI/MCP/Tools/IToolRegistry.cs

public interface ITool
{
    string Name { get; }
    string Description { get; }
    JsonSchema InputSchema { get; }
    Task<string> ExecuteAsync(string argumentsJson);
}

public interface IToolRegistry
{
    void RegisterTool(ITool tool);
    ITool? GetTool(string name);
    IReadOnlyList<ITool> GetAllTools();
}

public class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, ITool> _tools = new();
    private readonly ILogger<ToolRegistry> _logger;

    public ToolRegistry(ILogger<ToolRegistry> logger)
    {
        _logger = logger;
    }

    public void RegisterTool(ITool tool)
    {
        if (_tools.ContainsKey(tool.Name))
        {
            _logger.LogWarning($"Tool already registered: {tool.Name}. Overwriting.");
        }

        _tools[tool.Name] = tool;
        _logger.LogInformation($"Tool registered: {tool.Name}");
    }

    public ITool? GetTool(string name)
    {
        return _tools.TryGetValue(name, out var tool) ? tool : null;
    }

    public IReadOnlyList<ITool> GetAllTools()
    {
        return _tools.Values.ToList().AsReadOnly();
    }
}
```

### 4.3 Concrete Tool Implementation

```csharp
// File: Bookify.AI/MCP/Tools/SearchApartmentsTool.cs

using System.Text.Json;
using System.Text.Json.Serialization;
using Bookify.Application.Apartments.Queries;
using Bookify.AI.MCP.Models;
using MediatR;

public class SearchApartmentsTool : ITool
{
    private readonly IMediator _mediator;
    private readonly ILogger<SearchApartmentsTool> _logger;

    public string Name => "search_apartments";
    public string Description => "Search for available apartments with filters";

    public JsonSchema InputSchema => new JsonSchema
    {
        Type = "object",
        Properties = new Dictionary<string, JsonSchema>
        {
            ["startDate"] = new JsonSchema { Type = "string", Description = "Check-in date (YYYY-MM-DD)" },
            ["endDate"] = new JsonSchema { Type = "string", Description = "Check-out date (YYYY-MM-DD)" },
            ["minPrice"] = new JsonSchema { Type = "number", Description = "Minimum price per night" },
            ["maxPrice"] = new JsonSchema { Type = "number", Description = "Maximum price per night" },
            ["amenities"] = new JsonSchema { Type = "string", Description = "Comma-separated amenities" },
            ["location"] = new JsonSchema { Type = "string", Description = "Location/city filter" }
        },
        Required = new[] { "startDate", "endDate" }
    };

    public SearchApartmentsTool(IMediator mediator, ILogger<SearchApartmentsTool> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(string argumentsJson)
    {
        try
        {
            var options = JsonSerializer.Deserialize<SearchApartmentsArgs>(argumentsJson)
                ?? throw new ArgumentException("Invalid arguments");

            var query = new SearchApartmentsQuery(
                startDate: DateOnly.ParseExact(options.StartDate, "yyyy-MM-dd"),
                endDate: DateOnly.ParseExact(options.EndDate, "yyyy-MM-dd"),
                minPrice: options.MinPrice,
                maxPrice: options.MaxPrice,
                amenities: options.Amenities?.Split(','),
                location: options.Location,
                pageNumber: 1,
                pageSize: 10);

            var result = await _mediator.Send(query);

            if (result.IsSuccess)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                return JsonSerializer.Serialize(new { success = true, data = result.Value }, options);
            }

            var errorOptions = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(new { success = false, error = result.Error }, errorOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing search apartments tool");
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(new { success = false, error = ex.Message }, options);
        }
    }

    private class SearchApartmentsArgs
    {
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Amenities { get; set; }
        public string? Location { get; set; }
    }
}
```

### 4.4 MCP Request/Response Models

```csharp
// File: Bookify.AI/MCP/Models/MCPModels.cs

using System.Text.Json.Serialization;

namespace Bookify.AI.MCP.Models;

public class MCPRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public JsonElement? Params { get; set; }
}

public class MCPResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("result")]
    public object? Result { get; set; }

    [JsonPropertyName("error")]
    public MCPError? Error { get; set; }

    [JsonIgnore]
    public bool IsSuccess { get; set; }

    [JsonIgnore]
    public string? ErrorMessage { get; set; }

    [JsonIgnore]
    public object? Data { get; set; }
}

public class MCPError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

public class ToolDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("inputSchema")]
    public JsonSchema InputSchema { get; set; } = new();
}

public class JsonSchema
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    [JsonPropertyName("properties")]
    public Dictionary<string, JsonSchema>? Properties { get; set; }

    [JsonPropertyName("required")]
    public string[]? Required { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
```

---

## 5. AI AGENT SERVICE

### 5.1 Agent Orchestration

```csharp
// File: Bookify.AI/Services/AIAgentService.cs

// Microsoft Agentic Framework runtime imports (SDK-specific namespaces to be finalized)
using Bookify.AI.Memory;

public interface IAIAgentService
{
    Task<string> ProcessUserQueryAsync(string userId, string userQuery);
    Task<string> GetRecommendationsAsync(string userId);
    Task<string> AssistBookingAsync(string userId, string apartmentId, string dates);
}

public class AIAgentService : IAIAgentService
{
    private readonly IAgentRuntime _agentRuntime;
    private readonly IAgentMemoryManager _memoryManager;
    private readonly IMCPServerService _mcpServerService;
    private readonly ILogger<AIAgentService> _logger;
    private readonly IConfiguration _configuration;

    public AIAgentService(
        IAgentRuntime agentRuntime,
        IAgentMemoryManager memoryManager,
        IMCPServerService mcpServerService,
        ILogger<AIAgentService> logger,
        IConfiguration configuration)
    {
        _agentRuntime = agentRuntime;
        _memoryManager = memoryManager;
        _mcpServerService = mcpServerService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> ProcessUserQueryAsync(string userId, string userQuery)
    {
        try
        {
            _logger.LogInformation($"Processing query from user {userId}: {userQuery}");

            // 1. Retrieve user context
            var userPreferences = await _memoryManager.RetrieveUserPreferencesAsync(Guid.Parse(userId));
            var bookingHistory = await _memoryManager.RetrieveBookingHistoryAsync(Guid.Parse(userId));

            // 2. Build context for the agent
            var context = new AgentArguments
            {
                ["userId"] = userId,
                ["userQuery"] = userQuery,
                ["userPreferences"] = userPreferences ?? "None specified",
                ["bookingHistory"] = bookingHistory ?? "No bookings yet"
            };

            // 3. Invoke agent
            var chatCompletion = _agentRuntime.GetChatCompletionService();
            var history = new ChatHistory();
            
            history.AddSystemMessage($@"
You are Bookify, an AI assistant for apartment rental booking.
You help users find, book, and manage apartment reservations.

User Preferences: {context["userPreferences"]}
Booking History: {context["bookingHistory"]}

Available tools:
- search_apartments: Find available apartments
- get_apartment_details: Get detailed apartment info
- recommend_apartments: Get personalized recommendations
- reserve_apartment: Make a booking
- get_user_bookings: View user's bookings

Always be helpful, accurate, and confirm important details before taking action.");

            history.AddUserMessage(userQuery);

            // 4. Agent loop (simplified)
            var response = await chatCompletion.GetChatMessageContentAsync(history);

            // 5. Store interaction in memory
            await _memoryManager.SaveConversationContextAsync(
                sessionId: $"{userId}_{DateTime.UtcNow:yyyyMMdd}",
                context: $"Query: {userQuery}\nResponse: {response}");

            return response.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing query for user {userId}");
            return $"I apologize, but I encountered an error: {ex.Message}";
        }
    }

    public async Task<string> GetRecommendationsAsync(string userId)
    {
        try
        {
            var guidUserId = Guid.Parse(userId);
            
            // Call recommendation plugin
            var result = await _agentRuntime.InvokeAsync(
                "apartment",
                "recommend_apartments",
                new AgentArguments
                {
                    ["userId"] = userId
                });

            return result.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting recommendations for user {userId}");
            return "Unable to generate recommendations at this time.";
        }
    }

    public async Task<string> AssistBookingAsync(string userId, string apartmentId, string dates)
    {
        try
        {
            // Parse dates and guide user through booking
            var parts = dates.Split(" to ");
            var startDate = parts[0];
            var endDate = parts[1];

            // Step 1: Get apartment details
            var apartmentDetails = await _agentRuntime.InvokeAsync(
                "apartment",
                "get_apartment_details",
                new AgentArguments { ["apartmentId"] = apartmentId });

            // Step 2: Get apartment reviews
            // Step 3: Display pricing

            return $@"Great! I can help you book this apartment.
            
Apartment Details:
{apartmentDetails}

Dates: {startDate} to {endDate}

Would you like me to proceed with the booking?";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error assisting booking for user {userId}");
            return "I encountered an error assisting with your booking. Please try again.";
        }
    }
}
```

### 5.2 MCP Server Service

```csharp
// File: Bookify.AI/Services/MCPServerService.cs

public interface IMCPServerService
{
    Task<object?> CallToolAsync(string toolName, Dictionary<string, object> arguments);
    IReadOnlyList<ToolDefinition> GetAvailableTools();
}

public class MCPServerService : IMCPServerService
{
    private readonly IMCPServer _server;
    private readonly ILogger<MCPServerService> _logger;

    public MCPServerService(IMCPServer server, ILogger<MCPServerService> logger)
    {
        _server = server;
        _logger = logger;
    }

    public async Task<object?> CallToolAsync(string toolName, Dictionary<string, object> arguments)
    {
        try
        {
            var request = new MCPRequest
            {
                Id = Guid.NewGuid().ToString(),
                Method = "tools/call",
                Params = ConvertToDictionary(new { name = toolName, arguments })
            };

            var response = await _server.HandleRequestAsync(request);
            
            return response.IsSuccess ? response.Data : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error calling tool: {toolName}");
            return null;
        }
    }

    public IReadOnlyList<ToolDefinition> GetAvailableTools()
    {
        return _server.GetAvailableTools();
    }

    private JsonElement ConvertToDictionary(object obj)
    {
        var options = new JsonSerializerOptions { WriteIndented = false };
        var json = JsonSerializer.Serialize(obj, options);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
```

---

## 6. API CONTROLLER INTEGRATION

### 6.1 AI Agent Controller

```csharp
// File: Bookify.API/Controllers/AgentController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookify.AI.Services;

namespace Bookify.API.Controllers;

[ApiController]
[Route("api/v1/agent")]
[Authorize]
public class AgentController : ControllerBase
{
    private readonly IAIAgentService _agentService;
    private readonly ILogger<AgentController> _logger;

    public AgentController(IAIAgentService agentService, ILogger<AgentController> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    [HttpPost("query")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ProcessQuery([FromBody] QueryRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new InvalidOperationException("User ID not found in claims");

            var response = await _agentService.ProcessUserQueryAsync(userId, request.Query);
            
            return Ok(new { response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("recommendations")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetRecommendations()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new InvalidOperationException("User ID not found in claims");

            var recommendations = await _agentService.GetRecommendationsAsync(userId);
            
            return Ok(new { recommendations });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recommendations");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("booking-assist")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> AssistBooking([FromBody] BookingAssistRequest request)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new InvalidOperationException("User ID not found in claims");

            var guidance = await _agentService.AssistBookingAsync(
                userId, request.ApartmentId, request.Dates);
            
            return Ok(new { guidance });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assisting with booking");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class QueryRequest
{
    public string Query { get; set; } = string.Empty;
}

public class BookingAssistRequest
{
    public string ApartmentId { get; set; } = string.Empty;
    public string Dates { get; set; } = string.Empty;
}
```

---

## 7. IMPLEMENTATION PHASES

### Phase 1: Foundation (Weeks 1-2)

**Deliverables:**
- ✅ Microsoft Agentic Framework integration
- ✅ Basic plugin system setup
- ✅ MCP Server framework
- ✅ Memory management implementation
- ✅ Docker container for agent runtime
- ✅ Unit tests for core components

**Activities:**
1. Create `Bookify.AI` project structure
2. Install Microsoft Agentic Framework NuGet packages
3. Configure OpenAI/Azure integration
4. Implement core plugins (Apartment, Booking)
5. Setup MCP Server
6. Integration tests

**Success Criteria:**
- Agent runtime initializes successfully
- Plugins load without errors
- MCP server responds to requests
- Memory operations functional

---

### Phase 2: Tool Development (Weeks 3-4)

**Deliverables:**
- ✅ All MCP tools implemented
- ✅ Tool registry working
- ✅ Error handling & validation
- ✅ Tool documentation
- ✅ Integration tests for all tools

**Tools to Implement:**
- `search_apartments` - Search with filters
- `get_apartment_details` - Fetch full details
- `recommend_apartments` - Personalized recommendations
- `reserve_apartment` - Make booking
- `confirm_booking` - Confirm reservation
- `get_user_bookings` - View booking history
- `create_review` - Write review
- `get_reviews` - Fetch reviews
- `get_user_profile` - User info
- `update_preferences` - User preferences

---

### Phase 3: Agent Logic (Weeks 5-6)

**Deliverables:**
- ✅ AI Agent service
- ✅ Query processing logic
- ✅ Recommendation engine
- ✅ Booking assistance flow
- ✅ Context management

**Features:**
- Natural language understanding
- Intent classification
- Tool orchestration
- Response generation
- Conversation memory

---

### Phase 4: API Integration (Week 7)

**Deliverables:**
- ✅ Agent controller
- ✅ API endpoints
- ✅ Swagger documentation
- ✅ Integration with existing endpoints

**Endpoints:**
- `POST /api/v1/agent/query` - Process natural language
- `GET /api/v1/agent/recommendations` - Get recommendations
- `POST /api/v1/agent/booking-assist` - Booking guidance

---

### Phase 5: Testing & Optimization (Weeks 8-9)

**Deliverables:**
- ✅ Unit tests (>80% coverage)
- ✅ Integration tests
- ✅ Performance benchmarks
- ✅ Prompt optimization
- ✅ Error handling

**Testing Strategy:**
- Unit tests for plugins
- Integration tests for agent flows
- Performance tests (response time < 2s)
- Security tests (injection prevention)
- Prompt engineering validation

---

### Phase 6: Deployment (Week 10)

**Deliverables:**
- ✅ Docker image for AI service
- ✅ Kubernetes manifests
- ✅ CI/CD pipeline updates
- ✅ Monitoring setup
- ✅ Documentation

---

## 8. CONFIGURATION & SETUP

### 8.1 Environment Variables

```bash
# .env or appsettings.Development.json

# OpenAI/Azure Configuration
OPENAI_API_KEY=sk-...
OPENAI_MODEL_ID=gpt-4-turbo
OPENAI_EMBEDDING_MODEL=text-embedding-3-small

# Azure OpenAI (Alternative)
AZURE_OPENAI_ENDPOINT=https://{resource-name}.openai.azure.com/
AZURE_OPENAI_API_KEY=xxx
AZURE_OPENAI_DEPLOYMENT_ID=gpt-4-turbo

# MCP Server Configuration
MCP_SERVER_PORT=9090
MCP_SERVER_HOST=localhost

# Agent Configuration
AI_AGENT_MAX_TOKENS=2000
AI_AGENT_TEMPERATURE=0.7
AI_AGENT_TIMEOUT_SECONDS=30

# Memory Configuration
MEMORY_COLLECTION_SIZE=1000
MEMORY_TTL_HOURS=24

# Feature Flags
ENABLE_AI_AGENT=true
ENABLE_RECOMMENDATIONS=true
ENABLE_BOOKING_ASSIST=true
```

### 8.2 NuGet Packages

```xml
<!-- Add to Bookify.AI.csproj -->

<ItemGroup>
    <!-- Microsoft Agentic Framework packages (SDK names to be confirmed) -->
    <PackageReference Include="<Microsoft.AgenticFramework.Runtime.Package>" Version="<version>" />
    <PackageReference Include="<Microsoft.AgenticFramework.OpenAIConnector.Package>" Version="<version>" />
    <PackageReference Include="<Microsoft.AgenticFramework.Memory.Package>" Version="<version>" />
    
    <!-- Logging -->
    <PackageReference Include="Serilog" Version="3.1.1" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />
    
    <!-- Serialization -->
    <PackageReference Include="System.Text.Json" Version="10.0.0" />
    
    <!-- HTTP for MCP -->
    <PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="10.0.0" />
</ItemGroup>
```

---

## 9. SECURITY CONSIDERATIONS

### 9.1 API Key Management

```csharp
// Use Azure Key Vault or similar
public class SecretsManager
{
    private readonly IConfiguration _configuration;
    
    public string GetOpenAIApiKey() => 
        _configuration["OpenAI:ApiKey"] 
        ?? throw new InvalidOperationException("OpenAI API key not configured");
    
    public string GetAzureOpenAIKey() =>
        _configuration["AzureOpenAI:ApiKey"]
        ?? throw new InvalidOperationException("Azure OpenAI key not configured");
}
```

### 9.2 Rate Limiting

```csharp
// Limit AI agent usage per user
public class RateLimitMiddleware
{
    private readonly IMemoryCache _cache;
    private const int MaxRequestsPerMinute = 10;
    
    public async Task InvokeAsync(HttpContext context, IMemoryCache cache)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var key = $"ai-agent-ratelimit-{userId}";
        
        if (cache.TryGetValue(key, out var count) && (int)count >= MaxRequestsPerMinute)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            return;
        }
        
        cache.Set(key, ((int?)cache.Get(key) ?? 0) + 1, TimeSpan.FromMinutes(1));
        await next(context);
    }
}
```

### 9.3 Prompt Injection Prevention

```csharp
// Validate and sanitize user input
public class InputValidator
{
    public static string SanitizeQuery(string input)
    {
        // Remove potentially malicious patterns
        var suspicious = new[] { "<script>", "DROP TABLE", "DELETE FROM", "--" };
        var sanitized = input;
        
        foreach (var pattern in suspicious)
        {
            sanitized = sanitized.Replace(pattern, "", StringComparison.OrdinalIgnoreCase);
        }
        
        // Limit length
        return sanitized.Length > 500 
            ? sanitized[..500] 
            : sanitized;
    }
}
```

---

## 10. MONITORING & OBSERVABILITY

### 10.1 Logging Configuration

```csharp
// Configure Serilog for AI operations
var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/ai-agent-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

Log.Logger = logger;
```

### 10.2 Metrics

```csharp
// Track AI agent metrics
public class AIAgentMetrics
{
    public int TotalQueries { get; set; }
    public int SuccessfulQueries { get; set; }
    public int FailedQueries { get; set; }
    public double AverageResponseTime { get; set; }
    public Dictionary<string, int> ToolUsageCount { get; set; } = new();
}
```

---

## 11. COST ESTIMATION

### 11.1 OpenAI API Costs (Monthly Estimate)

| Component | Usage | Cost |
|-----------|-------|------|
| **GPT-4 Turbo** (prompt) | 1M tokens | ~$30 |
| **GPT-4 Turbo** (completion) | 500K tokens | ~$60 |
| **Embeddings** | 100K tokens | ~$1 |
| **Total Monthly** | - | ~$100-150 |

**Note:** Costs depend on usage patterns. For 100 active users with 10 queries/day:
- ~30,000 queries/month
- ~150M tokens/month (average)
- **Estimated: $300-500/month**

### 11.2 Infrastructure Costs

| Component | Service | Est. Cost |
|-----------|---------|-----------|
| **Microsoft Agentic Framework** | In-process | $0 |
| **MCP Server** | Container | $50-100/month |
| **Memory Store** | Redis | $20-50/month |
| **Logging** | Serilog/Seq | $50-100/month |

---

## 12. FUTURE ENHANCEMENTS

### Phase 2 Features (Post-MVP)

1. **Advanced Recommendation Engine**
   - Collaborative filtering
   - Behavioral analysis
   - Seasonal trends

2. **Multi-turn Conversations**
   - Context preservation
   - Session management
   - Conversation branching

3. **Voice Interface**
   - Speech-to-text
   - Text-to-speech
   - Voice commands

4. **Admin Dashboard**
   - Agent analytics
   - Usage metrics
   - Performance monitoring

5. **Custom LLM Models**
   - Fine-tuning on booking data
   - Domain-specific model
   - Privacy-focused options

6. **Integration with External APIs**
   - Weather data for recommendations
   - Currency conversion
   - Translation services

---

## 13. TROUBLESHOOTING GUIDE

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| **MCP Server not responding** | Port conflict | Check port availability: `netstat -ano \| findstr :9090` |
| **Agentic framework initialization fails** | Missing API key | Verify OpenAI API key in configuration |
| **Tool execution timeout** | Slow database query | Optimize CQRS handlers, add caching |
| **Memory overflow** | Too many stored contexts | Implement cleanup job, set TTL |
| **Rate limiting triggered** | Excessive requests | Implement request batching |

---

## 14. QUICK START GUIDE

```bash
# 1. Create Bookify.AI project
dotnet new classlib -n Bookify.AI -f net10.0

# 2. Install dependencies
cd Bookify.AI
dotnet add package <Microsoft.AgenticFramework.Runtime.Package>
dotnet add package <Microsoft.AgenticFramework.OpenAIConnector.Package>

# 3. Configure in Program.cs
builder.Services.AddAgenticFrameworkServices(builder.Configuration);

# 4. Add controller
dotnet add package Microsoft.AspNetCore.Mvc

# 5. Test
dotnet run

# Access: POST http://localhost:5000/api/v1/agent/query
```

---

## 15. REFERENCES & RESOURCES

- **Microsoft Agentic Framework Documentation**: <to be confirmed>
- **MCP Specification**: https://modelcontextprotocol.io/
- **OpenAI API**: https://platform.openai.com/docs/
- **Azure OpenAI**: https://learn.microsoft.com/en-us/azure/ai-services/openai/

---

**Document Version:** 1.0  
**Date:** May 17, 2026  
**Status:** Ready for Implementation

