using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using RoboCleanCloud.Application.Interfaces.Services;
using RoboCleanCloud.Domain.Enums;

namespace RoboCleanCloud.Infrastructure.Services;

public class RobotCommandGateway : IRobotCommandGateway
{
    private readonly IMqttClient _mqttClient;
    private readonly ILogger<RobotCommandGateway> _logger;

    public RobotCommandGateway(
        IMqttClient mqttClient,
        ILogger<RobotCommandGateway> logger)
    {
        _mqttClient = mqttClient;
        _logger = logger;
    }

    public async Task SendCleaningCommandAsync(
        Guid robotId,
        Guid sessionId,
        CleaningMode mode,
        List<Guid> zoneIds,
        CancellationToken cancellationToken = default)
    {
        var command = new
        {
            command = "start_cleaning",
            sessionId,
            mode = mode.ToString(),
            zones = zoneIds,
            timestamp = DateTime.UtcNow
        };

        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"robots/{robotId}/commands")
            .WithPayload(JsonSerializer.Serialize(command))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(false)
            .Build();

        try
        {
            if (!_mqttClient.IsConnected)
            {
                _logger.LogWarning("MQTT client not connected to robot {RobotId}", robotId);
                // В реальном проекте здесь должна быть логика подключения
                // await _mqttClient.ConnectAsync(_mqttClient.Options, cancellationToken);
            }

            await _mqttClient.PublishAsync(message, cancellationToken);
            _logger.LogInformation("Sent cleaning command to robot {RobotId}, session {SessionId}",
                robotId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send cleaning command to robot {RobotId}", robotId);
            throw;
        }
    }

    public async Task SendReturnToBaseCommandAsync(
        Guid robotId,
        CancellationToken cancellationToken = default)
    {
        var command = new
        {
            command = "return_to_base",
            timestamp = DateTime.UtcNow
        };

        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"robots/{robotId}/commands")
            .WithPayload(JsonSerializer.Serialize(command))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(message, cancellationToken);
        _logger.LogInformation("Sent return to base command to robot {RobotId}", robotId);
    }

    public async Task SendPauseCommandAsync(
        Guid robotId,
        CancellationToken cancellationToken = default)
    {
        var command = new
        {
            command = "pause",
            timestamp = DateTime.UtcNow
        };

        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"robots/{robotId}/commands")
            .WithPayload(JsonSerializer.Serialize(command))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(message, cancellationToken);
        _logger.LogInformation("Sent pause command to robot {RobotId}", robotId);
    }

    public async Task SendResumeCommandAsync(
        Guid robotId,
        CancellationToken cancellationToken = default)
    {
        var command = new
        {
            command = "resume",
            timestamp = DateTime.UtcNow
        };

        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"robots/{robotId}/commands")
            .WithPayload(JsonSerializer.Serialize(command))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(message, cancellationToken);
        _logger.LogInformation("Sent resume command to robot {RobotId}", robotId);
    }

    public async Task SendStopCommandAsync(
        Guid robotId,
        CancellationToken cancellationToken = default)
    {
        var command = new
        {
            command = "stop",
            timestamp = DateTime.UtcNow
        };

        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"robots/{robotId}/commands")
            .WithPayload(JsonSerializer.Serialize(command))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(message, cancellationToken);
        _logger.LogInformation("Sent stop command to robot {RobotId}", robotId);
    }

    public async Task<bool> TestConnectionAsync(
        Guid robotId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new
            {
                command = "ping",
                timestamp = DateTime.UtcNow
            };

            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"robots/{robotId}/ping")
                .WithPayload(JsonSerializer.Serialize(command))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _mqttClient.PublishAsync(message, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test connection to robot {RobotId}", robotId);
            return false;
        }
    }
}

