function cleanup() {
	echo "Cleaning up..."
	ziti edge delete config "hosted-svc.host.v1"
	ziti edge delete config "hosted-svc.intercept.v1"
	ziti edge delete config-type "hosted-config-type"
	ziti edge delete service hosted-svc
	ziti edge delete service-policy hosted-svc-dial
	ziti edge delete service-policy hosted-svc-bind
}

$prompt = $true
if("$args[1]".ToLower().StartsWith("y")) {
	$prompt=$false
}

$demoId = ziti edge list identities 'name=\"hosted.demo\"' -j | ConvertFrom-Json
if ($demoId.data.id) {
	if($prompt) {
		$createId = Read-Host "hosted.demo identity exists. Delete and overwrite?"
	} else {
		$createId="yes"
	}
	if ("${createId}".ToLower().StartsWith("y")) {
		ziti edge delete identity hosted.demo
	}
	else {
		echo "Not cleaning up identity"
		$createId = $false
	}
} else {
	$createId = $true;
}
if ($createId) {
	echo "creating identity: hosted.demo"
	$id = ziti edge create identity user hosted.demo -a "hosted-svc.binders" -o "${PSScriptRoot}\hosted.demo.jwt"
}

$demoId = ziti edge list identities 'name=\"hosted.demo.client\"' -j | ConvertFrom-Json
if ($demoId.data.id) {
	if($prompt) {
		$createId = Read-Host "hosted.demo.client identity exists. Delete and overwrite?"
	} else {
		$createId="yes"
	}
	if ("${createId}".ToLower().StartsWith("y")) {
		ziti edge delete identity hosted.demo.client
	}
	else {
		echo "Not cleaning up identity"
		$createId = $false
	}
} else {
	$createId = $true;
}
if ($createId) {
	echo "creating identity: hosted.demo.client"
	$id = ziti edge create identity user hosted.demo.client -a "hosted-svc.dialers" -o "${PSScriptRoot}\hosted.demo.client.jwt"
}

$createServices = $true
$service = ziti edge list identities 'name=\"hosted.demo\"' -j | ConvertFrom-Json
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
	ziti edge create config 'hosted-svc.host.v1' host.v1 '{\"protocol\":\"tcp\", \"address\":\"hosted.demo.ziti\",\"port\":443}'
	ziti edge create config 'hosted-svc.intercept.v1' intercept.v1 '{\"protocols\":[\"tcp\"],\"addresses\":[\"hosted.demo\"],\"portRanges\":[{\"low\":80, \"high\":443}]}'
	ziti edge create service 'hosted-svc'--configs 'hosted-svc.intercept.v1,hosted-svc.host.v1' -a "sdk.service"

	# authorize sdk clients to dial the sdk example services
	ziti edge create service-policy hosted-svc-dial Dial --service-roles "@hosted-svc" --identity-roles "#hosted-svc.dialers"
	# authorize the edge router to bind services
	ziti edge create service-policy hosted-svc-bind Bind --service-roles "@hosted-svc" --identity-roles "#hosted-svc.binders"
}
echo "====================================================="
echo "Setup for hosted.svc is complete                     "
echo "If this is the first time running this script, allow "
echo "time for the services to propegate to the router     "
echo "before running the sample or you may receive an error"
echo "like: no terminators exist for service               "
echo "====================================================="
echo " "