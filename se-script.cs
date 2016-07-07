public Program()
{
    // The constructor, called only once every session and always before any other method is called. Use it to
    // initialize your script. 
    
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

public void Main(string argument)
{
    Echo( argument + g_oTickCount );
    gts = GridTerminalSystem;
    g_oTickCount++;
    if( g_oTickCount > 100 )
        g_oTickCount = 0;

    if( argument == "station" )
    {
        RunStationCode( argument );
    }
    else if( argument == "large-ship" )
    {
        RunLargeShipCode( argument );
    }
    else if( argument == "con-ship" )
    {
        RunConstructionShipCode( argument );
    }
    else
    {
        throw new Exception( "Wrong argument: " + argument );
    }

    g_oFirstInit = true;
}

public void RunStationCode( string argument )
{
    LcdPanel oLcdLeft  = new LcdPanel( "LCD Panel Left" );
    LcdPanel oLcdRight = new LcdPanel( "LCD Panel Right" );
    LcdPanel oLcdDebug = new LcdPanel( "LCD Panel Debug" );
    if( g_oFirstInit == false )
    {
        oLcdLeft.MakePublicTextActive();
        oLcdRight.MakePublicTextActive();
        oLcdDebug.MakePublicTextActive();
    }

    string oTextLcdLeft = DisplayConsumablesLevel();
    oLcdLeft.UpdateText( oTextLcdLeft ); 


    string oTextLcdRight = "== Solar Panels == ";
    oTextLcdRight += DisplaySolarPanelPower( oTextLcdRight );

    oTextLcdRight += "\n== Power Usage ==";
    //oTextLcdRight += DisplayPowerUsage( oTextLcdRight );

    Rotor oRotor = new Rotor( "Advanced Rotor Solar" );
    SolarPanel oPanel = new SolarPanel( "Solar Panel Tracking" );
    oTextLcdRight = RunSolarPanelAlignment( oRotor, oPanel, oTextLcdRight );
    oLcdRight.UpdateText( oTextLcdRight ); 
    
    string oTextDebug = GetScriptInfoAndTime( argument );
    oTextDebug += GetTickCountText();
    oTextDebug += DebugOutput();
    oLcdDebug.UpdateText( oTextDebug );
}

public void RunLargeShipCode( string argument )
{
    LcdPanel oLcdLeft  = new LcdPanel( "Esc LCD Panel Left" );
    LcdPanel oLcdRight = new LcdPanel( "Esc LCD Panel Right" );
    if( g_oFirstInit == false )
    {
        oLcdLeft.MakePublicTextActive();
        oLcdRight.MakePublicTextActive();
    }

    string oTextLcdLeft = DisplayConsumablesLevel();
    oLcdLeft.UpdateText( oTextLcdLeft ); 
}

public void RunConstructionShipCode( string argument )
{
    LcdPanel oLcdLeft  = new LcdPanel( "Con LCD Left" );
    LcdPanel oLcdRight = new LcdPanel( "Con LCD Right" );
    LcdPanel oLcdMiddle = new LcdPanel( "Con LCD Middle" );
    if( g_oFirstInit == false )
    {
        oLcdLeft.MakePublicTextActive();
        oLcdRight.MakePublicTextActive();
        oLcdMiddle.MakePublicTextActive();
    }

    string oTextLcdLeft = GetBatteryInfo();
    oTextLcdLeft += "\n" + GetReactorInfo();
    oLcdLeft.UpdateText( oTextLcdLeft );

    string oTextLcdRight = GetTickCountText();
    oTextLcdRight += "\n==== CARGO ====\n" + GetCargoContainerInfo();
    oLcdRight.UpdateText( oTextLcdRight );

    string oTextLcdMiddle = GetScriptInfoAndTime( argument );
    oLcdMiddle.UpdateText( oTextLcdMiddle );
}

public string GetScriptInfoAndTime( string argument )
{
    string oText = "Script argument: \n" + argument + "\n";
    oText += "Time: " + System.DateTime.Now.ToString( "yyyy-MM-dd HH:mm" ) + "\n";
    return oText;
}

public string DebugOutput()
{
    string oText = "\n==== DEBUG ====\n";
    Battery oBat = new Battery( "Battery 1" );
    oText += oBat.GetDetailedInfo();
    return oText;
}

// =================================================================================================
// Application Code

public string DisplayConsumablesLevel()
{
    List< IMyTerminalBlock > oOxygenList = GetListOfBlocksOnLocalGrid< IMyOxygenTank >();
    string oText = "";
    for( int i = 0; i < oOxygenList.Count; i++ )
    {
        OxygenTank oOxygen = new OxygenTank( oOxygenList[ i ] );
        oText += "\n" + oOxygen.GetCustomName() + " level:";
        oText += "\n" + GetPercentMeter( oOxygen.GetLevel() ) 
                                      + String.Format( "{0:F1}", oOxygen.GetLevel() ) + "%";
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

public string GetBatteryInfo()
{
    //List< IMyTerminalBlock > oBatteryList = GetListOfBlocksOnLocalGrid< IMyBatteryBlock  >();
    Battery oBattery = new Battery( "Con Battery" );
    float oPercent = oBattery.GetStoredPowerAsPercent();
    string oText = "Battery:                " + String.Format( "{0:F1}", oPercent ) + "%\n";
    oText += GetPercentMeter( oPercent );
    return oText;
}

public string GetCargoContainerInfo()
{
    List< IMyTerminalBlock > oContainerList = GetListOfBlocksOnLocalGrid< IMyCargoContainer >();
    string oText = "";
    float oTotalMass = 0.0f;
    for( int i = 0; i < oContainerList.Count; i++ )
    {
        CargoContainer oInv = new CargoContainer( oContainerList[ i ] );
        oText += oInv.GetCustomName() + ": " + String.Format( "{0:F1}", oInv.GetSpaceLeftAsPercent() ) + "%\n";
        oText += GetPercentMeter( oInv.GetSpaceLeftAsPercent() ) + "\n";
        oTotalMass += oInv.GetMass();
    }
    oText += "Total Mass: " + oTotalMass + " kg\nMax Mass: 27000 kg\n";
    oText += GetPercentMeter( ( oTotalMass / 27000.0f ) * 100.0f );
    return oText;
}

public string GetReactorInfo()
{
    Reactor oReactor = new Reactor( "Con Reactor" );
    float oPercent = oReactor.GetOutputAsPercent();
    string oText = "Reactor:                 " + String.Format( "{0:F1}", oPercent ) + "%\n";
    oText += GetPercentMeter( oPercent );
    oText += "\nUranium: " + oReactor.GetUraniumAmount();
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

    public string GetStoredPowerAsText()
    {
        return GetDetailedInfoValue( "Stored power" );
    }

    public float GetStoredPowerAsPercent()
    {
        string oStoredPowerString = GetDetailedInfoValue( "Stored power" );
        string oMaxPowerString = GetDetailedInfoValue( "Max Stored Power" );
        float oStoredPower = 0.0f;
        float oMaxPower = 0.0f;
        float.TryParse( oStoredPowerString.Split( ' ' )[ 0 ], out oStoredPower );
        float.TryParse( oMaxPowerString.Split( ' ' )[ 0 ], out oMaxPower );
        
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

public List< IMyTerminalBlock > GetListOfBlocksOnLocalGrid< T >()
{
    List< IMyTerminalBlock > oList = new List< IMyTerminalBlock >();
    gts.GetBlocksOfType< T >( oList, delegate( IMyTerminalBlock b )
    {
        return IsBlockOnLocalGrid( b );
    });
    return oList;
}

// public List< IMyTerminalBlock > GetListOfBlocksThatStartWith< T >( string oPrefix )
// {
//     List< IMyTerminalBlock > oList = new List< IMyTerminalBlock >();
//     gts.GetBlocksOfType< T >( oList, delegate( IMyTerminalBlock b )
//     {
//         return b.CustomName.StartsWith( oPrefix );
//     }) => oPrefix;
//     return oList;
// }

public List< IMyTerminalBlock > GetListOfBlocks< T >()
{
    List< IMyTerminalBlock > oList = new List< IMyTerminalBlock >();
    gts.GetBlocksOfType< T >( oList );
    return oList;
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
    int oFilledBars = (int)( oPercent / 2 );
    string oData = new String( '|', oFilledBars );

    int oTheRest = 50 - oFilledBars;
    if( ( oTheRest + oFilledBars ) > 50 || ( oTheRest < 0 ) )
    {
       return ("oFilledBars = " + oFilledBars.ToString() + "; oTheRest = " + oTheRest.ToString());
    }
    else
    {
        string oTemp = new String( '\'', oTheRest );
        oData = "[" + oData + oTemp + "] ";
        return oData;
    }
}

public bool IsBlockOnLocalGrid( IMyTerminalBlock oBlock )
{ 
    return (oBlock.CubeGrid == Me.CubeGrid); 
}

// =================================================================================================
// Global Setup Stuff
