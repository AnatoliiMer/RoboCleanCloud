using System;
using RoboCleanCloud.Domain.Primitives;

namespace RoboCleanCloud.Domain.Entities;

public class CleaningError : Entity
{
    // Приватный конструктор для EF Core
    private CleaningError() { }

    // Публичный конструктор для создания
    public CleaningError(
        Guid sessionId,
        string errorCode,
        string? message = null)
    {
        Id = Guid.NewGuid();
        SessionId = sessionId;
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        Message = message ?? GetDefaultMessageForCode(errorCode);
        Timestamp = DateTime.UtcNow;
        IsResolved = false;
    }

    public Guid SessionId { get; set; }
    public CleaningSession? Session { get; set; }
    public string ErrorCode { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public bool IsResolved { get; set; }
    public string? Resolution { get; set; }

    public void Resolve(string? resolution = null)
    {
        IsResolved = true;
        Resolution = resolution;
    }

    private string GetDefaultMessageForCode(string errorCode)
    {
        return errorCode switch
        {
            "BATTERY_LOW" => "Battery level too low to continue cleaning",
            "WHEEL_STUCK" => "Wheel is stuck, robot cannot move",
            "BRUSH_JAM" => "Main brush is jammed",
            "DUSTBIN_FULL" => "Dustbin is full and needs emptying",
            "CLIFF_SENSOR" => "Cliff sensor triggered, robot is at edge",
            "BUMPER_STUCK" => "Bumper is stuck",
            "FAN_ERROR" => "Fan motor error",
            "WATER_TANK_EMPTY" => "Water tank is empty",
            "MAP_LOST" => "Robot lost its position, needs relocalization",
            "WIFI_DISCONNECTED" => "WiFi connection lost",
            _ => $"Unknown error code: {errorCode}"
        };
    }
}