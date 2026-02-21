using System;
using System.Collections.Generic;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Domain.Primitives;
using RoboCleanCloud.Domain.Events;
using RoboCleanCloud.Domain.Exceptions;

namespace RoboCleanCloud.Domain.Entities;

public class Robot : AggregateRoot
{
    private Robot() { } // For EF Core

    public Robot(
        string serialNumber,
        string model,
        string friendlyName,
        Guid ownerId)
    {
        Id = Guid.NewGuid();

        SerialNumber = serialNumber ?? throw new ArgumentNullException(nameof(serialNumber));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        FriendlyName = friendlyName ?? throw new ArgumentNullException(nameof(friendlyName));
        OwnerId = ownerId;
        ConnectionStatus = ConnectionStatus.Offline;
        BatteryLevel = 100;
        FirmwareVersion = "1.0.0";
        RegisteredAt = DateTime.UtcNow;
        DustbinLevel = 0;

        StatusHistory = new List<RobotStatusHistory>();
        MaintenanceItems = new List<MaintenanceItem>();
        CleaningSessions = new List<CleaningSession>();
    }

    public string SerialNumber { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public string FriendlyName { get; private set; } = null!;
    public Guid OwnerId { get; private set; }
    public ConnectionStatus ConnectionStatus { get; private set; }
    public int BatteryLevel { get; private set; }
    public string FirmwareVersion { get; private set; } = null!;
    public DateTime RegisteredAt { get; private set; }
    public DateTime? LastSeenAt { get; private set; }
    public int DustbinLevel { get; private set; }

    // Navigation properties
    public ICollection<RobotStatusHistory> StatusHistory { get; private set; } = null!;
    public ICollection<MaintenanceItem> MaintenanceItems { get; private set; } = null!;
    public ICollection<CleaningSession> CleaningSessions { get; private set; } = null!;

    // Domain methods
    public void UpdateStatus(ConnectionStatus status, string? reason = null)
    {
        var previousStatus = ConnectionStatus;
        ConnectionStatus = status;
        LastSeenAt = DateTime.UtcNow;

        StatusHistory.Add(new RobotStatusHistory(
            robotId: Id,
            previousStatus: previousStatus,
            newStatus: status,
            reason: reason
        ));

        AddDomainEvent(new RobotStatusChangedEvent(Id, previousStatus, status));
    }

    public void UpdateBatteryLevel(int level)
    {
        if (level < 0 || level > 100)
            throw new DomainException("Battery level must be between 0 and 100");

        BatteryLevel = level;

        if (level < 20)
            AddDomainEvent(new BatteryLowEvent(Id, level));
    }

    public void UpdateDustbinLevel(int level)
    {
        if (level < 0 || level > 100)
            throw new DomainException("Dustbin level must be between 0 and 100");

        DustbinLevel = level;
    }

    public bool CanStartCleaning()
    {
        return ConnectionStatus == ConnectionStatus.Online &&
               BatteryLevel >= 15;
    }

    public void StartCleaning()
    {
        if (!CanStartCleaning())
            throw new DomainException("Robot cannot start cleaning");

        UpdateStatus(ConnectionStatus.Busy, "Started cleaning");
    }

    public void ReturnToBase()
    {
        UpdateStatus(ConnectionStatus.ReturningToBase);
        AddDomainEvent(new RobotReturningToBaseEvent(Id));
    }
}