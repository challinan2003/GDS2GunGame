using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerGun2 : MonoBehaviour
{
    //these are used in the child classes
    protected bool isShooting;
    protected bool readyToShoot;

    protected enum GunState
    {
        RELOADING,
        EMPTY,
        FULL,
    }

    protected GunState currentGunState;

    protected PlayerMovement player;

    //this is specific to this class only.
    private bool allowReset = true;

    [Header("GUN STATS")]
    public float shootingDelay = 0.5f;
    public int GunAmmo;
    public int MaxGunAmmo;
    public int bulletPerBurst = 3;
    public int currentBulletsInBurst;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30f;
    public float bulletPrefabBulletTime = 3f;
    public float spreadIntensity;

    [Header("GUN UI")]
    public GameObject gunText;

    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }

    public ShootingMode currentShootingMode;

    [Header("REFERENCES")]
    public GameObject Player;
    //public GameObject muzzleFlash;

    private void Awake()
    {
        readyToShoot = true;

        //set bullets for burst, and gunAmmo
        currentBulletsInBurst = bulletPerBurst;

        currentGunState = GunState.FULL;

        GunAmmo = MaxGunAmmo;

        //get access to player state through the playerMovement script
        player = Player.GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        gunText = GameObject.Find("Ammo Text");
    }

    public IEnumerator Reload()
    {
        currentGunState = GunState.RELOADING;

        WaitForSeconds wait = new WaitForSeconds(2f);

        yield return wait;

        //set bullets for burst, and gunAmmo
        GunAmmo = MaxGunAmmo;
        currentGunState = GunState.FULL;
    }

    private void InputHandler()
    {
        if (currentShootingMode == ShootingMode.Auto)
        {
            isShooting = Input.GetKey(KeyCode.Mouse0);
        }
        else if (currentShootingMode == ShootingMode.Single)
        {
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        bool isReloadButtonPressed = Input.GetKeyDown(KeyCode.Tab);
        bool canReload = (currentGunState != GunState.RELOADING);
        bool isAmmoEmpty = GunAmmo <= 0;
        bool canReloadAmmo = (GunAmmo > 0 && GunAmmo < MaxGunAmmo);

        //1. is reload button pressed
        //2. is the player moving or idle
        //3. is the gunAmmo empty
        //4. is the gun state not already reloading
        if (isReloadButtonPressed && canReload && (isAmmoEmpty || canReloadAmmo))
        {
            StartCoroutine(Reload());
        }

    }

    // Update is called once per frame
    void Update()
    {
        InputHandler();
        ShowPlayerUI();

        //if the player is ready to shoot, and is pressing the shoot button, and is not sprinting
        if (readyToShoot && isShooting)
        {
            FireWeapon();
        }
    }

    // The Fire method will be used by most weapons and can be shared, but the virtual keyword allows it to be overriden in a child class.
    public virtual void FireWeapon()
    {
        //if ammo is empty return early
        if (GunAmmo <= 0)
        {
            currentGunState = GunState.EMPTY;
            return;
        }

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

        //point the bullet to face the shooting direction
        bullet.transform.forward = shootingDirection;

        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);

        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabBulletTime));

        //soft locks the FireWeapon Method with a shootingDelay
        if (allowReset)
        {
            Invoke(nameof(ResetShot), shootingDelay);//wait x seconds to shoot again.
            allowReset = false;
        }

        //if its a burst weapon shoot all three bullets then subtract three from the gunAmmo
        if (currentShootingMode == ShootingMode.Burst)
        {

            if (currentBulletsInBurst >= 0)
            {
                currentBulletsInBurst--;
                Invoke("FireWeapon", shootingDelay);
            }
            //if the bullets mode is burst and currentBullets in burst run out then restock the currentBulletsInBurst and subtract the GunAmmo by the bulletPerBurst
            else
            {
                GunAmmo -= bulletPerBurst;
                currentBulletsInBurst = bulletPerBurst;
            }
        }
        else
        {
            GunAmmo--;
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    private Vector3 CalculateDirectionAndSpread()
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

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float bulletPrefabBulletTime)
    {
        yield return new WaitForSeconds(bulletPrefabBulletTime);

        Destroy(bullet);
    }

    private void ShowPlayerUI()
    {
        TextMeshProUGUI textMeshPro = gunText.GetComponent<TextMeshProUGUI>();

        if (currentGunState == GunState.RELOADING)
        {
            textMeshPro.text = "Reloading... / " + MaxGunAmmo;
        }
        else {
            textMeshPro.text = GunAmmo + " / " + MaxGunAmmo;
        }
    }
}