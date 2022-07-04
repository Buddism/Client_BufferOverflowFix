//this will not be executed by default (should be named Script_BufferOverflowFix)

//basically robbed this from BLG default prefs
//https://forum.blockland.us/index.php?topic=320521.0 heres a good reference
function BOF_registerPref(%cat, %title, %type, %variable, %addon, %default, %params, %callback, %legacy, %isSecret, %isHostOnly) {
    new ScriptObject(Preference)
    {
        className     = "BOF_preference";

        addon         = %addon;
        category      = %cat;
        title         = %title;

        type          = %type;
        params        = %params;

        variable      = %variable;

        defaultValue  = %default;

        hostOnly      = %isHostOnly;
        secret        = %isSecret;

        loadNow        = false; // load value on creation instead of with pool (optional)
        noSave         = false; // do not save (optional)
        requireRestart = false; // denotes a restart is required (optional)
    };
}

if(!$BufferOverflow::SetUpPrefs)
{
	registerPreferenceAddon("Script_BufferOverflowFix", "Buffer Overflow Settings", "control_power_blue");

	BOF_registerPref("Options", "Enabled", "bool", "$Pref::Server::BufferOverflowFix::Enabled", "Script_BufferOverflowFix", 0, "");
	BOF_registerPref("Options", "Distance", "num", "$Pref::Server::BufferOverflowFix::Distance", "Script_BufferOverflowFix", 600, "100 100000 50");
}


package Script_BufferOverflowFix
{
	//when a client spawns
	function GameConnection::onClientEnterGame(%this)
	{
		if($Pref::Server::BufferOverflowFix::Enabled)
			commandToClient(%this, 'BufferOverflowHandshake', $Pref::Server::BufferOverflowFix::Distance);

		return parent::onClientEnterGame(%this);
	}
};
activatePackage(Script_BufferOverflowFix);