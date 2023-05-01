# BlogMan

A cross-platform static blog webpage generator, written in C#.

## SYNOPSIS

```shell
blogman [COMMAND] [OPTION]...
```

## INSTALLATION

`blogman` provides auto installing script for **linux**.
Thus, if you use linux, you can install `blogman` with below commands:

```shell
git clone https://Sharp0802/blogman
cd blogman-master/src
sudo ./install.sh
```

And, you can uninstall `blogman` with below commands:

```shell
git clone https://Sharp0802/blogman
cd blogman-master/src
sudo ./uninstall.sh
```

## DESCRIPTION

See [wiki](https://github.com/Sharp0802/blogman/wiki).

## SECURITY

`blogman` uses razor templating system.
Because of not only that razor template uses C# scripting internally
also that `RazorEngine` library has its own security vulnerability(remote code execution),
Using untrusted template file can be harmful to host computer.
Thus, do **NOT** use template file you cannot trust.
