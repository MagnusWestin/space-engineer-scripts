// The north pole is at 0,-60000,0 and the South pole is 0,60000,0. The sun rotates around the y axis of
// the star system. Points on the equator are +/- 60000 on the x and z axis'. These points will be below
// the surface by various depths due to the height map. If you line up with the center of the planet at
// each point you can find each of these points right at the surface. 

// LCD font size is 0.6

public Program()
{
    // The constructor, called only once every session and always before any other method is called. Use it to
    // initialize your script. 
    //StartAllTimers();
}

public void Save() {
    // Called when the program needs to save its state. Use this method to save your state to the Storage field
    // or some other means.
}

static public bool g_oFirstInit = false;
static public float g_oLastMaxSolarPanelOutput = 0.0f;
static public IMyGridTerminalSystem gts = null;
// 0 = Startup, 1 = tracking sun, 2 = resting
static public int g_oSolarPanelTrackingMode = 0;
static public int g_oRestingCount = 0;
static public int g_oTickCount = 0;
static public Vector3D g_oPosition = new Vector3D( 0, 0, 0 );

public void Main(string argument)
{
    Echo( argument + " tick: " + g_oTickCount );
    gts = GridTerminalSystem;
    g_oTickCount++;
    if( g_oTickCount > 100 )
        g_oTickCount = 0;

    if( argument == "BA" )
    {
        RunStationCode( argument );
    }
    else if( argument == "L1" )
    {
        RunLargeShipCode( argument );
    }
    else if( argument == "S1" || argument == "S2" || argument == "S3" )
    {
        RunSmallShipCode( argument );
    }
    else if( argument == "AtmoMiner" )
    {
        RunAMCode( argument );
    }
    else
    {
        throw new Exception( "Wrong argument: " + argument );
    }

    g_oFirstInit = true;
}

public void RunAMCode( string argument )
{
    LcdPanel oLcdLeft  = new LcdPanel( "AM LCD Left" );
    LcdPanel oLcdRight = new LcdPanel( "AM LCD Right" );
    if( g_oFirstInit == false )
    {
        oLcdLeft.MakePublicTextActive();
        oLcdRight.MakePublicTextActive();
    }

    string oTextLcdLeft = GetScriptInfoAndTime( argument );
    oTextLcdLeft += GetTickCountText();
    oTextLcdLeft += "-----------------------------------\n";

    oTextLcdLeft += "==== Energy ====\n";
    oTextLcdLeft += GetBatteryInfo( argument );
    oTextLcdLeft += GetReactorInfo();
    oLcdLeft.UpdateText( oTextLcdLeft );

    string oTextLcdRight = "==== CARGO ====\n" + GetCargoContainerInfo( 170000 );
    oLcdRight.UpdateText( oTextLcdRight );
}

public void RunStationCode( string argument )
{
    LcdPanel oLcd01  = new LcdPanel( "BA LCD Base Status" );
    LcdPanel oLcdDebug = new LcdPanel( "BA LCD Debug" );
    LcdPanel oLcdBatteryStatus = new LcdPanel( "BA LCD Battery Status" );
    if( g_oFirstInit == false )
    {
        oLcd01.MakePublicTextActive();
        oLcdDebug.MakePublicTextActive();
        oLcdBatteryStatus.MakePublicTextActive();
    }

    string oTextLcd01 = DisplayConsumablesLevel();
    oTextLcd01 += "\n== Solar Panels == ";
 	oTextLcd01 = DisplaySolarPanelPower( oTextLcd01 );
    oLcd01.UpdateText( oTextLcd01 ); 

    // Rotor oRotor = new Rotor( "Hab 1 Rotor Solar Track" );
    // SolarPanel oPanel = new SolarPanel( "Hab 1 Solar Panel Tracking" );
    // oTextLcdRight = RunSolarPanelAlignment( oRotor, oPanel, oTextLcdRight );
    // oLcdRight.UpdateText( oTextLcdRight ); 
    
    string oTextDebug = GetScriptInfoAndTime( argument );
    oTextDebug += GetTickCountText();
    oLcdDebug.UpdateText( oTextDebug );

    Battery oBattery = new Battery( "BA Battery 1" );
    string oLcdBatteryStatusText = "Status: " + oBattery.GetRechargeStatus();
	oLcdBatteryStatusText = DisplaySolarPanelPower( oLcdBatteryStatusText );
    oLcdBatteryStatus.UpdateText( oLcdBatteryStatusText );
}

public void RunLargeShipCode( string argument )
{
    LcdPanel oLcdLeft  = new LcdPanel( "L1-LCD Panel Left" );
    LcdPanel oLcdRight = new LcdPanel( "L1-LCD Panel Right" );
    LcdPanel oLcdOre = new LcdPanel( "L1-LCD Panel Ore" );
    if( g_oFirstInit == false )
    {
        oLcdLeft.MakePublicTextActive();
        oLcdRight.MakePublicTextActive();
        oLcdOre.MakePublicTextActive();
    }

    string oTextLcdLeft = DisplayConsumablesLevel();
    oTextLcdLeft += "\n\n" + GetReactorInfo();
    oLcdLeft.UpdateText( oTextLcdLeft ); 

    string oTextLcdRight = GetScriptInfoAndTime( argument );
    oTextLcdRight += GetTickCountText();
    oTextLcdRight += "-----------------------------------\n";
    oTextLcdRight += String.Format( "Speed : {0,6:F1} m/s\n", GetSpeed() );
    //oTextLcdRight += String.Format( "Height: {0,6:F1} m\n", GetHeight() );
    oLcdRight.UpdateText( oTextLcdRight ); 

    string oTextLcdOre = GetOreAmount();
    oLcdOre.UpdateText( oTextLcdOre );     
}

public void RunSmallShipCode( string argument )
{
    LcdPanel oLcdLeft  = new LcdPanel( argument + "LCD Left" );
    LcdPanel oLcdRight = new LcdPanel( argument + "LCD Right" );
    LcdPanel oLcdMiddle = new LcdPanel( argument + "LCD Middle" );
    if( g_oFirstInit == false )
    {
        oLcdLeft.MakePublicTextActive();
        oLcdRight.MakePublicTextActive();
        oLcdMiddle.MakePublicTextActive();
    }

    string oTextLcdLeft = GetBatteryInfo( argument );
    oTextLcdLeft += "\n" + GetReactorInfo();
    oLcdLeft.UpdateText( oTextLcdLeft );

    string oTextLcdRight = GetTickCountText();
    oTextLcdRight += "\n==== CARGO ====\n" + GetCargoContainerInfo( 28000 );
    oLcdRight.UpdateText( oTextLcdRight );

    string oTextLcdMiddle = GetScriptInfoAndTime( argument );
    oLcdMiddle.UpdateText( oTextLcdMiddle );
}

public string GetScriptInfoAndTime( string argument )
{
    string oText = "Script argument: " + argument + "\n";
    oText += "Time: " + System.DateTime.Now.ToString( "yyyy-MM-dd HH:mm" ) + "\n";
    return oText;
}

public string DebugOutput()
{
    string oText = "\n==== DEBUG ====\n";
    Battery oBat = new Battery( "Hab 1 Battery 1" );
    oText += oBat.GetDetailedInfo();
    return oText;
}

// =================================================================================================
// Application Code

public void DumpOreIfEjectorIsOn( string oName )
{
    IMyFunctionalBlock oBlock = gts.GetBlockWithName( oName ) as IMyFunctionalBlock;
    if( oBlock == null )
        throw new Exception( oName + " block not found, check name" );
    if( oBlock.Enabled == true )
    {

    }
}

public string DisplayConsumablesLevel()
{
    List< IMyTerminalBlock > oOxygenList = GetListOfBlocksOnLocalGrid< IMyOxygenTank >();
    string oText = "";
    for( int i = 0; i < oOxygenList.Count; i++ )
    {
        OxygenTank oOxygen = new OxygenTank( oOxygenList[ i ] );
        oText += oOxygen.GetCustomName() + " level:";
        oText += "\n" + GetPercentMeter( oOxygen.GetLevel() ) 
                                      + String.Format( "{0:F1}", oOxygen.GetLevel() ) + "%\n";
    }


    List< IMyTerminalBlock > oBatteryList = GetListOfBlocksOnLocalGrid< IMyBatteryBlock >();
    for( int i = 0; i < oBatteryList.Count; i++ )
    {
        Battery oBlock = new Battery( oBatteryList[ i ] );
        oText += "\n" + oBatteryList[ i ].CustomName + " Level (" + oBlock.GetRechargeStatus() + "): " + oBlock.GetStoredPowerAsText();
    }

    return oText;
}

public string GetTickCountText()
{

    return "Tick Count: " + g_oTickCount + "\n";
}

public string DisplaySolarPanelPower( string oText )
{
    List< IMyTerminalBlock > oSolarPanelList = GetListOfBlocks< IMySolarPanel  >();
    float oTotalPower = 0.0f;
    int oTotalPanels = 0;
    for( int i = 0; i < oSolarPanelList.Count; i++ )
    {
        SolarPanel oPanel = new SolarPanel( oSolarPanelList[ i ] );
        oTotalPanels++;
        oTotalPower += oPanel.GetMaxOutput();
    }
    oText += "\nTotal Solar Power (" + oTotalPanels + "): " + String.Format( "{0:F1}", oTotalPower ) + " kW";
    return oText;
}

public string GetBatteryInfo( string oName )
{
    List< IMyTerminalBlock > oBatteryList = GetListOfBlocksOnLocalGrid< IMyBatteryBlock  >();
    string oText = "";
    for( int i = 0; i < oBatteryList.Count; i++ )
    {
        Battery oBattery = new Battery( oBatteryList[ i ] );
        float oPercent = oBattery.GetStoredPowerAsPercent();
        oText += oBattery.GetCustomName() + "\n";
        oText += GetPercentMeter( oPercent ) + String.Format( " {0:F1}%\n", oPercent );
    }
    return oText;
}

public string GetCargoContainerInfo( int oMaxMass )
{
    List< IMyTerminalBlock > oContainerList = GetListOfBlocksOnLocalGrid< IMyCargoContainer >();
    string oText = "";
    float oTotalMass = 0.0f;
    for( int i = 0; i < oContainerList.Count; i++ )
    {
        CargoContainer oInv = new CargoContainer( oContainerList[ i ] );
        oText += oInv.GetCustomName() + "\n";
        oText += GetPercentMeter( oInv.GetSpaceLeftAsPercent() ) + String.Format( " {0:F1}%\n", oInv.GetSpaceLeftAsPercent() );
        oTotalMass += oInv.GetMass();
    }
    oText += "----------------------------\n";
    oText += "Total Mass: " + oTotalMass + " kg\nMax Mass: " + oMaxMass + " kg\n";
    oText += GetPercentMeter( ( oTotalMass / (float)oMaxMass ) * 100.0f ) + "\n";
    return oText;
}

public string GetOreAmount()
{
    StringBuilder displaytext = new StringBuilder();

    // get all attached inventories in a list
    List<IMyInventory> inventories = GetInventoryItems();

    // get list of materials to look at
    List<rawMaterial> rawMaterials = buildRawMaterialList(); 

    // loop through all inventories and add up the material amounts 
    rawMaterials = addAmountsToMaterialsList( inventories, rawMaterials );

    for( int i = 0; i < rawMaterials.Count; ++i )
    {
        displaytext.Append( rawMaterials[ i ].GetString() + "\n");
    }
    return displaytext.ToString();
}


// This function looks through all blocks in the system and returns all inventories within
public List<IMyInventory> GetInventoryItems()
{
    List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>(); //capture all blocks
    GridTerminalSystem.GetBlocks( allBlocks ); 
    List<IMyTerminalBlock> inventoryBlocks = new List<IMyTerminalBlock>(); // all blocks w/ inventories
    List<IMyInventory> inventories = new List<IMyInventory>(); //actual inventories

    // look for all blocks that have inventory
    for( int x = 0; x < allBlocks.Count; ++x )
    {
        if( allBlocks[ x ].HasInventory() == true ) //it's an inventory block
        {
           inventoryBlocks.Add( allBlocks[ x ] );
        }
    }

    // add all inventories to the list
    for( int i = 0; i < inventoryBlocks.Count; i++ )
    {
        // all blocks in this group have inventories, so add the first          
        inventories.Add( inventoryBlocks[ i ].GetInventory( 0 ) );

        // if more than one inventory, add the 2nd
        if (inventoryBlocks[i].GetInventoryCount() > 1)
        {
            IMyInventory inventory = inventoryBlocks[i].GetInventory(1);
            inventories.Add(inventory);
        }
    }
    return inventories;
}

// This function builds raw material objects based on the base materials in the game
// and puts them in a list
public List<rawMaterial> buildRawMaterialList()
{
    var materials = new List< rawMaterial >();
    materials.Add( new rawMaterial( "Stone",         "Gravel   ", 0, 0 ) );
    materials.Add( new rawMaterial( "Ice",           "Ice      ", 0, 0 ) );
    materials.Add( new rawMaterial( "Iron Ore",      "Iron     ", 0, 0 ) );
    materials.Add( new rawMaterial( "Silicon Ore",   "Silicon  ", 0, 0 ) );
    materials.Add( new rawMaterial( "Nickel Ore",    "Nickel   ", 0, 0 ) );
    materials.Add( new rawMaterial( "Cobalt Ore",    "Cobalt   ", 0, 0 ) );
    materials.Add( new rawMaterial( "Silver Ore",    "Silver   ", 0, 0 ) );
    materials.Add( new rawMaterial( "Gold Ore",      "Gold     ", 0, 0 ) );
    materials.Add( new rawMaterial( "Magnesium Ore", "Magnesium", 0, 0 ) );
    materials.Add( new rawMaterial( "Uranium Ore",   "Uranium  ", 0, 0 ) );
    materials.Add( new rawMaterial( "Platinum Ore",  "Platinum ", 0, 0 ) );

    return materials;
}

// This function takes in a group of inventories and adds quantity of materials to the list
public List<rawMaterial> addAmountsToMaterialsList(List<IMyInventory> inventories, List<rawMaterial> rawMaterials)
{
    for (int i = 0; i < inventories.Count; i++)
    {
        // get all items in the inventory item
        List<IMyInventoryItem> items = inventories[i].GetItems();

        // for each item in this inventory
        for (int j = 0; j < items.Count; j++)
        {
            // check the item to see if it matches up with a raw material
            for (int k = 0; k < rawMaterials.Count; k++)
            {
                // check the current item and see if its a refined material, if so, add the current quantity to the materials list
                if( items[ j ].Content.SubtypeName.Contains( rawMaterials[ k ].GetMaterialName() ) && items[ j ].Content.ToString().Contains( "Ore" ) )
                {
                    rawMaterials[k] = rawMaterials[k].UpdateRawAmount((double) items[j].Amount);
                }

                // check the current item and see if its a refined material, if so, add the current quantity to the materials list
                if( items[ j ].Content.SubtypeName.Contains( rawMaterials[ k ].GetMaterialName() ) && items[ j ].Content.ToString().Contains( "Ingot" ) )
                {
                    rawMaterials[k] = rawMaterials[k].UpdateRefinedAmount((double)items[j].Amount);
                }
            }
        }
    }
    return rawMaterials;
}

// This class represents an ore/ingot pair of materials
// i.e. Stone - Gravel
// it allows for calculating the conversion from stone to gravel (via a refinery)
// so that this information can be used elsewhere
public struct rawMaterial
{
    public string rawMaterialName;
    public string refinedMaterialName;
    public double rawMaterialAmount;
    public double refinedMaterialAmount;
    public rawMaterial(string rawMaterial, string refinedMaterial, double rawAmount, double refinedAmount)
    {
        rawMaterialName = rawMaterial;
        refinedMaterialName = refinedMaterial;
        rawMaterialAmount = rawAmount;
        refinedMaterialAmount = refinedAmount;
        }

    // Returns details about the object in a nicely formatted string
    // ex. Stone : 123 -- Gravel : 1234 Total : 123543
    public String GetString()
    {
        //return refinedMaterialName + ": " + Math.Round(rawMaterialAmount, 2) + " | " + Math.Round( refinedMaterialAmount, 2 );
        return String.Format( "{0}: {1,10:F2}    | {2,10:F2}", refinedMaterialName, rawMaterialAmount, refinedMaterialAmount );
    }

    // Gets the "name of the material" excluding ore/ingot descriptor
    public String GetMaterialName()
    {
        String name = rawMaterialName;
        if (rawMaterialName.IndexOf(" ") > 0)
        {
            name = rawMaterialName.Substring(0, rawMaterialName.IndexOf(" "));
        }
        return name;
    }

    // Creates a new copy of the structure with the updated raw material amount
    public rawMaterial UpdateRawAmount(double updateAmount)
    {
        double updatedAmount = updateAmount + rawMaterialAmount;
        rawMaterial newMaterial = new rawMaterial(rawMaterialName, refinedMaterialName, updatedAmount, refinedMaterialAmount );
        return newMaterial;
    }

    // Creates a new copy of the structure with the updated refined material amount
    public rawMaterial UpdateRefinedAmount(double updateAmount)
    {
        double updatedAmount = updateAmount + refinedMaterialAmount;
        rawMaterial newMaterial = new rawMaterial(rawMaterialName, refinedMaterialName, rawMaterialAmount, updatedAmount );
        return newMaterial;
    }
}

public string GetReactorInfo()
{
    List< IMyTerminalBlock > oReactorList = GetListOfBlocksOnLocalGrid< IMyReactor >();
    string oText = "";
    for( int i = 0; i < oReactorList.Count; i++ )
    {
        Reactor oReactor = new Reactor( oReactorList[ i ] );
        float oPercent = oReactor.GetOutputAsPercent();
        oText += oReactor.GetCustomName() + " Power output:\n";
        oText += GetPercentMeter( oPercent ) + String.Format( " {0:F1}", oPercent ) + "%\n";;
        oText += "Uranium: " + oReactor.GetUraniumAmount() + "\n";
    }
    return oText;
}

// This tracks the sun, you need to be close to the north pole for it to work
public string RunSolarPanelAlignment( Rotor oRotor, SolarPanel oSolarPanel, string oStatusText )
{
    float currentOutput = oSolarPanel.GetMaxOutput();
    oStatusText += "\nPrevious: " + g_oLastMaxSolarPanelOutput;
    oStatusText += "\nCurrent : " + currentOutput;

    if( g_oSolarPanelTrackingMode == 0 ) // Startup
    {
        g_oLastMaxSolarPanelOutput = currentOutput;
        g_oSolarPanelTrackingMode = 1;
        oStatusText += "\nMode: tracking sun";
        oRotor.SetSpeed( -0.2f );
    }
    else if( g_oSolarPanelTrackingMode == 1 ) // tracking sun
    {
        if(currentOutput < g_oLastMaxSolarPanelOutput )
        {
            oRotor.SetSpeed( 0.0f );
            g_oSolarPanelTrackingMode = 2;
            oStatusText += "\nMode: resting";
        }
        else
        {
            oStatusText += "\nMode: tracking sun";
            oRotor.SetSpeed( -0.2f );
        }
        g_oLastMaxSolarPanelOutput = currentOutput;
    }
    else if( g_oSolarPanelTrackingMode == 2 ) // resting
    {
        g_oRestingCount++;
        if( g_oRestingCount > 50 )
        {
            g_oRestingCount = 0;
            if( currentOutput < g_oLastMaxSolarPanelOutput )
            {
                g_oSolarPanelTrackingMode = 1;
                oStatusText += "\nMode: tracking sun";
                oRotor.SetSpeed( -0.2f );
            }
            else
            {
                oStatusText += "\nMode: resting";
            }
        }
        else
        {
            oStatusText += "\nMode: resting count " + g_oRestingCount;
        }
        g_oLastMaxSolarPanelOutput = currentOutput;
    }
    return oStatusText;
}

// =================================================================================================
// Block Classes

public class BaseBlock
{
    IMyTerminalBlock m_oTerminalBlock;

    public BaseBlock()
    {
        m_oTerminalBlock = null;
    }

    public BaseBlock( IMyTerminalBlock oBlock )
    {
        m_oTerminalBlock = oBlock;
    }

    public bool DoesDetailedInfoValueExist( string oName )
    {
        return !String.IsNullOrEmpty( GetDetailedInfoValue( oName ) );
    }

    public string GetCustomName()
    {
        return m_oTerminalBlock.CustomName;
    }

    public string GetDetailedInfo()
    {
        return m_oTerminalBlock.DetailedInfo;
    }

    public string GetDetailedInfoValue( string oName )
    {
        string[] oLines = GetDetailedInfo().Split( new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None );
        for( int i = 0; i < oLines.Length; i++ )
        {
            string[] oLine = oLines[ i ].Split( ':' );
            if( oLine[ 0 ].Equals( oName ) )
            {
                return oLine[ 1 ].Substring( 1 );
            }
        }
        return "";
    }

    public void Init( IMyTerminalBlock oBlock )
    {
        m_oTerminalBlock = oBlock;
    }
}

public class Battery : BaseBlock
{
    IMyBatteryBlock m_oBlock;
    public Battery( string oBlockName )
    {
        m_oBlock = gts.GetBlockWithName( oBlockName ) as IMyBatteryBlock;
        if( m_oBlock == null )
            throw new Exception( oBlockName + " block not found, check name" );
        Init( m_oBlock );
    }

    public Battery( IMyBatteryBlock oBlock ) : base( oBlock )
    {
        m_oBlock = oBlock;
    }

    public Battery( IMyTerminalBlock oBlock ) : base( oBlock )
    {
        m_oBlock = (IMyBatteryBlock)oBlock;
    }

    public string GetRechargeStatus()
    {
        if( DoesDetailedInfoValueExist( "Fully recharged in" ) == true )
            return "Recharging";
        else
            return "Normal";
    }

    public float GetMaxPowerAsFloat()
    {
        string oText = GetDetailedInfoValue( "Max Stored Power" );
        string[] oValues = oText.Split( ' ' );
        float oValue = 0.0f;
        float.TryParse( oValues[ 0 ], out oValue );
        float oPow = (float)Math.Pow( 1000.0f, ".kMGTPEZY".IndexOf( oValues[ 1 ].Substring( 0, 1 ) ) );
        return oValue * oPow;
    }

    public string GetStoredPowerAsText()
    {
        return GetDetailedInfoValue( "Stored power" );
    }

    public float GetStoredPowerAsFloat()
    {
        string oText = GetDetailedInfoValue( "Stored power" );
        string[] oValues = oText.Split( ' ' );
        float oValue = 0.0f;
        float.TryParse( oValues[ 0 ], out oValue );
        float oPow = (float)Math.Pow( 1000.0f, ".kMGTPEZY".IndexOf( oValues[ 1 ].Substring( 0, 1 ) ) );
        return oValue * oPow;
    }

    public float GetStoredPowerAsPercent()
    {
        float oStoredPower = GetStoredPowerAsFloat();
        float oMaxPower = GetMaxPowerAsFloat();
        float oPercent = ( oStoredPower / oMaxPower ) * 100.0f;
        return oPercent;
    }
}

public class CargoContainer : BaseBlock
{
    IMyCargoContainer m_oBlock;
    public CargoContainer( string oBlockName )
    {
        m_oBlock = gts.GetBlockWithName( oBlockName ) as IMyCargoContainer;
        if( m_oBlock == null )
            throw new Exception( oBlockName + " block not found, check name" );
        Init( m_oBlock );
    }

    public CargoContainer( IMyCargoContainer oBlock ) : base( oBlock )
    {
        m_oBlock = oBlock;
    }

    public CargoContainer( IMyTerminalBlock oBlock ) : base( oBlock )
    {
        m_oBlock = (IMyCargoContainer)oBlock;
    }

    public float GetCurrentVolume()
    {
        return (float)m_oBlock.GetInventory( 0 ).CurrentVolume;
    }

    public float GetMass()
    {
        return (float)m_oBlock.GetInventory( 0 ).CurrentMass;
    }

    public float GetMaxVolume()
    {
        return (float)m_oBlock.GetInventory( 0 ).MaxVolume;
    }

    public float GetSpaceLeftAsPercent()
    {
        return ( GetCurrentVolume() / GetMaxVolume() ) * 100.0f;
    }
}

public class LcdPanel : BaseBlock
{
    IMyTextPanel m_oBlock;
    public LcdPanel( string oBlockName )
    {
        m_oBlock = gts.GetBlockWithName( oBlockName ) as IMyTextPanel;
        if( m_oBlock == null )
            throw new Exception( oBlockName + " block not found, check name" );
        Init( m_oBlock );
    }

    public LcdPanel( IMyTextPanel oBlock ) : base( oBlock )
    {
        m_oBlock = oBlock;
    }

    public LcdPanel( IMyTerminalBlock oBlock ) : base( oBlock )
    {
        m_oBlock = (IMyTextPanel)oBlock;
    }

    public void MakePublicTextActive()
    {
        m_oBlock.ShowPublicTextOnScreen();
    }

    public void UpdateText( string oText )
    {
        m_oBlock.WritePublicText( oText ); 
    }
}

public class OxygenTank : BaseBlock
{
    // NOTE: This is also used for Hydrogen Tank blocks
    IMyOxygenTank m_oBlock;
    public OxygenTank( string oBlockName )
    {
        m_oBlock = gts.GetBlockWithName( oBlockName ) as IMyOxygenTank;
        if( m_oBlock == null )
            throw new Exception( oBlockName + " block not found, check name" );
        Init( m_oBlock );
    }

    public OxygenTank( IMyOxygenTank oBlock ) : base( oBlock )
    {
        m_oBlock = oBlock;
    }

    public OxygenTank( IMyTerminalBlock oBlock ) : base( oBlock )
    {
        m_oBlock = (IMyOxygenTank)oBlock;
    }

    public float GetLevel()
    {
        float oLevel = m_oBlock.GetOxygenLevel();
        oLevel *= 100.0F; // Make it go from 0 to 100
        return oLevel;
    }
}

public class Reactor : BaseBlock
{
    IMyReactor m_oBlock;
    public Reactor( string oBlockName )
    {
        m_oBlock = gts.GetBlockWithName( oBlockName ) as IMyReactor;
        if( m_oBlock == null )
            throw new Exception( oBlockName + " block not found, check name" );
        Init( m_oBlock );
    }

    public Reactor( IMyReactor oBlock ) : base( oBlock )
    {
        m_oBlock = oBlock;
    }

    public Reactor( IMyTerminalBlock oBlock ) : base( oBlock )
    {
        m_oBlock = (IMyReactor)oBlock;
    }

    public float GetOutputAsPercent()
    {
        string oMaxString = GetDetailedInfoValue( "Max Output" );
        string oCurrentString = GetDetailedInfoValue( "Current Output" );
        float oMax = 0.0f;
        float oCurrent = 0.0f;
        float.TryParse( oMaxString.Split( ' ' )[ 0 ], out oMax );
        float.TryParse( oCurrentString.Split( ' ' )[ 0 ], out oCurrent );
        
        float oPercent = ( oCurrent / oMax ) * 100.0f;
        return oPercent;
    }

    public float GetUraniumAmount()
    {
        return GetItemAmountInInventory( m_oBlock.GetInventory( 0 ), "Ingot", "Uranium" );
    }
}

public class Rotor : BaseBlock
{
    IMyMotorStator m_oBlock;
    public Rotor( string oBlockName )
    {
        m_oBlock = gts.GetBlockWithName( oBlockName ) as IMyMotorStator;
        if( m_oBlock == null )
            throw new Exception( oBlockName + " block not found, check name" );
        Init( m_oBlock );
    }

    public Rotor( IMyMotorStator oBlock ) : base( oBlock )
    {
        m_oBlock = oBlock;
    }

    public Rotor( IMyTerminalBlock oBlock ) : base( oBlock )
    {
        m_oBlock = (IMyMotorStator)oBlock;
    }

    public void SetSpeed( float oSpeed )
    {
        m_oBlock.SetValue( "Velocity", oSpeed );
    }
}

public class SolarPanel : BaseBlock
{
    IMySolarPanel m_oBlock;
    public SolarPanel( string oBlockName )
    {
        m_oBlock = gts.GetBlockWithName( oBlockName ) as IMySolarPanel;
        if( m_oBlock == null )
            throw new Exception( oBlockName + " block not found, check name" );
        Init( m_oBlock );
    }

    public SolarPanel( IMySolarPanel oBlock ) : base( oBlock )
    {
        m_oBlock = oBlock;
    }

    public SolarPanel( IMyTerminalBlock oBlock ) : base( oBlock )
    {
        m_oBlock = (IMySolarPanel)oBlock;
    }

    public float GetMaxOutput()
    {
        return m_oBlock.MaxOutput * 1000;
    }

}

// =================================================================================================
// Utility Functions

public List< IMyTerminalBlock > GetListOfBlocksOnLocalGrid< T >() where T: class
{
    List< IMyTerminalBlock > oList = new List< IMyTerminalBlock >();
    gts.GetBlocksOfType< T >( oList, delegate( IMyTerminalBlock b )
    {
        return IsBlockOnLocalGrid( b );
    });
    return oList;
}

public List< IMyTerminalBlock > GetListOfBlocksThatStartWith< T >( string oPrefix ) where T: class
{
    List< IMyTerminalBlock > oList = new List< IMyTerminalBlock >();
    List< IMyTerminalBlock > oListResult = new List< IMyTerminalBlock >();
    gts.GetBlocksOfType< T >( oList );
    for( int i = 0; i < oList.Count; i++ )
    {
        if( oList[ i ].CustomName.StartsWith( oPrefix ) == true )
        {
            oListResult.Add( oList[ i ] );
        }
    }

    // }
    // gts.GetBlocksOfType< T >( oList, delegate( IMyTerminalBlock b )
    // {
    //     return b.CustomName.StartsWith( oPrefix );
    // } );
    return oListResult;
}

public List< IMyTerminalBlock > GetListOfBlocks< T >() where T: class
{
    List< IMyTerminalBlock > oList = new List< IMyTerminalBlock >();
    gts.GetBlocksOfType< T >( oList );
    return oList;
}

public int GetHeight()
{
    // cant implement
    return 0;
}

static public float GetItemAmountInInventory( IMyInventory oInventory, string oType, string oSubType )
{
    int oIndex = -1;
    List< IMyInventoryItem > oItems = oInventory.GetItems();
    for( int i = 0; i < oItems.Count; i++ )
    {
        if( oItems[ i ].Content.TypeId.ToString().Contains( oType ) &&
            oItems[ i ].Content.SubtypeId.ToString().Contains( oSubType ) )
        {
            oIndex = i;
            break;
        }
    }
    if( oIndex != -1 )
    {
        return (float)oInventory.GetItems()[ oIndex ].Amount;
    }
    return 0.0f;
}

static public string GetPercentMeter( float oPercent )
{
    int oFilledBars = (int)( oPercent / 4 );
    string oData = new String( (char)0x2588, oFilledBars );

    int oTheRest = 25 - oFilledBars;
    if( ( oTheRest + oFilledBars ) > 25 || ( oTheRest < 0 ) )
    {
       return ("oFilledBars = " + oFilledBars.ToString() + "; oTheRest = " + oTheRest.ToString());
    }
    else
    {
        string oTemp = new String( (char)0x2591, oTheRest );
        oData = "[" + oData + oTemp + "]";
        return oData;
    }
}


public double GetSpeed()
{
    Vector3D oCurrentPos = Me.GetPosition(); // the position of this programmable block
    double speed = ( ( oCurrentPos - g_oPosition ) * 60 ).Length(); // how far the PB has moved since the last run (1/60s ago)
    g_oPosition = oCurrentPos; // update the global variable, which will be used on the next run

    return speed;
}

public void StartAllTimers()
{
    List< IMyTerminalBlock > oList = GetListOfBlocks< IMyTimerBlock >();
    for( int i = 0; i < oList.Count; i++ )
    {
        IMyTimerBlock oTimer = (IMyTimerBlock)oList[ i ];
        if( oTimer.IsCountingDown == false )
            oTimer.ApplyAction("Start");
    }
}

public bool IsBlockOnLocalGrid( IMyTerminalBlock oBlock )
{ 
    return (oBlock.CubeGrid == Me.CubeGrid); 
}

// =================================================================================================
// Global Setup Stuff
