using Common.Application.Messaging;
using FluentAssertions;
using MediatR;

namespace Common.Application.UnitTests.Messaging;

public class MessagingInterfacesTests
{
    [Fact]
    public void ICommand_ShouldInheritFromIRequest()
    {
        // Arrange & Act
        var commandType = typeof(ICommand);

        // Assert
        commandType.Should().BeAssignableTo<IRequest>();
    }

    [Fact]
    public void ICommandWithResponse_ShouldInheritFromIRequest()
    {
        // Arrange & Act
        var commandType = typeof(ICommand<string>);

        // Assert
        commandType.Should().BeAssignableTo<IRequest<string>>();
    }

    [Fact]
    public void IQuery_ShouldInheritFromIRequest()
    {
        // Arrange & Act
        var queryType = typeof(IQuery<string>);

        // Assert
        queryType.Should().BeAssignableTo<IRequest<string>>();
    }

    [Fact]
    public void TestCommand_ShouldImplementICommand()
    {
        // Arrange
        var command = new TestCommand();

        // Assert
        command.Should().BeAssignableTo<ICommand>();
        command.Should().BeAssignableTo<IRequest>();
    }

    [Fact]
    public void TestCommandWithResponse_ShouldImplementICommandWithResponse()
    {
        // Arrange
        var command = new TestCommandWithResponse();

        // Assert
        command.Should().BeAssignableTo<ICommand<string>>();
        command.Should().BeAssignableTo<IRequest<string>>();
    }

    [Fact]
    public void TestQuery_ShouldImplementIQuery()
    {
        // Arrange
        var query = new TestQuery();

        // Assert
        query.Should().BeAssignableTo<IQuery<string>>();
        query.Should().BeAssignableTo<IRequest<string>>();
    }

    [Fact]
    public void ICommandHandler_ShouldInheritFromIRequestHandler()
    {
        // Arrange & Act
        var handlerType = typeof(ICommandHandler<TestCommand>);

        // Assert
        handlerType.Should().BeAssignableTo<IRequestHandler<TestCommand>>();
    }

    [Fact]
    public void ICommandHandlerWithResponse_ShouldInheritFromIRequestHandler()
    {
        // Arrange & Act
        var handlerType = typeof(ICommandHandler<TestCommandWithResponse, string>);

        // Assert
        handlerType.Should().BeAssignableTo<IRequestHandler<TestCommandWithResponse, string>>();
    }

    [Fact]
    public void IQueryHandler_ShouldInheritFromIRequestHandler()
    {
        // Arrange & Act
        var handlerType = typeof(IQueryHandler<TestQuery, string>);

        // Assert
        handlerType.Should().BeAssignableTo<IRequestHandler<TestQuery, string>>();
    }

    [Fact]
    public void TestCommandHandler_ShouldImplementICommandHandler()
    {
        // Arrange
        var handler = new TestCommandHandler();

        // Assert
        handler.Should().BeAssignableTo<ICommandHandler<TestCommand>>();
        handler.Should().BeAssignableTo<IRequestHandler<TestCommand>>();
    }

    [Fact]
    public void TestCommandWithResponseHandler_ShouldImplementICommandHandler()
    {
        // Arrange
        var handler = new TestCommandWithResponseHandler();

        // Assert
        handler.Should().BeAssignableTo<ICommandHandler<TestCommandWithResponse, string>>();
        handler.Should().BeAssignableTo<IRequestHandler<TestCommandWithResponse, string>>();
    }

    [Fact]
    public void TestQueryHandler_ShouldImplementIQueryHandler()
    {
        // Arrange
        var handler = new TestQueryHandler();

        // Assert
        handler.Should().BeAssignableTo<IQueryHandler<TestQuery, string>>();
        handler.Should().BeAssignableTo<IRequestHandler<TestQuery, string>>();
    }

    [Fact]
    public async Task CommandHandler_ShouldExecuteSuccessfully()
    {
        // Arrange
        var handler = new TestCommandHandler();
        var command = new TestCommand { Value = "test" };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        handler.WasExecuted.Should().BeTrue();
        handler.ExecutedValue.Should().Be("test");
    }

    [Fact]
    public async Task CommandHandlerWithResponse_ShouldReturnValue()
    {
        // Arrange
        var handler = new TestCommandWithResponseHandler();
        var command = new TestCommandWithResponse { Value = "test" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be("test_processed");
    }

    [Fact]
    public async Task QueryHandler_ShouldReturnValue()
    {
        // Arrange
        var handler = new TestQueryHandler();
        var query = new TestQuery { Id = 123 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be("Result for 123");
    }

    // Test helper classes
    public class TestCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestCommandWithResponse : ICommand<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestQuery : IQuery<string>
    {
        public int Id { get; set; }
    }

    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public bool WasExecuted { get; private set; }
        public string ExecutedValue { get; private set; } = string.Empty;

        public Task Handle(TestCommand request, CancellationToken cancellationToken)
        {
            WasExecuted = true;
            ExecutedValue = request.Value;
            return Task.CompletedTask;
        }
    }

    public class TestCommandWithResponseHandler : ICommandHandler<TestCommandWithResponse, string>
    {
        public Task<string> Handle(TestCommandWithResponse request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"{request.Value}_processed");
        }
    }

    public class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Result for {request.Id}");
        }
    }
}
