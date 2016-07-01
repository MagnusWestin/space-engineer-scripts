public Program() {
    // The constructor, called only once every session and always before any other method is called. Use it to
    // initialize your script. 
}

public void Save() {
    // Called when the program needs to save its state. Use this method to save your state to the Storage field
    // or some other means.
}

static public IMyProgrammableBlock gThisCPU;
static public IMyGridTerminalSystem gGTS;
static public float g_oLastMaxSolarPanelOutput = 0.0f;
static public float g_oSecondLastMaxSolarPanelOutput = 0.0f;

public void Main(string argument)
{
    gGTS = GridTerminalSystem;
    SetupCPU();
    IMyTextPanel output = GridTerminalSystem.GetBlockWithName("LCD Panel Left") as IMyTextPanel; 
    if( output == null ) throw new Exception( "LCD Panel block not found, check name");
    DisplayConsumablesLevel( output );

    output = GridTerminalSystem.GetBlockWithName("LCD Panel Right") as IMyTextPanel; 
    if( output == null ) throw new Exception( "LCD Panel block not found, check name");
    DisplaySolarPanelPower( output );

    Rotor oRotor = new Rotor( "Advanced Rotor Solar" );
    SolarPanel oPanel = new SolarPanel( "Solar Panel 1" );
    RunSolarPanelAlignment( oRotor, oPanel );
}

// =================================================================================================
// Application Code

public void DisplayConsumablesLevel( IMyTextPanel oLCD )
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
        oText += "\n" + oBatteryList[ i ].CustomName + " Level (" + oBlock.GetRechargeStatus() + "): ";
    }

    oText += "\n Power: " + g_oLastMaxSolarPanelOutput;
    oLCD.WritePublicText( oText ); 
    oLCD.ShowPublicTextOnScreen();
}

public void DisplaySolarPanelPower( IMyTextPanel oLCD )
{
    List< IMyTerminalBlock > oSolarPanelList = GetListOfBlocks< IMySolarPanel  >();
    string oText = "";
    for( int i = 0; i < oSolarPanelList.Count; i++ )
    {
        SolarPanel oPanel = new SolarPanel( oSolarPanelList[ i ] );
        oText += "\n" + oPanel.GetCustomName() + ": " + String.Format( "{0:F1}", oPanel.GetMaxOutput() ) + " kW";
    }

    oLCD.WritePublicText( oText ); 
    oLCD.ShowPublicTextOnScreen();
}

public void RunSolarPanelAlignment( Rotor oRotor, SolarPanel oSolarPanel )
{
    if( oSolarPanel.GetMaxOutput() < g_oLastMaxSolarPanelOutput )
    {
        oRotor.SetSpeed( 0.0f );
    }
    else
    {
        oRotor.SetSpeed( -0.2f );
    }
    g_oSecondLastMaxSolarPanelOutput = g_oLastMaxSolarPanelOutput;
    g_oLastMaxSolarPanelOutput = oSolarPanel.GetMaxOutput();
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
        m_oBlock = gGTS.GetBlockWithName( oBlockName ) as IMyBatteryBlock;
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
}

public class OxygenTank : BaseBlock
{
    // NOTE: This is also used for Hydrogen Tank blocks
    IMyOxygenTank m_oBlock;
    public OxygenTank( string oBlockName )
    {
        m_oBlock = gGTS.GetBlockWithName( oBlockName ) as IMyOxygenTank;
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

public class Rotor : BaseBlock
{
    IMyMotorStator m_oBlock;
    public Rotor( string oBlockName )
    {
        m_oBlock = gGTS.GetBlockWithName( oBlockName ) as IMyMotorStator;
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
        m_oBlock = gGTS.GetBlockWithName( oBlockName ) as IMySolarPanel;
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
    GridTerminalSystem.GetBlocksOfType< T >( oList, delegate( IMyTerminalBlock b )
    {
        return IsBlockOnLocalGrid( b, gGTS );
    });
    return oList;
}

public List< IMyTerminalBlock > GetListOfBlocks< T >()
{
    List< IMyTerminalBlock > oList = new List< IMyTerminalBlock >();
    GridTerminalSystem.GetBlocksOfType< T >( oList );
    return oList;
}

public string GetPercentMeter( float oPercent )
{
    int oFilledBars = (int)( oPercent / 2 );
    string oData = new String( '|', oFilledBars );

    int oTheRest = 50 - oFilledBars;
    if( ( oTheRest + oFilledBars ) > 50 || ( oTheRest < 0 ) )
    {
       throw new Exception( "oFilledBars = " + oFilledBars.ToString() + "; oTheRest = " + oTheRest.ToString() );
    }
    string oTemp = new String( '\'', oTheRest );
    oData = "[" + oData + oTemp + "] ";
    return oData;
}

public bool IsBlockOnLocalGrid( IMyTerminalBlock oBlock, IMyGridTerminalSystem oGTS )
{ 
    return (oBlock.CubeGrid == gThisCPU.CubeGrid); 
}

// =================================================================================================
// Global Setup Stuff
public void SetupCPU()
{
    if( gThisCPU != null )
        return;
    var oList = new List< IMyTerminalBlock >();
    
    GridTerminalSystem.GetBlocksOfType< IMyProgrammableBlock >( oList, delegate( IMyTerminalBlock b )
    {
        return ( ((IMyProgrammableBlock)b).IsRunning);
    });

    if( oList.Count != 1)
        throw new Exception( "Number of running programmable blocks was not 1. Keen must have changed something!" );
    gThisCPU = (IMyProgrammableBlock)oList[ 0 ];
}