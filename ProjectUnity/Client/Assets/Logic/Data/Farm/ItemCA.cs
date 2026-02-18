using System;
using System.Data;

public class ItemCA : CABase
{
    public string Name;
    public int Price;
    public int Sell;

    public static ItemCA Init(DataRow row)
    {
        ItemCA item = new ItemCA();
        item.id = Convert.ToInt32(row["ID"]);
        item.Name = (string)row["Name"];
        item.Price = Convert.ToInt32(row["Price"]);
        item.Sell = Convert.ToInt32(row["Sell"]);
        return item;
    }
}
