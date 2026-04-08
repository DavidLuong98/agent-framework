// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace Microsoft.Agents.AI.FoundryLocal.UnitTests;

public class FoundryLocalChatClientTests
{
    [Fact]
    public async Task CreateAsync_WithNullOptions_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            FoundryLocalChatClient.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_WithNoModel_ThrowsInvalidOperation()
    {
        var previousValue = Environment.GetEnvironmentVariable("FOUNDRY_LOCAL_MODEL");
        try
        {
            Environment.SetEnvironmentVariable("FOUNDRY_LOCAL_MODEL", null);
            var options = new FoundryLocalClientOptions { Bootstrap = false };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                FoundryLocalChatClient.CreateAsync(options));
        }
        finally
        {
            Environment.SetEnvironmentVariable("FOUNDRY_LOCAL_MODEL", previousValue);
        }
    }

    [Fact]
    public void ImplementsIAsyncDisposable()
    {
        Assert.True(
            typeof(IAsyncDisposable).IsAssignableFrom(typeof(FoundryLocalChatClient)),
            "FoundryLocalChatClient should implement IAsyncDisposable to support async model unloading.");
    }

    [Fact]
    public void DisposeAsyncMethodExists()
    {
        var method = typeof(FoundryLocalChatClient).GetMethod(
            nameof(IAsyncDisposable.DisposeAsync),
            BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(method);
        Assert.Equal(typeof(ValueTask), method.ReturnType);
    }

    [Fact]
    public void StoresModelReferenceForDisposal()
    {
        // Verify the private _model field exists, ensuring the IModel reference
        // is retained for cleanup during DisposeAsync.
        var field = typeof(FoundryLocalChatClient).GetField(
            "_model",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(field);
        Assert.True(
            field.FieldType.FullName == "Microsoft.AI.Foundry.Local.IModel",
            $"Expected _model to be of type IModel but was {field.FieldType.FullName}");
    }
}
