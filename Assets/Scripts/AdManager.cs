using GoogleMobileAds.Api;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    private void Awake()
    {
        // Make sure this GameObject persists across scene loads.
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { });
    }
}