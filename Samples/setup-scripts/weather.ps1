function cleanup() {
	echo "Cleaning up..."
	ziti edge delete config "weather-svc.host.v1"
	ziti edge delete config "weather-svc.intercept.v1"
	ziti edge delete config-type "weather-config-type"
	ziti edge delete service weather-svc
	ziti edge delete service-policy weather-svc-dial
	ziti edge delete service-policy weather-svc-bind
	echo "Clean up complete."
}
echo " "

$prompt = $true
if("$args[1]".ToLower().StartsWith("y")) {
	$prompt=$false
}

if (!${EdgeRouter}) {
	$ers = ziti edge list edge-routers -j | ConvertFrom-Json
	echo "Router list:"
	foreach($r in $ers.data) {
		echo "  - $($r.name)"
	}
	echo " "
	$EdgeRouter = Read-Host "Enter the name of the edge router to use for offload from the OpenZiti overlay"
}

echo "EdgeRouter identity: ${EdgeRouter} will be used as the router to offload traffic for the demo."
$er=ziti edge list identities "name=\`"${EdgeRouter}\`"" -j | ConvertFrom-Json

$weatherAttr="weather-svc.binders"
if($er.data.id) {
	$found=$false
	foreach($attr in $er.data.roleAttributes) {
		if ("$attr" -eq "$weatherAttr") {
			$found=$true
		}
	}
	if($found) {
		echo "EdgeRouter identity: ${EdgeRouter} already has the expected attribute [${weatherAttr}]. No need to update, skipping."
	} else {
		ziti edge update identity ${EdgeRouter} -a "$weatherAttr" 
	}
} else {
	throw "ERROR: provided edge router identity [${EdgeRouter}] does not exist! Cannot continue."
}

$demoId = ziti edge list identities "name=\`"weather.demo\`"" -j | ConvertFrom-Json
if ($demoId.data.id) {
	if($prompt) {
		$createId = Read-Host "weather.demo identity exists. Delete and overwrite?"
	} else {
		$createId="yes"
	}
	if ("${createId}".ToLower().StartsWith("y")) {
		ziti edge delete identity weather.demo
	}
	else {
		echo "Not cleaning up identity"
		$createId = $false
	}
} else {
	$createId = $true;
}
if ($createId) {
	echo "creating identity: weather.demo"
	$id = ziti edge create identity user weather.demo -a "weather-svc.dialers" -o "${PSScriptRoot}/weather.demo.jwt"
}

$createServices = $true
$service = ziti edge list services "name=\`"weather-svc\`"" -j | ConvertFrom-Json
if($service.data.id) {
	if($prompt) {
		$svcCleanUp = Read-Host "Looks like the service already exists. Try to cleanup/start again?"
	} else {
		$svcCleanUp="yes"
	}
	if ("${svcCleanUp}".ToLower().StartsWith("y")) {
		cleanup
	} else {
		echo "Not cleaning up identity"
		$createServices = $false
	}
}

if ($createServices) {
	# create the weather sample example
	ziti edge create config "weather-svc.host.v1" host.v1 "{\`"protocol\`":\`"tcp\`", \`"address\`":\`"wttr.in\`",\`"port\`":443}"
	ziti edge create config "weather-svc.intercept.v1" intercept.v1 "{\`"protocols\`":[\`"tcp\`"],\`"addresses\`":[\`"wttr.in\`"],\`"portRanges\`":[{\`"low\`":80, \`"high\`":443}]}"
	ziti edge create service "weather-svc"--configs "weather-svc.intercept.v1,weather-svc.host.v1" -a "sdk.service"

	# authorize sdk clients to dial the sdk example services
	ziti edge create service-policy weather-svc-dial Dial --service-roles "@weather-svc" --identity-roles "#weather-svc.dialers"
	# authorize the edge router to bind services
	ziti edge create service-policy weather-svc-bind Bind --service-roles "@weather-svc" --identity-roles "#weather-svc.binders"
}
echo " "
echo "====================================================="
echo "Setup for weather.svc is complete                    "
echo "If this is the first time running this script, allow "
echo "time for the services to propegate to the router     "
echo "before running the sample or you may receive an error"
echo "like: no terminators exist for service               "
echo "====================================================="
echo " "