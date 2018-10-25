﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class Player_Moving : MonoBehaviour
{

    public GameMaster LevelManager;

    //AUDIO
    public AudioClip jump;
    private AudioSource sound;

    // MOVIMIENTO
    public float topSpeed = 25f; // Maxima velocidad del personaje
    bool facingRight = true; // Indica al Sprita a qué direccón apuntar
    public Animator anim;

    //VIDA PERSONAJE
    [SerializeField]
    Slider HealthSlider;
    public static float maxHealth = 100;
    public float currentHealth;
    public bool Damage;

    // SALTO
    public bool grounded = false;  // Referencia al animator
    public Transform groundCheck;   // transform a los pies del personaje para comprobar si toca el suelo
    float groundRadius = 2f;   // diametro del circulo que detecta el suelo
    public static float jumpForce = 2200f; // Fuerza del salto
    public LayerMask whatIsGround; // Capas que detecta como suelo

    // ATAQUE
    private float timeBtAttack;
    public float startTimeBtAttack;
    public Transform attackPos;
    public float attackRange;
    public LayerMask whatIsEnemies;


    // AGACHARSE
    public float Crouch;
    public bool crouching;


    // COLGARSE ORILLA
    public bool grabOnEdges = false;// variable del doble salto


    // ENEMIGOS

    public int damage;
    public GameObject frog;
    public int fireDamage;


    // RE APARECER CUANDO CAE
    public Vector3 respawnPoint;


    private float count;
    public Text PointText;


    public Collider2D[] myCols;
    public static Player_Moving instance;
    public bool Hurt= false;

    public Animator camAnim;

    void Awake()
    {
        sound = GetComponent<AudioSource>();
    }

    private void Start()
    {
        instance = this;
        myCols = this.GetComponents<Collider2D>();
        LevelManager = FindObjectOfType<GameMaster>();
        anim = GetComponent<Animator>();

        respawnPoint = transform.position;

        //PlayerHealth
        HealthSlider.value = maxHealth;
        currentHealth = HealthSlider.value;
        LevelManager = FindObjectOfType<GameMaster>();

        count = 0;

        SetCounter();

    }



    private void FixedUpdate()
    {
        // MOVERSE

        if (!this.anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {  //Condicion que interrumpe el movimiento si se ataca

            anim.SetFloat("vSpeed", GetComponent<Rigidbody2D>().velocity.y);

            // CORRER
            float move = Input.GetAxis("Horizontal"); // obtener la direccion del movimiento
            anim.SetFloat("Speed", Mathf.Abs(move));
            // Le da velocidad al rigidbody en la direccion de move, la velocidad
            GetComponent<Rigidbody2D>().velocity = new Vector2(move * topSpeed, GetComponent<Rigidbody2D>().velocity.y);
            // Condicion para voltear el personaje si se apunta en direccion contraria
            if (move > 0 && !facingRight)
                Flip();
            else if (move < 0 && facingRight)
                Flip();





        }
        anim.SetBool("Crouch", crouching);

    }






    private void Update()

    {


        // SALTAR

        // Verdadero o falso que el suelo se transformo. toco el grounRadius
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);
        // Diciendole al animator que tocamos el suelo
        anim.SetBool("Ground", grounded);
        float move = Input.GetAxis("Horizontal"); // obtener la direccion del movimiento
        if (!this.anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) //Condicion que interrumpe el movimienot si se ataca
        {

            if (grounded)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {

                    GetComponent<Rigidbody2D>().AddForce(new Vector2(0, jumpForce));
                    sound.PlayOneShot(jump);



                }
            }

            Crouch = Input.GetAxis("Vertical");
            CrouchFunction();


        }

        // ATAQUE
        if (timeBtAttack <= startTimeBtAttack)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                anim.SetTrigger("Attack");
                Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemies);
                for (int i = 0; i < enemiesToDamage.Length; i++)
                {

                    enemiesToDamage[i].GetComponent<Enemy>().Health -= damage;
                }
            }
            timeBtAttack = startTimeBtAttack;
        }
        else
        {
            timeBtAttack -= Time.deltaTime;
        }



    }






    // VOLTEAR AL PERSONAJE
    void Flip()
    {
        // Diciendole que apuntamos a la direccion opuesta
        facingRight = !facingRight;
        // Obtener LOCALSCALE
        Vector3 theScale = transform.localScale;
        // Voltaer en eje X
        theScale.x *= -1;
        // Aplicar transformacion en LOCAL SCALE
        transform.localScale = theScale;
    }



    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.tag == "FallDetector")
        {
            Respawn();
        }
        if (other.tag == "Checkpoint")
        {
            respawnPoint = other.transform.position;
        }
        if (other.gameObject.tag == "FireDamage")
        {
            currentHealth = currentHealth - fireDamage;
            //Debug.Log(currentHealth);
        }
        if (other.gameObject.tag == "PickUp")
        {
            Destroy(other.gameObject);
            count = count + 0.5f;
            Debug.Log(count);

            SetCounter();
        }
        if (other.gameObject.tag == "Slow")
        {
            topSpeed = 7f;
        }

        if (other.gameObject.tag == "x")
        {
            Debug.LogWarning("Entra de lado");
            float posX = other.gameObject.transform.parent.transform.localPosition.x + 10.0f;
            float posY = other.gameObject.transform.parent.transform.localPosition.y;
            float posZ = other.gameObject.transform.parent.transform.localPosition.z;
            other.gameObject.transform.parent.transform.localPosition += new Vector3(posX, posY, posZ);
            TakeDamage(damage);
        }
        if (other.gameObject.tag == "y")
        {
            Destroy(other.gameObject.transform.parent.gameObject);
            //frog = GameObject.FindWithTag("Enemy");
        }


        //GEMAS


    }
    void SetCounter()
    {
        PointText.text = "Ruby: " + count.ToString();


    }


    public void EnemyJump()
    {
        grounded = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Slow")
        {
            topSpeed = 14f;
        }
    }

    private void OnTriggerStay2D(Collider2D col)
    {

        if (col.gameObject.tag == "Spikes")
        {
            HealthSlider.value -= 1.5f;
            currentHealth = HealthSlider.value;
            anim.SetTrigger("Damage"); // Activa el Trigger en el Animator
            GetComponent<Rigidbody2D>().velocity = Vector2.zero; //Resetea la velocidad del personaje para que no avance mientras ataca
        }
    }


    void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(currentHealth);


    }



    public void Respawn()
    {

        GetComponent<CapsuleCollider2D>().transform.position = respawnPoint;

    }



    void CrouchFunction()
    {

        if (Crouch != 0 && grounded == true)
        {
            crouching = true;
        }


        else
        {
            crouching = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }




    public void TriggerHurt(float HurtTime)
    {
        StartCoroutine(HurtBlinker(HurtTime));
    }

    IEnumerator HurtBlinker(float HurtTime)
    {

        int enemyLayer =LayerMask.NameToLayer("Enemy");//  ignorar colision con enemigos
        int playerLayer = LayerMask.NameToLayer("Player");
        Physics2D.IgnoreLayerCollision(enemyLayer, playerLayer);
        foreach (Collider2D collider in Player_Moving.instance.myCols)
        {
            collider.enabled = false;
            collider.enabled = true;
       
        anim.SetBool("Hurt", Hurt);      // Comenzar Loop de parpadeo
        }



        yield return new WaitForSeconds(HurtTime);  // Tiempo en que no puede ser dañado de nuevo

       
        Physics2D.IgnoreLayerCollision(enemyLayer, playerLayer, false);   // Para de parpadear
       
    }


}
