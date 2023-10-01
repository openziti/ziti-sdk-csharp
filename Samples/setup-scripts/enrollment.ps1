$idName="enroll.demo"
$demoId = ziti edge list identities $('name=\"' + ${idName} + '\"') -j | ConvertFrom-Json

$id = $demoId.Data[0].Id
if (![string]::IsNullOrEmpty($id)) {
	echo "removing old identity: ${id}"
	ziti edge delete identity -- ${id}
}

echo "creating identity: ${idName}"
$id = ziti edge create identity ${idName} -o "${PSScriptRoot}/${idName}.jwt"

echo "====================================================="
echo "Setup for enroll sample is complete                  "
echo "  jwt located at: ${PSScriptRoot}/${idName}.jwt"      
echo "====================================================="
echo " "