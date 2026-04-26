using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    public void PlayMoveAnimation(float normalizedSpeed) => _animator.SetFloat("Speed", normalizedSpeed);

    public void PlayDieAnimation() => _animator.SetTrigger("Die");
}