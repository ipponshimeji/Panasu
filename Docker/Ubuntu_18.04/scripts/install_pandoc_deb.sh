#!/bin/bash

# Arguments:
#   1: The tag name (version) of pandoc to be installed.
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
    echo 'detecting the latest version of pandoc'
    tag_name=`curl -s https://api.github.com/repos/jgm/pandoc/releases/latest | jq -r .tag_name`
fi
# tag_name is supposed to be like 'X.Y.Z'
target_ver=$tag_name    # same to the tag name

# download the package of the version
package="pandoc-${target_ver}-1-${arch}.deb" 
echo 'downloading pandoc'
# use curl with -L option because the URL is redirected
curl -s -OL https://github.com/jgm/pandoc/releases/download/$tag_name/$package

# install the package
echo 'installing pandoc'
dpkg -i $package
