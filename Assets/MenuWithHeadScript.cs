using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
public class MenuWithHeadScript : LazyFollow
{
    // Start is called before the first frame update
    void Start()
    {
        targetOffset = new Vector3(0.0f, 0.0f, 0.5f);   
        maxDistanceAllowed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
