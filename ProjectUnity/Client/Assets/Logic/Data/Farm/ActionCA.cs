using System;
using System.Data;

public class ActionCA : CABase
{
    public int power;
    public static ActionCA Init(DataRow row)
    {
        ActionCA date = new ActionCA();
        date.id = Convert.ToInt32(row["ID"]);
        date.name = (string)row["Name"];
        date.power = Convert.ToInt32(row["Power"]);
        return date;
    }
}
