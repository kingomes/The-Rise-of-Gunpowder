using System.Collections;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private ProgressBar healthBar;

    Player player;
    private string playerTag = "Player";

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(playerTag).GetComponent<Player>();
    }

    public void SetData(Player player) {
        this.player = player;
        healthBar.SetProgress((float) player.GetHealth());
    }

    public IEnumerator UpdateHealth() {
        yield return healthBar.SetProgressSmooth((float) player.GetHealth());
    }
}
