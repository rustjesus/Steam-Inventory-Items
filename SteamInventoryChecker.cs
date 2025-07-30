using CodeStage.AntiCheat.Storage;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;


public class SteamInventoryChecker : MonoBehaviour
{
    private Callback<SteamInventoryResultReady_t> inventoryCallback;
    private SteamInventoryResult_t inventoryResult;

    // Replace with your actual Item Definition IDs
    //current build setting, 6 items max
    //for update
    private int[] targetItemDefIds = { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,
        25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,
        51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,
        81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,
        100, 200, 300, 400, 500, 600, 700, 800, 900, 1000,
        1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000, 2100, 2200, 2300, 2400,
        2500, 2600, 2700, 2800, 2900, 3000};
    //100 blue hat, 200 red hat, 1001 gold coin
    private HashSet<int> foundItemDefs = new HashSet<int>();
    public static bool oncePerSession = false;

    private void Awake()
    {
        //reset all item unlocks to ensure they are always 0 on init
        for (int i = 0; i < targetItemDefIds.Length; i++)
        {
            ObscuredPrefs.SetInt("Item" + targetItemDefIds[i], 0);
        }


        if (oncePerSession == false)
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam not initialized.");
                return;
            }

            oncePerSession = true;
        }
        inventoryCallback = Callback<SteamInventoryResultReady_t>.Create(OnSteamInventoryResultReady);

        if (!SteamInventory.GetAllItems(out inventoryResult))
        {
            Debug.LogError("Failed to request inventory.");
        }
    }
    private void Start()
    {
    }

    private void OnSteamInventoryResultReady(SteamInventoryResultReady_t callback)
    {
        if (callback.m_handle != inventoryResult || callback.m_result != EResult.k_EResultOK)
        {
            Debug.LogWarning("Inventory result failed or mismatched.");
            return;
        }

        uint itemCount = 0;

        if (!SteamInventory.GetResultItems(inventoryResult, null, ref itemCount))
        {
            Debug.LogWarning("Failed to get item count.");
            return;
        }

        SteamItemDetails_t[] items = new SteamItemDetails_t[itemCount];
        Dictionary<int, int> itemQuantities = new Dictionary<int, int>();

        if (SteamInventory.GetResultItems(inventoryResult, items, ref itemCount))
        {
            foreach (var item in items)
            {
                int defId = (int)item.m_iDefinition;

                if (System.Array.Exists(targetItemDefIds, id => id == defId))
                {
                    if (!itemQuantities.ContainsKey(defId))
                        itemQuantities[defId] = 0;

                    itemQuantities[defId] += item.m_unQuantity;

                    if (!foundItemDefs.Contains(defId))
                    {
                        foundItemDefs.Add(defId);
                        OnItemFound(item);
                    }
                }
            }

            // Show all items in target list, even if not found
            foreach (int id in targetItemDefIds)
            {
                int quantity = itemQuantities.ContainsKey(id) ? itemQuantities[id] : 0;
                Debug.Log($"Player owns item {id} x{quantity}");
            }
        }
        else
        {
            Debug.LogWarning("Failed to get result items.");
        }

        SteamInventory.DestroyResult(inventoryResult);
    }


    private void OnItemFound(SteamItemDetails_t item)
    {
        int quantity = item.m_unQuantity;
        // Handle unlocked content for this item
        Debug.Log($"Unlocked content for item {item.m_iDefinition}, Quantity: {quantity}");

        if (quantity > 0)
        {
            for (int i = 0; i < targetItemDefIds.Length; i++)
            {
                if ((int)item.m_iDefinition == targetItemDefIds[i])//blue hat
                {
                    ObscuredPrefs.SetInt("Item" + targetItemDefIds[i], 1);
                    Debug.Log("Item ID: " + targetItemDefIds[i] + " unlocked");
                }
            }

        }
    }
}
