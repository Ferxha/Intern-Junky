using UnityEngine;

public class SubstanceObject : MonoBehaviour
{
    [SerializeField] private SubstanceType substanceType;
    [SerializeField] private SubstanceManager substanceManager;
    
    void Awake()
    {
        if (substanceManager == null)
            substanceManager = FindFirstObjectByType<SubstanceManager>();
    }

    void Update()
    {
        RefreshVisibility();
    }

    public void RefreshVisibility()
    {
        if (substanceManager == null)
        {
            substanceManager = SubstanceManager.Instance != null
                ? SubstanceManager.Instance
                : FindFirstObjectByType<SubstanceManager>();
        }

        if (substanceManager == null)
        {
            return;
        }

        int inventory = substanceType switch
        {
            SubstanceType.Coca => substanceManager.GetCocaAmount(),
            SubstanceType.Extasis => substanceManager.GetExtasisAmount(),
            _ => 0
        };

        gameObject.SetActive(inventory > 0);
    }

    public bool TryConsume()
    {
        if (substanceManager == null)
        {
            substanceManager = SubstanceManager.Instance != null
                ? SubstanceManager.Instance
                : FindFirstObjectByType<SubstanceManager>();
        }

        if (substanceManager == null)
        {
            Debug.LogWarning($"{name}: SubstanceManager no encontrado.");
            return false;
        }

        return substanceType switch
        {
            SubstanceType.Coca => substanceManager.ConsumeCoca(),
            SubstanceType.Extasis => substanceManager.ConsumeExtasis(),
            _ => false
        };
    }
}
