using System;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Domain.Primitives;

namespace RoboCleanCloud.Domain.Entities;

public class MaintenanceItem : Entity
{
    // Приватный конструктор для EF Core
    private MaintenanceItem() { }

    // Публичный конструктор для создания
    public MaintenanceItem(
        Guid robotId,
        ItemType type,
        int initialHealth = 100)
    {
        Id = Guid.NewGuid();
        RobotId = robotId;
        Type = type;
        CurrentHealth = initialHealth;
        LastReplacedAt = DateTime.UtcNow;
        EstimatedDaysLeft = CalculateEstimatedDaysLeft(type, initialHealth);
    }

    public Guid RobotId { get; set; }
    public Robot? Robot { get; set; }  // Добавлен ?
    public ItemType Type { get; set; }
    public int CurrentHealth { get; set; }
    public DateTime LastReplacedAt { get; set; }
    public int EstimatedDaysLeft { get; set; }

    public void Replace()
    {
        LastReplacedAt = DateTime.UtcNow;
        CurrentHealth = 100;
        EstimatedDaysLeft = CalculateEstimatedDaysLeft(Type, 100);
    }

    public void UpdateHealth(int usageHours)
    {
        var maxLifetimeHours = Type switch
        {
            ItemType.MainBrush => 300,
            ItemType.SideBrush => 200,
            ItemType.Filter => 150,
            ItemType.Battery => 500,
            _ => 100
        };

        CurrentHealth = Math.Max(0, 100 - (usageHours * 100 / maxLifetimeHours));
        EstimatedDaysLeft = CalculateEstimatedDaysLeft(Type, CurrentHealth);
    }

    private int CalculateEstimatedDaysLeft(ItemType type, int health)
    {
        var maxLifetimeHours = type switch
        {
            ItemType.MainBrush => 300,
            ItemType.SideBrush => 200,
            ItemType.Filter => 150,
            ItemType.Battery => 500,
            _ => 100
        };

        // Rough estimate: assuming 1 hour of cleaning per day on average
        return health * maxLifetimeHours / (100 * 24);
    }
}