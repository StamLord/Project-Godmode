using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorExtensions
{
    public static void AnimationSwap(Animator animator, AnimationClip oldAnimation, AnimationClip newAnimation)
    {
        AnimatorOverrideController aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);

        var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();

        foreach (var a in aoc.animationClips)
        {
            if(a == oldAnimation)
            {
                anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, newAnimation));
            }
            else
            {
                anims.Add(new KeyValuePair<AnimationClip,AnimationClip>(a,a));
            }
        }

        aoc.ApplyOverrides(anims);
        animator.runtimeAnimatorController = aoc;
    }
}
