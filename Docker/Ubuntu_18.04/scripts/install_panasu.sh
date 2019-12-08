#!/bin/bash

# Arguments:
#   1: The tag name (version) of Panasu to be installed.
#      It installs the latest version if it is empty.

# exit if any command fails 
set -e

# select version to be installed
tag_name=$1
if [[ -z $targetver ]]; then
    # get the latest version
    # Note that GitHub API has rate limiting for requests.
    # For unauthenticated requests, it is 60 requests per hour. 
    # See https://developer.github.com/v3/#rate-limiting
    echo 'detecting the latest version of Panasu'
    tag_name=`curl -s https://api.github.com/repos/ipponshimeji/Panasu/releases/latest | jq -r .tag_name`
fi
# tag_name is supposed to be like 'X.Y.Z'
target_ver=$tag_name    # same to the tag name

# download the package of the version
package="Panasu_${target_ver}.zip" 
echo 'downloading Panasu'
# use curl with -L option because the URL is redirected
curl -s -OL https://github.com/ipponshimeji/Panasu/releases/download/$tag_name/$package

# expand the package
echo 'installing Panasu'
unzip -d ~/.local $package

# setup sample
curl -s -OL https://github.com/ipponshimeji/Panasu/archive/$tag_name.zip
unzip $tag_name.zip
cp -r Panasu-$tag_name/Samples/simple ~/
mv ~/simple ~/PanasuSample
sed -e 's/\[string\]$PanasuPath =.*$/\[string\]$PanasuPath = "$HOME\/.local\/Panasu"/' \
    -e 's/\[string\[\]\]$MetadataFiles =.*$/\[string\[\]\]$MetadataFiles = @\("html\.metadata\.yaml"\),/' \
    ~/.local/Panasu/format.template.ps1 > ~/PanasuSample/_scripts/format.ps1
chmod u+x ~/PanasuSample/_scripts/format.ps1
