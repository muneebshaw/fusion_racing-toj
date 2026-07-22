using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardEntry : MonoBehaviour
{
    //[SerializeField] private TextMeshProUGUI _positionText;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private Image _background;

    public void Setup(/*int position, */string playerName, /*string progress,*/ bool isLocal = default)
    {
        //_positionText.text = position.ToString();
        _nameText.text = playerName;
        //_progressText.text = progress;
        if (isLocal)
        {
            _background.color = Color.red;
        }
        else
        {
            _background.color = Color.white;
        }
    }

    //internal void UpdatePosition(int newPosition)
    //{
    //    _positionText.text = newPosition.ToString();
    //    transform.SetSiblingIndex(newPosition - 1);
    //}

    internal void UpdateProgress(string progress)
    {
        _progressText.text = progress;
    }
}