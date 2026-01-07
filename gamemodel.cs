using System;
using System.Collections.Generic;
using System.Linq;


public class CombinationAttribute : Attribute
{
	public string Key { get; }
	
	public CombinationAttribute(string key)
	{
		Key = key;
	}
}

public enum Combination
{
	[Combination("1")] ONE = 1,
	[Combination("2")] TWO = 2,
	[Combination("3")] THREE = 3,
	[Combination("4")] FOUR = 4,
	[Combination("5")] FIVE = 5,
	[Combination("6")] SIX = 6,
	[Combination("P")] PAIR,
	[Combination("D")] TWO_PAIRS,
	[Combination("T")] THREE_OF_A_KIND,
	[Combination("F")] FULL_HOUSE,
	[Combination("e")] STREET_SMALL,
	[Combination("S")] STREET_BIG,
	[Combination("C")] FOUR_OF_A_KIND,
	[Combination("A")] FIVE_OF_A_KIND,
	[Combination("Sm")] SUM_OF_A_KIND
}

public static class CombinationExtensions
{
	public static string GetKey(this Combination combination)
	{
		var field = combination.GetType().GetField(combination.ToString());
		var attribute = field?.GetCustomAttributes(typeof(CombinationAttribute), false)
			.FirstOrDefault() as CombinationAttribute;
		
		return attribute?.Key ?? combination.ToString();
	}

	public static Combination? FromKey(string key)
	{
		foreach (Combination value in Enum.GetValues(typeof(Combination)))
		{
			if (value.GetKey() == key)
				return value;
		}
		return null;
	}
}

public interface IBoardGameObserver
{
	void UpdatePrizeRow(int row);
	void UpdatePrizeColumn(int column);
}

public interface IBoardGame
{
	int[,] GetArr();
	int GetItemArr(int r, int c);
}

public class BoardGame : IBoardGameObserver, IBoardGame
{
	public const int N_ROW = 16;
	public const int N_COLUMN = 6;
	public const int PENALTY = 100;
	public int sumScool = 0;
	public int sumTotal = 0;
	public int[,] arr = new int[N_ROW, N_COLUMN];
	
	private IBoardGameObserver boardGameObserver = null;

	public BoardGame()
	{
		// Инициализация массива нулями
		for (int i = 0; i < N_ROW; i++)
		{
			for (int j = 0; j < N_COLUMN; j++)
			{
				arr[i, j] = 0;
			}
		}
	}
	
	public int[,] GetArr(){
		return arr;
	}
	
	public int GetItemArr(int r, int c)
	{
		if( r >= 0 && r < N_ROW && c >= 0 && c < N_COLUMN)
			return arr[r, c];
		return 	int.MaxValue;
	}

	public void SetSumScool(int val)
	{
		if (val == int.MaxValue) return;
		sumScool += val;
		sumTotal += val;
	}

	public void SetTotalSum(int val)
	{
		if (val == int.MaxValue) return;
		sumTotal += val;
	}

	public bool SetValue(int val, Combination symb)
	{
		if (val == 0) val = int.MaxValue;
		int row = (int)symb - 1;
		
		if (Combination.ONE <= symb && symb <= Combination.SIX)
		{
			if (val < 0 && -1 * val > sumScool)
			{
				bool isFillSymbol = IsFillSymbolBoard();
				if (!isFillSymbol) return false;
				val *= PENALTY; // умножаем на величину штрафа
			}
			return SetScoolCeil(val, row);
		}
		return SetCeil(val, row);
	}

	public bool SetScoolCeil(int val, int row)
	{
		if (row < 0 || row > 6) return false;
		int i = 0;
		
		while (i < N_COLUMN - 1 && arr[row, i] != 0)
			i++;
			
		if (i == N_COLUMN - 1) return false;
		
		arr[row, i] = val;
		SetSumScool(val);
		
		if (val < 0 && arr[row, N_COLUMN - 1] == 0)
			SetRowPrize(row, int.MaxValue);
			
		if (i == N_COLUMN - 2)
		{
			if (arr[row, N_COLUMN - 1] == 0)
			{
				int sum = GetSumPrizRowScool(row);
				if (sum == 0) sum = row + 1;
				SetRowPrize(row, sum);
				SetTotalSum(sum);
			}
			boardGameObserver?.UpdatePrizeRow(row);
		}
		
		int max = GetSumPrizeColumn(i);
		if (max != 0 && arr[N_ROW - 1, i] == 0)
		{
			SetColumnPrize(i, max);
			SetTotalSum(max);
		}
		
		if (max != 0)
			boardGameObserver?.UpdatePrizeColumn(i);
			
		return true;
	}

	public int GetMaxForRow(int r)
	{
		int max = arr[r, 0];
		for (int i = 0; i < N_COLUMN - 1; ++i)
			if (max < arr[r, i]) max = arr[r, i];
		return max;
	}

	public int GetSumPrizeColumn(int col)
	{
		int max = 0;
		int i = 0;
		
		for (; i < 6; ++i)
		{
			if (arr[i, col] == 0) break;
			if (max < arr[i, col] && arr[i, col] != int.MaxValue) max = arr[i, col];
		}
		
		if (i < 6) return 0;
		
		for (; i < N_ROW - 1; ++i)
		{
			if (arr[i, col] == 0) break;
			if (max < arr[i, col] && arr[i, col] != int.MaxValue) max = arr[i, col];
		}
		
		if (i < N_ROW - 1) return 0;
		return max;
	}

	public int GetSumPrizRowScool(int row)
	{
		int sum = 0;
		for (int i = 0; i < N_COLUMN - 1; ++i)
			if (arr[row, i] != int.MaxValue) sum += arr[row, i];
		return sum;
	}

	public bool SetCeil(int val, int row)
	{
		int i = 0;
		
		while (i < N_COLUMN - 1 && arr[row, i] != 0)
			i++;
			
		if (i == N_COLUMN - 1) return false;
		
		arr[row, i] = val;
		SetTotalSum(val);
		
		if (val == int.MaxValue)
		{
			SetRowPrize(row, int.MaxValue);
			SetColumnPrize(i, int.MaxValue);
		}
		
		if (i == N_COLUMN - 2)
		{
			if (arr[row, N_COLUMN - 1] == 0)
			{
				int max = GetMaxForRow(row);
				SetRowPrize(row, max);
				SetTotalSum(max);
			}
			boardGameObserver?.UpdatePrizeRow(row);
		}
		
		int maxCol = GetSumPrizeColumn(i);
		if (maxCol != 0 && arr[N_ROW - 1, i] == 0)
		{
			SetColumnPrize(i, maxCol);
			SetTotalSum(maxCol);
		}
		
		if (maxCol != 0)
			boardGameObserver?.UpdatePrizeColumn(i);
			
		return true;
	}

	public bool IsFillSymbolBoard()
	{
		for (int i = 6; i < N_ROW - 1; ++i)
			if (arr[i, N_COLUMN - 2] == 0) return false;
		return true;
	}

	public bool IsFillBoard()
	{
		for (int i = 0; i < N_ROW - 1; ++i)
			if (arr[i, N_COLUMN - 2] == 0) return false;
		return true;
	}

	public void SetColumnPrize(int col, int val)
	{
		arr[N_ROW - 1, col] = val;
	}

	public void SetRowPrize(int row, int val)
	{
		arr[row, N_COLUMN - 1] = val;
	}

	public void UpdatePrizeRow(int row)
	{
		if (arr[row, N_COLUMN - 1] == 0)
			SetRowPrize(row, int.MaxValue);
	}

	public void UpdatePrizeColumn(int col)
	{
		if (arr[N_ROW - 1, col] == 0)
			SetColumnPrize(col, int.MaxValue);
	}

	public void SetBoardGameObserver(IBoardGameObserver observer)
	{
		this.boardGameObserver = observer;
	}
}

public class Payer
{
	public string name;
	public BoardGame boardGame;
	
	public Payer(string _name)
	{
		name = _name;
		boardGame = new BoardGame();
	}
	
	public bool SetValue(int val, Combination symb)
	{
		return boardGame.SetValue(val, symb);
	}
	
	public bool IsAllFillField()
	{
		return boardGame.IsFillBoard();
	}
	
	public string GetNameToString()
	{
		return name;
	}
}

public class Game
{
	public List<Payer> vPaers = new List<Payer>();
	public const int N_STEP = 3;
	public int currStep = 0;
	public string messageResult = "";
	private int index = 0;
	
	public Game(int nPlayers, List<string> names)
	{
		foreach (var item in names)
		{
			vPaers.Add(new Payer(item));
		}
		
		vPaers[0].boardGame.SetBoardGameObserver(vPaers[1].boardGame);
		vPaers[1].boardGame.SetBoardGameObserver(vPaers[0].boardGame);
	}
	
	public Payer GetCurrentPayer()
	{
		return vPaers[index];
	}
	
	public int NextPayer()
	{
		index++;
		index %= vPaers.Count;
		currStep = 0;
		return index;
	}
	
	public bool SetValue(ref int val, Combination symb)
	{
		if (currStep == 1) val *= 2; // удваиваем, если с первого раза выпала комбинация
		return vPaers[index].SetValue(val, symb);
	}
	
	public bool IsGameOver()
	{
		foreach (var item in vPaers)
		{
			if (!item.IsAllFillField()) return false;
		}		
		FormResultGame();
		return true;
	}
	
	private void FormResultGame()
	{
		int i_max = 0;
		for (int i = 0; i < vPaers.Count; ++i)
		{
			if (vPaers[i].boardGame.sumTotal > vPaers[i_max].boardGame.sumTotal)
				i_max = i;
		}		
		messageResult = "Выиграл игрок " + vPaers[i_max].GetNameToString();
		messageResult += " со счетом " + vPaers[i_max].boardGame.sumTotal;
	}
}

public class ArrayCube
{
	public const int N_COUNT_DICE = 5;
	public List<int> diceValues;
	public List<int> stateUp;
	public int highlightedDice;
	private Random random;
	
	public ArrayCube()
	{
		highlightedDice = 0;
		random = new Random(DateTime.Now.Millisecond);
		diceValues = new List<int>(new int[N_COUNT_DICE]);
		stateUp = new List<int>(new int[N_COUNT_DICE]);
		
		for (int i = 0; i < N_COUNT_DICE; i++)
		{
			diceValues[i] = 1;
			stateUp[i] = 0;
		}
	}
	
	public int this[int index]
	{
		get{
			if (index < 0 || index >= diceValues.Count)
				throw new IndexOutOfRangeException();
			return diceValues[index];
		}
	}
	
	public bool IsStateUp(int index){
		if (index < 0 || index >= stateUp.Count)
				return true;
		return stateUp[index] == 1;
	}
	
	public void SetStateUp(int index, bool val){
		if (index < 0 || index >= stateUp.Count)
				return ;
		stateUp[index] = val ? 1:0;
	}
	
	public void SetDiceRandom()
	{
		for (int i = 0; i < N_COUNT_DICE; ++i)
		{
			if (stateUp[i] == 0)
				diceValues[i] = rollDice();
		}
	}
	
	public int NextSelect()
	{
		highlightedDice = (highlightedDice + 1) % N_COUNT_DICE;
		return highlightedDice;
	}
	
	public int PrevSelect()
	{
		highlightedDice = (highlightedDice - 1 + N_COUNT_DICE) % N_COUNT_DICE;
		return highlightedDice;
	}
	
	public void SetUp()
	{
		stateUp[highlightedDice] = 1;
	}
	
	public void SetDown()
	{
		stateUp[highlightedDice] = 0;
	}
	
	public void Reset()
	{
		for (int i = 0; i < stateUp.Count; ++i)
		{
			stateUp[i] = 0;
			diceValues[i] = i + 1;
		}
		highlightedDice = 0;
	}
	
	private int rollDice()
	{
		return random.Next(1, 7);
	}
	
	
}

public abstract class ScoreCalculator
{
	public Combination symbol;
	public abstract int Calculate(List<int> dice);
}

public class ScoolCalculator : ScoreCalculator
{
	public ScoolCalculator()
	{
		symbol = Combination.ONE;
	}
	
	public void SetKey(Combination key)
	{
		symbol = key;
	}
	
	public override int Calculate(List<int> dice)
	{
		int keyDice = (int)symbol;
		int sm = 0;
		for (int i = 0; i < dice.Count; ++i)
			if (dice[i] == keyDice) sm++;
		return (sm - 3) * keyDice;
	}
}

public class PairCalculator : ScoreCalculator
{
	public PairCalculator()
	{
		symbol = Combination.PAIR;
	}
	
	public override int Calculate(List<int> dice)
	{
		// Сортировка в порядке убывания
		List<int> _dice= dice.OrderByDescending(x => x).ToList();
		for (int i = 0; i < _dice.Count - 1; ++i)
		{
			if (_dice[i] == _dice[i + 1])
			{
				return 2 * _dice[i];
			}
		}
		return 0;
	}
}

public class TwoPairCalculator : ScoreCalculator
{
	public TwoPairCalculator()
	{
		symbol = Combination.TWO_PAIRS;
	}
	
	public override int Calculate(List<int> dice)
	{
		// Сортировка в порядке убывания
		List<int> _dice= dice.OrderByDescending(x => x).ToList();
		int sum = 0;
		int i = 0;
		while (i < _dice.Count - 1)
		{
			if (_dice[i] == _dice[i + 1])
			{
				sum += 2 * _dice[i];
				i++;
			}
			i++;
		}
		return sum;
	}
}

public class FullHouseCalculator : ScoreCalculator
{
	public FullHouseCalculator()
	{
		symbol = Combination.FULL_HOUSE;
	}
	
	public override int Calculate(List<int> dice)
	{
		if (dice.Count < 5) return 0;
		// Сортировка в порядке убывания
		List<int> _dice= dice.OrderByDescending(x => x).ToList();
		if (_dice[0] == _dice[1] && _dice[2] == _dice[4])
			return 2 * _dice[0] + 3 * _dice[2];
		if (_dice[0] == _dice[2] && _dice[3] == _dice[4])
			return 3 * _dice[0] + 2 * _dice[3];
		return 0;
	}
}

public class StreetSmallCalculator : ScoreCalculator
{
	public StreetSmallCalculator()
	{
		symbol = Combination.STREET_SMALL;
	}
	
	public override int Calculate(List<int> dice)
	{
		// Сортировка в порядке возрастания
		List<int> _dice = dice.OrderBy(x => x).ToList();
		int sum = 0;
		for (int i = 0; i < _dice.Count; ++i)
		{
			if (_dice[i] != i + 1) return 0;
			sum += _dice[i];
		}
		return sum;
	}
}

public class StreetBigCalculator : ScoreCalculator
{
	public StreetBigCalculator()
	{
		symbol = Combination.STREET_BIG;
	}
	
	public override int Calculate(List<int> dice)
	{
		// Сортировка в порядке возрастания
		List<int> _dice = dice.OrderBy(x => x).ToList();
		int sum = 0;
		for (int i = 0; i < _dice.Count; ++i)
		{
			if (_dice[i] != i + 2) return 0;
			sum += _dice[i];
		}
		return sum;
	}
}

public class ThreeOfAKindCalculator : ScoreCalculator
{
	public ThreeOfAKindCalculator()
	{
		symbol = Combination.THREE_OF_A_KIND;
	}
	
	public override int Calculate(List<int> dice)
	{
		// Сортировка в порядке убывания
		List<int> _dice= dice.OrderByDescending(x => x).ToList();
		for (int i = 0; i < _dice.Count - 2; ++i)
		{
			if (_dice[i] == _dice[i + 1] && _dice[i] == _dice[i + 2])
			{
				return 3 * _dice[i];
			}
		}
		return 0;
	}
}

public class FourOfAKindCalculator : ScoreCalculator
{
	public FourOfAKindCalculator()
	{
		symbol = Combination.FOUR_OF_A_KIND;
	}
	
	public override int Calculate(List<int> dice)
	{
		const int N_FOUR_ADD = 20;
		// Сортировка в порядке убывания
		List<int> _dice= dice.OrderByDescending(x => x).ToList();
		int n = _dice.Count;
		if (_dice[0] == _dice[n - 2])
			return (n - 1) * _dice[0] + N_FOUR_ADD;
		if (_dice[1] == _dice[n - 1])
			return (n - 1) * _dice[1] + N_FOUR_ADD;
		return 0;
	}
}

public class FiveOfAKindCalculator : ScoreCalculator
{
	public FiveOfAKindCalculator()
	{
		symbol = Combination.FIVE_OF_A_KIND;
	}
	
	public override int Calculate(List<int> dice)
	{
		const int N_FIVE_ADD = 50;
		// Сортировка в порядке убывания
		List<int> _dice= dice.OrderByDescending(x => x).ToList();
		int n = _dice.Count;
		return _dice[0] == _dice[n - 1] ? (n) * _dice[0] + N_FIVE_ADD : 0;
	}
}

public class SumKindCalculator : ScoreCalculator
{
	public SumKindCalculator()
	{
		symbol = Combination.SUM_OF_A_KIND;
	}
	
	public override int Calculate(List<int> dice)
	{
		int sum = 0;
		for (int i = 0; i < dice.Count; ++i)
			sum += dice[i];
		return sum;
	}
}
