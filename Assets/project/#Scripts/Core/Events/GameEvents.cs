// Scripts/Core/Events/GameEvents.cs
using UnityEngine;
using System.Collections.Generic;

// ── Enums ──────────────────────────────────────────────
public enum DamageType
{
    Physical,
    Magic,
    Fire,
    Poison,
    True         // ignores resistances
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

// ── Player Events ──────────────────────────────────────
public struct PlayerDiedEvent
{
    public string PlayerId;
    public string KillerId;
    public Vector3 Position;
}

public struct PlayerExtractedEvent
{
    public string PlayerId;
    public List<string> ItemIds;
    public int GoldExtracted;
}

public struct PlayerDamagedEvent
{
    public string PlayerId;
    public float Damage;
    public DamageType DamageType;
}

// ── Dungeon Events ─────────────────────────────────────
public struct ExtractionZoneOpenedEvent
{
    public string ZoneId;
    public float TimeRemaining;
}

public struct DungeonBossSpawnedEvent
{
    public string BossType;
    public Vector3 Position;
}

// ── Economy Events ─────────────────────────────────────
public struct ItemLootedEvent
{
    public string PlayerId;
    public string ItemId;
    public ItemRarity Rarity;
}

public struct StashUpdatedEvent
{
    public string PlayerId;
}