using UnityEngine;

public class ClickDebugger : MonoBehaviour
{
    [SerializeField]
    public bool debugMode;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && debugMode)
        {
            Clicked();
        }
    }

    void Clicked()
    {
        var ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast (ray, out hit))
        {
            Debug.Log($"Click position: {hit.point}");
        }
        else{
            return;
        }
    }
}
