//basically robbed this from BLG default prefs
//https://forum.blockland.us/index.php?topic=320521.0 heres a good reference
function BOF_registerPref(%cat, %title, %type, %variable, %addon, %default, %params, %className) {
	if(%className $= "")
		%className = "BOF_preference";

    %pref = new ScriptObject(Preference)
    {
        className     = %className;

        addon         = %addon;
        category      = %cat;
        title         = %title;

        variable      = %variable;

        type          = %type;
        params        = %params;
        defaultValue  = %default;

        hostOnly      = false;
        secret        = false;

        loadNow        = false; // load value on creation instead of with pool (optional)
        noSave         = false; // do not save (optional)
        requireRestart = false; // denotes a restart is required (optional)
    };

	return %pref;
}

function Pref_BufferOverflow_Enabled::onUpdate(%this, %value)
{
	%silent = $Pref::Server::BufferOverflowFix::Silent;
	%funcStr = $Pref::Server::BufferOverflowFix::Enabled ? "Enable" : "Disable";
	for(%i = 0; %i < clientGroup.getCount(); %i++)
		commandToClient(clientGroup.getObject(%i), 'BufferOverflowSet', %funcStr, %silent);
}
function Pref_BufferOverflow_Distance::onUpdate(%this, %value)
{
	%silent = $Pref::Server::BufferOverflowFix::Silent;
	%distance = $Pref::Server::BufferOverflowFix::Distance;
	for(%i = 0; %i < clientGroup.getCount(); %i++)
		commandToClient(clientGroup.getObject(%i), 'BufferOverflowSet', "Distance", %distance, %silent);
}
function Pref_BufferOverflow_InstantDistance::onUpdate(%this, %value)
{
	%silent = $Pref::Server::BufferOverflowFix::Silent;
	%distance = getMax($Pref::Server::BufferOverflowFix::InstantDistance, $Pref::Server::BufferOverflowFix::Distance * 1.1);
	$Pref::Server::BufferOverflowFix::InstantDistance = %distance;
	for(%i = 0; %i < clientGroup.getCount(); %i++)
		commandToClient(clientGroup.getObject(%i), 'BufferOverflowSet', "InstantDistance", %distance, %silent);
}

if(!$BufferOverflow::SetUpPrefs)
{
	registerPreferenceAddon("Script_BufferOverflowFix", "Buffer Overflow Settings", "control_power_blue");

	BOF_registerPref("Options", "Enabled"		 		, "bool", "$Pref::Server::BufferOverflowFix::Enabled" 		  , "Script_BufferOverflowFix", 0	, ""			, "Pref_BufferOverflow_Enabled" );
	BOF_registerPref("Options", "Silent Enable/Disable" , "bool", "$Pref::Server::BufferOverflowFix::Silent" 		  , "Script_BufferOverflowFix", 0	, ""			, "Pref_BufferOverflow_Silent" );
	BOF_registerPref("Options", "Distance"		 		, "num" , "$Pref::Server::BufferOverflowFix::Distance"		  , "Script_BufferOverflowFix", 600	, "1 100000 1", "Pref_BufferOverflow_Distance");
	BOF_registerPref("Options", "InstantDistance"		, "num" , "$Pref::Server::BufferOverflowFix::InstantDistance", "Script_BufferOverflowFix", 800	, "1 100000 1", "Pref_BufferOverflow_InstantDistance");

	$BufferOverflow::SetUpPrefs = true;
}


package Script_BufferOverflowFix
{
	//when a client spawns
	function GameConnection::onClientEnterGame(%this)
	{
		if($Pref::Server::BufferOverflowFix::Enabled)
		{
			commandToClient(%this, 'BufferOverflowHandshake');

			commandToClient(%this, 'BufferOverflowSet', "Enable", %silent = false);
			commandToClient(%this, 'BufferOverflowSet', "Distance", $Pref::Server::BufferOverflowFix::Distance);
			commandToClient(%this, 'BufferOverflowSet', "InstantDistance", $Pref::Server::BufferOverflowFix::InstantDistance);
		}

		return parent::onClientEnterGame(%this);
	}
};
activatePackage(Script_BufferOverflowFix);