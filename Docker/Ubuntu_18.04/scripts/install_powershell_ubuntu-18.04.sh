#!/bin/bash

# The script to install PowerShell Core into Ubuntu 18.04
#
# There are two way to install PowerShell Core into Ubuntu 18.04 system:
#   * Installation via Package Repository
#   * Installation via Direct Download
# See https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-linux#ubuntu-1804
# In this script, we run the latter way, because the former way requires
# too many dependency and it makes the container image fatter.
#  
# Arguments:
#   1: The tag name (version) of PowerShell Core to be installed.
#      It installs the latest version if it is empty.

# exit if any command fails 
set -e

# detect arch
case `uname -m` in
    'amd64' ) arch='amd64';;
    'x86_64' ) arch='amd64';;
    * ) echo 'unsupported arch' 1>&2; exit 1;;
esac

# select version to be installed
tag_name=$1
if [[ -z $targetver ]]; then
    # get the latest version
    # Note that GitHub API has rate limiting for requests.
    # For unauthenticated requests, it is 60 requests per hour. 
    # See https://developer.github.com/v3/#rate-limiting
    echo 'detecting the latest version of PowerShell'
    tag_name=`curl -s https://api.github.com/repos/PowerShell/PowerShell/releases/latest | jq -r .tag_name`
fi
# tag_name is supposed to be 'vX.Y.Z'
target_ver=${tag_name:1}    # remove the leading 'v'

# download the package of the version
package="powershell_${target_ver}-1.ubuntu.18.04_${arch}.deb" 
echo 'downloading PowerShell'
# use curl with -L option because the URL is redirected
curl -s -OL https://github.com/PowerShell/PowerShell/releases/download/$tag_name/$package

# install the package
echo 'installing PowerShell'
dpkg -i $package
apt-get install -f
