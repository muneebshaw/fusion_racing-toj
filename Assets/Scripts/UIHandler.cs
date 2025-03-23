using UnityEngine;
using UnityEngine.UI;

namespace Racing
{
    public class UIHandler : MonoBehaviour
    {
        [SerializeField] private Text speedText;
        private PrometeoCarController carController;

        private void Update()
        {
            if (!carController) return;

            float absoluteCarSpeed = Mathf.Abs(carController.carSpeed);
            speedText.text = Mathf.RoundToInt(absoluteCarSpeed).ToString();
        }

        private void OnEnable()
        {
            BasicSpawner.LocalPlayerSpawned += OnLocalPlayerSpawned;
        }

        private void OnDisable()
        {
            BasicSpawner.LocalPlayerSpawned -= OnLocalPlayerSpawned;
        }

        private void OnLocalPlayerSpawned(PrometeoCarController _carController)
        {
            carController = _carController;
        }
    }
}