using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public class PlayerHealth : MonoBehaviour
{
    public GameObject deathVFXPrefab;
    int trapsLayer;
    public AudioClip die;
    private AudioSource aud;

    private void Start()
    {
        aud = GetComponent<AudioSource>();
        trapsLayer = LayerMask.NameToLayer("Trap");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == trapsLayer)
        {
            Instantiate(deathVFXPrefab, transform.position, transform.rotation);

            Destroy(deathVFXPrefab);
            aud.PlayOneShot(die, Random.Range(0.3f, 0.5f));
        }
    }
}*/
