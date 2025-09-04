using Godot;
using System;
using System.Collections.Generic;

public partial class ResultGrid : GridContainer
{
	Dictionary<string, Action<int, int>> s_map = new Dictionary<string, Action<int, int>>();
	
	public void SetName1(string name){
		Label label = GetNode<Label>("Label4");
		label.Text = name;		
		s_map[name] = SetValue1;
	}
	public void SetName2(string name){
		Label label = GetNode<Label>("Label7");
		label.Text = name;
		s_map[name] = SetValue2;
	}
	
	public void SetValue1(int totalSum, int scoolSum){
		Label label = GetNode<Label>("Label5");
		label.Text = totalSum.ToString();
		Label label2 = GetNode<Label>("Label6");
		label2.Text = scoolSum.ToString();
	}
	
	public void SetValue2(int totalSum, int scoolSum){
		Label label = GetNode<Label>("Label8");
		label.Text = totalSum.ToString();
		Label label2 = GetNode<Label>("Label9");
		label2.Text = scoolSum.ToString();
	}
	
	public void SetValue( string name, int totalSum, int scoolSum)
	{
		 if (s_map.TryGetValue(name, out Action<int, int> action))
		{
			action(totalSum, scoolSum);
		}
		else
		{
			GD.PrintErr($"Имя {name} не найдено в словаре");
		}
	}
}
