using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class Move : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public CharacterController controller;
    public Transform cam; 
    public float walkSpeed = 6f;
    public float runSpeed = 7f;
    float currentSpeed; 
    public float turnSmoothTime = 0.1f; 
    float turnSmoothVelocity;

    [Header("Configurações de Câmera (FOV)")]
    public CinemachineCamera vcam; 
    public float normalFOV = 60f;
    public float runFOV = 75f;
    public float fovSmoothTime = 5f;
    float targetFOV;

    [Header("Configurações de Pulo Comum")]  
    public float jumpHeight = 2f; 
    public float gravity = -19.62f;
    Vector3 velocity;

    [Header("Configurações de Animação/Áudio")]
    public Animator PlayerAnim;
    public AudioSource audioSource;
    public AudioClip[] footstepSounds;
    public AudioClip somAtaque; 

    [Header("Configurações de Combo")]
    public int comboStep = 0;
    public float lastClickTime;
    public float maxComboDelay = 0.9f; 

    [Header("Configurações do Pulo Marreta")]
    public float launchForce = 20f; 
    public float forwardForce = 15f; 
    public bool isAttacking = false;
    private bool ataqueFoiCarregado = false;

    [Header("Ataque Carregado e Área")]
    public Slider barraCarga; 
    public float tempoParaCarregar = 0.8f; 
    private float tempoSegurando = 0f;
    public float forcaArremesso = 12f; 
    public float raioAtaqueArea = 5f; 
    public LayerMask camadasAlvo; 

    [Header("Mecânica de Arremesso (Caixas)")]
    public float forcaArremessoCaixa = 20f;
    [Range(0.1f, 1.5f)] public float curvaturaArremesso = 0.5f; 
    public GameObject objetoAlvo; 
    
    private GameObject caixaAlvo;
    private Renderer ultimoRendererCaixa; 
    private Color corOriginalCaixa = Color.white; 
    private bool travadoNaCaixa = false; 
    private bool alvoValido = false; 
    private bool arremessoCancelado = false;

    [Header("Restrição de Ângulo (20 Graus)")]
    public float limiteAnguloCorpo = 0.94f; 

    [Header("Referências de Ataque")]
    public HammerAttack hammerScript; 

    [Header("Sistema de Inventário")]
    public string[] slots = new string[3] { "", "", "" };
    public int slotSelecionado = -1; 

    [Header("Referências Visuais do Inventário")]
    public GameObject modeloMarteloMao;
    public GameObject modeloMarteloCostas;
    public GameObject modeloGarrafaMao;
    public GameObject modeloCurativoMao;

    [Header("Configurações de Arremesso de Itens")]
    public LineRenderer lineRenderer;
    public int resolucaoTrajetoria = 30;
    public float forcaArremessoProjetil = 15f;
    public Vector3 offsetLinha = new Vector3(0, 0.1f, 0);
    [Range(0f, 1f)] public float arcoArremesso = 0.25f; 
    public GameObject prefabGarrafaVoadora;
    public GameObject prefabCurativoVoador;

    [Header("Efeitos Visuais (VFX)")]
    public GameObject vfxPasso;        
    public GameObject vfxPulo;         
    public GameObject vfxAterrissagem;    
    public GameObject vfxAtaqueComum;  
    public GameObject vfxAtaqueAreaPrincipal; 
    public GameObject vfxAtaqueAreaSecundario; 
    public GameObject vfxAtaqueAreaTerciario; 
    public GameObject vfxLancamentoItem;    
    public GameObject vfxLancamentoCaixa;   
    public GameObject vfxDecolagemPlayer;   

    [Space(10)]
    public Transform spawnPeEsquerdo;  
    public Transform spawnPeDireito;   
    public Transform spawnMartelo;     
    
    private bool usarPeDireito = false; 
    private bool estavaNoChao = true; 
    private bool podeTocarAterrissagem = false;

    private List<Renderer> inimigosNoRaio = new List<Renderer>();
    private bool jaDisparouAnimacaoCura = false;
    private bool mirandoArremesso = false;

    void Start()
    {
        if (objetoAlvo) objetoAlvo.SetActive(false);
        if (barraCarga) barraCarga.gameObject.SetActive(false);
        if (lineRenderer) lineRenderer.enabled = false;
        targetFOV = normalFOV;
        AtualizarVisualItens();

        Invoke("LiberarEfeitoAterrissagem", 0.5f);
    }

    void LiberarEfeitoAterrissagem()
    {
        podeTocarAterrissagem = true;
    }

    void Update()
    {
        bool estaNoChaoAgora = controller.isGrounded;

        if (podeTocarAterrissagem && estaNoChaoAgora && !estavaNoChao && velocity.y < 0)
        {
            SpawnVFX("Aterrissagem");
        }
        
        estavaNoChao = estaNoChaoAgora;

        if (controller.isGrounded && velocity.y < 0) 
        {
            velocity.y = -2f;
            velocity.x = 0; velocity.z = 0; 
        }

        if (vcam != null)
        {
            vcam.Lens.FieldOfView = Mathf.Lerp(vcam.Lens.FieldOfView, targetFOV, Time.deltaTime * fovSmoothTime);
        }

        bool estaPegandoItem = PlayerAnim.GetCurrentAnimatorStateInfo(1).IsName("Pegando");
        bool estaUsandoItem = PlayerAnim.GetCurrentAnimatorStateInfo(1).IsName("Usar");

        if (tempoSegurando <= 0 && !mirandoArremesso)
        {
            DetectarObjetoPassivo();
        }

        if (!isAttacking && !estaPegandoItem && !estaUsandoItem)
        {
            HandleAttackInput();
            HandleInventoryInput(); 
            
            if (!travadoNaCaixa) 
            {
                MoveLogic(); 
            }
        }

        velocity.y += gravity * Time.deltaTime;  
        controller.Move(velocity * Time.deltaTime);
    }

    public void SpawnVFX(string tipo)
    {
        switch (tipo)
        {
            case "Passo":
                if (vfxPasso) 
                {
                    Transform peAlvo = usarPeDireito ? spawnPeDireito : spawnPeEsquerdo;
                    if (peAlvo) Instantiate(vfxPasso, peAlvo.position, vfxPasso.transform.rotation);
                    usarPeDireito = !usarPeDireito; 
                }
                break;
            case "Pulo":
                if (vfxPulo) Instantiate(vfxPulo, transform.position, vfxPulo.transform.rotation);
                break;
            case "Aterrissagem": 
                if (vfxAterrissagem) 
                {
                    if (spawnPeEsquerdo != null && spawnPeDireito != null)
                    {
                        Vector3 posicaoDosPes = (spawnPeEsquerdo.position + spawnPeDireito.position) / 2f;
                        Instantiate(vfxAterrissagem, posicaoDosPes, vfxAterrissagem.transform.rotation);
                    }
                    else
                    {
                        Instantiate(vfxAterrissagem, transform.position + new Vector3(0, -1f, 0), vfxAterrissagem.transform.rotation);
                    }
                }
                break;
            case "Ataque":
                if (vfxAtaqueComum && spawnMartelo) Instantiate(vfxAtaqueComum, spawnMartelo.position, transform.rotation);
                break;
            case "Area":
                if (vfxAtaqueAreaPrincipal) Instantiate(vfxAtaqueAreaPrincipal, transform.position, vfxAtaqueAreaPrincipal.transform.rotation);
                if (vfxAtaqueAreaSecundario) Instantiate(vfxAtaqueAreaSecundario, transform.position, vfxAtaqueAreaSecundario.transform.rotation);
                if (vfxAtaqueAreaTerciario) Instantiate(vfxAtaqueAreaTerciario, transform.position, vfxAtaqueAreaTerciario.transform.rotation);
                break;
            case "LancamentoItem": 
                if (vfxLancamentoItem) Instantiate(vfxLancamentoItem, modeloGarrafaMao.transform.position, transform.rotation);
                break;
            case "LancamentoCaixa": 
                if (vfxLancamentoCaixa) Instantiate(vfxLancamentoCaixa, transform.position, transform.rotation);
                break;
            case "DecolagemPlayer":
                if (vfxDecolagemPlayer) Instantiate(vfxDecolagemPlayer, transform.position, vfxDecolagemPlayer.transform.rotation);
                break;
        }
    }

    void HandleInventoryInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            Vector3 rayStart = cam.position + cam.forward * 0.5f;
            if (Physics.Raycast(rayStart, cam.forward, out hit, 10f, camadasAlvo))
            {
                if (hit.collider.CompareTag("bottle") || hit.collider.CompareTag("Curativo"))
                {
                    TentarColetarItem(hit.collider.tag, hit.collider.gameObject);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelecionarSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelecionarSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelecionarSlot(2);
        if (Input.GetKeyDown(KeyCode.Q)) SelecionarSlot(-1);

        if (slotSelecionado != -1 && !string.IsNullOrEmpty(slots[slotSelecionado]))
        {
            string itemAtual = slots[slotSelecionado];

            if (Input.GetMouseButton(0))
            {
                if (itemAtual == "Curativo" && !jaDisparouAnimacaoCura) SegurarCura();
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (itemAtual == "Curativo") CancelarCura();
            }

            if (Input.GetMouseButton(1))
            {
                if (itemAtual == "bottle") { mirandoArremesso = true; DesenharTrajetoria(); }
            }
            if (Input.GetMouseButtonUp(1))
            {
                if (itemAtual == "bottle")
                {
                    mirandoArremesso = false;
                    if (lineRenderer) lineRenderer.enabled = false;
                    PlayerAnim.SetTrigger("Lancar");
                }
            }
        }
    }

    void DesenharTrajetoria()
    {
        if (!lineRenderer) return;
        lineRenderer.enabled = true;
        lineRenderer.positionCount = resolucaoTrajetoria;
        Vector3 inicialPos = modeloGarrafaMao.transform.position + offsetLinha;
        Vector3 direcaoComArco = (cam.forward + Vector3.up * arcoArremesso).normalized;
        Vector3 inicialVelocidade = direcaoComArco * forcaArremessoProjetil;

        for (int i = 0; i < resolucaoTrajetoria; i++)
        {
            float t = i * 0.08f;
            Vector3 ponto = inicialPos + (inicialVelocidade * t) + (Physics.gravity * 0.5f * t * t);
            lineRenderer.SetPosition(i, ponto);
        }
    }

    public void RealizarArremesso()
    {
        if (slotSelecionado == -1) return;
        string itemNome = slots[slotSelecionado];
        GameObject prefab = (itemNome == "bottle") ? prefabGarrafaVoadora : prefabCurativoVoador;

        if (prefab != null)
        {
            SpawnVFX("LancamentoItem"); 
            Vector3 spawnPos = modeloGarrafaMao.transform.position + offsetLinha;
            GameObject projetil = Instantiate(prefab, spawnPos, Quaternion.identity);
            Rigidbody rb = projetil.GetComponent<Rigidbody>();
            if (rb != null) 
            {
                Vector3 direcaoComArco = (cam.forward + Vector3.up * arcoArremesso).normalized;
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(direcaoComArco * forcaArremessoProjetil, ForceMode.Impulse);
            }
        }
        slots[slotSelecionado] = ""; 
        AtualizarVisualItens();
    }

    void SegurarCura()
    {
        tempoSegurando += Time.deltaTime;
        barraCarga.gameObject.SetActive(true);
        barraCarga.value = tempoSegurando / tempoParaCarregar;
        if (tempoSegurando >= tempoParaCarregar)
        {
            jaDisparouAnimacaoCura = true;
            PlayerAnim.SetTrigger("Usar");
            barraCarga.gameObject.SetActive(false);
        }
    }

    void CancelarCura() { if (jaDisparouAnimacaoCura) return; tempoSegurando = 0f; barraCarga.gameObject.SetActive(false); }

    public void RealizarCura()
    {
        PlayerManager pm = GetComponent<PlayerManager>();
        if (pm != null) pm.AddLife(1);
        slots[slotSelecionado] = "";
        FinalizarCuraTotal();
    }

    void FinalizarCuraTotal() { tempoSegurando = 0f; jaDisparouAnimacaoCura = false; AtualizarVisualItens(); }

    void SelecionarSlot(int index)
    {
        if (slotSelecionado == index) slotSelecionado = -1;
        else slotSelecionado = index;
        CancelarCura();
        mirandoArremesso = false;
        if (lineRenderer) lineRenderer.enabled = false;
        AtualizarVisualItens();
    }

    void AtualizarVisualItens()
    {
        modeloMarteloMao.SetActive(false);
        modeloMarteloCostas.SetActive(false);
        modeloGarrafaMao.SetActive(false);
        modeloCurativoMao.SetActive(false);
        PlayerAnim.SetBool("SegurandoItem", false);

        if (slotSelecionado == -1) modeloMarteloMao.SetActive(true);
        else {
            modeloMarteloCostas.SetActive(true);
            string itemNoSlot = slots[slotSelecionado];
            if (!string.IsNullOrEmpty(itemNoSlot)) {
                PlayerAnim.SetBool("SegurandoItem", true);
                if (itemNoSlot == "bottle") modeloGarrafaMao.SetActive(true);
                if (itemNoSlot == "Curativo") modeloCurativoMao.SetActive(true);
            } else {
                modeloMarteloMao.SetActive(true);
                modeloMarteloCostas.SetActive(false);
                slotSelecionado = -1;
            }
        }
    }

    void TentarColetarItem(string tagItem, GameObject obj)
    {
        for (int i = 0; i < slots.Length; i++) {
            if (string.IsNullOrEmpty(slots[i])) { slots[i] = tagItem; PlayerAnim.SetTrigger("PegarItem"); Destroy(obj); return; }
        }
    }

    bool PersonagemEstaDeFrente(Vector3 pontoAlvo)
    {
        Vector3 direcaoParaAlvo = (pontoAlvo - transform.position).normalized;
        direcaoParaAlvo.y = 0; 
        float dot = Vector3.Dot(transform.forward, direcaoParaAlvo);
        return dot >= limiteAnguloCorpo; 
    }

    void DetectarObjetoPassivo()
    {
        if (travadoNaCaixa) return;
        RaycastHit hit;
        Vector3 rayStart = cam.position + cam.forward * 0.7f;
        if (Physics.Raycast(rayStart, cam.forward, out hit, 10f, camadasAlvo))
        {
            string tagAtingida = hit.collider.tag;
            bool podeDestacar = (tagAtingida == "Box" && PersonagemEstaDeFrente(hit.collider.transform.position)) || tagAtingida == "bottle" || tagAtingida == "Curativo";
            if (podeDestacar)
            {
                Renderer rend = hit.collider.GetComponentInChildren<Renderer>();
                if (ultimoRendererCaixa != null && ultimoRendererCaixa != rend) ultimoRendererCaixa.material.color = corOriginalCaixa;
                if (rend && ultimoRendererCaixa != rend) { corOriginalCaixa = rend.material.color; rend.material.color = Color.cyan; ultimoRendererCaixa = rend; }
                return;
            }
        }
        if (ultimoRendererCaixa != null) { ultimoRendererCaixa.material.color = corOriginalCaixa; ultimoRendererCaixa = null; }
    }

    void MoveLogic()
    {
        if (controller.isGrounded) PlayerAnim.SetBool("Jump", false);
        if (Input.GetButtonDown("Jump") && controller.isGrounded) 
        { 
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); 
            PlayerAnim.SetBool("Jump", true); 
            SpawnVFX("Pulo"); 
        }
        
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        targetFOV = normalFOV;
        
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
            if (controller.isGrounded)
            {
                if (Input.GetKey(KeyCode.LeftShift)) { currentSpeed = runSpeed; PlayerAnim.SetInteger("state", 2); targetFOV = runFOV; }
                else { currentSpeed = walkSpeed; PlayerAnim.SetInteger("state", 1); }
            }
            else
            {
                currentSpeed = walkSpeed;
            }
            
            controller.Move(moveDir * currentSpeed * Time.deltaTime);
        }
        else
        {
            if (controller.isGrounded)
            {
                PlayerAnim.SetInteger("state", 0);
            }
        }
    }

    void HandleAttackInput()
    {
        if (slotSelecionado != -1) return;
        if (Time.time - lastClickTime > maxComboDelay) comboStep = 0;
        if (Input.GetMouseButtonDown(0) && controller.isGrounded)
        {
            if (Input.GetMouseButton(1)) { arremessoCancelado = true; ResetarEstadoPosAtaque(); }
            else if (!PlayerAnim.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) Attack();
        }
        if (Input.GetMouseButton(1) && controller.isGrounded && !arremessoCancelado)
        {
            tempoSegurando += Time.deltaTime;
            if (travadoNaCaixa) AtualizarTrajetoriaCaixa(); else if (tempoSegurando > 0.15f) ProcessarCargaEArea();
        }
        if (Input.GetMouseButtonUp(1))
        {
            if (!arremessoCancelado)
            {
                if (tempoSegurando >= tempoParaCarregar)
                {
                    if (caixaAlvo != null) { if (alvoValido) { ataqueFoiCarregado = true; isAttacking = true; LancarCaixa(); } }
                    else if (inimigosNoRaio.Count > 0) { ataqueFoiCarregado = true; isAttacking = true; FinalizarAtaqueEmArea(); }
                }
                else if (tempoSegurando < 0.25f && controller.isGrounded) 
                { 
                    SpawnVFX("DecolagemPlayer"); 
                    ataqueFoiCarregado = false; 
                    isAttacking = true; 
                    PlayerAnim.SetTrigger("ChargeAttack"); 
                }
            }
            ResetarEstadoPosAtaque(); arremessoCancelado = false;
        }
    }

    void ResetarEstadoPosAtaque()
    {
        if (ultimoRendererCaixa != null) { ultimoRendererCaixa.material.color = corOriginalCaixa; ultimoRendererCaixa = null; }
        travadoNaCaixa = false; alvoValido = false; LimparDestaques(); tempoSegurando = 0f;
        if (barraCarga) barraCarga.gameObject.SetActive(false);
        if (objetoAlvo) objetoAlvo.SetActive(false); caixaAlvo = null;
    }

    void ProcessarCargaEArea()
    {
        if (barraCarga) { barraCarga.gameObject.SetActive(true); barraCarga.value = tempoSegurando / tempoParaCarregar; }
        RaycastHit hit; Vector3 rayStart = cam.position + cam.forward * 0.7f;
        if (Physics.Raycast(rayStart, cam.forward, out hit, 10f, camadasAlvo))
        {
            if (hit.collider.CompareTag("Box") && PersonagemEstaDeFrente(hit.collider.transform.position))
            {
                caixaAlvo = hit.collider.gameObject; travadoNaCaixa = true; 
                Renderer rendCaixa = caixaAlvo.GetComponentInChildren<Renderer>();
                if (rendCaixa) { rendCaixa.material.color = Color.red; ultimoRendererCaixa = rendCaixa; }
                AtualizarTrajetoriaCaixa(); LimparDestaques(); return;
            }
        }
        LimparDestaques();
        Collider[] alvos = Physics.OverlapSphere(transform.position, raioAtaqueArea, camadasAlvo);
        foreach (Collider col in alvos) { if (col.CompareTag("Enemy")) { Renderer rend = col.GetComponentInChildren<Renderer>(); if (rend) { rend.material.color = Color.red; inimigosNoRaio.Add(rend); } } }
    }

    void AtualizarTrajetoriaCaixa() { if (barraCarga) { barraCarga.gameObject.SetActive(true); barraCarga.value = tempoSegurando / tempoParaCarregar; } PosicionarAlvoImpacto(); }

    void PosicionarAlvoImpacto()
    {
        if (!objetoAlvo || !caixaAlvo) return;
        Vector3 startPos = caixaAlvo.transform.position + new Vector3(0, 0.5f, 0);
        Vector3 initialVelocity = (cam.forward + Vector3.up * curvaturaArremesso).normalized * forcaArremessoCaixa;
        Vector3 currentPos = startPos;
        alvoValido = false; 
        for (int i = 1; i <= 80; i++) {
            float t = i * 0.05f; Vector3 nextPos = startPos + (initialVelocity * t) + (0.5f * Physics.gravity * t * t);
            if (Physics.Linecast(currentPos, nextPos, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore)) {
                if (hit.collider.gameObject != gameObject && hit.collider.gameObject != caixaAlvo) {
                    objetoAlvo.SetActive(true); objetoAlvo.transform.position = hit.point + (hit.normal * 0.05f);
                    objetoAlvo.transform.up = hit.normal; alvoValido = true; return; 
                }
            }
            currentPos = nextPos;
        }
        objetoAlvo.SetActive(false);
    }

    void LancarCaixa() 
    { 
        PlayerAnim.SetTrigger("GroundSlam"); 
        SpawnVFX("LancamentoCaixa"); 
        Rigidbody rb = caixaAlvo.GetComponent<Rigidbody>();
        if (rb) { 
            rb.isKinematic = false; 
            Vector3 direcaoLançamento = (cam.forward + Vector3.up * curvaturaArremesso).normalized;
            rb.linearVelocity = Vector3.zero; 
            rb.AddForce(direcaoLançamento * forcaArremessoCaixa, ForceMode.Impulse); 
        } 
    }

    void FinalizarAtaqueEmArea() 
    { 
        PlayerAnim.SetTrigger("GroundSlam"); 
        SpawnVFX("Area"); 
        Collider[] alvos = Physics.OverlapSphere(transform.position, raioAtaqueArea, camadasAlvo);
        foreach (Collider col in alvos) { EnemyHealth hp = col.GetComponent<EnemyHealth>(); if (hp) hp.SerArremessado(transform.position, forcaArremesso); } 
    }

    void LimparDestaques() { foreach (Renderer rend in inimigosNoRaio) { if (rend) rend.material.color = Color.white; } inimigosNoRaio.Clear(); }

    void Attack() { lastClickTime = Time.time; comboStep++;
        if (comboStep == 1) PlayerAnim.SetTrigger("AttackTrigger"); else if (comboStep == 2) PlayerAnim.SetTrigger("AttackTrigger2");
        else if (comboStep == 3) { PlayerAnim.SetTrigger("AttackTrigger3"); comboStep = 0; } 
        SpawnVFX("Ataque"); 
    }

    public void ExecutarPulo() {
        if (ataqueFoiCarregado) { 
            SpawnVFX("DecolagemPlayer"); 
            StartCoroutine(LiberarMovimentoAtaque()); 
            return; 
        }
        PlayerAnim.SetInteger("state", 0); PlayerAnim.SetBool("Jump", true);
        velocity.y = launchForce; Vector3 impulso = transform.forward * forwardForce;
        velocity.x = impulso.x; velocity.z = impulso.z; StartCoroutine(LiberarMovimento()); 
    }

    IEnumerator LiberarMovimento() { yield return new WaitForSeconds(0.2f); isAttacking = false; }
    IEnumerator LiberarMovimentoAtaque() { yield return new WaitForSeconds(0.8f); isAttacking = false; ataqueFoiCarregado = false; }
    
    public void PlayFootstep() 
    { 
        if (audioSource && footstepSounds.Length > 0) audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]); 
        SpawnVFX("Passo"); 
    }

    public void TocarSomAtaque() { if (audioSource && somAtaque) audioSource.PlayOneShot(somAtaque); }
    public void EnableHammerDamage() { if(hammerScript) hammerScript.podeDarDano = true; }
    public void DisableHammerDamage() { if(hammerScript) hammerScript.podeDarDano = false; }
    
    void OnDrawGizmosSelected() { Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, raioAtaqueArea);
        Gizmos.color = Color.yellow; Vector3 frontal = transform.forward * 10f;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-10f, Vector3.up); Quaternion rightRayRotation = Quaternion.AngleAxis(10f, Vector3.up);
        Gizmos.DrawRay(transform.position, leftRayRotation * frontal); Gizmos.DrawRay(transform.position, rightRayRotation * frontal); }
}