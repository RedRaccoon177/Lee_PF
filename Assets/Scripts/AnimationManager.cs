using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void PlayIdleAnimation(bool isIdle)
    {
        _animator.SetBool("IsIdle", isIdle);
    }

    public void PlayWalkAnimation(bool isWalking)
    {
        _animator.SetBool("IsWalk", isWalking);
    }

    public void PlayRushAnimation(bool isRushing)
    {
        _animator.SetBool("IsRush", isRushing);
    }

    public void PlaySkillAnimation(string skillName)
    {
        _animator.SetTrigger(skillName);
    }
}
