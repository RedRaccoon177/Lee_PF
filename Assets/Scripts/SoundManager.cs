using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SoundManager : MonoBehaviour
{
    [Header("오디오 소스")]
    [SerializeField] private AudioSource _bgmSource;  // BGM 재생용
    [SerializeField] private AudioSource _sfxSource;  // UI 효과음 재생용
    [SerializeField] private AudioSource _gunSource;  // 총기 사운드 재생용
    [SerializeField] private AudioSource _hitSource;  // 피격 효과음 재생용

    [Header("오디오 클립")]
    [SerializeField] private AudioClip _bgmClip;      // 인게임 BGM
    [SerializeField] private AudioClip _hoverClip;    // 버튼 Hover 효과음
    [SerializeField] private AudioClip _clickClip;    // 버튼 클릭 효과음
    [SerializeField] private AudioClip _shootClip;    // 총 발사 효과음
    [SerializeField] private AudioClip _reloadClip;   // 장전 효과음
    [SerializeField] private AudioClip _enemyHitClip; // 적 피격 효과음
    [SerializeField] private AudioClip _wallHitClip;  // 벽 피격 효과음

    void Start()
    {
        PlayBGM(); // 인게임 시작 시 BGM 자동 재생
    }

    // BGM 재생 (무한 반복)
    public void PlayBGM()
    {
        if (_bgmSource != null && _bgmClip != null)
        {
            _bgmSource.clip = _bgmClip;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }
    }

    // 버튼 위에 마우스를 올릴 때 효과음
    public void PlayHoverSound()
    {
        if (_sfxSource != null && _hoverClip != null)
        {
            _sfxSource.PlayOneShot(_hoverClip);
        }
    }

    // 버튼 클릭할 때 효과음
    public void PlayClickSound()
    {
        if (_sfxSource != null && _clickClip != null)
        {
            _sfxSource.PlayOneShot(_clickClip);
        }
    }

    // 총 발사 사운드
    public void PlayShootSound()
    {
        if (_gunSource != null && _shootClip != null)
        {
            _gunSource.PlayOneShot(_shootClip);
        }
    }

    // 장전 사운드
    public void PlayReloadSound()
    {
        if (_gunSource != null && _reloadClip != null)
        {
            _gunSource.PlayOneShot(_reloadClip);
        }
    }

    // 피격 효과음
    public void PlayEnemyHitSound()
    {
        if (_hitSource != null && _enemyHitClip != null)
        {
            _hitSource.PlayOneShot(_enemyHitClip);
        }
    }

    // 벽 피격 효과음
    public void PlayWallHitSound()
    {
        if (_hitSource != null && _wallHitClip != null)
        {
            _hitSource.PlayOneShot(_wallHitClip);
        }
    }
}
