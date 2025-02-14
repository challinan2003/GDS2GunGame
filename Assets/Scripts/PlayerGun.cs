using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class GunScript : MonoBehaviour
{
    [Header("Gun Settings")]
    public bool isAutomatic = false; // Toggle for semi/auto fire mode
    public float fireRate = 0.2f; // Time between shots

    [Header("Bullet Settings")]
    public GameObject bulletPrefab; // Bullet prefab to spawn
    public Transform firePoint; // Where the bullet spawns
    public float bulletSpeed = 20f; // Speed of the bullet
    public int bulletsPerShot = 1; // Number of bullets per shot
    public float bulletSpread = 0f; // Angle variation for guns with spread

    [Header("Ammo Settings")]
    public int maxAmmo = 10; // Max bullets in a magazine
    private int currentAmmo; //current ammo
    public float reloadTime = 2f; // Time to reload
    private bool isReloading = false; // Reload state, false normally, true when reloading
    private float nextFireTime = 0f; // Tracks when we can fire next

    [Header("UI Elements")]
    public TextMeshProUGUI ammoText; // Reference to the TextMeshPro object on our canvas, assigned in the inspector

    [Header("Customization")]
    public Color gunColor = Color.gray; // Default color, but you can change this in the inspector

    void Start()
    {
        currentAmmo = maxAmmo; // Fill magazine at start

        GetComponent<Renderer>().material.color = gunColor; //assign the chosen color
    }

    void Update()
    {
        if (isReloading) //if we're reloading, we shouldn't do any of the other stuff
        {
            return; //return basically ends this code change
        }

        if (currentAmmo <= 0) //if we have zero (or somehow less than zero) ammo, automatically begin reloading. If you don't want this get rid of it
        {
            StartCoroutine(Reload());
            return; //again, if we're reloading, we shouldn't do anything else
        }

        if (isAutomatic)
        {
            if (Input.GetButton("Fire1") && Time.time >= nextFireTime) //slight difference between GetButton and GetButtonDown helps us read holding down the fire button or clicking it once
            {
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
            {
                Shoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.R)) //manual reload if necessary
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        if (currentAmmo > 0 && bulletPrefab != null && firePoint != null) //probably unnecesary to check if you have the bulletprefab and firepoint assigned, but better safe than sorry
        {
            for (int i = 0; i < bulletsPerShot; i++) //this is for if you're shooting multiple bullets at once, this "for loop" will run our fire code as many times as we have bullets we want to shoot
            {
                // Raycast shoots right through the center of our camera to see where the bullet should fire.
                Ray cameraRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                RaycastHit hit;

                Vector3 targetPoint;
                if (Physics.Raycast(cameraRay, out hit)) 
                {
                    targetPoint = hit.point; // If we hit something, aim there
                }
                else
                {
                    targetPoint = cameraRay.GetPoint(1000); // Otherwise, aim far into the distance
                }

                // Calculate bullet direction
                Vector3 shootDirection = (targetPoint - firePoint.position).normalized; //normalize keeps the direction but sets the magnitude to 1

                // Add spread (horizontal and vertical)
                float spreadX = Random.Range(-bulletSpread, bulletSpread);
                float spreadY = Random.Range(-bulletSpread, bulletSpread);
                Quaternion spreadRotation = Quaternion.Euler(spreadY, spreadX, 0);

                // Apply spread to direction
                shootDirection = spreadRotation * shootDirection;

                // Spawn bullet
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = shootDirection * bulletSpeed; //assign direction and speed to the bullet's rigidbody
                }
            }

            currentAmmo--; // Consumes one ammo per shot. FYI, the command "--" is the same as "-= 1", coders write "-= 1" and "+= 1" so much they wanted a shortcut
            nextFireTime = Time.time + fireRate; //we set when we'll next be able to shoot
            UpdateAmmoUI(); //we update our ui because we've spent one bullet
        }
        else
        {
            Debug.LogWarning("BulletPrefab or FirePoint not assigned!"); //this will play if you somehow are missing one of these references in the inspector
        }
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading..."); //more of a dev tool, get rid of it if you don't want it
        UpdateAmmoUI(); //this is so our ui can show that we're reloading

        yield return new WaitForSeconds(reloadTime); //wait as long as we've decided it takes to reload

        currentAmmo = maxAmmo; //refill our ammo
        isReloading = false;
        UpdateAmmoUI(); //Update our ui again
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null) //assumes we have a reference to some text in the canvas
        {
            if (!isReloading)
            {
                ammoText.text = currentAmmo + " / " + maxAmmo; //if we're not reloading "show currentammo / maxammo"
            }
            else
            {
                ammoText.text = "Reloading... / " + maxAmmo; //if reloading show that we're reloading. TOtally customize this how you like
            }
        }
    }
}