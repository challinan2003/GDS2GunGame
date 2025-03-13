using System.Collections.Generic;
using UnityEngine;

public class SkullWeaponSwapper : MonoBehaviour
{
    public List<GameObject> currentWeapons = new List<GameObject>();
    public int currentWeaponIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < currentWeapons.Count; i++)
        {
            MeshRenderer meshRenderer = currentWeapons[i].GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
            currentWeapons[i].SetActive(false);
        }
    }

    public void RandomizeWeaponType() {
        //here is where the collision code will call to switch to a different weapon ability, and possibly call
    }

    // Update is called once per frame
    void Update()
    {
        //keep the index between the index range of the weapons list
        if(currentWeaponIndex < 0) { currentWeaponIndex = currentWeapons.Count - 1; }
        if (currentWeaponIndex > currentWeapons.Count - 1) { currentWeaponIndex = 0; }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            currentWeaponIndex += 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            currentWeaponIndex -= 1;
        }

        for (int i = 0; i < currentWeapons.Count; i++)
        {
            if (i == currentWeaponIndex)
            {
                currentWeapons[i].SetActive(true);
            }
            else {
                currentWeapons[i].SetActive(false);
            }
        }
    }
}
