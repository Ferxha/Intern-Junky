using UnityEngine;

public class SubstanceObject : MonoBehaviour
{
    public SubstanceType substanceType;
    public SubstanceManager substanceManager;
    
    void Awake()
    {
        if (substanceManager == null)
            substanceManager = FindFirstObjectByType<SubstanceManager>();
    }

    void Update()
    {
        if (substanceManager == null) return;
        
        int inventory = substanceType switch
        {
            SubstanceType.Coca => substanceManager.GetCocaAmount(),
            SubstanceType.Extasis => substanceManager.GetExtasisAmount(),
            _ => 0
        };
        
        if (inventory <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
