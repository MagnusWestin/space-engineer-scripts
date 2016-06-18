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

public void Main(string argument)
{
    gGTS = GridTerminalSystem;
    SetupCPU();
    IMyTextPanel output; // This is for a Text Display or LCD Panel to print to 
    output = GridTerminalSystem.GetBlockWithName("LCD Panel") as IMyTextPanel; 
    if( output == null ) throw new Exception( "LCD Panel block not found, check name ");
    DisplayConsumablesLevel( output );
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

    oLCD.WritePublicText( oText ); 
    oLCD.ShowPublicTextOnScreen();
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
    // NOTE: This is also used for Hydrogen Tank blocks
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