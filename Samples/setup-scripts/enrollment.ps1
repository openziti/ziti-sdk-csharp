$idName="enroll.demo"
$demoId = ziti edge list identities "name=`"${idName}`"" -j | ConvertFrom-Json

if ($demoId.data.id) {
	ziti edge delete identity ${idName}
}

echo "creating identity: ${idName}"
$id = ziti edge create identity user ${idName} -o "${PSScriptRoot}/${idName}.jwt"

echo "====================================================="
echo "Setup for enroll sample is complete                  "
echo "====================================================="
echo " "