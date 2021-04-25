using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour
{
    public TMP_Text nameText, descText, priceText;
    public Button button;
    public Color disabledTextColor;

    public void MakeDisabled()
    {
        button.enabled = false;
        nameText.color = disabledTextColor;
        descText.color = disabledTextColor;
        priceText.color = disabledTextColor;
    }
}
