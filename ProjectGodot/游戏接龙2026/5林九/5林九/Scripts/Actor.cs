using System.Linq;

public class Actor
{
    public string Name;
    public int[] AttributeValues;
    public Item[] Items;

    public string GetName(int[] attributeValues)
    {
        int maxIndex = 0, minIndex = 0;
        for (int i = 1; i < 6; i++)
        {
            if (attributeValues[i] > attributeValues[maxIndex]) maxIndex = i;
            if (attributeValues[i] < attributeValues[minIndex]) minIndex = i;
        }

        Attributes MaxAttr = (Attributes)maxIndex;
        Attributes MinAttr = (Attributes)minIndex;
        int MaxLevel = attributeValues[maxIndex];
        int MinLevel = attributeValues[minIndex];

        string maxAbility = MaxAttr.GetLevelName(MaxLevel);
        string minAbility = MinAttr.GetLevelName(MinLevel);

        Name = $"{maxAbility}且{minAbility}的大四学生";
        return Name;
    }

    public int GetAttributeLevel(Attributes attribute)
    {
        int index = (int)attribute;
        if (index >= 0 && index < AttributeValues.Length)
        {
            return AttributeValues[index];
        }
        return 0; // 默认值
    }

    public void AddAttr(Attributes attribute, int value)
    {
        int index = (int)attribute;
        if (index >= 0 && index < AttributeValues.Length)
        {
            AttributeValues[index] += value;
        }
    }

    public void ShowAttr()
    {
        Find.Game.RadarChart.AnimateToValues([.. AttributeValues.Select(x => x == 0 ? 0.001f : (float)x)], 1.0f);
    }
}