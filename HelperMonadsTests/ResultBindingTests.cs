using HelperMonads.Result;

namespace HelperMonadsTests;

/// <summary>
/// Contains unit tests for various binding operations on the <see cref="Result{T}" /> class.
/// <para>
/// These tests validate the behavior of <c>Bind</c>, <c>BindWithData</c>, <c>BindWithDataAsync</c>, and <c>BindAsync</c>
/// methods under different scenarios including success, failure, and cancellation.
/// </para>
/// The tests ensure that these methods handle the propagation of success and failure states correctly, as well as pass data and
/// respect cancellation tokens.
/// </summary>
[TestFixture]
public class ResultBindingTests
{
    private static Exception TestException => new(FailureMessage);
    private const string FailureMessage = "ValidData";
    private const string SuccessMessage = "Success";
    private const string NextMessage = "Next";
    private const string ProcessedMessage = "Processed";

    /// <summary>
    /// Tests that the Bind method returns a failure result when the initial result is a failure.
    /// </summary>
    [Test]
    public void Bind_ShouldReturnResult_WhenFailure()
    {
        // Arrange
        IResult<string> failureResult = Result<string>.Failure(TestException);

        // Act
        var result = failureResult.Bind(() => Result<string>.Success(NextMessage));

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error?.Message, Is.EqualTo(FailureMessage));
    }

    /// <summary>
    /// Tests that the Bind method invokes the provided function and returns a success result when the initial result is successful.
    /// </summary>
    [Test]
    public void Bind_ShouldInvokeFunction_WhenSuccess()
    {
        // Arrange
        IResult<string> successResult = Result<string>.Success(SuccessMessage);

        // Act
        var result = successResult.Bind(() => Result<string>.Success(NextMessage));

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.EqualTo(NextMessage));
    }

    /// <summary>
    /// Tests that the BindWithData method returns a failure result when the initial result is a failure.
    /// </summary>
    [Test]
    public void BindWithData_ShouldReturnResult_WhenFailure()
    {
        // Arrange
        IResult<string> failureResult = Result<string>.Failure(TestException);

        // Act
        var result = failureResult.BindWithData(_ => Result<string>.Success(NextMessage));

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error?.Message, Is.EqualTo(FailureMessage));
    }

    /// <summary>
    /// Tests that the BindWithData method invokes the provided function and returns a success result when the initial result is
    /// successful.
    /// </summary>
    [Test]
    public void BindWithData_ShouldInvokeFunction_WhenSuccess()
    {
        // Arrange
        IResult<string> successResult = Result<string>.Success(SuccessMessage);

        // Act
        var result = successResult.BindWithData(data => Result<string>.Success(data + NextMessage));

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.EqualTo(SuccessMessage + NextMessage));
    }

    /// <summary>
    /// Tests that the BindWithDataAsync method returns a success result when the initial result is a success.
    /// </summary>
    [Test]
    public async Task BindWithDataAsync_ShouldReturnSuccess_WhenResultIsSuccess()
    {
        // Arrange
        IResult<string> result = Result<string>.Success(SuccessMessage);

        // Act
        var processedResult = await result.BindWithDataAsync(ProcessData);

        // Assert
        Assert.That(processedResult.IsSuccess, Is.True);
        Assert.That(processedResult.Data, Is.EqualTo(SuccessMessage + ProcessedMessage));
        return;

        Task<IResult<string>> ProcessData(string data)
        {
            return Task.FromResult<IResult<string>>(Result<string>.Success(data + ProcessedMessage));
        }
    }

    /// <summary>
    /// Tests that the BindWithDataAsync method returns a failure result when the initial result is a failure.
    /// </summary>
    [Test]
    public async Task BindWithDataAsync_ShouldReturnFailure_WhenResultIsFailure()
    {
        // Arrange
        IResult<string> result = Result<string>.Failure(TestException);

        // Act
        var processedResult = await result.BindWithDataAsync(ProcessData);

        // Assert
        Assert.That(processedResult.IsFailure, Is.True);
        Assert.That(processedResult.Error?.Message, Is.EqualTo(FailureMessage));
        return;

        Task<IResult<string>> ProcessData(string data)
        {
            return Task.FromResult<IResult<string>>(Result<string>.Success(data + ProcessedMessage));
        }
    }

    /// <summary>
    /// Tests that the BindWithDataAsync method passes data to the function when the initial result is a success.
    /// </summary>
    [Test]
    public async Task BindWithDataAsync_ShouldPassDataToFunction_WhenResultIsSuccess()
    {
        // Arrange
        IResult<int> result = Result<int>.Success(10);

        // Act
        var processedResult = await result.BindWithDataAsync(AddDataToResult);

        // Assert
        Assert.That(processedResult.IsSuccess, Is.True);
        Assert.That(processedResult.Data, Is.EqualTo(15));
        return;

        Task<IResult<int>> AddDataToResult(int data)
        {
            return Task.FromResult<IResult<int>>(Result<int>.Success(data + 5));
        }
    }

    /// <summary>
    /// Tests that the BindWithDataAsync method returns a failure result when the initial result is a failure, and a cancellation
    /// token is used.
    /// </summary>
    [Test]
    public async Task BindWithDataAsync_ShouldReturnFailure_WhenResultIsFailure_WithCancellationToken()
    {
        // Arrange
        IResult<int> result = Result<int>.Failure(TestException);
        var cancellationToken = CancellationToken.None;

        // Act
        var processedResult = await result.BindWithDataAsync(ProcessData, cancellationToken);

        // Assert
        Assert.That(processedResult.IsFailure, Is.True);
        Assert.That(processedResult.Error?.Message, Is.EqualTo(FailureMessage));
        return;

        Task<IResult<int>> ProcessData(int data, CancellationToken ct)
        {
            return Task.FromResult<IResult<int>>(Result<int>.Success(data + 10));
        }
    }

    /// <summary>
    /// Tests that the BindWithDataAsync method returns a success result when the initial result is a success, and a cancellation
    /// token is used.
    /// </summary>
    [Test]
    public async Task BindWithDataAsync_ShouldReturnSuccess_WhenResultIsSuccess_WithCancellationToken()
    {
        // Arrange
        IResult<string> result = Result<string>.Success(SuccessMessage);
        var cancellationToken = CancellationToken.None;

        // Act
        var processedResult = await result.BindWithDataAsync(AppendDataToResult, cancellationToken);

        // Assert
        Assert.That(processedResult.IsSuccess, Is.True);
        Assert.That(processedResult.Data, Is.EqualTo(SuccessMessage + ProcessedMessage));
        return;

        Task<IResult<string>> AppendDataToResult(string data, CancellationToken ct)
        {
            return Task.FromResult<IResult<string>>(Result<string>.Success(data + ProcessedMessage));
        }
    }

    /// <summary>
    /// Tests that the BindWithDataAsync method respects cancellation when the cancellation token is canceled.
    /// </summary>
    [Test]
    public void BindWithDataAsync_ShouldRespectCancellation_WhenTokenIsCancelled()
    {
        // Arrange
        IResult<string> result = Result<string>.Success(SuccessMessage);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            _ = await result.BindWithDataAsync(ProcessDataWithCancellation, cancellationTokenSource.Token));
        return;

        async Task<IResult<string>> ProcessDataWithCancellation(string data, CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return Result<string>.Success(data + ProcessedMessage);
        }
    }

    /// <summary>
    /// Tests that the BindAsync method returns a success result when the initial result is a success.
    /// </summary>
    [Test]
    public async Task BindAsync_ShouldReturnSuccess_WhenResultIsSuccess()
    {
        // Arrange
        IResult<string> result = Result<string>.Success(SuccessMessage);

        // Act
        var processedResult = await result.BindAsync(ProcessDataAsync);

        // Assert
        Assert.That(processedResult.IsSuccess, Is.True);
        Assert.That(processedResult.Data, Is.EqualTo(SuccessMessage + ProcessedMessage));
        return;

        Task<IResult<string>> ProcessDataAsync()
        {
            return Task.FromResult<IResult<string>>(Result<string>.Success(SuccessMessage + ProcessedMessage));
        }
    }

    /// <summary>
    /// Tests that the BindAsync method returns a failure result when the initial result is a failure.
    /// </summary>
    [Test]
    public async Task BindAsync_ShouldReturnFailure_WhenResultIsFailure()
    {
        // Arrange
        IResult<string> result = Result<string>.Failure(TestException);

        // Act
        var processedResult = await result.BindAsync(ProcessDataAsync);

        // Assert
        Assert.That(processedResult.IsFailure, Is.True);
        Assert.That(processedResult.Error?.Message, Is.EqualTo(FailureMessage));
        return;

        Task<IResult<string>> ProcessDataAsync()
        {
            return Task.FromResult<IResult<string>>(Result<string>.Success(FailureMessage + ProcessedMessage));
        }
    }

    /// <summary>
    /// Tests that the BindAsync method passes data to the function when the initial result is a success.
    /// </summary>
    [Test]
    public async Task BindAsync_ShouldPassDataToFunction_WhenResultIsSuccess()
    {
        // Arrange
        IResult<int> result = Result<int>.Success(10);

        // Act
        var processedResult = await result.BindAsync(AddDataToResultAsync);

        // Assert
        Assert.That(processedResult.IsSuccess, Is.True);
        Assert.That(processedResult.Data, Is.EqualTo(15));
        return;

        Task<IResult<int>> AddDataToResultAsync()
        {
            return Task.FromResult<IResult<int>>(Result<int>.Success(10 + 5));
        }
    }

    /// <summary>
    /// Tests that the BindAsync method returns a failure result when the initial result is a failure, and a cancellation token is
    /// used.
    /// </summary>
    [Test]
    public async Task BindAsync_ShouldReturnFailure_WhenResultIsFailure_WithCancellationToken()
    {
        // Arrange
        IResult<int> result = Result<int>.Failure(TestException);
        var cancellationToken = CancellationToken.None;

        // Act
        var processedResult = await result.BindAsync(ProcessDataAsync, cancellationToken);

        // Assert
        Assert.That(processedResult.IsFailure, Is.True);
        Assert.That(processedResult.Error?.Message, Is.EqualTo(FailureMessage));
        return;

        async Task<IResult<int>> ProcessDataAsync(CancellationToken ct)
        {
            await Task.Delay(50, ct);
            return Result<int>.Success(10 + 5);
        }
    }

    /// <summary>
    /// Tests that the BindAsync method returns a success result when the initial result is a success, and a cancellation token is
    /// used.
    /// </summary>
    [Test]
    public async Task BindAsync_ShouldReturnSuccess_WhenResultIsSuccess_WithCancellationToken()
    {
        // Arrange
        IResult<string> result = Result<string>.Success(SuccessMessage);
        var cancellationToken = CancellationToken.None;

        // Act
        var processedResult = await result.BindAsync(AppendDataToResultAsync, cancellationToken);

        // Assert
        Assert.That(processedResult.IsSuccess, Is.True);
        Assert.That(processedResult.Data, Is.EqualTo(SuccessMessage + ProcessedMessage));
        return;

        async Task<IResult<string>> AppendDataToResultAsync(CancellationToken ct)
        {
            await Task.Delay(50, ct);
            return Result<string>.Success(SuccessMessage + ProcessedMessage);
        }
    }

    /// <summary>
    /// Tests that the BindAsync method respects cancellation when the cancellation token is canceled.
    /// </summary>
    [Test]
    public void BindAsync_ShouldRespectCancellation_WhenTokenIsCancelled()
    {
        // Arrange
        IResult<string> result = Result<string>.Success(SuccessMessage);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            _ = await result.BindAsync(ProcessDataWithCancellationAsync, cancellationTokenSource.Token));
        return;

        async Task<IResult<string>> ProcessDataWithCancellationAsync(CancellationToken ct)
        {
            await Task.Delay(100, ct);
            ct.ThrowIfCancellationRequested();
            return Result<string>.Success(SuccessMessage + ProcessedMessage);
        }
    }

    /// <summary>
    /// Tests that the BindAndTransform method returns a failure result when the initial result is a failure.
    /// </summary>
    [Test]
    public void BindAndTransform_ShouldReturnResult_WhenFailure()
    {
        // Arrange
        IResult<string> failureResult = Result<string>.Failure(TestException);

        // Act
        var result = failureResult.BindAndTransform(_ => Result<int>.Success(1));

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error?.Message, Is.EqualTo(FailureMessage));
    }

    /// <summary>
    /// Tests that the BindAndTransform method invokes the provided function and returns a success result when the initial result is
    /// successful.
    /// </summary>
    [Test]
    public void BindAndTransform_ShouldInvokeFunction_WhenSuccess()
    {
        // Arrange
        IResult<string> successResult = Result<string>.Success(SuccessMessage);

        // Act
        var result = successResult.BindAndTransform(_ => Result<int>.Success(1));

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.EqualTo(1));
    }

    /// <summary>
    /// Tests that the BindAndTransformAsync method returns a success result when the initial result is a success.
    /// </summary>
    [Test]
    public async Task BindAndTransformAsync_ShouldReturnSuccess_WhenResultIsSuccess()
    {
        // Arrange
        IResult<string> result = Result<string>.Success(SuccessMessage);

        // Act
        var processedResult = await result.BindAndTransformAsync(ProcessData);

        // Assert
        Assert.That(processedResult.IsSuccess, Is.True);
        Assert.That(processedResult.Data, Is.EqualTo(1));
        return;

        Task<IResult<int>> ProcessData(string data)
        {
            return Task.FromResult<IResult<int>>(Result<int>.Success(1));
        }
    }

    /// <summary>
    /// Tests that the BindAndTransformAsync method returns a failure result when the initial result is a failure.
    /// </summary>
    [Test]
    public async Task BindAndTransformAsync_ShouldReturnFailure_WhenResultIsFailure()
    {
        // Arrange
        IResult<string> result = Result<string>.Failure(TestException);

        // Act
        var processedResult = await result.BindAndTransformAsync(ProcessData);

        // Assert
        Assert.That(processedResult.IsFailure, Is.True);
        Assert.That(processedResult.Error?.Message, Is.EqualTo(FailureMessage));
        return;

        Task<IResult<int>> ProcessData(string data)
        {
            return Task.FromResult<IResult<int>>(Result<int>.Success(1));
        }
    }

    /// <summary>
    /// Tests that the BindAndTransformAsync method passes data to the function when the initial result is a success.
    /// </summary>
    [Test]
    public async Task BindAndTransformAsync_ShouldPassDataToFunction_WhenResultIsSuccess()
    {
        // Arrange
        IResult<string> result = Result<string>.Success(SuccessMessage);

        // Act
        var processedResult = await result.BindAndTransformAsync(AddDataToResult);

        // Assert
        Assert.That(processedResult.IsSuccess, Is.True);
        Assert.That(processedResult.Data, Is.EqualTo(1));
        return;

        Task<IResult<int>> AddDataToResult(string data)
        {
            return Task.FromResult<IResult<int>>(Result<int>.Success(1));
        }
    }

    /// <summary>
    /// Tests that the BindAndTransformAsync method returns a failure result when the initial result is a failure, and a cancellation
    /// token is used.
    /// </summary>
    [Test]
    public async Task BindAndTransformAsync_ShouldReturnFailure_WhenResultIsFailure_WithCancellationToken()
    {
        // Arrange
        IResult<string> result = Result<string>.Failure(TestException);
        var cancellationToken = CancellationToken.None;

        // Act
        var processedResult = await result.BindAndTransformAsync(ProcessData, cancellationToken);

        // Assert
        Assert.That(processedResult.IsFailure, Is.True);
        Assert.That(processedResult.Error?.Message, Is.EqualTo(FailureMessage));
        return;

        Task<IResult<int>> ProcessData(string data, CancellationToken ct)
        {
            return Task.FromResult<IResult<int>>(Result<int>.Success(1));
        }
    }

    /// <summary>
    /// Tests that the BindAndTransformAsync method returns a success result when the initial result is a success, and a cancellation
    /// token is used.
    /// </summary>
    [Test]
    public async Task BindAndTransformAsync_ShouldReturnSuccess_WhenResultIsSuccess_WithCancellationToken()
    {
        // Arrange
        IResult<string> result = Result<string>.Success(SuccessMessage);
        var cancellationToken = CancellationToken.None;

        // Act
        var processedResult = await result.BindAndTransformAsync(AppendDataToResult, cancellationToken);

        // Assert
        Assert.That(processedResult.IsSuccess, Is.True);
        Assert.That(processedResult.Data, Is.EqualTo(1));
        return;

        Task<IResult<int>> AppendDataToResult(string data, CancellationToken ct)
        {
            return Task.FromResult<IResult<int>>(Result<int>.Success(1));
        }
    }

    /// <summary>
    /// Tests that the BindAndTransformAsync method respects cancellation when the cancellation token is canceled.
    /// </summary>
    [Test]
    public void BindAndTransformAsync_ShouldRespectCancellation_WhenTokenIsCancelled()
    {
        // Arrange
        IResult<string> result = Result<string>.Success(SuccessMessage);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            _ = await result.BindAndTransformAsync(ProcessDataWithCancellation, cancellationTokenSource.Token));
        return;

        async Task<IResult<int>> ProcessDataWithCancellation(string data, CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return Result<int>.Success(1);
        }
    }
}