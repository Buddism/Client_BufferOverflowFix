//most of the evidence supports that this is an nvidia driver issue
$BufferOverflowFix::Version = 2; //doubt ill ever need to change this
$BufferOverflowFix::DefaultDistance = 600;
$BufferOverflowFix::Distance = $BufferOverflowFix::DefaultDistance;

exec("./BufferOverflowFixIcon.gui");

function runBufferOverflowFix()
{
    cancel($bufferOverflowFixSchedule);

    %client = nameToID("serverConnection");
    if(!isObject(%client)) //if the client is no longer in a server this object should not exist
        return;

    %player = %client.getControlObject();
    if(!isObject(%player))
        return $bufferOverflowFixSchedule = schedule(1, 0, "runBufferOverflowFix");

    %position = %player.getTransform();

	if($BufferOverflow::LastFlushPosition $= "")
		$BufferOverflow::LastFlushPosition = %position;

    if(vectorDist($BufferOverflow::LastFlushPosition, %position) > $BufferOverflowFix::Distance)
    {
        //empty vram (if it exceeds a few gb (>3GB?) it can crash)
		BufferOverflowFixIcon.setVisible(true);
		schedule(1000, 0, "flushVBOCache");
		BufferOverflowFixIcon.schedule(1000, setVisible, 0);

        $BufferOverflow::LastFlushPosition = %position;
    }

    $bufferOverflowFixSchedule = schedule(1, 0, "runBufferOverflowFix");
}

function enableBufferOverflowFix(%silent)
{
	if(!isEventPending($bufferOverflowFixSchedule))
	{
		runBufferOverflowFix();
		if(!%silent)
			newChatHud_AddLine("\c6Buffer Overflow Fix enabled");
	}
}

function disableBufferOverflowFix(%silent)
{
	if(!%silent)
		newChatHud_AddLine("\c6Buffer Overflow Fix disabled");

	$BufferOverflowFix::Distance = $BufferOverflowFix::DefaultDistance;
	$BufferOverflow::LastFlushPosition = "";
	cancel($bufferOverflowFixSchedule);
}

function clientCmdBufferOverflowHandshake(%this)
{
	commandToServer('BufferOverflowFixHandshake', $BufferOverflowFix::Version);
}

function clientCmdBufferOverflowSet(%cmd, %value)
{
	switch$(%cmd)
	{
		case "Enable":
			enableBufferOverflowFix(%value == 1);
		case "Disable":
			disableBufferOverflowFix(%value == 1);

		case "Distance":
			if(%distance > 0)
				$BufferOverflowFix::Distance = mClamp(%distance, 100, 1000000); //dont know why youd want such a high number
	}
}

package Script_BufferOverflowFix
{
	function disconnectedCleanup(%doReconnect)
	{
		disableBufferOverflowFix(true);
		return parent::disconnectedCleanup(%doReconnect);
	}
};
activatePackage(Script_BufferOverflowFix);