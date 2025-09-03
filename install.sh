wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && chmod +x dotnet-install.sh && ./dotnet-install.sh --channel 9.0 && export PATH=$HOME/.dotnet:$PATH && dotnet --version

dotnet dev-certs https --trust