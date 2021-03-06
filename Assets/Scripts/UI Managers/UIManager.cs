using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Sprite[] _statusBarSprites;
    [SerializeField] private Image _statusBar;
    [SerializeField] private Courier _courier;
    [SerializeField] private Text moneyText;

    public void UpdateMoney(int newMoneyValue)
    {
        moneyText.text = newMoneyValue.ToString() + " r.";
    }

    public void UpdateStatusBar()
    {
        var damage = _courier.ParcelPreservationStatus;
        if (damage == 0)
            _statusBar.sprite = _statusBarSprites[0];
        else if (damage <= .2f)
            _statusBar.sprite = _statusBarSprites[1];
        else if (damage <= .4f)
            _statusBar.sprite = _statusBarSprites[2];
        else if (damage <= .6f)
            _statusBar.sprite = _statusBarSprites[3];
        else if (damage <= .8f)
            _statusBar.sprite = _statusBarSprites[4];
        else if (damage <= 1)
            _statusBar.sprite = _statusBarSprites[5];
    }
}
