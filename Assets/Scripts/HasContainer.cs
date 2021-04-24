using TMPro;
using UnityEngine;

public abstract class HasContainer : MonoBehaviour
{
    public TMP_Text countText;
    
    public abstract Fish? GetFish();

    protected void UpdateCountText(string text)
    {
        if (countText)
        {
            countText.text = text;
        }
    }
}