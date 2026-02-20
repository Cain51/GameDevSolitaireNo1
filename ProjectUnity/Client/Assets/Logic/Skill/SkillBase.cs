using System;

public class SkillBase
{
	public event Action OnCast;
	public void Cast()
	{
		OnCast?.Invoke();
	}
}
