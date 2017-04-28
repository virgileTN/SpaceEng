// (for developer) Enable debug to antenna or LCD marked with [DEBUG] 
public static bool EnableDebug = true; 


void Main(string argument){ 
    // Init MMAPI and debug panels marked with [DEBUG] 
    MM.Init(GridTerminalSystem, EnableDebug); 

    //TestProgram prog = new TestProgram(); 
    //prog.Run(argument); 

    GenericPartList list = new GenericPartList();
    list.Generation();
    MM.Debug("generation OK");
    list.CommandHandler(argument);
}  
public static class MMConfig 
{ 
    public const string GROUP_TAG = "airlock"; 
    public const string AIRLOCK_TAG = "airlock";
    public const string TEST_TAG = "test";
    public const string IN_TAG = "in:"; 
    public const string OUT_TAG = "out:";
    public const string MAIN_TAG = "main:"; 

    public static Color LOCK_COLOR = new Color(1.0f, 0.0f, 0.0f);  
    public static Color WARN_COLOR = new Color(1.0f, 0.5f, 0.0f); 
    public static Color OPEN_COLOR = new Color(0.0f, 1.0f, 0.0f);
}
public static class Actions{
    //références trouvées ici : http://www.spaceengineerswiki.com/Programming_Guide
    public static string    PowerOn = "OnOff_On",
    PowerOff = "OnOff_Off",
    SwitchPower = "OnOff",
    SwitchOpen = "Open",
    Open = "Open_On",
    Close = "Open_Off",
    SwitchUseConveyor = "UseConveyor",
    IncreaseRadius = "IncreaseRadius",
    DecreaseRadius = "DecreaseRadius",
    IncreaseBlinkInterval = "IncreaseBlink Interval",
    DecreaseBlinkInterval = "DecreaseBlink Interval",
    IncreaseBlinkLenght = "IncreaseBlink Lenght",
    DecreaseBlinkLenght = "DecreaseBlink Lenght",
    IncreaseBlinkOffset = "IncreaseBlink Offset",
    DecreaseBlinkOffset = "DecreaseBlink Offset",
    IncreaseRange = "IncreaseRange", 
    DecreaseRange = "DecreaseRange",
                            //airvent
    Depressurize = "Depressurize_On",
    Pressurize = "Depressurize_Off",
                            //TimerBlock
    Start = "Start",
    Stop = "Stop",
    IncreaseTriggerDelay = "IncreaseTriggerDelay",
    DecreaseTriggerDelay = "DecreaseTriggerDelay",
    TriggerNow = "TriggerNow",
                            //Landing Gear Connector
    Lock = "Lock",
    Unlock = "Unlock",
    SwitchLock = "SwitchLock",
    SwitchAutolock = "Autolock", 
    IncreaseGravity = "IncreaseGravity",
    DecreaseGravity = "DecreaseGravity ",
    IncreaseThrustOverride = "IncreaseOverride",
    DecreaseThrustOverride ="DecreaseOverride ",
                            //Passenger Seat et Remoote control
    SwitchControlThrusters = "ControlThrusters",
    SwitchControlWheels = "ControlWheels",
    SwitchHandBrake = "HandBrake",
    SwitchDampenersOverride = "DampenersOverride",
                            //piston
    Reverse = "Reverse",
    IncreaseVelocity = "IncreaseVelocity",
    DecreaseVelocity  = "DecreaseVelocity ",
    ResetVelocity = "ResetVelocity",
    IncreaseUpperLimit = "IncreaseUpperLimit",
    DecreaseUpperLimit = "DecreaseUpperLimit",
    IncreaseLowerLimit = "IncreaseLowerLimit",
    DecreaseLowerLimit = "DecreaseLowerLimit",
                            //programmable Block
    Run = "Run",
                            //Rotor
    Detach = "Detach",
    Attach = "Attach",
    IncreaseTorque = "IncreaseTorque",
    DecreaseTorque = "DecreaseTorque",
    IncreaseBrakingTorque = "IncreaseBrakingTorque",
    DecreaseBrakingTorque = "DecreaseBrakingTorque",
    IncreaseDisplacement = "IncreaseDisplacement",
    DecreaseDisplacement = "DecreaseDisplacement",
                            //Sensor
    SwitchDetectPlayers = "Detect Players",
    SwitchDetectFloatingObjects = "Detect Floating Objects",
    SwitchDetectSmallShips = "Detect Small Ships",
    SwitchDetectLargeShips = "Detect Large Ships",
    SwitchDetectStations = "Detect Stations",
    SwitchDetectAsteroids = "Detect Asteroids",
                            //ButtonPanel
    SwitchAnyoneCanUse = "AnyoneCanUse";
}
public abstract class APart{
    abstract public  string getName();
    abstract public string getType();
    abstract public void addGroup(GroupContainer group);
    abstract public void applyCommand(Command command);
    /*
    mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm
    FONCTIONS DE RECUPERATION DES BLOCS
    mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm
    */
    protected List<IMyFunctionalBlock> extractListFromGroup(GroupContainer groupContainer, string type){
        List<IMyFunctionalBlock> list = new List<IMyFunctionalBlock>();
        IMyBlockGroup group = groupContainer.getGroup();
        for (int i = 0; i < group.Blocks.Count; i++) { 
            if (MM.IsBlockOfExactType(group.Blocks[i], type))     
            list.Add(group.Blocks[i] as IMyFunctionalBlock);
        }     
        return list;
    }
    /*
    mmmmmmmmmmmmmmmmmmmm
    FONCTIONS GENERIQUES
    mmmmmmmmmmmmmmmmmmmm
    */
    protected void applyActionOnBlock(IMyFunctionalBlock block, string action){
        block.ApplyAction(action);
    }
    protected void applyActionOnBlockList(List<IMyFunctionalBlock> blocks, string action){
        for(int i=0;i<blocks.Count;i++){
            applyActionOnBlock(blocks[i],action);
        }
    }
    /*
    mmmmmmmmmmmmmmmmmmmmmmmm
    FONCTIONS GESTION LIGHTS
    mmmmmmmmmmmmmmmmmmmmmmmm
    */
    protected void SetLight(List<IMyFunctionalBlock> lights, Color c, float BlinkInt = 0f, float BlinkLen = 100f, float BlinkOff = 0f)  
    {  
        for (int i = 0; i < lights.Count; i++)  
        {  
            IMyLightingBlock light = lights[i] as IMyLightingBlock;
            if(light != null){
                if (light.GetProperty("Color").AsColor().GetValue(light) != c   
                    || BlinkInt != light.BlinkIntervalSeconds  
                    || !light.IsWorking)  
                {  
                    if (!light.IsWorking)  
                    {  
                        light.SetValue("Blink Interval", BlinkInt);  
                        light.SetValue("Blink Lenght", BlinkLen);  
                        light.SetValue("Blink Offset", BlinkOff);  
                        light.SetValue("Color", c);  
                        light.ApplyAction("OnOff_On");  
                        } else   
                        light.ApplyAction("OnOff_Off");  
                    } 
                }

            }  
        }
    /*
    mmmmmmmmmmmmmmmmmmmmmmmmmm
    FONCTIONS GESTION AIRVENTS
    mmmmmmmmmmmmmmmmmmmmmmmmmm
    */
    protected float getPressurizationLevel(IMyFunctionalBlock block){
        IMyAirVent av = block as IMyAirVent; 
        if (av == null) 
        return 0.0f; 

        if (av.IsPressurized() == false) 
        return -1f; 
        return av.GetOxygenLevel() * 100; 
    }
    /*
    mmmmmmmmmmmmmmmmmmmmmmmmmm
    FONCTIONS GESTION AIRTANKS
    mmmmmmmmmmmmmmmmmmmmmmmmmm
    */
    protected float getOxygenLevel(IMyFunctionalBlock block){
        IMyOxygenTank oxyTank = block as IMyOxygenTank; 
        if (oxyTank == null) 
        return 0.0f; 
        return oxyTank.GetOxygenLevel(); 
    }
    protected float getOxygenLevel(List<IMyFunctionalBlock> blocks){
        float sum = 0;
        for(int i=0;i<blocks.Count;i++){
            sum+= getOxygenLevel(blocks[i]);
        }
        sum = sum / blocks.Count;
        return sum;
    }
    protected float getOxygenUnits(List<IMyFunctionalBlock> blocks, bool Large = true){
        float oxyLevel = getOxygenLevel(blocks);
        if(Large){
            return 100000*oxyLevel*blocks.Count;
        }else{
            return 50000*oxyLevel*blocks.Count;
        }
    }
    /*
    mmmmmmmmmmmmmmmmmmmmmmmmmmmm
    FONCTIONS D'ECRITURE SUR LCD
    mmmmmmmmmmmmmmmmmmmmmmmmmmmm
    */
    protected void WriteLCD(IMyFunctionalBlock lcd, string message, bool append = false){
        MM.WriteLine((IMyTextPanel)lcd, message, append); 
    }  
    protected void WriteLCDList(List<IMyFunctionalBlock> lcds, string message, bool append = false){ 
        for (int i = 0; i < lcds.Count; i++) 
        { 
            WriteLCD(lcds[i],message,append);
        } 
    }
    protected void ClearLCD(IMyFunctionalBlock lcd){
        WriteLCD(lcd,"");
    }
    protected void ClearLCDList(List<IMyFunctionalBlock> lcds, bool append = false){  
        WriteLCDList(lcds,"");
    }

}
public class GroupContainer{
    private IMyBlockGroup group;
    private string typeTag = "Default", subTypeTag = "Default", name = "Default";
    private bool ok = false;
    public GroupContainer(IMyBlockGroup group){
        this.group=group;
        string[] sep = new [] { " " }; 
        string[] tags;
        tags = group.Name.ToLower().Split(sep,StringSplitOptions.RemoveEmptyEntries);
        if(tags.Length>=2){
            typeTag = tags[0];
            subTypeTag = tags[1];
            name = tags[2];
            ok = true;
        }else{
            MM.Debug("Incomplete Group Name: "+group.Name);
        }
    }
    public IMyBlockGroup getGroup(){
        return group;
    }
    public string getTypeTag(){
        return typeTag;
    }
    public string getSubTypeTag(){
        return subTypeTag;
    }
    public string getName(){
        return name;
    }
    public string getFullName(){
        return typeTag+" "+name;
    }
    public string ToString(){
        return typeTag+" "+subTypeTag+" "+name;
    }
    public bool isOk(){
        return ok;
    }
}
public class Command{
    private string target="",action="";
    public Command(String command){
        string[] sep = new [] { " " }; 
        string[] commandParts;
        commandParts = command.Split(sep,StringSplitOptions.RemoveEmptyEntries);
        if(commandParts.Length>2){
            target = commandParts[0].ToLower()+" "+commandParts[1].ToLower();
            action = commandParts[2].ToLower();
        }else{
            MM.Debug("Invalid Command too few arguments");
        }
    }
    public string getTarget(){
        return target;
    }
    public string getAction(){
        return action;
    }
    public string ToString(){
        return target+" "+action;
    }
}
public class GenericPartList{
    //private List<IGenericPart> parts = new List<IGenericPart>();
    private MMList<APart> aParts = new MMList<APart>();
    public int Generation(){
        if(MM._GridTerminalSystem.BlockGroups.Count == 0){
            MM.Debug("Pas de Groupe trouvé");
            return -1;
        }
        for (int i = 0; i < MM._GridTerminalSystem.BlockGroups.Count; i++) { 
            GroupContainer group = new GroupContainer(MM._GridTerminalSystem.BlockGroups[i]); 
            if(group.isOk()){
                PartFactory(group);
            }
        }
        return 0;
    }
    public void CommandHandler(string commandString){
        Command command = new Command(commandString);
        if(!Contains(command.getTarget())){
            MM.Debug("Invalid target: "+commandString);
            return;
        }
        getPart(command.getTarget()).applyCommand(command);
    }
    private int PartFactory(GroupContainer group){
        //MM.Debug("-- PartFactory START--");
        //MM.Debug(group.ToString());
        Airlock resultPart = null;
        if(Contains(group)){
            //MM.Debug("Contains ok");
            getPart(group.getFullName()).addGroup(group);
        }else{
            switch(group.getTypeTag()){
                case MMConfig.AIRLOCK_TAG:{
                    //MM.Debug("airlock found: "+group.getTypeTag()+""+group.getSubTypeTag()+" "+group.getName());
                    resultPart = new Airlock(group);
                    break;
                }
                case MMConfig.TEST_TAG:{
                    //MM.Debug("test found: "+group.getSubTypeTag()+" "+group.getName());
                    break;
                }
                default :{
                    MM.Debug("TypeTag no defined: "+group.getTypeTag());
                    return -1;
                    break;
                }
            }
            aParts.Add(resultPart);
            MM.Debug("part added");
        }
        //MM.Debug("-- PartFactory END--");
        return 0;
    }
    private bool Contains(GroupContainer group){
        return Contains(group.getFullName());
    }
    private bool Contains(string fullName){
        for(int i=0;i<aParts.Count;i++){
            if(aParts[i].getName() == fullName){
               return true;
            }
        }
        return false;
    }
    private APart getPart(string fullName){
        for(int i=0;i<aParts.Count;i++){
            if(aParts[i].getName() == fullName){
               return aParts[i];
            }
        }
        return null;
    }
}
public class Airlock : APart{
    private string name, type;
    private List<IMyFunctionalBlock>    lcds,
                                        doors,
                                        inDoors,
                                        inLights,
                                        outDoors,
                                        outLights,
                                        mainLights,
                                        airVents,
                                        inTimers,
                                        outTimers;
    private IMyTimerBlock inTimer, outTimer;

    public Airlock(GroupContainer group){
        MM.Debug("airlock constructor");
        name = group.getTypeTag()+" "+group.getName();
        MM.Debug(group.ToString());
        type = group.getTypeTag();
        addGroup(group);
    }
    public override void addGroup(GroupContainer group){
        MM.Debug("airlock addgroup");
        MM.Debug(group.ToString());
        switch(group.getSubTypeTag()){
            case MMConfig.MAIN_TAG :{
                lcds = extractListFromGroup(group,"TextPanel");
                mainLights = extractListFromGroup(group,"LightingBlock");
                airVents = extractListFromGroup(group,"AirVent"); 
                break;
            }
            case MMConfig.IN_TAG : {
                inTimers = extractListFromGroup(group,"TimerBlock");
                inDoors = extractListFromGroup(group,"Door");
                inLights = extractListFromGroup(group,"LightingBlock");
                if(inTimers.Count<0){
                    inTimer = inTimers[0] as IMyTimerBlock;
                }
                break;
            }
            case MMConfig.OUT_TAG : {
                outTimers = extractListFromGroup(group,"TimerBlock");
                outDoors = extractListFromGroup(group,"Door");
                outLights = extractListFromGroup(group,"LightingBlock");
                if(outTimers.Count<0){
                    outTimer = outTimers[0] as IMyTimerBlock;
                }
                break;
            }
            default : {
                MM.Debug("No such subType: "+group.getSubTypeTag());
                break;
            }
            MM.Debug("END addgroup");
        }
    }
    public override void applyCommand(Command command){
        switch(command.getAction()){
            case "pressurize": {
                pressurize();
                break;
            }
            case "depressurize": {
                WriteLCDList(lcds,"DEP¨RESSURIEZAZE LOL");
                depressurize();
                break;
            }
            default :{
                MM.Debug("Invalid Action: "+command);
                break;
            }
        }
    }
    public override string getName(){
        return name;
    }
    public override string getType(){
        return type;
    }
    private bool verification(){
        bool isOk = true;
        if(!(airVents.Count>0)){
            WriteLCDList(lcds,"No airvent detected");
            isOk = false;
        }
        if(!(outTimers.Count>0)){
            WriteLCDList(lcds,"No outTimer detected");
            isOk = false;
        }
        if(!(inTimers.Count>0)){
            WriteLCDList(lcds,"No inTimer detected");
            isOk = false;
        }
        return isOk;
    }
    private void pressurize(){
        if(!verification()){
            WriteLCDList(lcds,"Please Add required Components");
            return;
        }
        applyActionOnBlock(outTimers[0],Actions.Stop);
        WriteLCDList(lcds,"Phase 1");
        //applyActionOnBlock(outTimer,Actions.Stop);
        WriteLCDList(lcds,"Phase 2");
        applyActionOnBlockList(airVents,Actions.PowerOn);
        WriteLCDList(lcds,"Phase 3");
        applyActionOnBlockList(airVents,Actions.Pressurize);
        WriteLCDList(lcds,"Phase 4");
        float level = getPressurizationLevel(airVents[0]);
        
        if(level >0){
            applyActionOnBlockList(inDoors,Actions.PowerOn);
            applyActionOnBlockList(outDoors,Actions.PowerOn);

            applyActionOnBlockList(inDoors,Actions.Close);
            applyActionOnBlockList(outDoors,Actions.Close);

            SetLight(outLights, MMConfig.LOCK_COLOR, 2f, 50f);
            SetLight(inLights, MMConfig.LOCK_COLOR);

            
            WriteLCDList(lcds,"depressurization in progress:");
            WriteLCDList(lcds,"Pressurization Level of \""+airVents[0].CustomName+"\": "+getPressurizationLevel(airVents[0]),true);
        }else{
            WriteLCDList(lcds,"Airlock Depressurized");
            WriteLCDList(lcds,"openning out doors",true);

            SetLight(outLights, MMConfig.OPEN_COLOR);

            applyActionOnBlockList(inDoors,Actions.PowerOff);

            applyActionOnBlockList(outDoors,Actions.PowerOn);
            applyActionOnBlockList(outDoors,Actions.Open);
        }
        applyActionOnBlock(outTimer,Actions.Start);
    }
    private void depressurize(){
        if(!verification()){
            WriteLCDList(lcds,"Please Add required Components");
            return;
        }
        applyActionOnBlock(inTimers[0],Actions.Stop);
        //applyActionOnBlock(inTimer,Actions.Stop);
        applyActionOnBlockList(airVents,Actions.PowerOn);
        applyActionOnBlockList(airVents,Actions.Depressurize);
        float level = getPressurizationLevel(airVents[0]);
        if(level < 99){
            applyActionOnBlockList(inDoors,Actions.PowerOn);
            applyActionOnBlockList(outDoors,Actions.PowerOn);

            applyActionOnBlockList(inDoors,Actions.Close);
            applyActionOnBlockList(outDoors,Actions.Close);

            SetLight(inLights, MMConfig.LOCK_COLOR, 2f, 50f);
            SetLight(outLights, MMConfig.LOCK_COLOR);

            
            WriteLCDList(lcds,"Pressurization in progress:");
            WriteLCDList(lcds,"Pressurization Level of \""+airVents[0].CustomName+"\": "+getPressurizationLevel(airVents[0]),true);
        }else{
            WriteLCDList(lcds,"Airlock Pressurized");
            WriteLCDList(lcds,"openning in doors",true);

            SetLight(inLights, MMConfig.OPEN_COLOR);

            applyActionOnBlockList(outDoors,Actions.PowerOff);

            applyActionOnBlockList(inDoors,Actions.PowerOn);
            applyActionOnBlockList(inDoors,Actions.Open);
        }
        applyActionOnBlock(inTimer,Actions.Start);
    }
}
/*
public class TestProgram { 
        private List<IMyFunctionalBlock> lcds;
        private List<IMyFunctionalBlock> doors;
        private List<IMyFunctionalBlock> inDoors;
        private List<IMyFunctionalBlock> inLights;
        private List<IMyFunctionalBlock> outDoors;
        private List<IMyFunctionalBlock> outLights;
        private List<IMyFunctionalBlock> mainLights;
        private List<IMyFunctionalBlock> airVents;
        private List<IMyFunctionalBlock> oxyTanks;
        private List<IMyFunctionalBlock> timers;
        public int Run(string argument) { 
            MM.Debug("Execution Du programme de test");
        //On vérifie qu'il y des groupes de blocks
            if(MM._GridTerminalSystem.BlockGroups.Count == 0){
                MM.Debug("Pas de Groupe trouvé");
                return -1;
            }
            for (int i = 0; i < MM._GridTerminalSystem.BlockGroups.Count; i++) { 
                IMyBlockGroup group = MM._GridTerminalSystem.BlockGroups[i]; 
                string name = group.Name.ToLower();
                if (name.StartsWith(MMConfig.GROUP_TAG)) {
                    if(name.StartsWith(MMConfig.GROUP_TAG + ' ' + MMConfig.MAIN_TAG)){
                        MM.Debug(name);
                        MM.Debug(name);
                        lcds = extractListFromGroup(group,"TextPanel");
                        mainLights = extractListFromGroup(group,"LightingBlock");
                        airVents = extractListFromGroup(group,"AirVent");
                        oxyTanks = extractListFromGroup(group,"OxygenTank");
                        timers = extractListFromGroup(group,"TimerBlock");
                        }else if(name.StartsWith(MMConfig.GROUP_TAG + ' ' + MMConfig.IN_TAG)){
                            inDoors = extractListFromGroup(group,"Door");
                            inLights = extractListFromGroup(group,"LightingBlock");
                            }else if(name.StartsWith(MMConfig.GROUP_TAG + ' ' + MMConfig.OUT_TAG)){
                                outDoors = extractListFromGroup(group,"Door");
                                outLights = extractListFromGroup(group,"LightingBlock");
                            }
                        }
                    }

                    switch (argument){
                        case "open":{
                            applyActionOnBlockList(doors,Actions.PowerOn);
                            applyActionOnBlockList(doors,Actions.Open);
                            WriteLCDList(lcds,"openning doors",true);
                            break;
                        }
                        case "close":{
                            applyActionOnBlockList(doors,Actions.PowerOn);
                            applyActionOnBlockList(doors,Actions.Close);
                            WriteLCDList(lcds,"clossing doors",true);
                            break;
                        }
                        case "lock":{
                            applyActionOnBlockList(doors,Actions.PowerOff);
                            WriteLCDList(lcds,"locking doors",true);
                            break;
                        }
                        case "lightsOn":{
                            applyActionOnBlockList(mainLights,Actions.PowerOn);
                            WriteLCDList(lcds,"powering On lights",true);
                            break;
                        }
                        case "lightsOff":{
                            applyActionOnBlockList(mainLights,Actions.PowerOff);
                            WriteLCDList(lcds,"powering Off lights",true);
                            break;
                        }
                        case "pressurize":{
                            applyActionOnBlockList(airVents,Actions.PowerOn);
                            applyActionOnBlockList(airVents,Actions.Pressurize);
                            WriteLCDList(lcds,"pressurization",true);
                            break;
                        }
                        case "depressurization":{
                            applyActionOnBlock(timers[1],Actions.Stop);
                            applyActionOnBlockList(airVents,Actions.PowerOn);
                            applyActionOnBlockList(airVents,Actions.Depressurize);
                            WriteLCDList(lcds,"depressurization");
                            float level = getPressurizationLevel(airVents[0]);
                            if(level >0){
                                applyActionOnBlockList(inDoors,Actions.PowerOn);
                                applyActionOnBlockList(outDoors,Actions.PowerOn);

                                applyActionOnBlockList(inDoors,Actions.Close);
                                applyActionOnBlockList(outDoors,Actions.Close);

                                SetLight(outLights, MMConfig.LOCK_COLOR, 2f, 50f);
                                SetLight(inLights, MMConfig.LOCK_COLOR);

                                applyActionOnBlock(timers[0],Actions.Start);
                                WriteLCDList(lcds,"depressurization in progress:");
                                WriteLCDList(lcds,"Pressurization Level of \""+airVents[0].CustomName+"\": "+getPressurizationLevel(airVents[0]),true);
                            }else{
                                WriteLCDList(lcds,"Airlock Depressurized");
                                WriteLCDList(lcds,"openning out doors",true);

                                SetLight(outLights, MMConfig.OPEN_COLOR);

                                applyActionOnBlockList(inDoors,Actions.PowerOff);

                                applyActionOnBlockList(outDoors,Actions.PowerOn);
                                applyActionOnBlockList(outDoors,Actions.Open);

                                applyActionOnBlock(timers[0],Actions.Start);
                            }
                            break;
                        }
                        case "depressurize":{
                            applyActionOnBlockList(airVents,Actions.PowerOn);
                            applyActionOnBlockList(airVents,Actions.Depressurize);
                            WriteLCDList(lcds,"depressurization",true);
                            break;
                        }
                        case "pressurization":{
                            applyActionOnBlock(timers[0],Actions.Stop);
                            applyActionOnBlockList(airVents,Actions.PowerOn);
                            applyActionOnBlockList(airVents,Actions.Pressurize);
                            WriteLCDList(lcds,"Pressurization");
                            float level = getPressurizationLevel(airVents[0]);
                            if(level < 99){
                                applyActionOnBlockList(inDoors,Actions.PowerOn);
                                applyActionOnBlockList(outDoors,Actions.PowerOn);
                                applyActionOnBlockList(inDoors,Actions.Close);
                                applyActionOnBlockList(outDoors,Actions.Close);
                                applyActionOnBlock(timers[1],Actions.Start);

                                SetLight(inLights, MMConfig.LOCK_COLOR, 2f, 50f);
                                SetLight(outLights, MMConfig.LOCK_COLOR);

                                WriteLCDList(lcds,"pressurization in progress:");
                                WriteLCDList(lcds,"Pressurization Level of \""+airVents[0].CustomName+"\": "+getPressurizationLevel(airVents[0]),true);
                            }else{
                                WriteLCDList(lcds,"Airlock Pressurized");
                                WriteLCDList(lcds,"openning In doors",true);
                                applyActionOnBlockList(inDoors,Actions.PowerOn);
                                applyActionOnBlockList(inDoors,Actions.Open);

                                applyActionOnBlockList(outDoors,Actions.PowerOff);

                                SetLight(inLights, MMConfig.OPEN_COLOR);

                                applyActionOnBlock(timers[1],Actions.Start);

                            }
                            break;
                        }
                        case "getPressurization":{
                            for(int i=0; i<airVents.Count;i++){
                                WriteLCDList(lcds,"Pressurization Level of \""+airVents[i].CustomName+"\": "+getPressurizationLevel(airVents[i]),true);
                            }
                            break;
                        }
                        case "OxyTankStatus":{
                            WriteLCDList(lcds,"OxyTankStatus");
                            WriteLCDList(lcds,"OxygenTanks Level:"+getOxygenLevel(oxyTanks)*100+"%",true);
                            WriteLCDList(lcds,"Nb OxygenTanks:"+oxyTanks.Count,true);
                            WriteLCDList(lcds,"O2 units:"+getOxygenUnits(oxyTanks),true);
                            break;
                        }
                        case "clear":{
                            WriteLCDList(lcds,"");
                            break;
                        }
                    };
                    MM.Debug("FIN");
                    return 0; 
                }

    public List<IMyFunctionalBlock> extractListFromGroup(IMyBlockGroup group, string type){
        List<IMyFunctionalBlock> list = new List<IMyFunctionalBlock>();
        for (int i = 0; i < group.Blocks.Count; i++) { 
            if (MM.IsBlockOfExactType(group.Blocks[i], type))     
            list.Add(group.Blocks[i] as IMyFunctionalBlock);
        }     
        return list;
    }

    public void applyActionOnBlock(IMyFunctionalBlock block, string action){
        block.ApplyAction(action);
    }
    public void applyActionOnBlockList(List<IMyFunctionalBlock> blocks, string action){
        for(int i=0;i<blocks.Count;i++){
            applyActionOnBlock(blocks[i],action);
        }
    }

    private void SetLight(List<IMyFunctionalBlock> lights, Color c, float BlinkInt = 0f, float BlinkLen = 100f, float BlinkOff = 0f)  
    {  
        for (int i = 0; i < lights.Count; i++)  
        {  
            IMyLightingBlock light = lights[i] as IMyLightingBlock;
            if(light != null){
                if (light.GetProperty("Color").AsColor().GetValue(light) != c   
                    || BlinkInt != light.BlinkIntervalSeconds  
                    || !light.IsWorking)  
                {  
                    if (!light.IsWorking)  
                    {  
                        light.SetValue("Blink Interval", BlinkInt);  
                        light.SetValue("Blink Lenght", BlinkLen);  
                        light.SetValue("Blink Offset", BlinkOff);  
                        light.SetValue("Color", c);  
                        light.ApplyAction("OnOff_On");  
                        } else   
                        light.ApplyAction("OnOff_Off");  
                    } 
                }

            }  
        }

    public float getPressurizationLevel(IMyFunctionalBlock block){
        IMyAirVent av = block as IMyAirVent; 
        if (av == null) 
        return 0.0f; 

        if (av.IsPressurized() == false) 
        return -1f; 
        return av.GetOxygenLevel() * 100; 
    }

    public float getOxygenLevel(IMyFunctionalBlock block){
        IMyOxygenTank oxyTank = block as IMyOxygenTank; 
        if (oxyTank == null) 
        return 0.0f; 
        return oxyTank.GetOxygenLevel(); 
    }
    public float getOxygenLevel(List<IMyFunctionalBlock> blocks){
        float sum = 0;
        for(int i=0;i<blocks.Count;i++){
            sum+= getOxygenLevel(blocks[i]);
        }
        sum = sum / blocks.Count;
        return sum;
    }
    public float getOxygenUnits(List<IMyFunctionalBlock> blocks, bool Large = true){
        float oxyLevel = getOxygenLevel(blocks);
        if(Large){
            return 100000*oxyLevel*blocks.Count;
        }else{
            return 50000*oxyLevel*blocks.Count;
        }
    }
    public void WriteLCD(IMyFunctionalBlock lcd, string message, bool append = false){
        MM.WriteLine((IMyTextPanel)lcd, message, append); 
    }  
    public void WriteLCDList(List<IMyFunctionalBlock> lcds, string message, bool append = false){ 
        for (int i = 0; i < lcds.Count; i++) 
        { 
            WriteLCD(lcds[i],message,append);
        } 
    }
    public void ClearLCD(IMyFunctionalBlock lcd){
        WriteLCD(lcd,"");
    }
    public void ClearLCDList(List<IMyFunctionalBlock> lcds, bool append = false){  
        WriteLCDList(lcds,"");
    }
} 
*/
// MMAPI below (do not modify)   

// IMyTerminalBlock collection with useful methods   
public class MMBlockCollection 
{ 
    public List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>(); 

    // add Blocks with name containing nameLike   
    public void AddBlocksOfNameLike(string nameLike) 
    { 
        if (nameLike == "" || nameLike == "*") 
        { 
            Blocks.AddList(MM._GridTerminalSystem.Blocks); 
            return; 
        } 

        string group = (nameLike.StartsWith("G:") ? nameLike.Substring(2).Trim().ToLower() : ""); 
        if (group != "") 
        { 
            for (int i = 0; i < MM._GridTerminalSystem.BlockGroups.Count; i++) 
            { 
                IMyBlockGroup g = MM._GridTerminalSystem.BlockGroups[i]; 
                if (g.Name.ToLower() == group) 
                Blocks.AddList(g.Blocks); 
            } 
            return; 
        } 

        MM._GridTerminalSystem.SearchBlocksOfName(nameLike, Blocks); 
    } 

    // add Blocks of type (optional: with name containing nameLike)   
    public void AddBlocksOfType(string type, string nameLike = "") 
    { 
        if (nameLike == "" || nameLike == "*") 
        { 
            List<IMyTerminalBlock> blocksOfType = new List<IMyTerminalBlock>(); 
            MM.GetBlocksOfType(ref blocksOfType, type); 
            Blocks.AddList(blocksOfType); 
        } 
        else 
        { 
            string group = (nameLike.StartsWith("G:") ? nameLike.Substring(2).Trim().ToLower() : ""); 
            if (group != "") 
            { 
                for (int i = 0; i < MM._GridTerminalSystem.BlockGroups.Count; i++) 
                { 
                    IMyBlockGroup g = MM._GridTerminalSystem.BlockGroups[i]; 
                    if (g.Name.ToLower() == group) 
                    { 
                        for (int j = 0; j < g.Blocks.Count; j++) 
                        if (MM.IsBlockOfType(g.Blocks[j], type)) 
                        Blocks.Add(g.Blocks[j]); 
                        return; 
                    } 
                } 
                return; 
            } 
            List<IMyTerminalBlock> blocksOfType = new List<IMyTerminalBlock>(); 
            MM.GetBlocksOfType(ref blocksOfType, type); 

            for (int i = 0; i < blocksOfType.Count; i++) 
            if (blocksOfType[i].CustomName.Contains(nameLike)) 
            Blocks.Add(blocksOfType[i]); 
        } 
    } 

    // add all Blocks from collection col to this collection   
    public void AddFromCollection(MMBlockCollection col) 
    { 
        Blocks.AddList(col.Blocks); 
    } 

    // clear all blocks from this collection   
    public void Clear() 
    { 
        Blocks.Clear(); 
    } 

    // number of blocks in collection   
    public int Count() 
    { 
        return Blocks.Count; 
    } 
} 

// MMAPI Helper functions   
public static class MM 
{ 
    public static bool EnableDebug = false; 
    public static IMyGridTerminalSystem _GridTerminalSystem = null; 
    public static MMBlockCollection _DebugTextPanels = null; 
    public static Dictionary<string, Action<List<IMyTerminalBlock>>> BlocksOfStrType = null; 

    public static void Init(IMyGridTerminalSystem gridSystem, bool _EnableDebug) 
    { 
        _GridTerminalSystem = gridSystem; 
        EnableDebug = _EnableDebug; 
        _DebugTextPanels = new MMBlockCollection(); 

        // prepare debug panels 
        // select all text panels with [DEBUG] in name  
        if (_EnableDebug) 
        { 
            _DebugTextPanels.AddBlocksOfType("textpanel", "[DEBUG]"); 
            Debug("DEBUG Panel started.", false, "DEBUG PANEL"); 
        } 
    } 

    public static float GetAirVentPressure(IMyTerminalBlock airvent) 
    { 
        IMyAirVent av = airvent as IMyAirVent; 
        if (av == null) 
        return 0.0f; 

        if (av.IsPressurized() == false) 
        return -1f; 
        return av.GetOxygenLevel() * 100; 
    } 

    public static double GetPercent(double current, double max) 
    { 
        return (max > 0 ? (current / max) * 100 : 100); 
    } 

    public static List<double> GetDetailedInfoValues(IMyTerminalBlock block) 
    { 
        List<double> result = new List<double>(); 

        string di = block.DetailedInfo; 
        string[] attr_lines = block.DetailedInfo.Split('\n'); 
        string valstr = ""; 

        for (int i = 0; i < attr_lines.Length; i++) 
        { 
            string[] parts = attr_lines[i].Split(':'); 
            // broken line? (try German) 
            if (parts.Length < 2) 
            parts = attr_lines[i].Split('r'); 
            valstr = (parts.Length < 2 ? parts[0] : parts[1]); 
            string[] val_parts = valstr.Trim().Split(' '); 
            string str_val = val_parts[0]; 
            char str_unit = (val_parts.Length > 1 ? val_parts[1][0] : '.'); 

            double val = 0; 
            double final_val = 0; 
            if (Double.TryParse(str_val, out val)) 
            { 
                final_val = val * Math.Pow(1000.0, ".kMGTPEZY".IndexOf(str_unit)); 
                result.Add(final_val); 
            } 
        } 

        return result; 
    } 

    public static string GetLastDetailedValue(IMyTerminalBlock block) 
    { 
        string[] info_lines = block.DetailedInfo.Split('\n'); 
        string[] state_parts = info_lines[info_lines.Length - 1].Split(':'); 
        string state = (state_parts.Length > 1 ? state_parts[1] : state_parts[0]); 
        return state; 
    } 


    public static string GetBlockTypeDisplayName(IMyTerminalBlock block) 
    { 
        return block.DefinitionDisplayNameText; 
    } 

    public static void GetBlocksOfType<T>(List<IMyTerminalBlock> blocks) 
    { 
        _GridTerminalSystem.GetBlocksOfType<T>(blocks); 
    } 

    public static void GetBlocksOfExactType(ref List<IMyTerminalBlock> blocks, string exact) 
    { 
        if (exact == "CargoContainer") _GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(blocks); 
        else 
        if (exact == "TextPanel") _GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(blocks); 
        else 
        if (exact == "Assembler") _GridTerminalSystem.GetBlocksOfType<IMyAssembler>(blocks); 
        else 
        if (exact == "Refinery") _GridTerminalSystem.GetBlocksOfType<IMyRefinery>(blocks); 
        else 
        if (exact == "Reactor") _GridTerminalSystem.GetBlocksOfType<IMyReactor>(blocks); 
        else 
        if (exact == "SolarPanel") _GridTerminalSystem.GetBlocksOfType<IMySolarPanel>(blocks); 
        else 
        if (exact == "BatteryBlock") _GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(blocks); 
        else 
        if (exact == "Beacon") _GridTerminalSystem.GetBlocksOfType<IMyBeacon>(blocks); 
        else 
        if (exact == "RadioAntenna") _GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(blocks); 
        else 
        if (exact == "AirVent") _GridTerminalSystem.GetBlocksOfType<IMyAirVent>(blocks); 
        else 
        if (exact == "OxygenTank") _GridTerminalSystem.GetBlocksOfType<IMyOxygenTank>(blocks); 
        else 
        if (exact == "OxygenGenerator") _GridTerminalSystem.GetBlocksOfType<IMyOxygenGenerator>(blocks); 
        else 
        if (exact == "LaserAntenna") _GridTerminalSystem.GetBlocksOfType<IMyLaserAntenna>(blocks); 
        else 
        if (exact == "Thrust") _GridTerminalSystem.GetBlocksOfType<IMyThrust>(blocks); 
        else 
        if (exact == "Gyro") _GridTerminalSystem.GetBlocksOfType<IMyGyro>(blocks); 
        else 
        if (exact == "SensorBlock") _GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(blocks); 
        else 
        if (exact == "ShipConnector") _GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(blocks); 
        else 
        if (exact == "ReflectorLight") _GridTerminalSystem.GetBlocksOfType<IMyReflectorLight>(blocks); 
        else 
        if (exact == "InteriorLight") _GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(blocks); 
        else 
        if (exact == "LandingGear") _GridTerminalSystem.GetBlocksOfType<IMyLandingGear>(blocks); 
        else 
        if (exact == "ProgrammableBlock") _GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(blocks); 
        else 
        if (exact == "TimerBlock") _GridTerminalSystem.GetBlocksOfType<IMyTimerBlock>(blocks); 
        else 
        if (exact == "MotorStator") _GridTerminalSystem.GetBlocksOfType<IMyMotorStator>(blocks); 
        else 
        if (exact == "PistonBase") _GridTerminalSystem.GetBlocksOfType<IMyPistonBase>(blocks); 
        else 
        if (exact == "Projector") _GridTerminalSystem.GetBlocksOfType<IMyProjector>(blocks); 
        else 
        if (exact == "ShipMergeBlock") _GridTerminalSystem.GetBlocksOfType<IMyShipMergeBlock>(blocks); 
        else 
        if (exact == "SoundBlock") _GridTerminalSystem.GetBlocksOfType<IMySoundBlock>(blocks); 
        else 
        if (exact == "Collector") _GridTerminalSystem.GetBlocksOfType<IMyCollector>(blocks); 
        else 
        if (exact == "Door") _GridTerminalSystem.GetBlocksOfType<IMyDoor>(blocks); 
        else 
        if (exact == "GravityGeneratorSphere") _GridTerminalSystem.GetBlocksOfType<IMyGravityGeneratorSphere>(blocks); 
        else 
        if (exact == "GravityGenerator") _GridTerminalSystem.GetBlocksOfType<IMyGravityGenerator>(blocks); 
        else 
        if (exact == "ShipDrill") _GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(blocks); 
        else 
        if (exact == "ShipGrinder") _GridTerminalSystem.GetBlocksOfType<IMyShipGrinder>(blocks); 
        else 
        if (exact == "ShipWelder") _GridTerminalSystem.GetBlocksOfType<IMyShipWelder>(blocks); 
        else 
        if (exact == "LargeGatlingTurret") _GridTerminalSystem.GetBlocksOfType<IMyLargeGatlingTurret>(blocks); 
        else 
        if (exact == "LargeInteriorTurret") _GridTerminalSystem.GetBlocksOfType<IMyLargeInteriorTurret>(blocks); 
        else 
        if (exact == "LargeMissileTurret") _GridTerminalSystem.GetBlocksOfType<IMyLargeMissileTurret>(blocks); 
        else 
        if (exact == "SmallGatlingGun") _GridTerminalSystem.GetBlocksOfType<IMySmallGatlingGun>(blocks); 
        else 
        if (exact == "SmallMissileLauncherReload") _GridTerminalSystem.GetBlocksOfType<IMySmallMissileLauncherReload>(blocks); 
        else 
        if (exact == "SmallMissileLauncher") _GridTerminalSystem.GetBlocksOfType<IMySmallMissileLauncher>(blocks); 
        else 
        if (exact == "VirtualMass") _GridTerminalSystem.GetBlocksOfType<IMyVirtualMass>(blocks); 
        else 
        if (exact == "Warhead") _GridTerminalSystem.GetBlocksOfType<IMyWarhead>(blocks); 
        else 
        if (exact == "FunctionalBlock") _GridTerminalSystem.GetBlocksOfType<IMyFunctionalBlock>(blocks); 
        else 
        if (exact == "LightingBlock") _GridTerminalSystem.GetBlocksOfType<IMyLightingBlock>(blocks); 
        else 
        if (exact == "ControlPanel") _GridTerminalSystem.GetBlocksOfType<IMyControlPanel>(blocks); 
        else 
        if (exact == "Cockpit") _GridTerminalSystem.GetBlocksOfType<IMyCockpit>(blocks); 
        else 
        if (exact == "MedicalRoom") _GridTerminalSystem.GetBlocksOfType<IMyMedicalRoom>(blocks); 
        else 
        if (exact == "RemoteControl") _GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(blocks); 
        else 
        if (exact == "ButtonPanel") _GridTerminalSystem.GetBlocksOfType<IMyButtonPanel>(blocks); 
        else 
        if (exact == "CameraBlock") _GridTerminalSystem.GetBlocksOfType<IMyCameraBlock>(blocks); 
        else 
        if (exact == "OreDetector") _GridTerminalSystem.GetBlocksOfType<IMyOreDetector>(blocks); 
    } 

    public static void GetBlocksOfType(ref List<IMyTerminalBlock> blocks, string typestr) 
    { 
        typestr = typestr.Trim().ToLower(); 

        GetBlocksOfExactType(ref blocks, TranslateToExactBlockType(typestr)); 
    } 

    public static bool IsBlockOfExactType(IMyTerminalBlock block, string exact) 
    { 
        if (exact == "FunctionalBlock") 
        return block.IsFunctional; 
        else 
        if (exact == "LightingBlock") 
        return ((block as IMyLightingBlock) != null); 
        return block.BlockDefinition.ToString().Contains(exact); 
    } 

    public static bool IsBlockOfType(IMyTerminalBlock block, string typestr) 
    { 
        string exact = TranslateToExactBlockType(typestr); 
        if (exact == "FunctionalBlock") 
        return block.IsFunctional; 
        else 
        if (exact == "LightingBlock") 
        return ((block as IMyLightingBlock) != null); 
        return block.BlockDefinition.ToString().Contains(exact); 
    } 

    public static string TranslateToExactBlockType(string typeInStr) 
    { 
        typeInStr = typeInStr.ToLower(); 

        if (typeInStr.StartsWith("carg") || typeInStr.StartsWith("conta")) 
        return "CargoContainer"; 
        if (typeInStr.StartsWith("text") || typeInStr.StartsWith("lcd")) 
        return "TextPanel"; 
        if (typeInStr.StartsWith("ass")) 
        return "Assembler"; 
        if (typeInStr.StartsWith("refi")) 
        return "Refinery"; 
        if (typeInStr.StartsWith("reac")) 
        return "Reactor"; 
        if (typeInStr.StartsWith("solar")) 
        return "SolarPanel"; 
        if (typeInStr.StartsWith("bat")) 
        return "BatteryBlock"; 
        if (typeInStr.StartsWith("bea")) 
        return "Beacon"; 
        if (typeInStr.Contains("vent")) 
        return "AirVent"; 
        if (typeInStr.Contains("tank") && typeInStr.Contains("oxy")) 
        return "OxygenTank"; 
        if (typeInStr.Contains("gene") && typeInStr.Contains("oxy")) 
        return "OxygenGenerator"; 
        if (typeInStr == "laserantenna") 
        return "LaserAntenna"; 
        if (typeInStr.Contains("antenna")) 
        return "RadioAntenna"; 
        if (typeInStr.StartsWith("thrust")) 
        return "Thrust"; 
        if (typeInStr.StartsWith("gyro")) 
        return "Gyro"; 
        if (typeInStr.StartsWith("sensor")) 
        return "SensorBlock"; 
        if (typeInStr.Contains("connector")) 
        return "ShipConnector"; 
        if (typeInStr.StartsWith("reflector")) 
        return "ReflectorLight"; 
        if ((typeInStr.StartsWith("inter") && typeInStr.EndsWith("light"))) 
        return "InteriorLight"; 
        if (typeInStr.StartsWith("land")) 
        return "LandingGear"; 
        if (typeInStr.StartsWith("program")) 
        return "ProgrammableBlock"; 
        if (typeInStr.StartsWith("timer")) 
        return "TimerBlock"; 
        if (typeInStr.StartsWith("motor")) 
        return "MotorStator"; 
        if (typeInStr.StartsWith("piston")) 
        return "PistonBase"; 
        if (typeInStr.StartsWith("proj")) 
        return "Projector"; 
        if (typeInStr.Contains("merge")) 
        return "ShipMergeBlock"; 
        if (typeInStr.StartsWith("sound")) 
        return "SoundBlock"; 
        if (typeInStr.StartsWith("col")) 
        return "Collector"; 
        if (typeInStr == "door") 
        return "Door"; 
        if ((typeInStr.Contains("grav") && typeInStr.Contains("sphe"))) 
        return "GravityGeneratorSphere"; 
        if (typeInStr.Contains("grav")) 
        return "GravityGenerator"; 
        if (typeInStr.EndsWith("drill")) 
        return "ShipDrill"; 
        if (typeInStr.Contains("grind")) 
        return "ShipGrinder"; 
        if (typeInStr.EndsWith("welder")) 
        return "ShipWelder"; 
        if ((typeInStr.Contains("turret") && typeInStr.Contains("gatl"))) 
        return "LargeGatlingTurret"; 
        if ((typeInStr.Contains("turret") && typeInStr.Contains("inter"))) 
        return "LargeInteriorTurret"; 
        if ((typeInStr.Contains("turret") && typeInStr.Contains("miss"))) 
        return "LargeMissileTurret"; 
        if (typeInStr.Contains("gatl")) 
        return "SmallGatlingGun"; 
        if ((typeInStr.Contains("launcher") && typeInStr.Contains("reload"))) 
        return "SmallMissileLauncherReload"; 
        if ((typeInStr.Contains("launcher"))) 
        return "SmallMissileLauncher"; 
        if (typeInStr.Contains("mass")) 
        return "VirtualMass"; 
        if (typeInStr == "warhead") 
        return "Warhead"; 
        if (typeInStr.StartsWith("func")) 
        return "FunctionalBlock"; 
        if (typeInStr.StartsWith("light")) 
        return "LightingBlock"; 
        if (typeInStr.StartsWith("contr")) 
        return "ControlPanel"; 
        if (typeInStr.StartsWith("coc")) 
        return "Cockpit"; 
        if (typeInStr.StartsWith("medi")) 
        return "MedicalRoom"; 
        if (typeInStr.StartsWith("remote")) 
        return "RemoteControl"; 
        if (typeInStr.StartsWith("but")) 
        return "ButtonPanel"; 
        if (typeInStr.StartsWith("cam")) 
        return "CameraBlock"; 
        if (typeInStr.Contains("detect")) 
        return "OreDetector"; 
        return "Unknown"; 
    } 

    public static string FormatLargeNumber(double number, bool compress = true) 
    { 
        if (!compress) 
        return number.ToString( 
            "#,###,###,###,###,###,###,###,###,###"); 

        string ordinals = " kMGTPEZY"; 
        double compressed = number; 

        var ordinal = 0; 

        while (compressed >= 1000) 
        { 
            compressed /= 1000; 
            ordinal++; 
        } 

        string res = Math.Round(compressed, 1, MidpointRounding.AwayFromZero).ToString(); 

        if (ordinal > 0) 
        res += " " + ordinals[ordinal]; 

        return res; 
    } 

    public static void WriteLine(IMyTextPanel textpanel, string message, bool append = true, string title = "") 
    { 
        textpanel.WritePublicText(message + "\n", append); 
        if (title != "") 
        textpanel.WritePublicTitle(title); 
        textpanel.ShowTextureOnScreen(); 
        textpanel.ShowPublicTextOnScreen(); 
    } 

    public static void Debug(string message, bool append = true, string title = "") 
    { 
        if (!EnableDebug) 
        return; 
        if (_DebugTextPanels == null || _DebugTextPanels.Count() == 0) 
        DebugAntenna(message, append, title); 
        else 
        DebugTextPanel(message, append, title); 
    } 

    public static void DebugAntenna(string message, bool append = true, string title = "") 
    { 
        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>(); 

        _GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(blocks); 
        IMyRadioAntenna ant = blocks[0] as IMyRadioAntenna; 
        if (append) 
        ant.SetCustomName(ant.CustomName + message + "\n"); 
        else 
        ant.SetCustomName("PROG: " + message + "\n"); 
    } 

    public static void DebugTextPanel(string message, bool append = true, string title = "") 
    { 
        for (int i = 0; i < _DebugTextPanels.Count(); i++) 
        { 
            IMyTextPanel debugpanel = _DebugTextPanels.Blocks[i] as IMyTextPanel; 
            debugpanel.SetCustomName("[DEBUG] Prog: " + message); 
            WriteLine(debugpanel, message, append, title); 
        } 
    } 
} 


// Dictionary helper 
public class MMDict<TKey, TValue> 
{ 
    public Dictionary<TKey, TValue> dict; 
    public List<TKey> keys; 

    public MMDict(int size = 10) 
    { 
        dict = new Dictionary<TKey, TValue>(size); 
        keys = new List<TKey>(size); 
    } 

    public void AddItem(TKey key, TValue item) 
    { 
        if (!dict.ContainsKey(key)) 
        { 
            keys.Add(key); 
            dict.Add(key, item); 
        } 
    } 

    public void RemoveKey(TKey key) 
    { 
        keys.Remove(key); 
        dict.Remove(key); 
    } 

    public TValue GetItem(TKey key) 
    { 
        if (dict.ContainsKey(key)) 
        { 
            return dict[key]; 
        } 
        else 
        { 
            return default(TValue); 
        } 
    } 

    public TValue GetItemAt(int index) 
    { 
        return dict[keys[index]]; 
    } 

    public bool ContainsKey(TKey key) 
    { 
        return keys.Contains(key); 
    } 

    public int CountAll() 
    { 
        return dict.Count; 
    } 

    public void ClearAll() 
    { 
        keys.Clear(); 
        dict.Clear(); 
    } 

    public void SortAll() 
    { 
        keys.Sort(); 
    } 
} 
// List implementation using dictionary to allow List with custom class 
public class MMList<T> 
{ 
    private Dictionary<int, T> _dictionary; 
    private List<int> _keys; 

    public MMList(int size = 20) 
    { 
        _dictionary = new Dictionary<int, T>(size); 
        _keys = new List<int>(size); 
    } 

    public void RemoveAt(int index) 
    { 
        _dictionary.Remove(_keys[index]); 
        _keys.RemoveAt(index); 
    } 

    public T this[int index] 
    { 
        get { return _dictionary[_keys[index]]; } 
        set { _dictionary[_keys[index]] = value; } 
    } 

    public void Add(T item) 
    { 
        int index = _keys.Count == 0 ? 0 : _keys[_keys.Count - 1] + 1; 
        _dictionary.Add(index, item); 
        _keys.Add(index); 
    } 

    public void ClearItems() 
    { 
        _dictionary.Clear(); 
        _keys.Clear(); 
    } 

    public int Count { get { return _dictionary.Count; } } 

    public bool Remove(T item) 
    { 
        for (int i = 0; i < _keys.Count; i++) 
        { 
            if (_dictionary[_keys[i]].Equals(item)) 
            { 
                _dictionary.Remove(_keys[i]); 
                _keys.RemoveAt(i); 
                return true; 
            } 
        } 
        return false; 
    } 
}
