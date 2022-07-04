function antiNvidiaCrash(%lastFlushPosition)
{
    cancel($antiNvidiaCrash);

    %client = nameToID("serverConnection");
    if(!isObject(%client))
        return $antiNvidiaCrash = schedule(1, 0, antiNvidiaCrash);

    %player = %client.getControlObject();
    if(!isObject(%player))
        return $antiNvidiaCrash = schedule(1, 0, antiNvidiaCrash);

    %position = %player.getTransform();
    if(vectorDist(%lastFlushPosition, %position) > 500)
    {
        //empty vram
        flushVBOCache();

        %lastFlushPosition = %position;
    }
    $antiNvidiaCrash = schedule(1, 0, antiNvidiaCrash, %lastFlushPosition);
}

function doot()
{
	if(!isEventPending($antiNvidiaCrash))
	{
		antiNvidiaCrash();
		echo("Crash prevention enabled");
	}
	else
	{
		cancel($antiNvidiaCrash);
		echo("Crash prevention disabled");
	}
}