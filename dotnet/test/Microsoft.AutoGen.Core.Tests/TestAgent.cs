// Copyright (c) Microsoft Corporation. All rights reserved.
// TestAgent.cs

using Microsoft.AutoGen.Contracts;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Microsoft.AutoGen.Core.Tests;

public class TextMessage
{
    public string Source { get; set; } = "";
    public string Content { get; set; } = "";
}

public class RpcTextMessage
{
    public string Source { get; set; } = "";
    public string Content { get; set; } = "";
}

/// <summary>
/// The test agent is a simple agent that is used for testing purposes.
/// </summary>
public class TestAgent(AgentId id,
        IAgentRuntime runtime,
        Logger<BaseAgent>? logger = null) : BaseAgent(id, runtime, "Test Agent", logger),
        IHandle<TextMessage>,
        IHandle<string>,
        IHandle<RpcTextMessage, string>

{
    public ValueTask HandleAsync(TextMessage item, MessageContext messageContext)
    {
        ReceivedMessages[item.Source] = item.Content;
        return ValueTask.CompletedTask;
    }

    public ValueTask HandleAsync(string item, MessageContext messageContext)
    {
        ReceivedItems.Add(item);
        return ValueTask.CompletedTask;
    }

    public ValueTask HandleAsync(int item, MessageContext messageContext)
    {
        ReceivedItems.Add(item);
        return ValueTask.CompletedTask;
    }

    public ValueTask<string> HandleAsync(RpcTextMessage item, MessageContext messageContext)
    {
        ReceivedMessages[item.Source] = item.Content;
        return ValueTask.FromResult(item.Content);
    }

    public List<object> ReceivedItems { get; private set; } = [];

    /// <summary>
    /// Key: source
    /// Value: message
    /// </summary>
    protected Dictionary<string, object> _receivedMessages = new();
    public Dictionary<string, object> ReceivedMessages => _receivedMessages;
}

[TypeSubscription("TestTopic")]
public class SubscribedAgent : TestAgent
{
    public SubscribedAgent(AgentId id,
        IAgentRuntime runtime,
        Logger<BaseAgent>? logger = null) : base(id, runtime, logger)
    {
    }
}

[TypeSubscription("TestTopic")]
public class SubscribedSaveLoadAgent : TestAgent
{
    public SubscribedSaveLoadAgent(AgentId id,
        IAgentRuntime runtime,
        Logger<BaseAgent>? logger = null) : base(id, runtime, logger)
    {
    }

    public override ValueTask<IDictionary<string, JsonElement>> SaveStateAsync()
    {
        var jsonSafeDictionary = _receivedMessages.ToDictionary(
            kvp => kvp.Key,
            kvp => JsonSerializer.SerializeToElement(kvp.Value) // Convert each object to JsonElement
        );

        return ValueTask.FromResult<IDictionary<string, JsonElement>>(jsonSafeDictionary);
    }

    public override ValueTask LoadStateAsync(IDictionary<string, JsonElement> state)
    {
        _receivedMessages.Clear();
        
        foreach (var kvp in state)
        {
            _receivedMessages[kvp.Key] = kvp.Value.Deserialize<object>() ?? throw new Exception($"Failed to deserialize key: {kvp.Key}");
        }

        return ValueTask.CompletedTask;
    }
}
