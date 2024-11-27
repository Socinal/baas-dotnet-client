{ pkgs, ... }:

{
  cachix.enable = false;
  packages = with pkgs; [ csharp-ls ];

  languages.dotnet = {
    enable = true;
    package = pkgs.dotnet-sdk;
  };
}
