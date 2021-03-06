﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
//Everyone
[Serializable]
public class Player : MovingObject
{
    public static Player instance;
    [SerializeField] private GameObject camera;

    public bool canAttack;

    private bool goLeft;
    private bool goUp;
    private bool goRight;
    private bool goDown;

    private bool noForcedMove;
    public bool testForDash;
    private bool testForInvincibility;

    private bool nearShop;
    private bool nearChest;

    private Rigidbody2D rigid;  //utile pour déplacement glace

    [SerializeField] private PlayerAsset playerAsset;
    [SerializeField] private GameObject gameover;
    [SerializeField] private GameObject SlotItem1;
    [SerializeField] private GameObject SlotItem2;

    private Weapon weapon;
    private GameObject actualWeapon;

    private Board.Type roomType;
    private Animator animator;
    private int animMoveHashID = Animator.StringToHash("Move");
    private int animDirectionHashID = Animator.StringToHash("Direction");
    private int animDashID = Animator.StringToHash("Dash");
    private int animDeathID = Animator.StringToHash("Death");

    private float moveX;
    private float moveY;
    private Vector2 firstPos;

    [SerializeField] private UnlockedItemsAsset unlockedItems;

    public GameObject Camera { get => camera; set => camera = value; }
    public PlayerAsset PlayerAsset { get => playerAsset; set => playerAsset = value; }
    public Board.Type RoomType { get => roomType; set => roomType = value; }
    public UnlockedItemsAsset UnlockedItems { get => unlockedItems; set => unlockedItems = value; }

    private bool affected = false;

    public bool inventorySignal = false;
    private bool inEditor;

    private void Start()
    {
        canAttack = true;
        roomType = Board.Type.Shop;
        weapon = GetComponentInChildren<Weapon>();

        instance = this;
        noForcedMove = true;

        unlockedItems.CheckDuplicate();
        /*foreach (GameObject itm in unlockedItems.Unlocked)
        {
            WeaponItem weaponData = itm.GetComponent<WeaponItem>();
            if (weaponData != null)
                weaponData.Init();
        }*/

        inEditor = Application.isEditor;
        actualWeapon = playerAsset.WeaponsList[0];
        weapon.Init(actualWeapon.GetComponent<WeaponItem>().WeaponAsset, playerAsset);
        if (inEditor)
            Inventory.instance.Add(actualWeapon);

        playerAsset.Position = transform.position;
        playerAsset.Invicibility = false;
        testForDash = true;

        firstPos = transform.position;
        animator = GetComponent<Animator>();

        nearShop = false;
        nearChest = false;
        if (playerAsset.Floor != 0)
            GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>().ChangeBO(playerAsset.Floor + 3);
    }

    public void RestartPlayer()
    {
        playerAsset.Hp = 100;
        playerAsset.MaxHP = 100;
        SetEffect(0);
        playerAsset.MaxEffectValue = 50;
        playerAsset.Gold = 0;

        for (int i = 0; i < 2; i++)
        {
            if (playerAsset.WeaponsList[i] != null)
                playerAsset.WeaponsList[i].GetComponent<WeaponItem>().WeaponAsset.ResetWeapon();
        }

        instance.PlayerAsset.WeaponsList = new[] { Resources.Load("Pistol") as GameObject, null };
        instance.PlayerAsset.ObjectsList[0] = null;
        instance.PlayerAsset.ObjectsList[1] = null;
        instance.PlayerAsset.Floor = 0;
        instance.PlayerAsset.Invicibility = false;
        instance.PlayerAsset.Speed = 3;
        Inventory.instance.items.Clear();
    }

    private void FixedUpdate()
    {
        if (!inEditor && inventorySignal) //fix a bug in build
        {
            gameObject.GetComponent<Inventory>().Add(actualWeapon);
            inventorySignal = false;
        }

        if (affected)
            SetLife(playerAsset.Hp - 0.2f);

        moveX = PlayerAsset.Speed * Time.deltaTime;
        moveY = PlayerAsset.Speed * Time.deltaTime;

        //déplacement et collision
        if (!Input.anyKey)
        {
            animator.SetBool(animMoveHashID, false);
        }
        Camera.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.transform.position.z);
        if (playerAsset.ObjectsList[0] != null)
        {
            SlotItem1.GetComponent<Image>().enabled = true;
            SlotItem1.GetComponent<Image>().sprite = playerAsset.ObjectsList[0].GetComponent<Object>().ObjectsAsset.Sprite;
        }
        else
        {
            SlotItem1.GetComponent<Image>().enabled = false;
            SlotItem1.GetComponent<Image>().sprite = null;
        }
        if (playerAsset.ObjectsList[1] != null)
        {
            SlotItem2.GetComponent<Image>().enabled = true;
            SlotItem2.GetComponent<Image>().sprite = playerAsset.ObjectsList[1].GetComponent<Object>().ObjectsAsset.Sprite;
        }
        else
        {
            SlotItem2.GetComponent<Image>().enabled = false;
            SlotItem2.GetComponent<Image>().sprite = null;
        }
    }

    public Vector3 GetPos()
    {
        return transform.position;
    }

    public Vector3 GetFirstPos()
    {
        return firstPos;
    }

    public void SetMaxLife(float value)
    {
        playerAsset.MaxHP = value;
        UIController.uIController.changeMaxHp.Invoke();
    }

    public void SetLife(float value)
    {
        playerAsset.Invicibility = true;
        StartCoroutine(InvicibilityCoolDown());

        playerAsset.Hp = value;

        if (playerAsset.Hp > playerAsset.MaxHP)
            playerAsset.Hp = playerAsset.MaxHP;

        UIController.uIController.changeHp.Invoke();
        if (playerAsset.Hp <= 0)
            StartCoroutine(GameOver());
    }

    IEnumerator GameOver()
    {
        animator.SetTrigger(animDeathID);
        SoundManager soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
        soundManager.PlaySingle(soundManager.lfx[4]);
        yield return new WaitForSeconds(0.51f);
        Time.timeScale = 0f;
        gameover.SetActive(true);
    }

    IEnumerator InvicibilityCoolDown()
    {
        yield return new WaitForSeconds(1);
        playerAsset.Invicibility = false;
    }

    public void SetEffect(float value)
    {
        playerAsset.EffectValue = value;
        UIController.uIController.changeEffect.Invoke();
        if (playerAsset.EffectValue >= playerAsset.MaxEffectValue)
        {
            affected = true;
        }
        else
            affected = false;
    }

    public void SetMaxEffect(int value)
    {
        PlayerAsset.MaxEffectValue = value;
        UIController.uIController.changeMaxEffect.Invoke();
    }

    public void Attack()
    {
        if (canAttack && playerAsset.Floor != 0)
        {
            weapon.Attack();
        }
    }

    public void doDash()
    {
        if ((goUp || goRight || goDown || goLeft) && testForDash)
        {
            StartCoroutine(Dash());
            SoundManager soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<SoundManager>();
            soundManager.PlaySingle(soundManager.lfx[3]);
        }
    }

    IEnumerator Dash()
    {
        if (!Input.GetButton("Up"))
        {
            goUp = false;
        }

        if (!Input.GetButton("Down"))
        {
            goDown = false;
        }

        if (!Input.GetButton("Left"))
        {
            goLeft = false;
        }

        if (!Input.GetButton("Right"))
        {
            goRight = false;
        }

        if (testForDash == true)
        {
            if (goUp && goRight)
            {
                Vector3 firstPos = this.transform.position;
                Vector3 lastPos = firstPos + new Vector3(0.8f, 0.8f, 0);

                StartCoroutine(Movement(lastPos, 8f));
            }
            else if (goDown && goRight)
            {
                Vector3 firstPos = this.transform.position;
                Vector3 lastPos = firstPos + new Vector3(0.8f, -0.8f, 0);

                StartCoroutine(Movement(lastPos, 8f));
            }
            else if (goUp && goLeft)
            {
                Vector3 firstPos = this.transform.position;
                Vector3 lastPos = firstPos + new Vector3(-0.8f, 0.8f, 0);

                StartCoroutine(Movement(lastPos, 8f));
            }
            else if (goDown && goLeft)
            {
                Vector3 firstPos = this.transform.position;
                Vector3 lastPos = firstPos + new Vector3(-0.8f, -0.8f, 0);

                StartCoroutine(Movement(lastPos, 8f));
            }
            else if (goUp)
            {
                Vector3 firstPos = this.transform.position;
                Vector3 lastPos = firstPos + new Vector3(0, 1.3f, 0);

                StartCoroutine(Movement(lastPos, 8f));
            }
            else if (goRight)
            {
                Vector3 firstPos = this.transform.position;
                Vector3 lastPos = firstPos + new Vector3(1.3f, 0, 0);

                StartCoroutine(Movement(lastPos, 8f));
            }
            else if (goDown)
            {
                Vector3 firstPos = this.transform.position;
                Vector3 lastPos = firstPos + new Vector3(0, -1.3f, 0);

                StartCoroutine(Movement(lastPos, 8f));
            }
            else if (goLeft)
            {
                Vector3 firstPos = this.transform.position;
                Vector3 lastPos = firstPos + new Vector3(-1.3f, 0, 0);

                StartCoroutine(Movement(lastPos, 8f));
            }
            playerAsset.Position = transform.position;

            testForDash = false;
            yield return new WaitForSeconds(1f);
            
            testForDash = true;
        }
    }

    public void MoveUp()
    {
        if (noForcedMove)
        {
            goUp = true;

            animator.SetInteger(animDirectionHashID, 0);
            animator.SetTrigger(animMoveHashID);
            if (this.Collision(transform.position, playerAsset.Speed, 1))
            {
                this.transform.Translate(0, moveY, 0);
            }

            playerAsset.Position = transform.position;
        }
    }

    public void StopMoveUp()
    {
        goUp = false;
    }

    public void MoveRight()
    {
        if (noForcedMove)
        {
            goRight = true;

            animator.SetInteger(animDirectionHashID, 1);
            animator.SetTrigger(animMoveHashID);
            if (this.Collision(transform.position, playerAsset.Speed, 2))
            {
                this.transform.Translate(moveX, 0, 0);
            }

            playerAsset.Position = transform.position;
        }
    }

    public void StopMoveRight()
    {
        goRight = false;
    }

    public void MoveDown()
    {
        if (noForcedMove)
        {
            goDown = true;

            animator.SetInteger(animDirectionHashID, 2);
            animator.SetTrigger(animMoveHashID);
            if (this.Collision(transform.position, playerAsset.Speed, 3))
            {
                this.transform.Translate(0, -moveY, 0);
            }

            playerAsset.Position = transform.position;
        }
    }

    public void StopMoveDown()
    {
        goDown = false;
    }

    public void MoveLeft()
    {
        if (noForcedMove)
        {
            goLeft = true;

            animator.SetInteger(animDirectionHashID, 3);
            animator.SetBool(animMoveHashID, true);
            if (this.Collision(transform.position, playerAsset.Speed, 0))
            {
                this.transform.Translate(-moveX, 0, 0);
            }

            playerAsset.Position = transform.position;
        }
    }

    public void StopMoveLeft()
    {
        goLeft = false;
    }

    public void ForcedMovement(Vector3 direction)                   //makes player move without his consent
    {
        StartCoroutine(Movement(transform.position + direction * 0.5f, 3f));
    }

    IEnumerator Movement(Vector3 finalPos, float speed)
    {
        noForcedMove = false;

        Vector2 pos = transform.position;

        float step = speed * Time.deltaTime;
        Vector2 direction = (Vector2)finalPos - pos;

        bool noWall = true;

        while (noWall && (transform.position.x > finalPos.x + 0.1f || transform.position.x < finalPos.x - 0.1f) || (transform.position.y > finalPos.y + 0.1f || transform.position.y < finalPos.y - 0.1f))
        {
            if (!Collision(transform.position, playerAsset.Speed, 1))
            {
                break;
            }
            if (!this.Collision(transform.position, playerAsset.Speed, 2))
            {
                break;
            }
            if (!Collision(transform.position, playerAsset.Speed, 3))
            {
                break;
            }
            if (!Collision(transform.position, playerAsset.Speed, 0))
            {
                break;
            }
            transform.position = Vector3.MoveTowards(transform.position, finalPos, step);
            playerAsset.Position = transform.position;
            yield return null;
        }
        noForcedMove = true;
    }

    public void IsNearChest()
    {
        nearChest = !nearChest;
    }

    public bool GetNearChest()
    {
        return nearChest;
    }

    public void ActiveChestUI()
    {
        InventoryUI.instance.EnableUI(false);
        ChestUI.instance.EnableUI();
    }

    public void IsNearShop()
    {
        nearShop = !nearShop;
    }

    public bool GetNearShop()
    {
        return nearShop;
    }

    public void ActiveShopUI()
    {
        if (playerAsset.Floor != 0)
        {
            InventoryUI.instance.EnableUI(true);
        }
        ShopUI.instance.EnableUI();
    }
}