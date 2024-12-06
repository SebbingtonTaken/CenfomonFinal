using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, Bag, PartyScreen, AboutToUse, MoveToForget, BattleOver }
public enum BattleAction { Move, SwitchCenfomon, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    public event Action<bool> OnBattleOver;

    BattleState state;

    int currentAction;
    int currentMove;
    bool aboutToUseChoice = true;

    CenfomonParty playerParty;
    CenfomonParty trainerParty;
    Cenfomon wildCenfomon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    MoveBase moveToLearn;

    public void StartBattle(CenfomonParty playerParty, Cenfomon wildCenfomon)
    {
        SFXBuilder builder = gameObject.AddComponent<SFXBuilder>();
        builder.Create(3);
        builder.AgregarTono(1);
        builder.AgregarVolumen(0.2f);
        builder.Play();

        this.playerParty = playerParty;
        this.wildCenfomon = wildCenfomon;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(CenfomonParty playerParty, CenfomonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            playerUnit.Setup(playerParty.GetHealthyCenfomon());
            enemyUnit.Setup(wildCenfomon);

            dialogBox.SetMoveNames(playerUnit.Cenfomon.Moves);
            yield return dialogBox.TypeDialog($"Un {enemyUnit.Cenfomon.Base.Name} salvaje apareció.");
        }
        else
        {

            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} quiere pelear");

            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyCenfomon = trainerParty.GetHealthyCenfomon();
            enemyUnit.Setup(enemyCenfomon);
            yield return dialogBox.TypeDialog($"{trainer.Name} envió a {enemyCenfomon.Base.Name}");

            // Send out first cenfomon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerCenfomon = playerParty.GetHealthyCenfomon();
            playerUnit.Setup(playerCenfomon);
            yield return dialogBox.TypeDialog($"Ve {playerCenfomon.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Cenfomon.Moves);
        }

        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Cenfomons.ForEach(p => p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Escoge una acción");
        dialogBox.EnableActionSelector(true);
    }

    void OpenBag()
    {
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);
    }

    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(Cenfomon newCenfomon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} va a usar {newCenfomon.Base.Name}. Quieres cambiar de cenfomon?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(Cenfomon cenfomon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Escoge un movimiento para olvidar");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(cenfomon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Cenfomon.CurrentMove = playerUnit.Cenfomon.Moves[currentMove];
            enemyUnit.Cenfomon.CurrentMove = enemyUnit.Cenfomon.GetRandomMove();

            int playerMovePriority = playerUnit.Cenfomon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Cenfomon.CurrentMove.Base.Priority;

            // Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Cenfomon.Speed >= enemyUnit.Cenfomon.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondCenfomon = secondUnit.Cenfomon;

            // First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Cenfomon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondCenfomon.HP > 0)
            {
                // Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Cenfomon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchCenfomon)
            {
                var selectedCenfomon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchCenfomon(selectedCenfomon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                // This is handled from item screen, so do nothing and skip to enemy move
                dialogBox.EnableActionSelector(false);
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            // Enemy Turn
            var enemyMove = enemyUnit.Cenfomon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {   
        bool canRunMove = sourceUnit.Cenfomon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Cenfomon);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Cenfomon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Cenfomon.Base.Name} usó {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Cenfomon, targetUnit.Cenfomon))
        {

            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();

            SFXBuilder builder = gameObject.AddComponent<SFXBuilder>();
            builder.Create(4);
            builder.AgregarTono(1);
            builder.AgregarVolumen(0.2f);
            builder.Play();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Cenfomon, targetUnit.Cenfomon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Cenfomon.TakeDamage(move, sourceUnit.Cenfomon);
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Cenfomon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Cenfomon, targetUnit.Cenfomon, secondary.Target);
                }
            }

            if (targetUnit.Cenfomon.HP <= 0)
            {
                yield return HandleCenfomonFainted(targetUnit);
            }

        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Cenfomon.Base.Name}'s attack missed");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Cenfomon source, Cenfomon target, MoveTarget moveTarget)
    {

        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);


        sourceUnit.Cenfomon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Cenfomon);
        yield return sourceUnit.Hud.WaitForHPUpdate();
        if (sourceUnit.Cenfomon.HP <= 0)
        {
            yield return HandleCenfomonFainted(sourceUnit);
    
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    bool CheckIfMoveHits(Move move, Cenfomon source, Cenfomon target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Cenfomon cenfomon)
    {
        while (cenfomon.StatusChanges.Count > 0)
        {
            var message = cenfomon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandleCenfomonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Cenfomon.Base.Name} fue derrotado");
        faintedUnit.PlayFaintAnimation();

        //sonido secreto de quack cuando muere cenfomon



        
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {

            int expYield = faintedUnit.Cenfomon.Base.ExpYield;
            int enemyLevel = faintedUnit.Cenfomon.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Cenfomon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Cenfomon.Base.Name} ganó {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();


            while (playerUnit.Cenfomon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Cenfomon.Base.Name} subió a nivel {playerUnit.Cenfomon.Level}");

                var newMove = playerUnit.Cenfomon.GetLearnableMoveAtCurrLevel();
                if (newMove != null)
                {
                    if (playerUnit.Cenfomon.Moves.Count < CenfomonBase.MaxNumOfMoves)
                    {
                        playerUnit.Cenfomon.LearnMove(newMove.Base);
                        yield return dialogBox.TypeDialog($"{playerUnit.Cenfomon.Base.Name} aprendió {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerUnit.Cenfomon.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Cenfomon.Base.Name} quiere aprender {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"Pero no puede tener más de {CenfomonBase.MaxNumOfMoves} movimientos");
                        yield return ChooseMoveToForget(playerUnit.Cenfomon, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }


            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextCenfomon = playerParty.GetHealthyCenfomon();
            if (nextCenfomon != null)
                OpenPartyScreen();
            else
                BattleOver(false);
        }
        else
        {
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextCenfomon = trainerParty.GetHealthyCenfomon();
                if (nextCenfomon != null)
                    StartCoroutine(AboutToUse(nextCenfomon));
                else
                    BattleOver(true);
            }
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("Golpe crítico!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("Es super efectivo!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("No es muy efectivo!");
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };

            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
            };

            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == CenfomonBase.MaxNumOfMoves)
                {

                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Cenfomon.Base.Name} no aprendió {moveToLearn.Name}"));
                }
                else
                {

                    var selectedMove = playerUnit.Cenfomon.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Cenfomon.Base.Name} olvídó {selectedMove.Name} y aprendió {moveToLearn.Name}"));

                    playerUnit.Cenfomon.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {

                MoveSelection();
            }
            else if (currentAction == 1)
            {

                OpenBag();
            }
            else if (currentAction == 2)
            {

                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {

                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Cenfomon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Cenfomon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Cenfomon.Moves[currentMove];
            if (move.PP == 0) return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("No puedes enviar a un cenfomon derrotado");
                return;
            }
            if (selectedMember == playerUnit.Cenfomon)
            {
                partyScreen.SetMessageText("No puedes cambiar con el mismo cenfomon");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchCenfomon));
            }
            else
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchCenfomon(selectedMember, isTrainerAboutToUse));
            }

            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Cenfomon.HP <= 0)
            {
                partyScreen.SetMessageText("Tienes que escoger un cenfomon para continuar");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerCenfomon());
            }
            else
                ActionSelection();

            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelected, onBack);
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                // Si
                OpenPartyScreen();
            }
            else
            {
                // No
                StartCoroutine(SendNextTrainerCenfomon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerCenfomon());
        }
    }

    IEnumerator SwitchCenfomon(Cenfomon newCenfomon, bool isTrainerAboutToUse = false)
    {
        if (playerUnit.Cenfomon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Vuelve {playerUnit.Cenfomon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newCenfomon);
        dialogBox.SetMoveNames(newCenfomon.Moves);
        yield return dialogBox.TypeDialog($"Ve {newCenfomon.Base.Name}!");

        if (isTrainerAboutToUse)
            StartCoroutine(SendNextTrainerCenfomon());
        else
            state = BattleState.RunningTurn;
    }

    IEnumerator SendNextTrainerCenfomon()
    {
        state = BattleState.Busy;

        var nextCenfomon = trainerParty.GetHealthyCenfomon();
        enemyUnit.Setup(nextCenfomon);
        yield return dialogBox.TypeDialog($"{trainer.Name} envió a {nextCenfomon.Base.Name}!");

        state = BattleState.RunningTurn;
    }

    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        if (usedItem is PokeballItem)
        {
            yield return ThrowPokeball((PokeballItem)usedItem);
        }

        StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"No puedes robarle cenfomones a otros entrenadores!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} usó {pokeballItem.Name.ToUpper()}!");

        var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();
        pokeball.sprite = pokeballItem.Icon;

        // Animations
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1.3f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchCenfomon(enemyUnit.Cenfomon, pokeballItem);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            // Cenfomon is caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Cenfomon.Base.Name} fue capturado");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddCenfomon(enemyUnit.Cenfomon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Cenfomon.Base.Name} ha sido añadido a tu equipo");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            // Cenfomon broke out
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.Cenfomon.Base.Name} escapó");
            else
                yield return dialogBox.TypeDialog($"Casi!");

            Destroy(pokeball);
            state = BattleState.RunningTurn;
        }
    }

    int TryToCatchCenfomon(Cenfomon cenfomon, PokeballItem pokeballItem)
    {
        float a = (3 * cenfomon.MaxHp - 2 * cenfomon.HP) * cenfomon.Base.CatchRate * pokeballItem.CatchRateModifier * ConditionsDB.GetStatusBonus(cenfomon.Status) / (3 * cenfomon.MaxHp);

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++shakeCount;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"No puedes escapar de un entrenador!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Cenfomon.Speed;
        int enemySpeed = enemyUnit.Cenfomon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Escapaste a salvo!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Escapaste a salvo!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"No puedes escapar!");
                state = BattleState.RunningTurn;
            }
        }
    }
}
