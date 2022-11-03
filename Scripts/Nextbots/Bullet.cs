using Experiments.Global.Audio;
using Experiments.Global.Managers;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    Rigidbody Body;
    public float BulletSpeed;
    public float BulletRange;
    float T;

    // Start is called before the first frame update
    void Start()
    {
        Body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        T += BulletSpeed * Time.deltaTime;
        if(T > BulletRange)
        {
            Explode();
        }
    }
    private void FixedUpdate() {
        Body.velocity = transform.forward * BulletSpeed;
    }

    private void OnCollisionEnter(Collision other) {
        Explode();
    }

    void Explode()
    {
        FXManager.Instance.SpawnFX("Bullet Hit", transform.position);
        AudioManager.Instance.InteractWithSFX("Bullet Hit", SoundEffectBehaviour.Play);
        Destroy(gameObject);
    }
}
