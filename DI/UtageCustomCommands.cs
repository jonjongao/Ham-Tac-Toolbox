using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utage;

public class UtageCustomCommands : AdvCustomCommandManager
{
    public override void OnBootInit()
    {
        Utage.AdvCommandParser.OnCreateCustomCommandFromID += CreateCustomCommand;
    }

    public override void OnClear()
    {
        base.OnClear();
    }

    protected virtual void CreateCustomCommand(string id, StringGridRow row, AdvSettingDataManager dataManager, ref AdvCommand command)
    {
        JDebug.Log($"On create custom command");
        switch (id)
        {
            case "LoadBattle":
                command = new LoadBattle(row);
                break;
        }
    }
}

public class LoadBattle : AdvCommand
{
    //float waitEndTime;
    //float time = 600f;
    string skirmishId;

    public LoadBattle(StringGridRow row) : base(row)
    {
        skirmishId = ParseCellOptional<string>(AdvColumnName.Arg1, string.Empty);
    }

    public override void DoCommand(AdvEngine engine)
    {
        GameManagerBase.current.LoadSkirmish(skirmishId);
    }

    //public override bool Wait(AdvEngine engine)
    //{
    //    return false;
    //}
}