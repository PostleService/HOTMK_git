using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStageMonitor_SimplifiedTutorialTeleporter : MonoBehaviour
{
    private LevelManagerScript _levelManagerScript;
    [Tooltip("At which level stage will this object spawn and teleport a tutorial message trigger on top of player")]
    public int ReactToLevelStage = 0;
    public GameObject TutorialMessageTrigger;

    private float _delayTillTeleport = 0.65f;

    // Start is called before the first frame update
    void Start()
    { _levelManagerScript = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>(); }

    // Update is called once per frame
    void FixedUpdate()
    { TeleportMessageTrigger(); }

    public void TeleportMessageTrigger()
    {
        if (_levelManagerScript.LevelStage == ReactToLevelStage)
        {
            _delayTillTeleport -= Time.fixedDeltaTime;

            if (_delayTillTeleport <= 0)
            {
                GameObject Player = GameObject.Find("Player");
                if (TutorialMessageTrigger != null && Player != null)
                { TutorialMessageTrigger.transform.position = Player.transform.position; }
                Destroy(this.gameObject);
            }
        }
    }
}
