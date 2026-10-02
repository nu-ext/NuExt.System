namespace NuExt.System.Tests;

internal class AsyncLazyTests
{
    #region Constructor Tests

    [Test]
    public void Constructor_NullFactory_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AsyncLazy<string>((Func<Task<string>>)null!));
        Assert.Throws<ArgumentNullException>(() => new AsyncLazy<string>((Func<string>)null!));
    }

    [Test]
    public void Constructor_WithSyncFactory_WrapsInTaskRun()
    {
        // Arrange
        bool wasExecuted = false;
        var lazy = new AsyncLazy<bool>(() => { wasExecuted = true; return true; });

        // Act
        var task = lazy.Task;
        task.Wait();

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(wasExecuted, Is.True);
            Assert.That(lazy.IsTaskCreated, Is.True);
            Assert.That(lazy.IsCompletedSuccessfully, Is.True);
        }
    }

    #endregion

    #region Async Factory Tests

    [Test]
    public async Task AsyncFactory_RunSynchronouslyFalse_ExecutesOnThreadPool()
    {
        // Arrange
        int originalThreadId = Environment.CurrentManagedThreadId;
        int executionThreadId = originalThreadId;

        var lazy = new AsyncLazy<int>(async () =>
        {
            executionThreadId = Environment.CurrentManagedThreadId;
            await Task.Delay(10);
            return 42;
        }, runSynchronously: false);

        // Act
        var result = await lazy;

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.EqualTo(42));
            Assert.That(executionThreadId, Is.Not.EqualTo(originalThreadId));
            Assert.That(lazy.IsCompletedSuccessfully, Is.True);
        }
    }

    [Test]
    public void AsyncFactory_RunSynchronouslyTrue_ExecutesSynchronouslyOnFirstAccess()
    {
        // Arrange
        int executionThreadId = -1;
        int accessThreadId = Environment.CurrentManagedThreadId;

        var lazy = new AsyncLazy<int>(() =>
        {
            executionThreadId = Environment.CurrentManagedThreadId;
            return Task.FromResult(100);
        }, runSynchronously: true);

        // Act
        var task = lazy.Task; // First access - should execute synchronously
        task.Wait();

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(task.Result, Is.EqualTo(100));
            Assert.That(executionThreadId, Is.EqualTo(accessThreadId));
            Assert.That(lazy.IsTaskCreated, Is.True);
        }
    }

    #endregion

    #region Lazy Behavior Tests

    [Test]
    public async Task IsTaskCreated_False_UntilFirstAccess()
    {
        // Arrange
        int executionCount = 0;
        var lazy = new AsyncLazy<int>(() =>
        {
            executionCount++;
            return Task.FromResult(1);
        });

        using (Assert.EnterMultipleScope())
        {
            // Assert before access
            Assert.That(lazy.IsTaskCreated, Is.False);
            Assert.That(executionCount, Is.Zero);
        }

        // Act - First access
        var task = lazy.Task;

        Assert.That(lazy.IsTaskCreated, Is.True);

        await task;

        Assert.That(executionCount, Is.EqualTo(1));
    }

    [Test]
    public Task TaskProperty_ReturnsSameInstance_OnMultipleCalls()
    {
        // Arrange
        int executionCount = 0;
        var lazy = new AsyncLazy<int>(() =>
        {
            executionCount++;
            return Task.FromResult(99);
        }, runSynchronously: true);

        // Act
        var task1 = lazy.Task;
        var task2 = lazy.Task;
        var task3 = lazy.Task;

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(ReferenceEquals(task1, task2), Is.True);
            Assert.That(ReferenceEquals(task2, task3), Is.True);
            Assert.That(executionCount, Is.EqualTo(1)); // Factory executed only once
        }

        return Task.CompletedTask;
    }

    #endregion

    #region State Properties Tests

    [Test]
    public async Task IsCompletedSuccessfully_True_AfterSuccessfulCompletion()
    {
        // Arrange
        var lazy = new AsyncLazy<int>(() => Task.FromResult(123));

        // Assert before completion
        Assert.That(lazy.IsCompletedSuccessfully, Is.False);

        // Act
        await lazy;

        using (Assert.EnterMultipleScope())
        {
            // Assert after completion
            Assert.That(lazy.IsCompletedSuccessfully, Is.True);
            Assert.That(lazy.IsFaulted, Is.False);
        }
    }

    [Test]
    public async Task IsFaulted_True_AfterFailedTask()
    {
        // Arrange
        var exception = new InvalidOperationException("Test failure");
        var lazy = new AsyncLazy<int>(() => Task.FromException<int>(exception));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await lazy);

        // Wait a moment for task completion
        await Task.Delay(10);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(lazy.IsFaulted, Is.True);
            Assert.That(lazy.IsCompletedSuccessfully, Is.False);
        }
    }

    [Test]
    public async Task IsFaulted_False_WhenExceptionCaughtAndHandled()
    {
        // Arrange
        var lazy = new AsyncLazy<int>(() =>
            throw new InvalidOperationException("Immediate exception"));

        // Act
        Task<int> task = null!;
        try
        {
            task = lazy.Task;
            await task;
        }
        catch
        {
            // Expected
        }

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(lazy.IsTaskCreated, Is.True);
            Assert.That(task.IsFaulted, Is.True);
            Assert.That(lazy.IsFaulted, Is.True);
        }
    }

    #endregion

    #region Awaitability Tests

    [Test]
    public async Task GetAwaiter_CanBeAwaited()
    {
        // Arrange
        var lazy = new AsyncLazy<string>(() => Task.FromResult("Hello"));

        // Act
        var result = await lazy;

        // Assert
        Assert.That(result, Is.EqualTo("Hello"));
    }

    [Test]
    public async Task ConfigureAwait_CanBeAwaited()
    {
        // Arrange
        var lazy = new AsyncLazy<int>(() => Task.FromResult(42));

        // Act
        var result = await lazy.ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public async Task ConfigureAwait_WithFalse_CanBeAwaited()
    {
        // Arrange
        var expected = Guid.NewGuid();
        var lazy = new AsyncLazy<Guid>(() => Task.FromResult(expected));

        // Act
        var result = await lazy.ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public async Task ConfigureAwait_WithTrue_CanBeAwaited()
    {
        // Arrange
        var expected = 3.14;
        var lazy = new AsyncLazy<double>(() => Task.FromResult(expected));

        // Act
        var result = await lazy.ConfigureAwait(true);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public async Task ConfigureAwait_WithTrue_CanBeAwaited2()
    {
        // Arrange
        var lazy = new AsyncLazy<int>(() => Task.FromResult(42));

        // Act
        var result = await lazy.ConfigureAwait(true);

        // Assert
        Assert.That(result, Is.EqualTo(42));
    }

    [Test, Apartment(ApartmentState.STA)]
    public async Task ConfigureAwait_WithFalse_DoesNotReturnToOriginalContext()
    {
        // Arrange
        var syncContext = new SingleThreadSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(syncContext);
        var lazy = new AsyncLazy<int>(() => Task.FromResult(42));
        int continuationThreadId = -1;

        // Act
        await lazy.ConfigureAwait(false);
        continuationThreadId = Environment.CurrentManagedThreadId;

        // Reset context
        SynchronizationContext.SetSynchronizationContext(null);

        // Assert - continuation should NOT run on original context thread
        Assert.That(continuationThreadId, Is.Not.EqualTo(syncContext.ThreadId));
    }


    [Test]
    public async Task MultipleAwaits_ReturnSameResult()
    {
        // Arrange
        int counter = 0;
        var lazy = new AsyncLazy<int>(async () =>
        {
            await Task.Delay(10);
            return ++counter;
        });

        // Act
        var result1 = await lazy;
        var result2 = await lazy;
        var result3 = await lazy;

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result1, Is.EqualTo(1));
            Assert.That(result2, Is.EqualTo(1)); // Same result, factory not re-executed
            Assert.That(result3, Is.EqualTo(1));
            Assert.That(counter, Is.EqualTo(1)); // Executed only once
        }
    }

    #endregion

    #region Edge Cases

    [Test]
    public async Task AsyncLazy_WithLongRunningOperation_CompletesSuccessfully()
    {
        // Arrange
        var lazy = new AsyncLazy<int>(async () =>
        {
            await Task.Delay(100);
            return 999;
        });

        // Act
        var task = lazy.Task;
        var result = await task;

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.EqualTo(999));
            Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }
    }

    [Test]
    public async Task AsyncLazy_WithCancellation_HandlesCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        var lazy = new AsyncLazy<int>(async () =>
        {
            await Task.Delay(1000, cts.Token);
            return 1;
        });

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await lazy);
    }

    [Test]
    public async Task AsyncLazy_ValueType_WorksCorrectly()
    {
        // Arrange
        var expected = DateTime.UtcNow;
        var lazy = new AsyncLazy<DateTime>(() => Task.FromResult(expected));

        // Act
        var result = await lazy;

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.EqualTo(expected).Within(TimeSpan.FromMilliseconds(1)));
            Assert.That(lazy.IsCompletedSuccessfully, Is.True);
        }
    }

    [Test]
    public async Task AsyncLazy_NullableValueType_HandlesNull()
    {
        // Arrange
        var lazy = new AsyncLazy<int?>(() => Task.FromResult<int?>(null));

        // Act
        var result = await lazy;

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.Null);
            Assert.That(lazy.IsCompletedSuccessfully, Is.True);
        }
    }

    [Test]
    public async Task AsyncLazy_ReferenceType_HandlesNull()
    {
        // Arrange
        var lazy = new AsyncLazy<string?>(() => Task.FromResult<string?>(null));

        // Act
        var result = await lazy;

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(result, Is.Null);
            Assert.That(lazy.IsCompletedSuccessfully, Is.True);
        }
    }


    #endregion

    #region Race Condition Tests

    [Test]
    public async Task ConcurrentAccess_CreatesTaskOnlyOnce()
    {
        // Arrange
        int executionCount = 0;
        var lazy = new AsyncLazy<int>(() =>
        {
            Interlocked.Increment(ref executionCount);
            Thread.Sleep(50); // Simulate some work
            return Task.FromResult(42);
        });

        // Act
        var tasks = new Task<int>[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(async () => await lazy);
        }

        await Task.WhenAll(tasks);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(executionCount, Is.EqualTo(1), "Factory should be executed only once");
            Assert.That(lazy.IsTaskCreated, Is.True);
        }
        foreach (var task in tasks)
        {
            Assert.That(task.Result, Is.EqualTo(42));
        }
    }

    [Test]
    public async Task AsyncLazy_WithExceptionInFactory_PropagatesExceptionOnEveryAccess()
    {
        // Arrange
        var exception = new InvalidOperationException("Factory failed");
        var lazy = new AsyncLazy<int>(() => throw exception);

        // Act & Assert - First access
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await lazy);

        // Second access should throw the same exception
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await lazy);

        // Third access via Task property
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await lazy.Task);
    }

    #endregion

    private class SingleThreadSynchronizationContext : SynchronizationContext
    {
        public int ThreadId { get; } = Environment.CurrentManagedThreadId;

        public override void Post(SendOrPostCallback d, object? state)
        {
            // Simulate marshaling to another thread
            ThreadPool.QueueUserWorkItem(_ => d(state));
        }
    }
}
