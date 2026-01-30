using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente genérico que lida com toda a lógica de input e movimento
/// para arrastar um objeto na UI ou no mundo.
/// Requer um componente no mesmo GameObject que implemente IDraggableTarget.
/// </summary>
[RequireComponent(typeof(IDraggableTarget))]
public class Draggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [ShowInInspector, ReadOnly]
    private Vector3 dragStartPosition;

    [ShowInInspector, ReadOnly]
    private Quaternion dragStartRotation;

    private IDraggableTarget target;
    private bool isDragging = false;
    private Vector3 currentMousePosition;


    private Vector3 movementDelta;
    private Vector3 rotationDelta;

    [Title("Visual Settings")]
    [SerializeField] private float followSpeed = 30f;
    [SerializeField] private float rotationAmount = 20;
    [SerializeField] private float rotationSpeed = 20;
    [SerializeField] private float manualTiltAmount = 20;
    [SerializeField] private float tiltSpeed = 20;

    private void Awake()
    {
        target = GetComponent<IDraggableTarget>();
    }
    private void Update()
    {
        if (!isDragging) return;

        // Executa as animações visuais todo frame
        SmoothFollow();
        FollowRotation();
        CardTilt();
    }


    /// <summary>
    //  Move o objeto suavemente em direção ao mouse
    /// </summary>
    private void SmoothFollow()
    {
        Vector3 targetPos = currentMousePosition;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    private void FollowRotation()
    {
        // Calcula a inércia baseada na diferença entre onde o objeto está e para onde ele deveria ir
        Vector3 movement = (transform.position - currentMousePosition);
        movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);

        Vector3 movementRotation = movementDelta * rotationAmount;
        rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);

        // Aplica a rotação no eixo Z (inércia lateral)
        // Mantemos X e Y baseados no CardTilt para não sobrescrever
        float zRotation = Mathf.Clamp(rotationDelta.x, -60, 60);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, zRotation);
    }

    private void CardTilt()
    {
        // Inclinação baseada na posição do mouse em relação ao objeto (efeito 3D)
        Vector3 offset = transform.position - MouseUtil.GetMousePositionInWorldSpace();

        float tiltX = (offset.y * -1) * manualTiltAmount;
        float tiltY = (offset.x) * manualTiltAmount;

        // Lerp para suavizar a inclinação
        float lerpX = Mathf.LerpAngle(transform.eulerAngles.x, tiltX, tiltSpeed * Time.deltaTime);
        float lerpY = Mathf.LerpAngle(transform.eulerAngles.y, tiltY, tiltSpeed * Time.deltaTime);


        float currentZ = transform.eulerAngles.z;

        transform.eulerAngles = new Vector3(lerpX, lerpY, currentZ);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!target.CanStartDrag(eventData)) return;

        isDragging = true;
        dragStartPosition = transform.position;
        dragStartRotation = transform.rotation;

        // Prepara posição inicial
        currentMousePosition = MouseUtil.GetMousePositionInWorldSpace(-1);

        // Zera rotações anteriores para evitar "snaps" visuais
        movementDelta = Vector3.zero;
        rotationDelta = Vector3.zero;

        target.OnDragStart();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        currentMousePosition = MouseUtil.GetMousePositionInWorldSpace(-1);
        target.OnDrag();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;

        // Notifica o target que o drag terminou passando a posição/rotação
        target.OnDragEnd(dragStartPosition, dragStartRotation);
    }


}
public interface IDraggableTarget
{
    /// <summary>
    /// Chamado pelo Draggable em OnPointerDown.
    /// Pergunta se o objeto pode começar a ser arrastado.
    /// </summary>
    bool CanStartDrag(PointerEventData eventData);

    /// <summary>
    /// Chamado pelo Draggable quando o drag é confirmado e iniciado.
    /// Use para lógica de UI (ex: esconder tooltips, mudar sorting order).
    /// </summary>
    void OnDragStart();

    /// <summary>
    /// Chamado pelo Draggable todo frame durante OnDrag.
    /// </summary>
    void OnDrag();

    /// <summary>
    /// Chamado pelo Draggable em OnPointerUp.
    /// É aqui que a lógica de "soltar" (drop) acontece.
    /// </summary>
    /// <param name="startPosition">A posição original do objeto antes do drag.</param>
    /// <param name="startRotation">A rotação original do objeto antes do drag.</param>
    void OnDragEnd(Vector3 startPosition, Quaternion startRotation);
}