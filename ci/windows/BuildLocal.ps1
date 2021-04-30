$root = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ..\..))

$env:DOTVVM_ROOT = $root
git clean -dfx $root\src
cd $root\src\DotVVM.Framework `
    && npm ci `
    && npm run build `
    && cd $root `
    && nuget restore $root\src\Windows.sln `
    && msbuild $root\src\Windows.sln `
        -v:q `
        -p:DeployOnBuild=true `
        -p:PublishProfile=$root\ci\windows\GenericPublish.pubxml
