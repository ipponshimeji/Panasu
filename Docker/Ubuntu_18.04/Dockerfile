ARG repo='mcr.microsoft.com/dotnet/core/runtime'
ARG tag='3.1-bionic'
FROM $repo:$tag
ARG powershell_tag=''
ARG pandoc_tag=''
ARG panasu_tag=''

WORKDIR /tmp/panasu
COPY ./scripts/ ./
WORKDIR /
RUN cd /tmp/panasu \
    && apt-get update \
    && apt-get install -y jq unzip libssl1.0.0 liblttng-ust0 \
    && chmod u+x *.sh \
    && ./install_powershell_ubuntu-18.04.sh $powershell_tag \
    && ./install_pandoc_deb.sh $pandoc_tag \
    && ./install_panasu.sh $pandoc_tag \
    && cd .. \
    && rm -rf panasu
