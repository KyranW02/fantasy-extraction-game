// Scripts/Server/Validation/InventoryValidator.cs
using System.Collections.Generic;
using UnityEngine;

public static class InventoryValidator
{
    // Called when player connects to instance
    // Validates their gear against database records
    public static bool ValidateEntryInventory(
        string playerId,
        List<string> claimedItems)
    {
        // Fetch authoritative inventory from database
        var dbInventory = DatabaseProxy.GetPlayerInventory(playerId);

        if (dbInventory == null)
        {
            LogSuspicious(playerId, "Entry validation failed — no DB record");
            return false;
        }

        // Every item player claims to have must exist in DB
        foreach (var itemId in claimedItems)
        {
            if (!dbInventory.Contains(itemId))
            {
                LogSuspicious(playerId,
                    $"Item injection attempt — item {itemId} not in DB");
                return false;
            }
        }

        return true;
    }

    // Called when player reaches extraction zone
    // Validates what they're trying to extract
    public static bool ValidateExtraction(
        string playerId,
        List<string> itemsToExtract,
        List<string> itemsPickedUpInRaid)
    {
        // Items being extracted must only be:
        // 1. Items they entered with (validated on entry)
        // 2. Items picked up during THIS raid (tracked server-side)
        foreach (var itemId in itemsToExtract)
        {
            bool enteredWith = DatabaseProxy
                .GetPlayerInventory(playerId)
                .Contains(itemId);

            bool pickedUpInRaid = itemsPickedUpInRaid.Contains(itemId);

            if (!enteredWith && !pickedUpInRaid)
            {
                LogSuspicious(playerId,
                    $"Extraction dupe attempt — item {itemId} origin unknown");
                return false;
            }
        }

        return true;
    }

    private static void LogSuspicious(string playerId, string reason)
    {
        Debug.LogWarning($"[SECURITY] Player {playerId} — {reason}");
        // TODO: Send to backend logging system
        // TODO: Increment violation counter
        // TODO: Auto-ban after threshold
    }
}