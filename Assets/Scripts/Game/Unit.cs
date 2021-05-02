using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField]
    private UnitInfo[] units;

    [SerializeField]
    private Transform unitTransform;

    [SerializeField]
    private SpriteRenderer unitRenderer;

    [SerializeField]
    private Animator ani;

    ushort lasttype = 5;

    ushort type;
    byte team;
    float x;
    float y;
    float angle;
    uint h;
    uint mh;
    uint p;

    bool isReverse;

    public void SetUnit(ushort t, byte tea, float xx, float yy, float ang, uint hh, uint mhh, uint pp, byte status)
    {
        type = t;
        team = tea;
        x = xx;
        y = yy;
        angle = ang;
        h = hh;
        mh = mhh;
        p = pp;

        isReverse = status % 2 > 0;
        status /= 2;

        UpdateObject();
    }

    private void Update()
    {
        // todo : init shader value

        if (lasttype != type && units[type].anime)
        {
            AnimationClip clip = ani.GetCurrentAnimatorClipInfo(0)[0].clip;

            RuntimeAnimatorController controller = ani.runtimeAnimatorController;

            AnimatorOverrideController overrideController = new AnimatorOverrideController();
            overrideController.runtimeAnimatorController = controller; // error
            overrideController[clip] = units[type].anime;

            ani.runtimeAnimatorController = overrideController; // error
            lasttype = type;
        }

        switch (team)
        {
            case 0:
                unitRenderer.material.SetColor("HealthColor", Color.red);
                break;
            case 1:
                unitRenderer.material.SetColor("HealthColor", Color.yellow);
                break;
            case 2:
                unitRenderer.material.SetColor("HealthColor", Color.green);
                break;
            default:
                Debug.Log("not enable team");
                break;
        }

        if (mh == 0)
        {
            unitRenderer.material.SetFloat("Health", 0);
            unitRenderer.material.SetFloat("Poison", 0);
        } else
        {
            unitRenderer.material.SetFloat("Health", h / (float)mh);
            unitRenderer.material.SetFloat("Poison", p / (float)mh);
        }

        // transform.position = new Vector2(x + ((isReverse) ? unitRenderer.bounds.size.x : 0), y);
    }

    private void UpdateObject()
    {
        unitTransform.rotation = Quaternion.Euler(0, 0, angle);
        unitRenderer.flipX = isReverse;
    }
}
