using UnityEngine;
using UnityEngine.UI;

public class ButtonDebugger : MonoBehaviour
{
    [SerializeField] private Button btn;

    void Start()
    {
        if (btn == null) btn = GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError("ButtonDebugger: nenhum Button atribuído ou encontrado.");
            return;
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        Debug.Log("ButtonDebugger: botão clicado!");
        if (VictoryManager.Instance == null)
        {
            Debug.LogError("ButtonDebugger: VictoryManager.Instance == null");
            return;
        }
        Debug.Log("ButtonDebugger: chamando VictoryManager.Instance.ReturnToMainMenu()");
        VictoryManager.Instance.ReturnToMainMenu();
    }
}
