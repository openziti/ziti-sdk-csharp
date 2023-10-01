$svcName="hosted-svc"
$demoServerName="${svcName}.server"
$demoClientName="${svcName}.client"

function cleanup() {
	echo "Cleaning up..."
	ziti edge delete service "${svcName}"
	ziti edge delete service-policy "${svcName}-dial"
	ziti edge delete service-policy "${svcName}-bind"
	echo "Clean up complete."
}
echo " "

$prompt = $true
if("$args[1]".ToLower().StartsWith("y")) {
	$prompt=$false
}

$demoServerId = ziti edge list identities $('name=\"' + ${demoServerName} + '\"') -j | ConvertFrom-Json
if ($demoServerId.data.id) {
	if($prompt) {
		$createId = Read-Host "${demoServerName} identity exists. Delete and overwrite?"
	} else {
		$createId="yes"
	}
	if ("${createId}".ToLower().StartsWith("y")) {
		echo "removing: ${demoServerName}"
		ziti edge delete identity ${demoServerName}
	}
	else {
		echo "Not cleaning up identity"
		$createId = $false
	}
} else {
	$createId = $true;
}
if ($createId) {
	echo "creating identity: ${demoServerName}"
	$id = ziti edge create identity ${demoServerName} -a "${svcName}.binders" -o "${PSScriptRoot}\${demoServerName}.jwt"
}

$demoClientId = ziti edge list identities $('name=\"' + ${demoClientName} + '\"') -j | ConvertFrom-Json
if ($demoClientId.data.id) {
	if($prompt) {
		$createId = Read-Host "${demoClientName} identity exists. Delete and overwrite?"
	} else {
		$createId="yes"
	}
	if ("${createId}".ToLower().StartsWith("y")) {
		echo "removing: ${demoClientName}"
		ziti edge delete identity ${demoClientName}
	}
	else {
		echo "Not cleaning up identity"
		$createId = $false
	}
} else {
	$createId = $true;
}
if ($createId) {
	echo "creating identity: ${demoClientName}"
	$id = ziti edge create identity ${demoClientName} -a "${svcName}.dialers" -o "${PSScriptRoot}\${demoClientName}.jwt"
}

$createServices = $true
$service = ziti edge list services $('name=\"' + ${svcName} + '\" limit none') -j | ConvertFrom-Json
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
	# create the hosted sample example
	ziti edge create service "${svcName}" --configs '${svcName}.intercept.v1,${svcName}.host.v1' -a "sdk.service"

	# authorize sdk clients to dial the sdk example services
	ziti edge create service-policy ${svcName}-dial Dial --service-roles "@${svcName}" --identity-roles "#${svcName}.dialers"
	# authorize the edge router to bind services
	ziti edge create service-policy ${svcName}-bind Bind --service-roles "@${svcName}" --identity-roles "#${svcName}.binders"
}
echo "====================================================="
echo "Setup for hosted.svc is complete                     "
echo "If this is the first time running this script, allow "
echo "time for the services to propegate to the router     "
echo "before running the sample or you may receive an error"
echo "like: no terminators exist for service               "
echo "====================================================="
echo " "