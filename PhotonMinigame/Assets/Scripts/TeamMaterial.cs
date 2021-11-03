using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamMaterial : MonoBehaviour
{
    public MeshRenderer renderer;
    public int teamID = 0;
    int oldID = 0;
    // Start is called before the first frame update
    void Start()
    {
        SetColor();
    }

    // Update is called once per frame
    void Update()
    {
        if (teamID != oldID)
        {
            //team changed
            SetColor();
        }
    }

    void SetColor()
    {
        //renderer.materials[0].SetColor("TeamColor", TeamManager.instance.teams[teamID]);
    }
}
