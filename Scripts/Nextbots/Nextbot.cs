using TMPro;
using Experiments.Global.Managers;
using Experiments.Global.Audio;
using EasyPlayerController;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Nextbot : MonoBehaviour
{
    public bool Dummy;

    [Header("Looks")]
    public Texture2D Image;
    public Material RefMat;
    public MeshRenderer mesh;
    public int ImageDimentions;

    [Header("Movement")]
    public float AccelerationSpeed;
    public float MaxAcceleration;
    Transform Player;
    Material Mat;
    bool Landed;
    Vector3 Direction;
    float Acceleration;
    Rigidbody Body;

    [Header("Health")]
    public TMP_Text HealthText;
    public float HealthTextMargin;
    public int BulletDamage;
    public int MinHealth;
    public int MaxHealth;
    int Health;

    // Start is called before the first frame update
    void Start()
    {
        Body = GetComponent<Rigidbody>();
        Player = FindObjectOfType<CCPlayerController>().transform;

        Health = Random.Range(MinHealth, MaxHealth + 1);

        AssignImage();
    }

    public void AssignImage()
    {
        if(Mat == null)
        {
            Mat = new Material(RefMat);
            Mat.name = Image.name + "_Nextbot_" + GetHashCode();
            mesh.sharedMaterial = Mat;
        }

        Mat.mainTexture = Image;
        float TargWidth = (Image.width > Image.height) ? Image.width : Image.height;
        float Scale = TargWidth / ImageDimentions;
        mesh.transform.localScale = new Vector3(Image.width / Scale, Image.height / Scale, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        HealthText.gameObject.SetActive(!Dummy);
        HealthText.transform.position = transform.position + (Vector3.up * ((ImageDimentions / 2f) + HealthTextMargin));
        HealthText.text = "<sprite=0>" + Health.ToString("000");

        Body.isKinematic = Dummy;

        if(Landed && !Dummy)
        {
            if(Acceleration < MaxAcceleration) { Acceleration += AccelerationSpeed * Time.deltaTime; }

            Vector3 Gap = Player.position - transform.position;
            Direction = Vector3.Scale(Gap.normalized, new Vector3(1f, 0f, 1f));
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(Dummy) { return; }

        if(other.collider.tag == "Floor")
        {
            Landed = true;
            Acceleration = 0f;
        }
        else if(other.collider.tag == "Bullets")
        {
            if(!NextbotGameManager.Instance.IsInGame()) { return; }

            Health -= BulletDamage;
            FXManager.Instance.SpawnText(transform.position, "-" + BulletDamage.ToString(), Color.yellow, 5f, true, ImageDimentions * 1.5f, 0.5f);
            if(Health <= 0)
            {
                FXManager.Instance.SpawnFX("Bot Die", transform.position);
                AudioManager.Instance.InteractWithSFX("Enemy Die", SoundEffectBehaviour.Play);
                FXManager.Instance.SpawnText(transform.position, Image.name, Color.white, 5f, false, ImageDimentions * 2f, 2f);
                NextbotGameManager.Instance.DiscoverImage(Image.name);
                NextbotGameManager.Instance.Kills++;
                Destroy(gameObject);
            }
            else
            {
                AudioManager.Instance.InteractWithSFX("Enemy Hit", SoundEffectBehaviour.Play);
                FXManager.Instance.SpawnFX("Bot Hit", transform.position);
            }
        }
        else if(other.collider.tag == "Player")
        {
            if(NextbotGameManager.Instance.IsInGame())
            {
                Vector3 Dir = other.transform.position - transform.position;
                other.collider.GetComponent<CCPlayerController>().Ragdoll(Vector3.Scale(Dir.normalized, new Vector3(1f, 0f, 1f)));
                AudioManager.Instance.InteractWithSFX("Player Die", SoundEffectBehaviour.Play);
                AudioManager.Instance.MuteMusic();
                NextbotGameManager.Instance.KilledBy = Image.name;
                NextbotGameManager.Instance.GameState = NextbotGameManager.GameStates.GameOver;
            }
        }
    }

    private void FixedUpdate() {
        if(Landed && !Dummy) { Body.velocity = (Direction * Acceleration) + (Vector3.up * Body.velocity.y); }
    }
}
