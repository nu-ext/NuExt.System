using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NuExt.System.Tests;

[SuppressMessage("Assertion", "NUnit2045:Use Assert.Multiple", Justification = "<Pending>")]
public class ValueTaskTests
{
    [Test]
    public async Task WhenAll_AllTasksSucceed_ReturnsResults_IReadOnlyList()
    {
        var tasks = new List<ValueTask<int>>
        {
            new(Task.FromResult(1)),
            new(Task.FromResult(2)),
            new(Task.FromResult(3))
        };

        var result = await tasks.WhenAll();

        Assert.That(result, Is.EqualTo([1, 2, 3]));
    }

    [Test]
    public async Task WhenAll_AllTasksSucceed_ReturnsResults_IEnumerable()
    {
        var tasks = new List<ValueTask<int>>
        {
            new(Task.FromResult(1)),
            new(Task.FromResult(2)),
            new(Task.FromResult(3))
        };

        var result = await ValueTask.WhenAll(tasks);

        Assert.That(result, Is.EqualTo([1, 2, 3]));
    }

    [Test]
    public async Task WhenAll_AllTasksSucceed_ReturnsResults_Params()
    {
        var tasks = new List<ValueTask<int>>
        {
            new(Task.FromResult(1)),
            new(Task.FromResult(2)),
            new(Task.FromResult(3))
        }.ToArray();

        var result = await ValueTask.WhenAll(tasks);

        Assert.That(result, Is.EqualTo([1, 2, 3]));
    }

    [Test]
    public async Task WhenAll_AllTasksSucceed_ReturnsResults_ReadOnlySpan()
    {
        var tasks = new List<ValueTask<int>>
        {
            new(Task.FromResult(1)),
            new(Task.FromResult(2)),
            new(Task.FromResult(3))
        }.ToArray();

        var result = await ValueTask.WhenAll(tasks.AsSpan());

        Assert.That(result, Is.EqualTo([1, 2, 3]));
    }

    [Test]
    public async Task WhenAll_TaskFails_ThrowsAggregateException()
    {
        var tasks = new List<ValueTask<int>>
        {
            new(Task.FromResult(1)),
            new(Task.FromException<int>(new InvalidOperationException("Test exception"))),
            new(Task.FromResult(3))
        };

        var ex = await Assert.ThrowsAsync<AggregateException>(async () => await tasks.WhenAll());
        Assert.That(ex?.InnerExceptions.Count, Is.EqualTo(1));
        Assert.That(ex?.InnerException, Is.TypeOf<InvalidOperationException>());
    }

    [Test]
    public async Task WhenAll_EmptyTaskList_ReturnsEmptyArray()
    {
        // Arrange
        var tasks = new List<ValueTask<int>>();

        // Act
        var result = await tasks.WhenAll();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    /*[Test]
    public async Task WhenAll_CancellationTokenIsCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var tasks = new List<ValueTask<int>>
        {
            new ValueTask<int>(Task.Delay(1000).ContinueWith(_ => 1)),
            new ValueTask<int>(Task.Delay(1000).ContinueWith(_ => 2))
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<OperationCanceledException>(async () => await tasks.WhenAll(cts.Token));
        Assert.That(ex,  Is.InstanceOf<OperationCanceledException>());
    }*/

    [Test]
    public async Task WhenAll_EnumerableTasks_AllSucceed()
    {
        var tasks = new List<ValueTask>
        {
            ValueTask.CompletedTask,
            ValueTask.CompletedTask,
            ValueTask.CompletedTask
        };

        await Assert.DoesNotThrowAsync(async () => await tasks.WhenAll());
    }

    [Test]
    public async Task WhenAll_EnumerableTasks_SomeFail()
    {
        var tasks = new List<ValueTask>
    {
        ValueTask.CompletedTask,
        new(Task.FromException(new InvalidOperationException())),
        new(Task.FromException(new ArgumentNullException()))
    };

        var exception = await Assert.ThrowsAsync<AggregateException>(async () => await tasks.WhenAll());
        Assert.That(exception.InnerExceptions, Has.Exactly(2).Items);
        Assert.That(exception.InnerExceptions, Has.Some.InstanceOf<InvalidOperationException>());
        Assert.That(exception.InnerExceptions, Has.Some.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public async Task WhenAll_EnumerableTasks_Empty()
    {
        var tasks = Enumerable.Empty<ValueTask>();

        await Assert.DoesNotThrowAsync(async () => await ValueTask.WhenAll(tasks));
    }

    [Test]
    public async Task WhenAll_ParamsTasks_AllSucceed()
    {
        var tasks = new[]
        {
            ValueTask.CompletedTask,
            ValueTask.CompletedTask,
            ValueTask.CompletedTask
        };

        await Assert.DoesNotThrowAsync(async () => await ValueTask.WhenAll(tasks));
    }

    [Test]
    public async Task WhenAll_ParamsTasks_SomeFail()
    {
        var tasks = new[]
        {
            ValueTask.CompletedTask,
            new ValueTask(Task.FromException(new InvalidOperationException())),
            new ValueTask(Task.FromException(new ArgumentNullException()))
        };

        var exception = await Assert.ThrowsAsync<AggregateException>(async () => await ValueTask.WhenAll(tasks));
        Assert.That(exception.InnerExceptions, Has.Exactly(2).Items);
        Assert.That(exception.InnerExceptions, Has.Some.InstanceOf<InvalidOperationException>());
        Assert.That(exception.InnerExceptions, Has.Some.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public async Task WhenAll_ParamsTasks_Empty()
    {
        var tasks = Array.Empty<ValueTask>();

        await Assert.DoesNotThrowAsync(async () => await ValueTask.WhenAll(tasks));
    }

    [Test]
    public async Task WhenAll_ReadOnlySpanTasks_AllSucceed()
    {
        var tasks = new[]
        {
            ValueTask.CompletedTask,
            ValueTask.CompletedTask,
            ValueTask.CompletedTask
        };

        await Assert.DoesNotThrowAsync(async () => await ValueTask.WhenAll(tasks.AsSpan()));
    }

    [Test]
    public async Task WhenAll_ReadOnlySpanTasks_SomeFail()
    {
        var tasks = new[]
        {
            ValueTask.CompletedTask,
            new ValueTask(Task.FromException(new InvalidOperationException())),
            new ValueTask(Task.FromException(new ArgumentNullException()))
        };

        var exception = await Assert.ThrowsAsync<AggregateException>(async () => await ValueTask.WhenAll(tasks.AsSpan()));
        Assert.That(exception.InnerExceptions, Has.Exactly(2).Items);
        Assert.That(exception.InnerExceptions, Has.Some.InstanceOf<InvalidOperationException>());
        Assert.That(exception.InnerExceptions, Has.Some.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public async Task WhenAll_ReadOnlySpanTasks_Empty()
    {
        var tasks = Array.Empty<ValueTask>();

        await Assert.DoesNotThrowAsync(async () => await ValueTask.WhenAll(tasks.AsSpan()));
    }
}
