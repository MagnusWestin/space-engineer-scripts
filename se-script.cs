// The north pole is at 0,-60000,0 and the South pole is 0,60000,0. The sun rotates around the y axis of
// the star system. Points on the equator are +/- 60000 on the x and z axis'. These points will be below
// the surface by various depths due to the height map. If you line up with the center of the planet at
// each point you can find each of these points right at the surface. 
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
    else if( argument == "E1" )
    {
        RunLargeShipCode( argument );
    }
    else if( argument == "A1" )
    {
        RunConstructionShipCode( argument );
    }
    else if( argument == "T2" )
    {
        RunT2Code( argument );
    }
    else
    {
        throw new Exception( "Wrong argument: " + argument );
    }

    g_oFirstInit = true;
}

public void RunT2Code( string argument )
{
    LcdPanel oLcdLeft  = new LcdPanel( "T2 LCD Left" );
    LcdPanel oLcdRight = new LcdPanel( "T2 LCD Right" );
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

    string oTextLcdRight = "==== CARGO ====\n" + GetCargoContainerInfo( 67000 );
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
    LcdPanel oLcdLeft  = new LcdPanel( "Exp 1 LCD Panel Left" );
    LcdPanel oLcdRight = new LcdPanel( "Exp 1 LCD Panel Right" );
    if( g_oFirstInit == false )
    {
        oLcdLeft.MakePublicTextActive();
        oLcdRight.MakePublicTextActive();
    }

    string oTextLcdLeft = DisplayConsumablesLevel();
    oLcdLeft.UpdateText( oTextLcdLeft ); 

    string oTextLcdRight = GetScriptInfoAndTime( argument );
    oTextLcdRight += GetTickCountText();
    oTextLcdRight += "-----------------------------------\n";
    oLcdRight.UpdateText( oTextLcdRight ); 
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

    string oTextLcdLeft = GetBatteryInfo( argument );
    oTextLcdLeft += "\n" + GetReactorInfo();
    oLcdLeft.UpdateText( oTextLcdLeft );

    string oTextLcdRight = GetTickCountText();
    oTextLcdRight += "\n==== CARGO ====\n" + GetCargoContainerInfo( 27000 );
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

public string GetBatteryInfo( string oName )
{
    List< IMyTerminalBlock > oBatteryList = GetListOfBlocksOnLocalGrid< IMyBatteryBlock  >();
    string oText = "";
    for( int i = 0; i < oBatteryList.Count; i++ )
    {
        Battery oBattery = new Battery( oBatteryList[ i ] );
        float oPercent = oBattery.GetStoredPowerAsPercent();
        oText += oBattery.GetCustomName() + ":                " + String.Format( "{0:F1}", oPercent ) + "%\n";
        oText += GetPercentMeter( oPercent ) + "\n";
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
        oText += oInv.GetCustomName() + ": " + String.Format( "{0:F1}", oInv.GetSpaceLeftAsPercent() ) + "%\n";
        oText += GetPercentMeter( oInv.GetSpaceLeftAsPercent() ) + "\n";
        oTotalMass += oInv.GetMass();
    }
    oText += "-----------------------------------\n";
    oText += "Total Mass: " + oTotalMass + " kg\nMax Mass: " + oMaxMass + " kg\n";
    oText += GetPercentMeter( ( oTotalMass / (float)oMaxMass ) * 100.0f ) + "\n";
    return oText;
}

public string GetReactorInfo()
{
    List< IMyTerminalBlock > oReactorList = GetListOfBlocksOnLocalGrid< IMyReactor >();
    string oText = "";
    for( int i = 0; i < oReactorList.Count; i++ )
    {
        Reactor oReactor = new Reactor( oReactorList[ i ] );
        float oPercent = oReactor.GetOutputAsPercent();
        oText += oReactor.GetCustomName() + ":                 " + String.Format( "{0:F1}", oPercent ) + "%\n";
        oText += GetPercentMeter( oPercent );
        oText += "\nUranium: " + oReactor.GetUraniumAmount() + "\n";
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
