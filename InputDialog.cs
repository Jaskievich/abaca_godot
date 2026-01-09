using Godot;
using System;

public partial class InputDialog : AcceptDialog
{
	[Export] public string OkText { get; set; } = "OK";
	[Export] public string CancelText { get; set; } = "Отмена";

	private LineEdit _firstEdit = null;
	private LineEdit _secondEdit = null;
	//private Button _cancelButton;

	[Signal] public delegate void OkPressedEventHandler(string firstText, string secondText);
	[Signal] public delegate void CancelPressedEventHandler();
	[Signal] public delegate void _OnCheckButtonToggledEventHandler();
	[Signal] public delegate void _OnCheckButton2ToggledEventHandler(bool s);
	[Signal] public delegate void _OnCheckButton3ToggledEventHandler(bool s);
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
	
	public string GetFirstName(){
		if(_firstEdit!=null)
			return _firstEdit.Text;
		return "";
	}
	public string GetSecondName(){
		if(_secondEdit!=null)
			return _secondEdit.Text;
		return "";
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
	
	public void OnCheckButton2Toggled(bool s){
		if(s == true && _secondEdit!=null){			
			_secondEdit.Text = s?"Игрок X":"";
			_secondEdit.Visible = !s;
			GetNode<Label>("VBoxContainer/Label2").Visible = !s;
		}
		EmitSignal(SignalName._OnCheckButton2Toggled, s);
	}
	
	public void OnCheckButton3Toggled(bool s){
		if(s == true && _secondEdit!=null){			
			_secondEdit.Text = "";
			_secondEdit.Visible = s;
			GetNode<Label>("VBoxContainer/Label2").Visible = s;
			GetNode<Label>("VBoxContainer/Label2").Text = "IP-адрес сервера";
		}
		EmitSignal(SignalName._OnCheckButton3Toggled, s);
	}
	
	public void OnCheckButtonToggled(bool s){
		if( s == false ) return;
		if(_secondEdit!=null){			
			_secondEdit.Visible = s;
			_secondEdit.Text = "";
			GetNode<Label>("VBoxContainer/Label2").Visible = s;
			GetNode<Label>("VBoxContainer/Label2").Text = "Игрок 2";
		}
	//	EmitSignal(SignalName._OnCheckButtonToggled);
	}
}
