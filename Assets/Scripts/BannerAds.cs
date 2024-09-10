using GoogleMobileAds.Api;
using UnityEngine;

public class BannerAd : MonoBehaviour
{
    private BannerView bannerView;
    // Replace with your Ad Unit ID
    // ca-app-pub-6448301991598591/9558139550"
    // test ca-app-pub-3940256099942544/6300978111
    string adUnitId = "ca-app-pub-6448301991598591/9558139550";

    public void CreateBannerView()
    {
        // If we already have a banner, destroy the old one.
        if (bannerView != null)
        {
            DestroyAd();
        }

        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.BottomLeft);
    }

    public void DestroyAd()
    {
        if (bannerView != null)
        {
            Debug.Log("Destroying banner view.");
            bannerView.Destroy();
            bannerView = null;
        }
    }

    public void LoadAd()
    {
        // create an instance of a banner view first.
        if (bannerView == null)
        {
            CreateBannerView();
        }

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        Debug.Log("Loading banner ad.");
        bannerView.LoadAd(adRequest);
    }
}