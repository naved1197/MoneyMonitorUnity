using CubeHole;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueTxt;
    [SerializeField] private Animator animator;
    [SerializeField] private float typingSpeed;
    [SerializeField] private float nextTipDelay;
    [SerializeField] private float blinkDelay;
    private StringResourceLibrary tips;
    private bool active;
    private Coroutine blinkCoroutine;
    private bool canShowTips;
    private Coroutine tipsCoroutine;

    public void Init()
    {
        dialogueTxt.text = "";
        tips = AppResources.GetStringsLibrary(R_Strings.BudgetTips);
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        active = true;
        blinkCoroutine = StartCoroutine(BlinkAnimation());
        ChangeBehaviour("");
    }
    public void StopAnimations()
    {
        active = false;
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine= null;
        if (tipsCoroutine != null)
        {
            StopCoroutine(tipsCoroutine);
        }
        tipsCoroutine = null;
        animator.SetBool("speak", false);
        canShowTips = false;
    }
    IEnumerator BlinkAnimation()
    {
        while(active)
        {
            animator.SetTrigger("blink");
            yield return new WaitForSeconds(Random.Range(blinkDelay, blinkDelay + 5));
        }
    }
    public void ChangeBehaviour(string amountTxt)
    {
        if(string.IsNullOrEmpty(amountTxt))
        {
            string tip = "Enter your budget value";
            StartCoroutine(TypeDialogue(tip));
        }
        else 
        {
            if(canShowTips)
            {
                return;
            }
            canShowTips=true;
            if (tipsCoroutine != null)
            {
                StopCoroutine(tipsCoroutine);
            }
            tipsCoroutine = StartCoroutine(StarTipping());
        }
    }
    IEnumerator StarTipping()
    {
        while (canShowTips)
        {
            string tip = tips.stringLibraries[Random.Range(0, tips.stringLibraries.Count)].value;
            yield return StartCoroutine(TypeDialogue(tip));
            yield return new WaitForSeconds(nextTipDelay);
        }
    }
    IEnumerator TypeDialogue(string tip)
    {
        animator.SetBool("speak", true);
        dialogueTxt.text = "";
        for (int i = 0; i < tip.Length; i++)
        {
            dialogueTxt.text += tip[i];
            yield return new WaitForSeconds(typingSpeed);
        }
        animator.SetBool("speak", false);
    }

}
