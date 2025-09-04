using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class Hud : CanvasLayer
{
	[Export] private Button _throwButton;
	[Export] private Node2D _diceSet; // Назначается в редакторе!
	[Export] private float _throwForce = 250f;
	[Export] private float _throwTorque = 6f;
	[Export] private float _rollDuration = 2f;
		
	private ArrayCube arrayCube = new ArrayCube();
	
	private Game game = new Game(2, new List<string> { "Алиса", "Боб" });
	private TableGame currTableGame = null;
	private TableGame [] arrTableGame = new TableGame[2];
	private ResultGrid resultGrid;
	
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
		Payer currPayer = game.GetCurrentPayer();
		arrTableGame[0] =  GetParent().GetNode<TableGame>("TableGame1");
		arrTableGame[0].SetNamePlayer(currPayer.name);
		resultGrid.SetName1(currPayer.name);
		currTableGame = arrTableGame[0];
		game.NextPayer();
		currPayer = game.GetCurrentPayer();
		arrTableGame[1] =  GetParent().GetNode<TableGame>("TableGame2");
		arrTableGame[1].SetNamePlayer(currPayer.name);
		arrTableGame[1].SetSceneDisabled(true);	
		resultGrid.SetName2(currPayer.name);
		game.NextPayer();
		currPayer = game.GetCurrentPayer();
	}
	
	//// debag
	//private void PrintTree(Node node, int depth)
	//{
		//string indent = new string(' ', depth * 2);
		//GD.Print($"{indent}{node.Name} ({node.GetType().Name})");
		//
		//foreach (Node child in node.GetChildren())
		//{
			//PrintTree(child, depth + 1);
		//}
	//}
	
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
	
	//private void PrintArr()
	//{
		//StringBuilder db = new StringBuilder("arr : ");
		//for( int i = 0; i < arrayCube.diceValues.Count; ++i){
			//db.Append(arrayCube.diceValues[i]);
			//db.Append(" ");
		//}
		//GD.Print(db.ToString());
	//}
	//
	//private void _PrintArr()
	//{
		//StringBuilder db = new StringBuilder("arrUp : ");
		//for( int i = 0; i < arrayCube.stateUp.Count; ++i){
			//db.Append(arrayCube.stateUp[i]);
			//db.Append(" ");
		//}
		//GD.Print(db.ToString());
	//}
	
	private void OnClickSymbol(Combination comb)
	{
		GD.Print("OnClickSymbol ");
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
						//MessageBox::ShowInfo(u8"Игра завершена", game.messageResult);
						//isGameStop = true;
						return;
					}
					arrayCube.Reset();
					NextTableGame(game.NextPayer());
					// Загрузка для второй таблицы забанить призы
					currPayer = game.GetCurrentPayer();
					currTableGame.FillTable(currPayer.boardGame);
				}
			}
		//	GD.Print("OnClickSymbol ", val);
		}
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
