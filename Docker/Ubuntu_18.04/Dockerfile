ARG repo='mcr.microsoft.com/dotnet/core/runtime'
ARG tag='3.1-bionic'
FROM $repo:$tag
ARG powershell_tag=''
ARG pandoc_tag=''
ARG panasu_tag=''

WORKDIR /tmp/panasu
COPY ./scripts/ ./
WORKDIR /
# Note that if you build image using "docker build <options> <GitHub URL>" form,
# the resources are checked out with default new line code handling.
# That is, the new line code in scripts is converted to CRLF
# if you build with Docker Desktop on Windows system.
# This is why sed command is inserted below.
RUN cd /tmp/panasu \
    && apt-get update \
    && apt-get install -y jq unzip libssl1.0.0 liblttng-ust0 \
    && sed -i -e 's/\r$//' *.sh \
    && chmod u+x *.sh \
    && ./install_powershell_ubuntu-18.04.sh $powershell_tag \
    && ./install_pandoc_deb.sh $pandoc_tag \
    && ./install_panasu.sh $pandoc_tag \
    && cd .. \
    && rm -rf panasu
