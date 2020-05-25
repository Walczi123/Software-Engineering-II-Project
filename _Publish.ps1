$folderName = "_Release"
$local = Get-Location
New-Item -ItemType Directory -Force -Path "$local\$folderName"

cd -Path "$local\The Game\CommunicationServer"
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true --output "$local/$folderName"

cd -Path "$local\The Game\GameServices\GameServices"
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true --output "$local/$folderName"

cd -Path "$local\The Game\GameMaster"
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true --output "$local/$folderName" 

foreach($file in Get-ChildItem "$local\$folderName")
{
    if ($file -Like "*.pdb" )
    {
		Remove-item $file.FullName
    }
}