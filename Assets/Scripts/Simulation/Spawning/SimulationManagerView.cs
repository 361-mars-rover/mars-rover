using TMPro;
using UnityEngine;
class SimulationManagerView : MonoBehaviour{
    [SerializeField] private TextMeshProUGUI topRightText;
    void Awake()
    {
        topRightText.text = "TESTING TEXT!";
    }
}