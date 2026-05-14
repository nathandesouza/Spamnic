using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;

    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    public float jumpHeight = 2f;
    public float Impulso = 6f;
    public float gravity = -19.62f;
    Vector3 velocity;

    private bool PodeMarretar = true;
    private bool PodeAtaquar = true;

    public Animator animator;

    [Header("Configuração UI")]
    [SerializeField] private Image imageAttack;
    [SerializeField] private Image imageMarreta;
    [SerializeField] private TextMeshProUGUI textoContador;

    private void Start()
    {
        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (textoContador != null) textoContador.gameObject.SetActive(false);
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 moveDir = Vector3.zero;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            animator.SetBool("Correr", true);
        }
        else
        {
            animator.SetBool("Correr", false);
        }

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetBool("Jump", true);
        }
        else
        {
            animator.SetBool("Jump", false);
        }

        animator.SetBool("Queda", !controller.isGrounded && velocity.y < 0);

        velocity.y += gravity * Time.deltaTime;
        Vector3 finalMovement = (moveDir * speed) + velocity;
        controller.Move(finalMovement * Time.deltaTime);

        Attack();
    }

    void Attack()
    {
        if (Input.GetMouseButtonDown(0) && PodeAtaquar)
        {
            PodeAtaquar = false;
            animator.SetTrigger("Attack");
            imageAttack.color = Color.black;
            StartCoroutine(DelayAtaque(0.5f));
        }

        if (Input.GetMouseButtonDown(1) && controller.isGrounded && PodeMarretar)
        {
            PodeMarretar = false;
            animator.SetTrigger("Impulso");
            
            if (imageMarreta != null) imageMarreta.color = Color.black;

            StartCoroutine(DelayImpulso(2f));
        }
    }

    IEnumerator DelayAtaque(float tempo)
    {
        yield return new WaitForSeconds(tempo);
        PodeAtaquar = true;
        imageAttack.color = Color.white;
    }

    IEnumerator DelayImpulso(float tempo)
    {
        if (textoContador != null) textoContador.gameObject.SetActive(true);

        float tempoRestante = tempo;

        yield return new WaitForSeconds(0.3f);
        velocity.y = Mathf.Sqrt(Impulso * -2f * gravity);

        while (tempoRestante > 0)
        {
            if (textoContador != null) textoContador.text = tempoRestante.ToString("F0");
            yield return new WaitForSeconds(1f);
            tempoRestante--;
        }

        PodeMarretar = true;
        if (imageMarreta != null) imageMarreta.color = Color.white;
        if (textoContador != null) textoContador.gameObject.SetActive(false);
    }
}