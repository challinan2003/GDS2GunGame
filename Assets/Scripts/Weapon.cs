using System.Collections;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    //these are used in the child classes
    private bool isShooting;
    private bool readyToShoot;

    private enum GunState
    {
        RELOADING,
        EMPTY,
        FULL,
    }

    private GunState currentGunState;

    //this is specific to this class only.
    private bool allowReset = true;

    [Header("GUN STATS")]
    public float shootingDelay = 0.5f;
    public float reloadTime;
    protected int currentWeaponAmmo;
    public int MaxWeaponAmmo;
    public int bulletPerBurst = 3;
    private int currentBulletsInBurst;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30f;
    public float bulletPrefabBulletTime = 3f;
    public float spreadIntensity;
    public ShootingMode currentShootingMode;
    public GameObject muzzleFlash;

    private GameObject gunText;

    [Header("Keybinds")]
    public KeyCode reload = KeyCode.R;

    public WeaponSwap wp;

    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }

    private void Awake()
    {
        readyToShoot = true;

        //set bullets for burst, and gunAmmo
        currentBulletsInBurst = bulletPerBurst;

        currentGunState = GunState.FULL;

        currentWeaponAmmo = MaxWeaponAmmo;
    }

    private void Start()
    {
        gunText = GameObject.Find("Ammo Text");
    }

    public IEnumerator Reload()
    {
        Debug.Log("reloading");

        currentGunState = GunState.RELOADING;

        WaitForSeconds wait = new WaitForSeconds(reloadTime);

        yield return wait;

        //set bullets for burst, and gunAmmo
        currentWeaponAmmo = MaxWeaponAmmo;

        currentGunState = GunState.FULL;

        readyToShoot = true;
    }

    private void InputHandler()
    {
        if (currentShootingMode == ShootingMode.Auto)
        {
            isShooting = Input.GetKey(KeyCode.Mouse0);
        }
        else if (currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst)
        {
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        //RELOAD HANDLER
        bool canReload = (currentGunState != GunState.RELOADING);
        bool isAmmoEmpty = currentWeaponAmmo <= 0;
        bool canReloadAmmo = (currentWeaponAmmo > 0 && currentWeaponAmmo < MaxWeaponAmmo);

        //1. is reload button pressed
        //2. is the player moving or idle
        //3. is the gunAmmo empty
        //4. is the gun state not already reloading
        if (Input.GetKeyDown(reload) && canReload && (isAmmoEmpty || canReloadAmmo))
        {
            StartCoroutine(Reload());
        }
    }

    // Update is called once per frame
    void Update()
    {
        InputHandler();

        ShowPlayerUI();

        // 1. can we shoot(boolean)
        // 2. isShooting key pressed down or at all
        if (readyToShoot && isShooting)
        {
            //Debug.Log("fire");
            FireWeapon();
        }
    }

    public void CreateBulletAndAddForce()
    {
        ////////////////////////////////////////////////////////////////////////////////

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

        //point the bullet to face the shooting direction 
        bullet.transform.forward = shootingDirection;

        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);

        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabBulletTime));//destroy bullet after some time
    }

    // The Fire method will be used by most weapons and can be shared, but the virtual keyword allows it to be overriden in a child class.
    public void FireWeapon()
    {
        //THIS IS USED TO MAKE SURE THAT WE CAN NOT SHOOT AGAIN
        readyToShoot = false;

        // Prevent shooting if out of ammo, currently reloading, or don't have enough ammo for a burst
        if (currentWeaponAmmo <= 0 || currentGunState == GunState.RELOADING || (currentShootingMode == ShootingMode.Burst && currentWeaponAmmo < bulletPerBurst))
        {
            //Debug.Log("Can't shoot");
            return;
        }

        if (currentShootingMode == ShootingMode.Single)
        {
            CreateBulletAndAddForce();
            currentWeaponAmmo--;
            Invoke(nameof(ResetShot), shootingDelay);//WAIT XXX Seconds Before shooting again
        }

        //burst
        else if (currentShootingMode == ShootingMode.Burst)
        {
            Debug.Log("shoot burst");

            // Check if we have enough ammo for a full burst
            if (currentWeaponAmmo < bulletPerBurst)
            {
                Debug.Log("Not enough ammo for burst fire");
                return;
            }

            // bulletPerBurst = 3
            // bulletPrefabBulletTime = 0.5
            // delay between each shot is the average time between each one
            // shootingDelay = 0.5 / 3 = 0.1666667 seconds
            //float delayBetweenBullet = (shootingDelay / bulletPerBurst);

            // Start the burst sequence with a delay between each shot
            float delayBetweenBullet = (shootingDelay / bulletPerBurst);

            for (int i = 0; i < bulletPerBurst; i++)
            {
                Invoke(nameof(CreateBulletAndAddForce), delayBetweenBullet * i);
            }

            // Subtract ammo after the burst sequence is complete
            currentWeaponAmmo -= bulletPerBurst;

            // Enforce the burst cooldown before next shot
            Invoke(nameof(ResetShot), shootingDelay);
        }
        else if(currentShootingMode == ShootingMode.Auto) // Auto mode
        {
            float delayBetweenBullet = (shootingDelay / MaxWeaponAmmo);

            CreateBulletAndAddForce();
            currentWeaponAmmo--;

            //Delay Between each bullet shot
            Invoke(nameof(CreateBulletAndAddForce), delayBetweenBullet);

            // set readyToShoot to true
            Invoke(nameof(ResetShot), shootingDelay);
        }

    }

    public void ResetShot()
    {
        //Debug.Log("reset shot");
        readyToShoot = true;
    }

    public Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100);
        }

        Vector3 direction = targetPoint - bulletSpawn.position;

        float x = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        return direction + new Vector3(x, y, 0);
    }

    public IEnumerator DestroyBulletAfterTime(GameObject bullet, float bulletPrefabBulletTime)
    {
        yield return new WaitForSeconds(bulletPrefabBulletTime);

        Destroy(bullet);
    }

    private void ShowPlayerUI()
    {
        GameObject textMeshObject1 = GameObject.Find("GunText");
        TextMeshProUGUI textMeshInstanceOne = textMeshObject1.GetComponent<TextMeshProUGUI>();

        if (currentGunState == GunState.RELOADING)
        {
            textMeshInstanceOne.text = "Reloading... / " + MaxWeaponAmmo;
        }
        else {
            textMeshInstanceOne.text = currentWeaponAmmo + " / " + MaxWeaponAmmo;
        }

        GameObject currentWeapon = wp.currentWeapons[wp.currentWeaponIndex];

        // GameObject textMeshObject2 = GameObject.Find("txtCurrentWeapon");
        // TextMeshProUGUI textMeshInstanceTwo = textMeshObject2.GetComponent<TextMeshProUGUI>();

        // textMeshInstanceTwo.text = "" + currentWeapon.name;
    }
}