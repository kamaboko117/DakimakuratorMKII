using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;

public class Dakimakura : UdonSharpBehaviour
{
    public ParticleSystem firePS;
    public AudioSource fireAudio;
    public AudioSource fireScream;

    public VRCObjectPool pool;

    private bool isIgnited = false;

    // ignite when colliding with fire layer trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!isIgnited && other.gameObject.layer == 23) // Fire layer in VRChat
        {
            Ignite();
        }
    }

    public void Ignite()
    {
        if (isIgnited)
            return;
        isIgnited = true;

        firePS.Play();
        fireAudio.Play();
        fireScream.Play();
        SendCustomEventDelayedSeconds(nameof(Despawn), 5f);
    }

    public void Despawn()
    {
        isIgnited = false;
        pool.Return(gameObject);
    }
}
