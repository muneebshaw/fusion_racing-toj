using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _positionText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private Image _background;

    public void Setup(int position, string playerName, string time, bool isLocal = default)
    {
        _positionText.text = position.ToString();
        _nameText.text = playerName;
        _timeText.text = time;
        if (isLocal)
            _background.color = Color.red;
    }

    //internal void UpdatePosition(int newPosition)
    //{
    //    _positionText.text = newPosition.ToString();
    //    transform.SetSiblingIndex(newPosition - 1);
    //}
}