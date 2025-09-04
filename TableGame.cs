using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class TableGame : Control
{
	[Signal]
	public delegate void ClickSymbolEventHandler(Combination comb);	
	private LabelGrid labelGrid = new LabelGrid(BoardGame.N_ROW, BoardGame.N_COLUMN );
	//private Dictionary<Key, string> k_map = new Dictionary<Key, string>(){
		//{Key.Key1, "1"},
		//{Key.Key2, "2"},
		//{Key.Key3, "3"},
		//{Key.Key4, "4"},
		//{Key.Key5, "5"},
		//{Key.Key6, "6"},
		//{Key.P, "P"}
	//};


	public override void _Ready()
	{
	//	GD.Print("_Ready ");
		// Прямой доступ к GridContainer
		GridContainer grid = GetNode<GridContainer>("GridContainer");
		
		InitializerLabelGrid initializerLabelGrid = new InitializerLabelGrid(labelGrid);
		// Подключаем обработчики ко всем детям GridContainer
		int cn = 0;
		foreach (Node child in grid.GetChildren())
		{
			if (child is Label label)
			{
				cn++;
				if( !string.IsNullOrEmpty(label.Text) ){
					label.GuiInput += (inputEvent) => OnGuiInput(label, inputEvent);
					label.MouseFilter = Control.MouseFilterEnum.Stop;
					//GD.Print($"Connected handler to: {label.Text}");
				}
				else{
					// Формирование массива из остальных элементов
				//	GD.Print("_Ready ", cn);
					initializerLabelGrid.AddInitLabel(label);
				//	GD.Print($"Added to grid: {label.Name}");
				}
			}
		}
		//GD.Print($"_Ready End. Processed {cn} labels");
		//GD.Print($"LabelGrid initialized: {labelGrid.RowCount}x{labelGrid.ColumnCount}");
	}
	public void FillTable(IBoardGame boardGame)
	{
		if (boardGame == null || labelGrid == null) return;     
		int[,] arr = boardGame.GetArr();
		for (int i = 0; i < arr.GetLength(0); i++)
		{
			for (int j = 0; j < arr.GetLength(1); j++)
			{
				if (labelGrid[i, j] != null)
				{
					if(  arr[i, j] == int.MaxValue )
					{
						SetSymbolBlock(labelGrid[i, j]);
					}
					else if ( arr[i, j]!= 0 )
					{
						labelGrid[i, j].Text = arr[i, j].ToString();
					}
				}
			}
		}
	}
	
	private void SetSymbolBlock(Label label)
	{
		var styleBox = new StyleBoxTexture();
		styleBox.Texture = GD.Load<Texture2D>("res://image/free-icon-rejected.png");
		label.AddThemeStyleboxOverride("normal", styleBox);
	} 
	
	public void SetNamePlayer(string name)
	{
		Label namePayer = GetNode<Label>("NamePlayr");
		namePayer.Text = name;
	}
	
	public void OnGuiInput(Label sourceLabel, InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && 
			mouseEvent.DoubleClick && mouseEvent.ButtonIndex == MouseButton.Left)
		{	
			ProcessLabelClick(sourceLabel);
		}
		 //// Обработка клавиатуры
		//else if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		//{
			//GD.Print($"OnGuiInput key");
			//HandleKeyboardInput(sourceLabel, keyEvent);
		//}
	}
	
	//private void HandleKeyboardInput(Label sourceLabel, InputEventKey keyEvent)
	//{
		//// Enter или Space для активации
		//if ( k_map.ContainsKey(keyEvent.Keycode) ){
			//if( k_map[keyEvent.Keycode] == sourceLabel.Text){
				//ProcessLabelClick(sourceLabel);
				//GetViewport().SetInputAsHandled();
			//}
		//}
	//}
	
	private void ProcessLabelClick(Label sourceLabel)
	{
		if (sourceLabel != null && !string.IsNullOrEmpty(sourceLabel.Text))
		{
			var combination = CombinationExtensions.FromKey(sourceLabel.Text); 
			if (combination.HasValue)
			{
				GD.Print("OnGuiInput ", combination.Value);
				EmitSignal(SignalName.ClickSymbol, (int)combination.Value);
			}
		}
	}
	
	public void SetSceneDisabled(bool disabled)
	{
		DisableAllControls(this, disabled);
	}

	private void DisableAllControls(Node node, bool disabled)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is Control control)
			{
				// Обрабатываем разные типы Control
				if (control is BaseButton baseButton)
				{
					baseButton.Disabled = disabled;
				}
				else if (control is LineEdit lineEdit)
				{
					lineEdit.Editable = !disabled;
				}
				else if (control is TextEdit textEdit)
				{
					textEdit.Editable = !disabled;
				}
				else if (control is Slider slider)
				{
					slider.Editable = !disabled;
				}
				else
				{
					control.MouseFilter = disabled ? 
						Control.MouseFilterEnum.Ignore : 
						Control.MouseFilterEnum.Stop;
				}

				// Визуальное обозначение
				control.Modulate = disabled ? 
					new Color(1, 1, 1, 0.5f) : 
					new Color(1, 1, 1, 1f);
			}

			// Рекурсивно обходим детей
			if (child.GetChildCount() > 0)
			{
				DisableAllControls(child, disabled);
			}
		}
	}	
	
	public class LabelGrid
	{
		private Label[,] grid;
		
		public LabelGrid(int rows, int columns)
		{
			grid = new Label[rows, columns];
		}
		
		public Label this[int row, int col]
		{
			get 
			{
				if (row < 0 || row >= grid.GetLength(0) || col < 0 || col >= grid.GetLength(1))
					return null;
				return grid[row, col];
			}
			set 
			{
				if (row >= 0 && row < grid.GetLength(0) && col >= 0 && col < grid.GetLength(1))
					grid[row, col] = value;
			}
		}
		
		public int RowCount => grid?.GetLength(0) ?? 0;
		public int ColumnCount => grid?.GetLength(1) ?? 0;
		public int TotalCount => grid?.Length ?? 0;
	}

	public class InitializerLabelGrid
	{
		private LabelGrid labelGrid;
		private int currRow = 0;
		private int currCol = 0;
		
		public InitializerLabelGrid(LabelGrid labelGrid)
		{
			this.labelGrid = labelGrid; 
		}
		
		public void AddInitLabel(Label label)
		{
			if (labelGrid == null) return;
			
			if (currRow >= labelGrid.RowCount)
			{
				return;
			}
			
			labelGrid[currRow, currCol] = label;
			
			// Переход к следующей ячейке
			currCol++;
			if (currCol >= labelGrid.ColumnCount)
			{
				currCol = 0;
				currRow++;
			}
		}
	}
	
	
}
