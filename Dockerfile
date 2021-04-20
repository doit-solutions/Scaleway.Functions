FROM mcr.microsoft.com/dotnet/sdk:5.0.202-buster-slim AS development
RUN apt update && apt install -y zsh git
ENV SHELL /bin/zsh
ADD https://github.com/JanDeDobbeleer/oh-my-posh3/releases/latest/download/posh-linux-amd64 /usr/local/bin/oh-my-posh
RUN chmod +x /usr/local/bin/oh-my-posh
ADD https://github.com/JanDeDobbeleer/oh-my-posh3/raw/main/themes/paradox.omp.json /root/downloadedtheme.omp.json
RUN echo $'function powerline_precmd() {\n\
    PS1="$(oh-my-posh -config ~/downloadedtheme.omp.json --error $?)"\n\
}\n\
function install_powerline_precmd() {\n\
  for s in "${precmd_functions[@]}"; do\n\
    if [ "$s" = "powerline_precmd" ]; then\n\
      return\n\
    fi\n\
  done\n\
  precmd_functions+=(powerline_precmd)\n\
}\n\
if [ "$TERM" != "linux" ]; then\n\
    install_powerline_precmd\n\
fi\n' >> /root/.zshrc
RUN dotnet tool install --global dotnet-outdated-tool
RUN dotnet tool install --global dotnet-ossindex
ENV PATH ${PATH}:/root/.dotnet/tools
ENV GIT_EDITOR "code --wait"
