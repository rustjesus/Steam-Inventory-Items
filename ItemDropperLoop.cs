using UnityEngine;

public class ItemDropperLoop : MonoBehaviour
{
    private SteamItemDropper steamItemDropper;

    private void Awake()
    {
        steamItemDropper = GetComponent<SteamItemDropper>();
    }

    private void Start()
    {
        StartCoroutine(DropCheckLoop());
    }

    private System.Collections.IEnumerator DropCheckLoop()
    {
        yield return new WaitForSeconds(900f); // 15 minutes
        steamItemDropper.TriggerSteamItemDropCheck();
        Debug.Log("Steam item drop loop checking...");
        StartCoroutine(DropCheckLoop());
    }
}
