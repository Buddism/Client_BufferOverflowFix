//most of the evidence supports that this is an nvidia driver issue
$BufferOverflowFix::Version = 2.1;
$BufferOverflowFix::DefaultDistance = 600;
$BufferOverflowFix::DefaultInstantDistance = 800;

$BufferOverflowFix::Distance			  = $BufferOverflowFix::DefaultDistance;
$BufferOverflowFix::InstantDistance 	  = $BufferOverflowFix::DefaultInstantDistance;
$BufferOverflowFix::hasSetInstantDistance = false;

exec("./BufferOverflowFixIcon.gui");

function runBufferOverflowFix()
{
    cancel($BufferOverflowFix::loopSchedule);

    %client = nameToID("serverConnection");
    if(!isObject(%client)) //if the client is no longer in a server this object should not exist
        return;

    %player = %client.getControlObject();
    if(!isObject(%player))
        return $BufferOverflowFix::loopSchedule = schedule(1, 0, "runBufferOverflowFix");

    %position = %player.getTransform();

	if($BufferOverflowFix::LastFlushPosition $= "")
		$BufferOverflowFix::LastFlushPosition = %position;

	%distance = vectorDist($BufferOverflowFix::LastFlushPosition, %position);
    if(%distance > $BufferOverflowFix::Distance)
    {	
        //empty vram (if it exceeds a few gb (>3GB?) it can crash)
		BufferOverflowFixIcon.setVisible(true);

		cancel($BufferOverflowFix::flushSchedule);
		cancel($BufferOverflowFix::iconSchedule);

		if(%distance > $BufferOverflowFix::InstantDistance)
			flushVBOCache();
		else
			$BufferOverflowFix::flushSchedule = schedule(1000, 0, "flushVBOCache");

		$BufferOverflowFix::iconSchedule = BufferOverflowFixIcon.schedule(1000, setVisible, false);
		$BufferOverflowFix::LastFlushPosition = %position;
    }

    $BufferOverflowFix::loopSchedule = schedule(1, 0, "runBufferOverflowFix");
}

function enableBufferOverflowFix(%silent)
{
	if(!isEventPending($BufferOverflowFix::loopSchedule))
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

	$BufferOverflowFix::LastFlushPosition = "";

	cancel($bufferOverflowFix::loopSchedule);
	cancel($BufferOverflowFix::flushSchedule);
	cancel($BufferOverflowFix::iconSchedule);

	BufferOverflowFixIcon.setVisible(false);
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
			%distance = mClampF(%value, 100, 1000000);
			if(%value > 0 && %distance > 0)
			{
				$BufferOverflowFix::Distance = %distance;
				if(!$BufferOverflowFix::hasSetInstantDistance)
					$BufferOverflowFix::InstantDistance = $BufferOverflowFix::Distance * 1.333333;
				else {
					$BufferOverflowFix::InstantDistance = getMax($BufferOverflowFix::InstantDistance, %distance * 1.1);
				}
			}

		case "InstantDistance":
			%distance = mClampF(%value, 100, 1000000);
			if(%value > 0 && %distance >= $BufferOverflowFix::Distance * 1.1)
			{
				$BufferOverflowFix::InstantDistance = %distance;
				$BufferOverflowFix::hasSetInstantDistance = true;
			}
	}
}

package Script_BufferOverflowFix
{
	function disconnectedCleanup(%doReconnect)
	{
		$BufferOverflowFix::Distance = $BufferOverflowFix::DefaultDistance;
		$BufferOverflowFix::InstantDistance = $BufferOverflowFix::DefaultInstantDistance;
		$BufferOverflowFix::hasSetInstantDistance = false;
		disableBufferOverflowFix(true);
		return parent::disconnectedCleanup(%doReconnect);
	}
};
activatePackage(Script_BufferOverflowFix);