using Godot;
using System;

public partial class InputDialog : AcceptDialog
{
	[Export] public string OkText { get; set; } = "OK";
	[Export] public string CancelText { get; set; } = "Отмена";

	private LineEdit _firstEdit;
	private LineEdit _secondEdit;
	//private Button _cancelButton;

	[Signal] public delegate void OkPressedEventHandler(string firstText, string secondText);
	[Signal] public delegate void CancelPressedEventHandler();

	public override void _Ready()
	{
		_firstEdit = GetNode<LineEdit>("VBoxContainer/LineEdit");
		_secondEdit = GetNode<LineEdit>("VBoxContainer/LineEdit2");

		// Настраиваем стандартные кнопки
		Button okButton = GetOkButton();
		okButton.Text = OkText;
		Button _cancelButton = AddCancelButton(CancelText);
		
		  // Задаём размеры
		okButton.CustomMinimumSize = new Vector2(120, 45);
		_cancelButton.CustomMinimumSize = new Vector2(120, 45);

		// (опционально) Центрируем
		okButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		_cancelButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;

		// Подключаем сигналы
		okButton.Pressed += OnOkPressed;
		_cancelButton.Pressed += OnCancelPressed;

		// Обработка Enter/ESC уже встроена в AcceptDialog
	}

	private void OnOkPressed()
	{
		EmitSignal(SignalName.OkPressed, _firstEdit.Text, _secondEdit.Text);
		Hide();
	}

	private void OnCancelPressed()
	{
		EmitSignal(SignalName.CancelPressed);
		Hide();
	}

	public void ShowDialog(string title = "Ввод данных")
	{
		Title = title;
		PopupCentered(new Vector2I(400, 250));
		_firstEdit.GrabFocus();
	}
}
