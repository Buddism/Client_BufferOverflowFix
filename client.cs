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

function enableBufferOverflowFix()
{
	if(!isEventPending($bufferOverflowFixSchedule))
	{
		runBufferOverflowFix();
		newChatHud_AddLine("\c6Buffer Overflow Fix enabled, Distance: " @ $BufferOverflowFix::Distance);
	}
}

function disableBufferOverflowFix()
{
	$BufferOverflowFix::Distance = $BufferOverflowFix::DefaultDistance;
	$BufferOverflow::LastFlushPosition = "";
	cancel($bufferOverflowFixSchedule);
}

function clientCmdBufferOverflowHandshake(%this)
{
	commandToServer('BufferOverflowFixHandshake', $BufferOverflowFix::Version);
}

function clientCmdBufferOverflowSet(%this, %cmd, %value)
{
	switch$(%cmd)
	{
		case "Enable":
			enableBufferOverflowFix();
		case "Disable":
			disableBufferOverflowFix();

		case "Distance":
			if(%distance > 0)
				$BufferOverflowFix::Distance = mClamp(%distance, 100, 1000000); //dont know why youd want such a high number
	}
}

package Client_BufferOverflowFix
{
	function disconnectedCleanup(%doReconnect)
	{
		disableBufferOverflowFix();
		return parent::disconnectedCleanup(%doReconnect);
	}
};
activatePackage(Client_BufferOverflowFix);