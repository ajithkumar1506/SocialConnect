using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SocialConnect.Application.Common.Interfaces;

namespace SocialConnect.Infrastructure.Services;

public class RabbitMqEventBus : IEventBus, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public RabbitMqEventBus(IConfiguration configuration, ILogger<RabbitMqEventBus> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection != null && _channel != null)
            return;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection != null && _channel != null)
                return;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest",
            };

            if (int.TryParse(_configuration["RabbitMQ:Port"], out var port))
            {
                factory.Port = port;
            }

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            // Declare a direct exchange
            await _channel.ExchangeDeclareAsync(
                exchange: "socialconnect_exchange",
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken
            );
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            await EnsureConnectionAsync(cancellationToken);

            var eventName = @event.GetType().Name;
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties
            {
                Persistent = true,
                MessageId = Guid.NewGuid().ToString(),
                Type = eventName,
            };

            var channel = _channel;
            if (channel == null)
            {
                throw new InvalidOperationException("RabbitMQ channel is not initialized.");
            }

            await channel.BasicPublishAsync(
                exchange: "socialconnect_exchange",
                routingKey: eventName,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Published event {EventName} to RabbitMQ", eventName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Could not publish event {EventName} to RabbitMQ",
                @event.GetType().Name
            );
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
    }
}
