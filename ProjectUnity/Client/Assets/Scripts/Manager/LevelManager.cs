using RG.Zeluda;
using System;

public class LevelManager : ManagerBase
{
	public int Level { get; private set; } = 1;
	public int CurrentExp { get; private set; } = 0;
	public int MaxLevel { get; private set; } = 100;

	public event Action<int> OnLevelUp;
	public event Action<int, int> OnExpChanged;


	public void AddExp(int amount)
	{
		if (Level >= MaxLevel)
			return;

		CurrentExp += amount;

        while (CurrentExp >= GetRequiredExp() && Level < MaxLevel)
		{
			CurrentExp -= GetRequiredExp();
			Level++;

            OnLevelUp?.Invoke(Level);
		}

        OnExpChanged?.Invoke(CurrentExp, GetRequiredExp());


    }

	public void updateexp()
	{
        OnExpChanged?.Invoke(CurrentExp, GetRequiredExp());

    }
	public int GetRequiredExp()
	{
		return ExponentialFormula(Level);
	}
	int ExponentialFormula(int level)
	{
		return (int)(100 * Math.Pow(1.2f, level - 1));
	}
}
