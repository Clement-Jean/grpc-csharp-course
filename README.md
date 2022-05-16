# gRPC C#

## Reminder

- In order to run SSL features, do not forget to run the shell script
- If you find any missing or uncorrect code, please report an issue.
- Feel free to make pull requests !

## Working directory

When cloning the repository the working directory will be empty, in order to get its content run:

```
git submodule update --init --recursive
```

then to install the templates run:

```
dotnet new --install working/templates/grpcserver
dotnet new --install working/templates/grpcclient
```

and to uninstall:

```
dotnet new --uninstall working/templates/grpcserver
dotnet new --uninstall working/templates/grpcclient
```