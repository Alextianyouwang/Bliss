using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System;
public class CursorBlock : NumberBlocks
{
    public int clickTimes = 0;
    public GameObject cursorBlock;
    private GameObject cursorBlock_instance;
    float timer= 0;

    Vector3 initialScale, targetScale;
    MeshRenderer mr;
    bool hasBeenClicked = false;

    public Action<bool> OnBommerangAnimationFinished;

    Coroutine boomrangeAnimation;

    void Start()
    {

    }

#if UNITY_EDITOR

    [MenuItem("Quickies/Cursor Mode/Fade")]
    public static void ToggleCursorValidate_s()
    {
        PlayerPrefs.SetInt("CursorSpam",1);
        UnityEngine.Debug.Log("Cursor Mode Set to Fade");
    }
    [MenuItem("Quickies/Cursor Mode/Bommerang")]
    public static void ToggleCursorValidate_b()
    {
        PlayerPrefs.SetInt("CursorSpam",0);
        UnityEngine.Debug.Log("Cursor Mode Set to Boomerang");
    }
     [MenuItem("Quickies/Cursor Mode/Boomerang Restricted")]
    public static void ToggleCursorValidate_br()
    {
        PlayerPrefs.SetInt("CursorSpam",2);
        UnityEngine.Debug.Log("Cursor Mode Set to Boomerang Restricted");
    }
    [MenuItem("Quickies/Cursor Mode/Fade", true)]
    public static bool v_toggle_s()
    {
        return PlayerPrefs.GetInt("CursorSpam")==0 ||PlayerPrefs.GetInt("CursorSpam")==2 ;
    }
    [MenuItem("Quickies/Cursor Mode/Bommerang", true)]
    public static bool v_toggle_b()
    {
        return PlayerPrefs.GetInt("CursorSpam")==1 ||PlayerPrefs.GetInt("CursorSpam")==2 ;
    }
      [MenuItem("Quickies/Cursor Mode/Boomerang Restricted",true)]
    public static bool v_toggle_br()
    {
        return PlayerPrefs.GetInt("CursorSpam")==0 ||PlayerPrefs.GetInt("CursorSpam")==1 ;

    }

 

#endif

    void Update()
    {

        if (PlayerPrefs.GetInt("CursorSpam") == 1)
        {
            CursorDisappearLogic();
        }
        else if (PlayerPrefs.GetInt("CursorSpam") == 0)
        {

        }
        else if (PlayerPrefs.GetInt("CursorSpam") == 2) 
        {
        
        }
        else
        {
            Debug.LogWarning("You have to set Cursor Mode in Quickies");
            return;
        }
    }

    public IEnumerator CollectAnimation(Transform target)
    {
       
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        float percent = 0;
        Vector3
            initialPosition = transform.position,
            //targetPosition = target,
            velRef = Vector3.zero,
            interpolatedPosition;

        float randomizationScale = 5f,
            randomValue = UnityEngine. Random.value;

        float originalDistance = Vector3.Distance(transform.position, target.position);
        float distancePercent;

        while (percent < 1 || Vector3.Distance(transform.position, target.position) > 0.1f)
        {
            distancePercent = (originalDistance - Vector3.Distance(transform.position, target.position)) / originalDistance;
            Vector3 travelDir = (target.position - initialPosition).normalized;
            Vector3 orthoToTravelDir = Vector3.Cross(travelDir, Vector3.up).normalized;
            Vector3 randomHorizComponent = Vector3.Lerp(orthoToTravelDir, -orthoToTravelDir, randomValue);
            Vector3 randomVertComponent = Vector3.up * randomValue;
            Vector3 finalRandom = (randomHorizComponent + randomVertComponent).normalized * randomizationScale;
            interpolatedPosition = Utility.QuadraticBezier(initialPosition, (initialPosition + target.position) / 2 + finalRandom, target.position, percent);
            transform.position = Vector3.SmoothDamp(transform.position, interpolatedPosition, ref velRef, 0.08f);

        
            //transform.localScale = Vector3.Lerp(initialScale, targetScale, percent);
            percent += Time.deltaTime * 1.5f;
            yield return null;
        }
        if (PlayerPrefs.GetInt("CursorSpam") == 2)
        {
            if (parentManager)
                parentManager.SetBoomerangRestrictedState(true);
       
        }
        if (boomrangeAnimation != null)
            StopCoroutine(boomrangeAnimation);

        Destroy(gameObject);
    }

    IEnumerator ResetBoomerang() 
    {
        yield return new WaitForSeconds(1.1f);
        if (PlayerPrefs.GetInt("CursorSpam") == 2)
        {
            if (parentManager)
                parentManager.SetBoomerangRestrictedState(true);
            if (boomrangeAnimation != null)
                StopCoroutine(boomrangeAnimation);

            Destroy(gameObject);
        }
    }
    void CursorDisappearLogic()
    {
        if (clickTimes >= 1 && !hasBeenClicked)
        {
            hasBeenClicked = true;
            gameObject.GetComponent<Renderer>().enabled = false;
            cursorBlock_instance = Instantiate(cursorBlock);
            cursorBlock_instance.transform.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);
            mr = cursorBlock_instance.GetComponent<MeshRenderer>();
            initialScale = cursorBlock_instance.transform.localScale;
            targetScale = initialScale * 3f;

        }
        if (!cursorBlock_instance)
            return;
        if (hasBeenClicked)
        {

            timer += Time.deltaTime;
            if (timer < 1)
            {

                cursorBlock_instance.transform.localScale = Vector3.Lerp(initialScale, targetScale, timer);
                mr.material.SetFloat("_Alpha", 1 - timer);
            }
            else
            {
                Destroy(cursorBlock_instance);
                Destroy(gameObject);
            }
        }
    }



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<CursorBlock>())
            return;
        if (!AM_BlissMain.isInTeleporting)
            clickTimes += 1;

        if (!hasBeenClicked && 
            
            (PlayerPrefs.GetInt("CursorSpam") == 0 || PlayerPrefs.GetInt("CursorSpam") == 2)) 
        {
            hasBeenClicked = true;
            if (boomrangeAnimation != null)
                StopCoroutine(boomrangeAnimation);
            boomrangeAnimation = StartCoroutine(CollectAnimation(InteractionManager.throwPointTransform));
            StartCoroutine(ResetBoomerang());

            

        }


        //CursorDissapearAnimation();

        if (collision.gameObject.tag.Equals("Quit")) 
        {
            Application.Quit();
        }
        if (collision.gameObject.tag.Equals("Restart")) 
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Bliss_Terrain Remodel");
            Cursor.lockState = CursorLockMode.None;
            //AudioManager.instance.StopAllSound();
        }

        if (collision.gameObject.tag.Equals("Application") ||
           collision.gameObject.tag.Equals("Quit") ||
           collision.gameObject.tag.Equals("Restart"))
        {
            if (!hasCollided)
            {
                //AudioManager.instance.Play("Click");
                //hasCollided = true;
            }
        }
    }
}
