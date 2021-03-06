﻿using System;
using System.Collections;
using UnityEngine;
//Nicolas I et Sarah
[Serializable]
public class Weapon : MonoBehaviour
{
    public GameObject weaponGO;
    private WeaponAsset weapon;
    private PlayerAsset playerAsset;
    private bool shot;

    [SerializeField] private GameObject projectile;

    public GameObject Projectile { get => projectile; set => projectile = value; }
    public WeaponAsset.WeaponType Type { get => weapon.Type; }

    public void Init(WeaponAsset weapon, PlayerAsset playerAsset)
    {
        shot = true;
        this.weapon = weapon;
        this.playerAsset = playerAsset;
        GetComponent<SpriteRenderer>().sprite = weapon.Sprite;
    }

    private void FixedUpdate()
    {
        if (GameObject.Find("Player").GetComponent<Player>().PlayerAsset.WeaponsList[0] != null)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
            gameObject.GetComponent<SpriteRenderer>().sprite = GameObject.Find("Player").GetComponent<Player>().PlayerAsset.WeaponsList[0].GetComponent<WeaponItem>().WeaponAsset.Sprite;
        }
        else if (GameObject.Find("Player").GetComponent<Player>().PlayerAsset.WeaponsList[0] != null)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
            gameObject.GetComponent<SpriteRenderer>().sprite = GameObject.Find("Player").GetComponent<Player>().PlayerAsset.WeaponsList[1].GetComponent<WeaponItem>().WeaponAsset.Sprite;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public WeaponAsset GetAsset()
    {
        return weapon;
    }

    public void SetWeapon(GameObject weapon)
    {
        if (weapon != null)
        {
            this.weapon = weapon.GetComponent<WeaponItem>().WeaponAsset;
            GameObject temp;
            temp = playerAsset.WeaponsList[1];
            playerAsset.WeaponsList[1] = playerAsset.WeaponsList[0];
            playerAsset.WeaponsList[0] = temp;

            UIController.uIController.changeWeapon.Invoke();
            UIController.uIController.changeAmmo.Invoke();
            weaponGO.GetComponent<SpriteRenderer>().sprite = weapon.GetComponent<WeaponItem>().WeaponAsset.Sprite;
        }
    }

    public void MultiplyDamage(float dmg)
    {
        weapon.Damage *= (int)dmg;
    }
    public int GetDamage()
    {
        return weapon.Damage;
    }

    public int GetAmmunitions()
    {
        return weapon.Ammunitions;
    }

    public void SetAmmunitions(int ammunitions)
    {
        weapon.Ammunitions = ammunitions;
        UIController.uIController.changeAmmo.Invoke();
    }

    public void AddAmmunitions(int ammo)
    {
        weapon.Ammunitions = weapon.Ammunitions + ammo - (weapon.Ammunitions + ammo) % weapon.MaxAmmunitions;
        UIController.uIController.changeAmmo.Invoke();
    }

    public int GetLoaderAmmo()
    {
        return weapon.Loader;
    }

    IEnumerator ReloadTimer()
    {
        shot = false;
        Reload();
        yield return new WaitForSeconds(weapon.ReloadingTime);
        if (weapon.Loader > 0)
            shot = true;
    }

    public void Reload()
    {
        if (weapon.Ammunitions > 0)
        {
            if (weapon.LoaderCappacity != 0 && weapon.Loader > 0)
            {
                int bullets = weapon.Loader;
                weapon.Loader = weapon.LoaderCappacity;
                weapon.Ammunitions -= weapon.LoaderCappacity + bullets;
            }
            else
            {
                if (weapon.Ammunitions < weapon.LoaderCappacity)
                {
                    weapon.Loader = weapon.Ammunitions;
                    weapon.Ammunitions = 0;
                }
                else
                {
                    weapon.Loader = weapon.LoaderCappacity;
                    weapon.Ammunitions -= weapon.LoaderCappacity;
                }
            }
        }

    }

    IEnumerator CoolDown()
    {
        if (weapon.Type == WeaponAsset.WeaponType.CQC || (weapon.Ammunitions >= 0 && weapon.Loader >= 0))
        {
            Shot();
            shot = false;
            yield return new WaitForSeconds(weapon.Cooldown);
            shot = true;
        }
        else
            shot = false;
    }

    public void Attack() //tourne l'arme dans le bon sens
    {
        if (shot)
        {
            Vector3 angle = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            float rotz = Mathf.Atan2(angle.y, angle.x) * Mathf.Rad2Deg;
            if (!(weapon.Type == WeaponAsset.WeaponType.CQC))
            {
                transform.rotation = Quaternion.Euler(0f, 0f, rotz - 90);

            }
            StartCoroutine(CoolDown());
        }
    }
    //réalisé par Sarah à partir d'ici

    public void Shot()
    {
        if (weapon.Type != WeaponAsset.WeaponType.CQC)
        {
            if (weapon.Loader <= 0)//teste si on a encore des balles
                StartCoroutine(ReloadTimer());
            weapon.Loader--;
            UIController.uIController.changeAmmo.Invoke();

            if ((weapon.Type == WeaponAsset.WeaponType.Line))
                //attaque en ligne avec RailGun
                LineShot(weapon.Speed, weapon.Damage, 0);
            if ((weapon.Type == WeaponAsset.WeaponType.Shotgun))
                //attaque en Arc avec Shotgun
                ArcShot(weapon.Nbbulletsbyshot);
            if ((weapon.Type == WeaponAsset.WeaponType.Circle))
                //attaque en cercle avec Circleshot
                CircleShot(weapon.Nbbulletsbyshot);
        }
        //attaque corps à corps
        Cqc();
        GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>().PlaySingle(playerAsset.WeaponsList[0].GetComponent<WeaponItem>().WeaponAsset.Clip);
    }

    private void Cqc()
    {
        Vector3 launching = new Vector3(transform.position.x, transform.position.y, 0);
        Collider2D[] enemiesTouched =
            Physics2D.OverlapCircleAll(launching, 0.75f, LayerMask.GetMask("Aerial", "Grounsd"));
        for (int i = 0; i < enemiesTouched.Length; i++)
        {
            if (enemiesTouched[i].CompareTag("Enemy"))
                enemiesTouched[i].gameObject.GetComponent<Enemy>().TakeDamage(weapon.Damage);
            if (enemiesTouched[i].CompareTag("Boss"))
                enemiesTouched[i].gameObject.GetComponent<Boss>().ChangeLife(-weapon.Damage);
        }

    }
    private void LineShot(float speed, int damage, float angle)//tir linéaire
    {
        Vector3 launching = new Vector3(transform.position.x, transform.position.y, 0);
        Instantiate(projectile, launching,
            transform.rotation).GetComponent<Projectile>().Init(weapon.Bullet, speed, damage, transform.position, angle);
    }

    private void CircleShot(int nbprojectile)//tir en cercle
    {

        float angle = 360 / nbprojectile;

        for (int i = 0; i < nbprojectile; i++)
        {

            LineShot(weapon.Speed, weapon.Damage, angle * i);
        }
    }
    private void ArcShot(int nbprojectile) //tir type shotgun
    {
        if (nbprojectile % 2 != 0)
        {
            LineShot(weapon.Speed, weapon.Damage, 0);
        }

        for (int i = 1; i <= nbprojectile / 2; i++)
        {
            LineShot(weapon.Speed, weapon.Damage, 10 * i);
            LineShot(weapon.Speed, weapon.Damage, -10 * i);
        }

    }
}