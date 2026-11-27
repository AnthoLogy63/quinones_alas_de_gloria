using UnityEngine;

/// <summary>Shows one distant fire per terrain round, alternating sides.</summary>
public sealed class FireRoundAlternatorUI : MonoBehaviour
{
    [SerializeField] private float roundDurationSeconds = 65f;

    private GameObject leftFire;
    private GameObject leftSmoke;
    private GameObject rightFire;
    private GameObject rightSmoke;
    private float roundStartTime;
    private int displayedRound;

    private void Awake()
    {
        leftFire = FindChild("Fondo_3/Fuego_distante");
        leftSmoke = FindChild("Fondo_3/Humo_negro");
        rightFire = FindChild("Fondo_1/Fuego_distante_derecha");
        rightSmoke = FindChild("Fondo_1/Humo_negro_derecha");

        roundStartTime = Time.unscaledTime;
        displayedRound = 0;
        ShowRightSide(false);
    }

    private void Update()
    {
        float duration = Mathf.Max(0.01f, roundDurationSeconds);
        int currentRound = Mathf.FloorToInt((Time.unscaledTime - roundStartTime) / duration);
        if (currentRound == displayedRound) return;

        displayedRound = currentRound;
        ShowRightSide((currentRound & 1) == 1);
    }

    private GameObject FindChild(string relativePath)
    {
        Transform child = transform.Find(relativePath);
        return child != null ? child.gameObject : null;
    }

    private void ShowRightSide(bool showRight)
    {
        if (leftFire != null) leftFire.SetActive(!showRight);
        if (leftSmoke != null) leftSmoke.SetActive(!showRight);
        if (rightFire != null) rightFire.SetActive(showRight);
        if (rightSmoke != null) rightSmoke.SetActive(showRight);
    }
}
