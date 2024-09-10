using UnityEngine;

public class OrientationManager : MonoBehaviour
{
    void Start()
    {
        // Enforce landscape orientation
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        // Optionally lock the orientation to landscape
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }

    void Update()
    {
        // Check if the orientation is not landscape and force it back
        if (Screen.orientation != ScreenOrientation.LandscapeLeft && Screen.orientation != ScreenOrientation.LandscapeRight)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
    }
}
