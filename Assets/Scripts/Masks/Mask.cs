using UnityEngine;

public abstract class Mask : MonoBehaviour
{
    [SerializeField] protected MaskType maskType;
    [SerializeField] protected float cooldown = 2f;
    [SerializeField] private GameObject[] maskModels;
    protected float cooldownTimer;

    void Awake()
    {
        Instantiate(maskModels[Random.Range(0,3)], this.transform);
    }

    public MaskType GetMaskType() => maskType;

    public bool CanUse() => cooldownTimer <= 0f;

    public void Tick(float deltaTime)
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= deltaTime;
    }

    public void TryUse(GameObject player)
    {
        if (!CanUse()) return;
        Use(player);
        cooldownTimer = cooldown;
    }

    protected abstract void Use(GameObject player);
}
