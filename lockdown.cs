/* v:0.61 
In-game script by MMaster 
         
Closes doors, starts warning lights, dims normal lights and plays sound in sections  
if any of the section air vents report depressurization. 
Reports leaking sections on text panels and LCDs. 
 
QUICK GUIDE: 
 * Load this script to programmable block 
 * Setup timer block to 1 second 
 * Set timer actions to: 1. run programmable block, 2. start timer 
 * Start timer 
  
Setup groups for sections like this:  
1. group needs to be named "O2 Section: Section Name" 
This group should contain:  
 * monitored air vents inside section 
 * doors that will be closed during lockdown & opened when lockdown ends 
 * warning lights that will be turned on during lockdown & off when lockdown ends 
 * sound blocks that will play sound when lockdown starts 
 
2. group is named "O2 Section Close: Section Name" 
This group should contain: 
 * doors that will be closed during lockdown but not opened when it ends  
(useful for NOT opening outside doors) 
  
3. group is named "O2 Section Dim: Section Name" 
This group should contain: 
 * normal lights that will be dimmed (intensity lowered to 0.5) during lockdown 
 
4. group is named "O2 LCDs" 
All LCDs in this group will report leaking sections. 
 * Use LCD Public title to specify header displayed on that LCD.  
 * Set LCD Public title to empty string to hide header. 
 
EVERYTHING IS OPTIONAL - You don't need to use any doors, lights or sound blocks & you don't need to have 2., 3. group and 4. group. 
 
 * Done. 
 
 
ALWAYS check the OWNERSHIP. 
Programmable block needs to be have the same ownership as all the other blocks you want it to control. 
So all doors, lights, airvents, sound blocks and LCDs need to have the same owner as programmable block. 
  
 
Special Thanks 
 Morphologis - for his support of community & awesome videos 
 * check out his YouTube channel: https://www.youtube.com/user/Morphologis 
 
 
Watch MMaster's Steam group: http://steamcommunity.com/groups/mmnews 
Twitter: https://twitter.com/MattsPlayCorner 
and Facebook: https://www.facebook.com/MattsPlayCorner1080p 
for more crazy stuff from me in the future :) 
 */ 
 
void Configuration()  
{ 
    // Group name must start with this to be considered by this script 
    MMConfig.GROUP_TAG = "O2"; 
 
    // Unreliable time - lag level  
    // - set higher if your sections are opening/closing in a loop 
    MMConfig.LAG_LEVEL = 5; 
 
 
    // Section group that contains: 
    // air vents  
    // doors that will be closed during lockdown & opened when lockdown ends 
    // warning lights that will be turned on during lockdown & off when lockdown ends 
    // sound blocks that will play sound when lockdown starts 
    MMConfig.SECTION_TAG = "Section:"; 
    // Doors that will be closed when some air vent reports depressurization 
    // Note: you usually do not want to open airlock doors, but want to close them in case of internal oxygen leak 
    MMConfig.CLOSE_DOORS = "Section Close:"; 
    // Lights that will be dimmed (intensity lowered to 0.5) during lockdown 
    MMConfig.DIM_LIGHTS = "Section Dim:"; 
    // LCDs for leak report 
    MMConfig.LCDS = "LCDs"; 
} 
 
// (for developer) Enable debug to antenna or LCD marked with [DEBUG] 
public static bool EnableDebug = false; 
 
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 
// DO NOT MODIFY ANYTHING BELOW THIS 
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 
void Main() 
{ 
    Configuration(); 
    MMConfig.PostProcess(); 
    MMStorage.Load(Storage); 
 
    // Init MMAPI and debug panels marked with [DEBUG] 
    MM.Init(GridTerminalSystem, EnableDebug); 
 
    OxygenLockdownProgram prog = new OxygenLockdownProgram(); 
    prog.Run(); 
    string store; 
    MMStorage.Save(out store); 
    Storage = store; 
} 
} 
 
 
public static class MMConfig 
{ 
    public static string GROUP_TAG = "O2"; 
    public static string SECTION_TAG = "Section:"; 
    public static string CLOSE_DOORS = "Section Close:"; 
    public static string DIM_LIGHTS = "Section Dim:"; 
    public static string LCDS = "LCDs"; 
    public static int LAG_LEVEL = 5; 
 
    public static void PostProcess() 
    { 
        GROUP_TAG = GROUP_TAG.ToLower(); 
        SECTION_TAG = SECTION_TAG.ToLower(); 
        CLOSE_DOORS = CLOSE_DOORS.ToLower(); 
        DIM_LIGHTS = DIM_LIGHTS.ToLower(); 
        LCDS = LCDS.ToLower(); 
    } 
} 
 
public static class MMStorage 
{ 
    public static List<string> lockdowns = null; 
    // initial unreliable time 
    public static int unreliableTime = MMConfig.LAG_LEVEL * 3; 
 
    public static void Save(out string Storage) 
    { 
        if (lockdowns != null) 
            Storage = String.Join("@|$", lockdowns); 
        else 
            Storage = ""; 
    } 
 
    public static void Load(string Storage) 
    { 
        string[] sep = new [] { "@|$" }; 
        string[] str = Storage.Split(sep, StringSplitOptions.RemoveEmptyEntries); 
        lockdowns = new List<string>(str); 
    } 
 
} 
 
public class MMShipSectionState 
{ 
    public bool IsLockdownActive = false; 
    public bool IsLeaking = false; 
    public bool IsFull = true; 
    public float PressureSum = 0f; 
    public int airVentCount = 0; 
 
    public MMShipSectionState() 
    { 
    } 
 
    public MMShipSectionState(MMShipSectionState state) 
    { 
        IsLockdownActive = state.IsLockdownActive; 
        IsLeaking = state.IsLeaking; 
        IsFull = state.IsFull; 
        PressureSum = state.PressureSum; 
        airVentCount = state.airVentCount; 
    } 
} 
 
public class MMShipSection 
{ 
    public bool isBooting = true; 
    public int counter = 0; 
    public MMShipSectionState state = new MMShipSectionState(); 
    public MMList<MMShipSectionState> prevStates = new MMList<MMShipSectionState>(); 
 
    public List<IMyTerminalBlock> closeDoors = new List<IMyTerminalBlock>(); 
    public List<IMyTerminalBlock> openDoors = new List<IMyTerminalBlock>(); 
    public List<IMyTerminalBlock> warningLights = new List<IMyTerminalBlock>(); 
    public List<IMyTerminalBlock> dimLights = new List<IMyTerminalBlock>(); 
    public List<IMyTerminalBlock> soundBlocks = new List<IMyTerminalBlock>(); 
    public string sectionName = ""; 
 
    public MMShipSection(string name) 
    { 
        sectionName = name; 
        state.IsLockdownActive = MMStorage.lockdowns.Contains(sectionName); 
    } 
 
    public void SetLockdown(bool active) 
    { 
        state.IsLockdownActive = active; 
 
        if (active) 
        { 
            MMStorage.lockdowns.Add(sectionName); 
        } 
        else 
        { 
            MMStorage.lockdowns.Remove(sectionName); 
        } 
    } 
 
    public void Reset() 
    { 
        state.PressureSum = 0f; 
        state.airVentCount = 0; 
        state.IsLockdownActive = MMStorage.lockdowns.Contains(sectionName); 
        state.IsLeaking = false; 
        state.IsFull = true; 
        closeDoors.Clear(); 
        openDoors.Clear(); 
        warningLights.Clear(); 
        dimLights.Clear(); 
        soundBlocks.Clear(); 
    } 
 
    public float GetPressure() 
    { 
        return (state.airVentCount <= 0 ? 0f : state.PressureSum / state.airVentCount); 
    } 
 
    // NOT USED THROWS WEIRD EXCEPTION 
    public bool IsReliable() 
    { 
        if (prevStates.Count <= 1 || MMStorage.unreliableTime > 0) 
            return false; 
 
        float pres = (state.airVentCount <= 0 ? 0f : state.PressureSum / state.airVentCount); 
        MMShipSectionState s = prevStates[prevStates.Count - 1]; 
        float p = (s.airVentCount <= 0 ? 0f : s.PressureSum / s.airVentCount); 
        float trend = p - pres; 
        pres = p; 
 
        // check 4 previous states 
        for (int i = prevStates.Count - 1; i > Math.Max(prevStates.Count - 4, 0); --i) 
        { 
            /*s = prevStates[i]; 
            p = (s.airVentCount <= 0 ? 0f : s.PressureSum / s.airVentCount); 
            float t = p - pres; 
            if (!((t <= 0 && trend <= 0) || (t >= 0 && trend >= 0))) 
                return false; 
            pres = p;*/ 
        } 
 
        return true; 
    } 
 
    public void CalculateRealState() 
    { 
        if (state.airVentCount <= 0) 
            prevStates.ClearItems(); 
 
        // add previous state before modification 
        MMShipSectionState s = new MMShipSectionState(state); 
        prevStates.Add(s); 
        if (prevStates.Count > MMConfig.LAG_LEVEL + 4) 
        { 
            prevStates.RemoveAt(0); 
        } 
         
        // first 9 seconds are not reliable 
        if (isBooting)  
        { 
            // 9th state is considered first valid state 
            if (prevStates.Count >= 9) { 
                isBooting = false; 
                prevStates.ClearItems(); 
                return; 
            } 
            state.IsFull = false; 
            state.IsLeaking = false; 
            return; 
        } 
 
        if (state.IsFull) 
        { 
            int check_cnt = (MMStorage.unreliableTime > 0 ? MMConfig.LAG_LEVEL + 3 : 2); 
            // check previous states 
            for (int i = prevStates.Count - 1; i > Math.Max(prevStates.Count - (check_cnt + 1), 0); --i) 
            { 
                if (!prevStates[i].IsFull) 
                { 
                    state.IsFull = false; 
                    return; 
                } 
            } 
            return; 
        } 
 
        if (state.IsLeaking) 
        { 
            int check_cnt = (MMStorage.unreliableTime > 0 ? MMConfig.LAG_LEVEL + 3 : 2); 
            bool allFull = true; 
            // check previous states 
            for (int i = prevStates.Count - 1; i > Math.Max(prevStates.Count - (check_cnt + 1), 0); --i) 
            { 
                if (!prevStates[i].IsLeaking) 
                    state.IsLeaking = false; 
                if (!prevStates[i].IsFull) 
                    allFull = false; 
            } 
            if (allFull) 
                state.IsLeaking = true; 
            return; 
        } 
    } 
} 
 
public class SectionList 
{ 
    public MMList<MMShipSection> list = new MMList<MMShipSection>(); 
 
    public void AddSection(MMShipSection section) 
    { 
        list.Add(section); 
    } 
} 
 
public class MMShipSectionCollection 
{ 
    // static - holds section state for lag probability calculation 
    public static MMDict<string, MMShipSection> sectionsDict = new MMDict<string, MMShipSection>(); 
 
    public List<IMyTextPanel> lcds = new List<IMyTextPanel>(); 
    public Dictionary<IMyTerminalBlock, SectionList> sectionsByBlock = new Dictionary<IMyTerminalBlock, SectionList>(); 
 
    public void ClearSectionsBlocks() 
    { 
        for (int i = 0; i < sectionsDict.CountAll(); i++) 
        { 
            sectionsDict.GetItemAt(i).Reset(); 
        } 
    } 
 
    public int SectionCount() 
    { 
        return sectionsDict.CountAll(); 
    } 
 
    public MMShipSection GetSection(string name) 
    { 
        MM.Debug("Get Section: " + name); 
        if (sectionsDict.ContainsKey(name)) 
            return sectionsDict.GetItem(name); 
 
        MMShipSection sect = new MMShipSection(name); 
        sectionsDict.AddItem(name, sect); 
        return sect; 
    } 
 
    public MMShipSection GetSectionAt(int idx) 
    { 
        return sectionsDict.GetItemAt(idx); 
    } 
 
    public void AddBlock(IMyTerminalBlock block, MMShipSection section) 
    { 
        if (!sectionsByBlock.ContainsKey(block)) 
        { 
            sectionsByBlock.Add(block, new SectionList()); 
        } 
        sectionsByBlock[block].AddSection(section); 
    } 
 
    public void AddBlockGroup(IMyBlockGroup group) 
    { 
 
        if (group.Name.ToLower() == MMConfig.GROUP_TAG + ' ' + MMConfig.LCDS) 
        { 
            for (int i = 0; i < group.Blocks.Count; i++) 
            { 
                if (MM.IsBlockOfExactType(group.Blocks[i], "TextPanel")) 
                    lcds.Add(group.Blocks[i] as IMyTextPanel); 
            } 
            return; 
        } 
 
        int seppos = group.Name.IndexOf(':'); 
        if (seppos < 0) 
            return; 
 
        string tag = group.Name.Substring(0, seppos + 1).ToLower(); 
        string name = (group.Name.Length > seppos + 1 ? group.Name.Substring(seppos + 1).Trim() : ""); 
 
        if (tag == MMConfig.GROUP_TAG + ' ' + MMConfig.SECTION_TAG) 
        { 
            MMShipSection section = GetSection(name); 
            for (int i = 0; i < group.Blocks.Count; i++) { 
                IMyTerminalBlock block = group.Blocks[i]; 
                if (MM.IsBlockOfExactType(block, "LightingBlock")) 
                { 
                    AddBlock(block, section); 
                    section.warningLights.Add(block); 
                    continue; 
                } 
                if (MM.IsBlockOfExactType(block, "Door")) 
                { 
                    if (section.closeDoors.Contains(block)) 
                        continue; 
                    AddBlock(block, section); 
                    section.openDoors.Add(block); 
                    continue; 
                } 
                if (MM.IsBlockOfExactType(block, "AirVent")) 
                { 
                    float perc = MM.GetAirVentPressure(block); 
                    if (perc < 0) 
                        section.state.IsLeaking = true; 
                    else 
                        section.state.PressureSum += perc; 
                    if (perc < 99) 
                        section.state.IsFull = false; 
                    section.state.airVentCount++; 
                    continue; 
                } 
                if (MM.IsBlockOfExactType(block, "SoundBlock")) 
                { 
                    AddBlock(block, section); 
                    section.soundBlocks.Add(block); 
                    continue; 
                } 
            } 
            return; 
        } 
 
        if (tag == MMConfig.GROUP_TAG + ' ' + MMConfig.CLOSE_DOORS) 
        { 
            MMShipSection section = GetSection(name); 
 
            for (int i = 0; i < group.Blocks.Count; i++) 
            { 
                IMyTerminalBlock block = group.Blocks[i]; 
                if (MM.IsBlockOfExactType(block, "Door")) 
                { 
                    if (section.openDoors.Contains(block)) 
                        section.openDoors.Remove(block); 
                    else 
                        AddBlock(block, section); 
                    section.closeDoors.Add(block); 
                    continue; 
                } 
            } 
            return; 
        } 
 
        if (tag == MMConfig.GROUP_TAG + ' ' + MMConfig.DIM_LIGHTS) 
        { 
            MMShipSection section = GetSection(name); 
 
            for (int i = 0; i < group.Blocks.Count; i++) 
            { 
                if (MM.IsBlockOfExactType(group.Blocks[i], "LightingBlock")) 
                { 
                    AddBlock(group.Blocks[i], section); 
                    section.dimLights.Add(group.Blocks[i]); 
                } 
            } 
            return; 
        } 
 
    } 
    public bool StartLockdown(MMShipSection section) 
    { 
        MM.Debug("StartLockdown: " + section.sectionName); 
        if (section.state.IsLockdownActive) 
        { 
            if (section.counter <= 0) 
                return false; 
            section.counter--; 
        } 
        else 
        { 
            section.counter = 3; 
            section.SetLockdown(true); 
        } 
         
        for (int i = 0; i < section.closeDoors.Count; i++) 
            section.closeDoors[i].ApplyAction("Open_Off"); 
        for (int i = 0; i < section.openDoors.Count; i++) 
            section.openDoors[i].ApplyAction("Open_Off"); 
        for (int i = 0; i < section.warningLights.Count; i++) 
            section.warningLights[i].ApplyAction("OnOff_On"); 
        for (int i = 0; i < section.dimLights.Count; i++) 
        { 
            IMyLightingBlock light = section.dimLights[i] as IMyLightingBlock; 
            if (light.CustomName.Contains(" ~Dim ")) 
                continue; 
            light.SetCustomName(light.CustomName + " ~Dim " + light.Intensity.ToString("F1")); 
            light.SetValueFloat("Intensity", 0.5f); 
        } 
        for (int i = 0; i < section.soundBlocks.Count; i++) 
            section.soundBlocks[i].ApplyAction("PlaySound"); 
        return true; 
    } 
 
    public bool IsBlockLockedDown(IMyTerminalBlock block) 
    { 
        MMList<MMShipSection> secs = sectionsByBlock[block].list; 
        for (int j = 0; j < secs.Count; j++) 
            if (secs[j].state.IsLockdownActive) 
                return true; 
        return false; 
    } 
 
    public bool StopLockdown(MMShipSection section) 
    { 
        MM.Debug("StopLockdown: " + section.sectionName); 
        if (!section.state.IsLockdownActive) { 
            if (section.counter <= 0) 
                return false; 
            section.counter--; 
        } else { 
            section.counter = 3; 
            section.SetLockdown(false); 
        } 
         
        IMyTerminalBlock block; 
 
        for (int i = 0; i < section.openDoors.Count; i++) 
        { 
            block = section.openDoors[i]; 
            if (!IsBlockLockedDown(block)) 
                block.ApplyAction("Open_On"); 
        } 
        for (int i = 0; i < section.warningLights.Count; i++) 
        { 
            block = section.warningLights[i]; 
            if (!IsBlockLockedDown(block)) 
                block.ApplyAction("OnOff_Off"); 
        } 
        for (int i = 0; i < section.dimLights.Count; i++) 
        { 
            IMyLightingBlock light = section.dimLights[i] as IMyLightingBlock; 
            string name = light.CustomName; 
            int seppos = name.LastIndexOf(" ~Dim "); 
            if (seppos < 0) 
                continue; 
 
            light.SetCustomName(name.Substring(0, seppos)); 
            string str = name.Substring(seppos + 6); 
            float intensity = 0f; 
            if (float.TryParse(str, out intensity)) 
                light.SetValueFloat("Intensity", intensity); 
        } 
        for (int i = 0; i < section.soundBlocks.Count; i++) 
        { 
            block = section.soundBlocks[i]; 
            if (!IsBlockLockedDown(block)) 
                block.ApplyAction("StopSound"); 
        } 
        return true; 
    } 
} 
 
public class OxygenLockdownProgram 
{ 
    public MMShipSectionCollection sections = new MMShipSectionCollection(); 
 
    public void Run() 
    { 
        sections.ClearSectionsBlocks(); 
        for (int i = 0; i < MM._GridTerminalSystem.BlockGroups.Count; i++) 
        { 
            IMyBlockGroup group = MM._GridTerminalSystem.BlockGroups[i]; 
            if (!group.Name.ToLower().StartsWith(MMConfig.GROUP_TAG)) 
                continue; 
            sections.AddBlockGroup(group); 
        } 
        MM.Debug("Sections processed. Applying.."); 
 
        MMStorage.unreliableTime = Math.Max(MMStorage.unreliableTime - 1, 0); 
        ProcessLCDTitle(); 
 
        MM.Debug("Unreliable time: " + MMStorage.unreliableTime.ToString()); 
 
        bool allFull = true; 
        bool change = false; 
        for (int i = 0; i < sections.SectionCount(); i++) 
        { 
            MMShipSection section = sections.GetSectionAt(i); 
            MM.Debug("Apply: " + section.sectionName + " [" + section.GetPressure().ToString("F1") + "%]"); 
            section.CalculateRealState(); 
 
            if (section.state.IsLeaking) 
            { 
                allFull = false; 
                // not pressurized 
                if (sections.StartLockdown(section)) 
                    change = true; 
                WriteLCD(section.sectionName + " is leaking."); 
                continue; 
            } 
 
            if (section.state.IsFull) 
            { 
                // if we got here everything is fine.. open doors, turn off lights 
                if (sections.StopLockdown(section)) 
                    change = true; 
 
                continue; 
            } 
            allFull = false; 
            WriteLCD(section.sectionName + " is filling up [" + section.GetPressure().ToString("F1") + "%]"); 
            MM.Debug("No change"); 
        } 
 
        if (change) 
            MMStorage.unreliableTime = MMConfig.LAG_LEVEL; 
 
        if (allFull) 
            WriteLCD("All sections are sealed."); 
    } 
    public void WriteLCD(string message, bool append = true) 
    { 
        for (int i = 0; i < sections.lcds.Count; i++) 
        { 
            IMyTextPanel lcd = sections.lcds[i]; 
            MM.WriteLine(lcd, message, append); 
        } 
    } 
    public void ProcessLCDTitle() 
    { 
        for (int i = 0; i < sections.lcds.Count; i++) 
        { 
            IMyTextPanel lcd = sections.lcds[i]; 
            string title = lcd.GetPublicTitle(); 
            if (title == "") 
                lcd.WritePublicText(""); 
            else if (title == "Public title") 
                MM.WriteLine(lcd, "Oxygen Status\n", false); 
            else 
                MM.WriteLine(lcd, title + "\n", false); 
        } 
    } 
} 
 
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
