using UnityEngine;

public sealed class UnitSound : MonoBehaviour, IUnitSound
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _footstepSource;
    [SerializeField] private AudioSource _interactSource;

    [Header("Sounds")]
    [SerializeField] private AudioClip _footstepSound;
    [SerializeField] private AudioClip _crunchSound;

    private void Awake()
    {
        SetupSource(_footstepSource);
    }

    private void SetupSource(AudioSource source)
    {
        if (source == null) return;
        source.playOnAwake = false;
        source.spatialBlend = 1f;
    }

    public void PlayFootstep()
    {
        if (_footstepSound == null) return;

        AudioClip clip = _footstepSound;
        float pitch = Random.Range(0.9f, 1.1f);

        _footstepSource.pitch = pitch;
        _footstepSource.PlayOneShot(clip, 0.3f);
    }

    public void PlayInteractSound(UnitKind kind)
    {
        Debug.Log("PlayInteractSound");
        switch (kind)
        {
            case UnitKind.Booster:
                if (_interactSource == null) return;
                _interactSource.PlayOneShot(_crunchSound);
                Debug.Log("Play crunch");
                break;
        }
    }

    public void UpdateFootstepVolume(float speed)
    {
        if (_footstepSource == null) return;
        _footstepSource.volume = Mathf.Clamp01(speed);
    }
}