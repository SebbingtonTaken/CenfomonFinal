using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    public float Dinero = 1000;

    private Vector2 input;

    private ControladorPuntos controladorPuntos;

    private Character character;
    private void Awake()
    {
        controladorPuntos = ControladorPuntos.Instancia;
        character = GetComponent<Character>();

    }

    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // remove diagonal movement
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
            StartCoroutine(Interact());
    }

    IEnumerator Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        // Debug.DrawLine(transform.position, interactPos, Color.green, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    IPlayerTriggerable currentlyInTrigger;
    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayers);

        IPlayerTriggerable triggerable = null;
        foreach (var collider in colliders)
        {
            triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                if (triggerable == currentlyInTrigger && !triggerable.TriggerRepeatedly)
                    break;

                triggerable.OnPlayerTriggered(this);
                currentlyInTrigger = triggerable;
                break;
            }
        }

        if (colliders.Count() == 0 || triggerable != currentlyInTrigger)
            currentlyInTrigger = null;
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            cenfomons = GetComponent<CenfomonParty>().Cenfomons.Select(p => p.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;

        // Restor Position
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        // Restore Party
        GetComponent<CenfomonParty>().Cenfomons = saveData.cenfomons.Select(s => new Cenfomon(s)).ToList();
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    public Character Character => character;

    public bool HasEnoughPoints(int amount)
    {
        return controladorPuntos.CantidadPuntos >= amount;
    }

    public void DeductPoints(int amount)
    {
        controladorPuntos.CantidadPuntos -= amount;
    }

    public void ApplyDecorators(IDecorator decorator)
    {

        decorator.ApplyDecorator(this);
    }
    public void ApplyMoney(int pointsToAdd)
    {
        if (Dinero < 10)
        {
            Dinero += pointsToAdd;
        }

    }
    private void LoadMoney()
    {
        Dinero = PlayerDataManager.Instance.Money;
    }

    private List<IDecorator> purchasedDecorators = new List<IDecorator>();

    public void ApplyDecorator(IDecorator decorator)
    {
        purchasedDecorators.Add(decorator);
        decorator.ApplyDecorator(this);

    }

    public void ApplySpeedDecorator(float speedBoostAmount, float duration)
    {
        character.moveSpeed += speedBoostAmount;
        StartCoroutine(RestoreSpeedAfterDuration(speedBoostAmount, duration));

    }

    private IEnumerator RestoreSpeedAfterDuration(float speedBoostAmount, float duration)
    {
        yield return new WaitForSeconds(duration);
        character.moveSpeed -= speedBoostAmount;
    }
    
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<CenfomonSaveData> cenfomons;
}

