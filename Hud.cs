using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks; 

public interface INetWorkCtrl
{
	Task SendMessge(string msg);
	//string ReciveMessge();
}

public partial class Hud : CanvasLayer
{
	[Export] private Button _throwButton;
	[Export] private Node2D _diceSet; // Назначается в редакторе!
	[Export] private float _throwForce = 250f;
	[Export] private float _throwTorque = 6f;
	[Export] private float _rollDuration = 2f;
		
	private ArrayCube arrayCube = new ArrayCube();
	
	private Game game = new Game(2, new List<string> { "Игрок 1", "Игрок 2" });
	private TableGame currTableGame = null;
	private TableGame [] arrTableGame = new TableGame[2];
	private ResultGrid resultGrid;
	
	private INetWorkCtrl netWorkCtrl = null;
	
	Dictionary<Combination, ScoreCalculator> calculators = new Dictionary<Combination, ScoreCalculator>
	{
		{ Combination.ONE, new ScoolCalculator() },
		{ Combination.TWO, new ScoolCalculator() },
		{ Combination.THREE, new ScoolCalculator() },
		{ Combination.FOUR, new ScoolCalculator() },
		{ Combination.FIVE, new ScoolCalculator() },
		{ Combination.SIX, new ScoolCalculator() },
		{ Combination.PAIR, new PairCalculator() },
		{ Combination.TWO_PAIRS, new TwoPairCalculator() },
		{ Combination.THREE_OF_A_KIND, new ThreeOfAKindCalculator() },
		{ Combination.FULL_HOUSE, new FullHouseCalculator() },
		{ Combination.STREET_SMALL, new StreetSmallCalculator() },
		{ Combination.STREET_BIG, new StreetBigCalculator() },
		{ Combination.FOUR_OF_A_KIND, new FourOfAKindCalculator() },
		{ Combination.FIVE_OF_A_KIND, new FiveOfAKindCalculator() },
		{ Combination.SUM_OF_A_KIND, new SumKindCalculator() }
	};
	
	public void SetNetWorkCtrl(INetWorkCtrl _netWorkCtrl){
		this.netWorkCtrl = _netWorkCtrl;
	}
	
	public override void _Ready()
	{		
		_throwButton.Pressed += OnThrowButtonPressed;
		foreach (Node node in _diceSet.GetChildren())
		{
			if (node is Dice dice)
			{
				dice._SetDiceEnabled += OnSetDiceEnabled;
			}
		}		
		resultGrid = GetNode<ResultGrid>("ResultGrid");

	}
	
	public void OnFirstButtonPressed(string firstText, string secondText)
	{
		GD.Print($"Игроки: {firstText}, {secondText}");		
		// Устанавливаем имена игроков
		if (game.vPaers.Count >= 2)
		{
			game.vPaers[0].name = string.IsNullOrEmpty(firstText) ? "Игрок 1" : firstText;
			game.vPaers[1].name = string.IsNullOrEmpty(secondText) ? "Игрок 2" : secondText;
		}		
		// Инициализируем UI с новыми именами
		InitializeGameUI();
		GD.Print("Игра началась!");
	}

	public void OnSecondButtonPressed()
	{
		GD.Print("Ввод имен отменен");		
		// Инициализируем UI с именами по умолчанию
		InitializeGameUI();
	}

	// Новый метод для инициализации UI после ввода имен
	private void InitializeGameUI()
	{
		Payer currPayer = game.GetCurrentPayer();	
		// Инициализируем таблицы игроков
		arrTableGame[0] = GetNode<TableGame>("TableGame1");
		arrTableGame[0].SetNamePlayer(game.vPaers[0].name);
		resultGrid.SetName1(game.vPaers[0].name);
		
		currTableGame = arrTableGame[0];
		
		game.NextPayer();
		currPayer = game.GetCurrentPayer();
		
		arrTableGame[1] = GetNode<TableGame>("TableGame2");
		arrTableGame[1].SetNamePlayer(game.vPaers[1].name);
		arrTableGame[1].SetSceneDisabled(true);    
		resultGrid.SetName2(game.vPaers[1].name);
		
		game.NextPayer();
		currPayer = game.GetCurrentPayer();
	}
		
	private void OnThrowButtonPressed()
	{
		if( game.currStep >=  Game.N_STEP) return ;
		arrayCube.SetDiceRandom();
		// Отключаем кнопку на время броска
		_throwButton.Disabled = true;
		game.currStep++;
		GetNode<Label>("Label").Text = game.currStep.ToString();
	
		// Запускаем бросок всех кубиков		
		ThrowAllDice();
		// Включаем кнопку через время
		GetTree().CreateTimer(_rollDuration + 0.5f).Timeout += () => 
		{
			_throwButton.Disabled = false;
		};
	}
	
	private void ThrowAllDice()
	{
		int index = 0;
		foreach (Node node in _diceSet.GetChildren())
		{
			if (node is RigidBody2D dice)
			{
				if( !arrayCube.IsStateUp(index) ){
					int val = arrayCube[index];
					ThrowSingleDice(dice, val);
				}
				index++;
			}
		}
	}
	
	private void ThrowSingleDice(RigidBody2D dice, int val)
	{
		// Разбудить кубик
		dice.Sleeping = false;		
		dice.LinearVelocity = Vector2.Zero;
		dice.AngularVelocity = 1200f;	
		
		// Устанавливаем случайное значение через время
		GetTree().CreateTimer(_rollDuration).Timeout += () => 
		{
			SetRandomDiceValue(dice, val);
		};
	}
	
	private void SetRandomDiceValue(RigidBody2D dice, int randomValue)
	{
		// Останавливаем кубик
		dice.LinearVelocity = Vector2.Zero;
		dice.AngularVelocity = 0f;
		dice.Rotation = 0f;
		dice.Sleeping = true;
		
		// Если у кубика есть скрипт с методом SetValue
		if (dice.HasMethod("SetValue"))
		{
			dice.Call("SetValue", randomValue);
		}
		//GD.Print("Кубик остановился на значении: ", randomValue);
	}
	
	private void OnSetDiceEnabled(int index, bool newVal)
	{
		//GD.Print("OnSetDiceEnabled ", index, " ", newVal);
		if( arrayCube!=null )
			arrayCube.SetStateUp(index, !newVal);
	}
	
	
	private async void OnClickSymbol(Combination comb)
	{
	//	GD.Print("OnClickSymbol ");
		if (calculators.TryGetValue(comb, out ScoreCalculator calculator))
		{
		//	GD.Print("OnClickSymbol ", comb);
			if( calculator is ScoolCalculator ) {
				((ScoolCalculator)calculator).SetKey(comb);
			}
			int val = calculator.Calculate(arrayCube.diceValues);
			GD.Print("OnClickSymbol val", val);
			if( game.SetValue(val, calculator.symbol)){			
			//	GD.Print("OnClickSymbol name: ", currPayer.name);
				if (currTableGame != null)
				{		
					Payer currPayer = game.GetCurrentPayer();			
					currTableGame.FillTable(currPayer.boardGame);
					resultGrid.SetValue(currPayer.name, currPayer.boardGame.sumTotal, currPayer.boardGame.sumScool);
					if (game.IsGameOver()) {
				//// Вывести сообщение о победителе и выйти из цикла игры 					
						ShowAcceptDialog("Игра окончена", game.messageResult);
						return;
					}
					arrayCube.Reset();
					NextTableGame(game.NextPayer());
					// Загрузка для второй таблицы забанить призы
					currPayer = game.GetCurrentPayer();
					currTableGame.FillTable(currPayer.boardGame);
					
					if( netWorkCtrl!= null )
						await netWorkCtrl.SendMessge($"next game {val}");
				}
			}
		//	GD.Print("OnClickSymbol ", val);
		}
	}
	
	 public void ShowAcceptDialog(string title, string message)
	{
		var dialog = new AcceptDialog();
		dialog.Title = title;
		dialog.DialogText = message;
		
		GetTree().Root.AddChild(dialog);
		dialog.PopupCentered();
		//
		//dialog.Confirmed += () => GD.Print("Диалог подтвержден");
		//dialog.Canceled += () => GD.Print("Диалог отменен");
	}
	
	private void ResetDiceSet()
	{
		int index_dice = 0;
		foreach (Node node in _diceSet.GetChildren())
		{
			if (node is Dice dice)
			{
				dice.SetDiceEnabledView(true);
				dice.Highlight(false);
				dice.IsEnabled = true;
				int val = arrayCube[index_dice];
				dice.SetValue(val);
				index_dice++;
			}
		}
	}
	
	private void NextTableGame(int index){
		currTableGame.SetSceneDisabled(true);
		currTableGame = arrTableGame[index];
		currTableGame.SetSceneDisabled(false);
		GetNode<Label>("Label").Text = game.currStep.ToString();
		ResetDiceSet();
	}
}
