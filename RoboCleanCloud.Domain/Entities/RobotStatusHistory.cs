using System;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Domain.Primitives;

namespace RoboCleanCloud.Domain.Entities;

public class RobotStatusHistory : Entity
{
    // Приватный конструктор для EF Core
    private RobotStatusHistory() { }

    // Публичный конструктор для создания
    public RobotStatusHistory(
        Guid robotId,
        ConnectionStatus previousStatus,
        ConnectionStatus newStatus,
        string? reason = null)  // Добавлен ?
    {
        Id = Guid.NewGuid();
        RobotId = robotId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason;
        Timestamp = DateTime.UtcNow;
    }

    public Guid RobotId { get; set; }
    public Robot? Robot { get; set; }  // Добавлен ?
    public ConnectionStatus PreviousStatus { get; set; }
    public ConnectionStatus NewStatus { get; set; }
    public string? Reason { get; set; }  // Добавлен ?
    public DateTime Timestamp { get; set; }
}