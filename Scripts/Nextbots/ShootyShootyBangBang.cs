using TMPro;
using Experiments.Global.Managers;
using Experiments.Global.Audio;
using UnityEngine.UI;
using UnityEngine;

public class ShootyShootyBangBang : MonoBehaviour
{
    [Header("General")]
    public Transform GunTip;
    public GameObject BulletPrefab;
    public Animator GunAnim;
    public Animator GunContainerAnim;

    [Header("Bullets")]
    public float BulletSpeed;
    public float BulletRange;
    public float AttackSpeed;
    float AttackTimer;

    [Header("Ammo")]
    public int MaxAmmo;
    public float ReloadTime;
    public TMP_Text AmmoText;
    public Slider ReloadSlider;
    int Ammo;
    float ReloadTimer;
    bool Loaded;

    // Start is called before the first frame update
    void Start()
    {
        AttackTimer = AttackSpeed;
        ReloadTimer = ReloadTime;
        Ammo = MaxAmmo;

        Loaded = true;
        ReloadSlider.minValue = 0f;
        ReloadSlider.maxValue = ReloadTime;
    }

    // Update is called once per frame
    void Update()
    {
        AttackTimer -= Time.deltaTime;
        if(Input.GetMouseButtonDown(0) && AttackTimer <= 0f && NextbotGameManager.Instance.IsInGame())
        {
            if(!Loaded)
            {
                AudioManager.Instance.InteractWithSFX("No Ammo", SoundEffectBehaviour.Play);
                return;
            }

            AttackTimer = AttackSpeed;

            GunAnim.SetTrigger("Shoot");
            FXManager.Instance.SpawnFX("Bullet Hit", GunTip.position);
            AudioManager.Instance.InteractWithSFX("Shoot", SoundEffectBehaviour.Play);

            Shoot();

            Ammo--;
            if(Ammo == 0)
            {
                Reload();
            }
        }

        GunContainerAnim.SetBool("Loaded", Loaded);

        ReloadSlider.value = ReloadTimer;
        AmmoText.text = "Ammo " + Ammo.ToString("00") + " | " + MaxAmmo.ToString("00");
        AmmoText.color = (Loaded) ? Color.white : Color.red;
        if(!Loaded)
        {
            ReloadTimer += Time.deltaTime;
            if(ReloadTimer >= ReloadTime && NextbotGameManager.Instance.IsInGame())
            {
                Ammo = MaxAmmo;
                Loaded = true;
                AudioManager.Instance.InteractWithSFX("Load Gun", SoundEffectBehaviour.Play);
            }
        }
        if(Input.GetKeyDown("r") && Loaded) { Reload(); }
    }
    void Shoot()
    {
        RaycastHit hit;
        bool CamRay = Physics.Raycast(transform.position, transform.forward, out hit, BulletRange);
        Vector3 LookPos = Vector3.zero;
        if(CamRay) { LookPos = hit.point; }
        else { LookPos =  transform.position + (transform.forward * BulletRange); }

        GameObject NewBullet = Instantiate(BulletPrefab, GunTip.position, GunTip.rotation);
        NewBullet.transform.LookAt(LookPos);
        Bullet bullet = NewBullet.GetComponent<Bullet>();
        bullet.BulletSpeed = BulletSpeed;
        bullet.BulletRange = BulletRange;
    }
    void Reload()
    {
        if(Ammo >= MaxAmmo || !NextbotGameManager.Instance.IsInGame()) { return; }

        Loaded = false;
        ReloadTimer = 0f;
        AudioManager.Instance.InteractWithSFX("Reload", SoundEffectBehaviour.Play);
    }

    public void ResetAmmo()
    {
        Ammo = MaxAmmo;
        ReloadTimer = ReloadTime;
        Loaded = true;
    }
}
